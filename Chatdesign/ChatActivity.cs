using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Chatdesign;
using Parse;
using System.Net.Http;

namespace Chat
{
    [Activity(Label = "Chat")]
    public class ChatActivity : Activity
    {
        public static readonly int PickImageId = 1000;

        private EditText msg_edittext;
        private String user1, user2;
        private bool user1First;
        private Random random;
        private List<ChatMessage> chatlist;
        private static ChatAdapter chatAdapter;
        private ListView msgListView;

        private DateTime? lastMsgDate = DateTime.MinValue;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Chat);

            user1 = ParseUser.CurrentUser.Username;
            user2 = Intent.GetStringExtra("contactUsername") ?? "Data not available";
            setUserAlphabeticalOrder();
            this.Title = user2;

            random = new Random();
            msg_edittext = (EditText)FindViewById(Resource.Id.messageEditText);
            msgListView = (ListView)FindViewById(Resource.Id.msgListView);
            ImageButton sendButton = (ImageButton)FindViewById(Resource.Id.sendMessageButton);
            sendButton.Click += delegate
            {
                sendTextMessage();
            };

            ImageButton attachButton = (ImageButton)FindViewById(Resource.Id.attachmentButton);
            attachButton.Click += ButtonOnClick;

            msgListView.TranscriptMode = TranscriptMode.AlwaysScroll;
            msgListView.StackFromBottom = true;
            chatlist = new List<ChatMessage>();
            chatAdapter = new ChatAdapter(this, chatlist);
            msgListView.Adapter = chatAdapter;

