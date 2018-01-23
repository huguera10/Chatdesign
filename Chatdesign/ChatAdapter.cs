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
using Android.Graphics;

namespace Chatdesign
{
    class ChatAdapter : BaseAdapter
    {
        List<ChatMessage> chatMessageList;
        private static LayoutInflater inflater = null;
        Context context;

        public ChatAdapter(Activity activity, List<ChatMessage> list)
        {
            chatMessageList = list;
            inflater = (LayoutInflater)activity.GetSystemService(Context.LayoutInflaterService);
        }


        public override Java.Lang.Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ChatMessage message = (ChatMessage)chatMessageList.ElementAt(position);
            var vi = convertView;

            if (convertView == null)
                vi = inflater.Inflate(Chat.Resource.Layout.chatbubble, null);
            TextView msg = (TextView)vi.FindViewById(Chat.Resource.Id.message_text);
            msg.Text = message.body;
            LinearLayout layout = (LinearLayout)vi
                    .FindViewById(Chat.Resource.Id.bubble_layout);
            LinearLayout parent_layout = (LinearLayout)vi
                    .FindViewById(Chat.Resource.Id.bubble_layout_parent);

            // if message is mine then align to right
            if (message.isMine)
            {
                layout.SetBackgroundResource(Chat.Resource.Drawable.bubble2);
                parent_layout.SetGravity(GravityFlags.Right);
            }
            // If not mine then align to left
            else
            {
                layout.SetBackgroundResource(Chat.Resource.Drawable.bubble1);
                parent_layout.SetGravity(GravityFlags.Left);
            }
            msg.SetTextColor(Color.Black);
            return vi;
            
        }

        public void add(ChatMessage msg)
        {
            chatMessageList.Add(msg);
        }

        //Fill in cound here, currently 0
        public override int Count
        {
            get
            {
                return chatMessageList.Count;
            }
        }

    }

    class ChatAdapterViewHolder : Java.Lang.Object
    {
        //Your adapter views to re-use
        //public TextView Title { get; set; }
    }

}