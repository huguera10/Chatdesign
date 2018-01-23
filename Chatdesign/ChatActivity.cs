using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Chatdesign;
using Parse;

namespace Chat
{
    [Activity(Label = "Chat")]
    public class ChatActivity : Activity
    {
        private EditText msg_edittext;
        private String user1, user2;
        private bool user1First;
        private Random random;
        private List<ChatMessage> chatlist;
        private static ChatAdapter chatAdapter;
        ListView msgListView;

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

            msgListView.TranscriptMode = TranscriptMode.AlwaysScroll;
            msgListView.StackFromBottom = true;
            chatlist = new List<ChatMessage>();
            chatAdapter = new ChatAdapter(this, chatlist);
            msgListView.Adapter = chatAdapter;

            loadPreviousChatsAsync();
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

            Console.WriteLine("username " + user1 + "\tcontacts " + users.Count());

            foreach (var user in users)
            {
                ChatMessage chatMessage;
                if (user1First)
                {
                    chatMessage = new ChatMessage(user1, user2,
                        user.Get<string>("message"), DateTime.Now,
                        user.Get<bool>("isMine"), false);
                }
                else
                {
                     chatMessage = new ChatMessage(user1, user2,
                     user.Get<string>("message"), DateTime.Now,
                     !user.Get<bool>("isMine"), false);
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
                chatMessage.body = message;
                msg_edittext.Text = "";
                chatAdapter.add(chatMessage);
                chatAdapter.NotifyDataSetChanged();

                chatBotMessage();
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