            loadPreviousChatsAsync();

        }

        private void ButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                Intent = new Intent();
                Intent.SetType("image/*");
                Intent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), PickImageId);
            }
            catch (Exception)
            {
                Toast.MakeText(this, "Select a JPG image!", ToastLength.Long);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if ((requestCode == PickImageId) && (resultCode == Result.Ok) && (data != null))
                
            {
                Android.Net.Uri uriImage = data.Data;
                string path = GetPathToImage(uriImage);

                byte[] img = System.IO.File.ReadAllBytes(path);
                ParseFile parseFile = new ParseFile("img.jpg", img);

                ChatMessage chatMessage = new ChatMessage(user1, user2,
                            "", DateTime.Now, true, true);
                msg_edittext.Text = "";
                chatMessage.setByteFile(img);
                chatMessage.setParseFile(parseFile);
                chatMessage.storeAttachmentOnDataBaseAsync();

                chatAdapter.add(chatMessage);
                chatAdapter.NotifyDataSetChanged();
            }
        }

        private string GetPathToImage(Android.Net.Uri uri)
        {
            string doc_id = "";
            using (var c1 = ContentResolver.Query(uri, null, null, null, null))
            {
                c1.MoveToFirst();
                String document_id = c1.GetString(0);
                doc_id = document_id.Substring(document_id.LastIndexOf(":") + 1);
            }

            string path = null;

            // The projection contains the columns we want to return in our query.
            string selection = Android.Provider.MediaStore.Images.Media.InterfaceConsts.Id + " =? ";
            using (var cursor = ManagedQuery(Android.Provider.MediaStore.Images.Media.ExternalContentUri, null, selection, new string[] { doc_id }, null))
            {
                if (cursor == null) return path;
                var columnIndex = cursor.GetColumnIndexOrThrow(Android.Provider.MediaStore.Images.Media.InterfaceConsts.Data);
                cursor.MoveToFirst();
                path = cursor.GetString(columnIndex);
            }
            return path;
        }

        private void setUserAlphabeticalOrder()
        {
            if (user1.CompareTo(user2) < 0)
            {
                user1First = true;
            }
            else
            {
                user1First = false;
            }
        }

        private async void loadPreviousChatsAsync()
        {

            IEnumerable<ParseObject> users;
            if (user1First)
            //os dados estão salvos com o "user1" sempre 'menor'
            //que o "user2" pela ordem alfabética
            {
                users = await ParseObject.GetQuery("usersChats")
                    .WhereEqualTo("user1", ParseObject.CreateWithoutData("_User", user1))
                    .WhereEqualTo("user2", ParseObject.CreateWithoutData("_User", user2))
                    .OrderBy("createdAt")
                    .FindAsync();
            }
            else
            {
                users = await ParseObject.GetQuery("usersChats")
                       .WhereEqualTo("user2", ParseObject.CreateWithoutData("_User", user1))
                       .WhereEqualTo("user1", ParseObject.CreateWithoutData("_User", user2))
                       .OrderBy("createdAt")
                       .FindAsync();
            }

            updateChatViewAsync(users);

            while (true)
            {
                await Task.Delay(3000);

                System.Console.WriteLine("FAZER CHAMADO AO BANCO AQUI");
                if (user1First)
                //os dados estão salvos com o "user1" sempre 'menor'
                //que o "user2" pela ordem alfabética
                {
                    users = await ParseObject.GetQuery("usersChats")
                        .WhereEqualTo("user1", ParseObject.CreateWithoutData("_User", user1))
                        .WhereEqualTo("user2", ParseObject.CreateWithoutData("_User", user2))
                        .WhereEqualTo("isMine", false)
                        .WhereGreaterThan("createdAt", lastMsgDate)
                        .OrderBy("createdAt")
                        .FindAsync();

                    updateChatViewAsync(users);
                }
                else
                {
                    users = await ParseObject.GetQuery("usersChats")
                           .WhereEqualTo("user2", ParseObject.CreateWithoutData("_User", user1))
                           .WhereEqualTo("user1", ParseObject.CreateWithoutData("_User", user2))
                           .WhereEqualTo("isMine", true)
                           .WhereGreaterThan("createdAt", lastMsgDate)
                           .OrderBy("createdAt")
                           .FindAsync();

                    updateChatViewAsync(users);
                }
            }
        }

        private async Task updateChatViewAsync(IEnumerable<ParseObject> users)
        {
            foreach (var user in users)
            {
                ChatMessage chatMessage;
                if (user1First)
                {
                    if (!user.Get<string>("message").Equals(""))
                    {
                        chatMessage = new ChatMessage(user1, user2,
                        user.Get<string>("message"), DateTime.Now,
                        user.Get<bool>("isMine"), false);
                    }
                    else
                    {
                        chatMessage = new ChatMessage(user1, user2,
                        "", DateTime.Now,
                        user.Get<bool>("isMine"), false);

                        var file = user.Get<ParseFile>("attach");
                        byte[] byteFile = await new HttpClient().GetByteArrayAsync(file.Url);
                        chatMessage.setByteFile(byteFile);
                    }

                    if (!user.Get<bool>("isMine"))
                    {
                        lastMsgDate = user.CreatedAt;
                    }

                }
                else
                {
                    if (!user.Get<string>("message").Equals(""))
                    {
                        chatMessage = new ChatMessage(user1, user2,
                    user.Get<string>("message"), DateTime.Now,
                    !user.Get<bool>("isMine"), false);
                    }
                    else
                    {
                        chatMessage = new ChatMessage(user1, user2,
                        "", DateTime.Now,
                        !user.Get<bool>("isMine"), false);

                        var file = user.Get<ParseFile>("attach");
                        byte[] byteFile = await new HttpClient().GetByteArrayAsync(file.Url);
                        chatMessage.setByteFile(byteFile);
                    }

                    if (user.Get<bool>("isMine"))
                    {
                        lastMsgDate = user.CreatedAt;
                    }
                }

                chatAdapter.add(chatMessage);
                chatAdapter.NotifyDataSetChanged();
            }
            
        }

        public void sendTextMessage()
        {
            String message = msg_edittext.EditableText.ToString();
            if (!message.Equals("", StringComparison.InvariantCultureIgnoreCase))
            {
                ChatMessage chatMessage = new ChatMessage(user1, user2,
                        message, DateTime.Now, true, true);
                msg_edittext.Text = "";
                chatAdapter.add(chatMessage);
                chatAdapter.NotifyDataSetChanged();

                //chatBotMessage();
            }
        }

        public void chatBotMessage()
        {
            char[] alphabet = {' ','a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l',
                'm', 'n', 'o', 'p', 'q', 'r', 's','t','u','v','w','x','y','z'};
            String message = "";

            int maxMessageSize = random.Next(25);

            for (int i = 0; i < maxMessageSize; i++)
            {
                int alphabetId = random.Next(alphabet.Length);
                message += alphabet[alphabetId];
            }

            ChatMessage chatMessage = new ChatMessage(user1, user2,
                    message, DateTime.Now, false, true);
            msg_edittext.Text = "";
            chatAdapter.add(chatMessage);
            chatAdapter.NotifyDataSetChanged();
        }
    }
}