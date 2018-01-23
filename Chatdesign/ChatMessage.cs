using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Parse;

namespace Chatdesign
{
    class ChatMessage
    {
        public String body, sender, receiver, senderName;
        public DateTime date;
        public Boolean isMine;// Did I send the message.

        public ChatMessage(String Sender, String Receiver, String messageString,
                    DateTime Date, Boolean isMINE, Boolean storeOnDatabase)
        {
            body = messageString;
            isMine = isMINE;
            sender = Sender;
            receiver = Receiver;
            senderName = sender;
            date = Date;

            if (storeOnDatabase)
            {
                storeMessageOnDataBaseAsync();
            }
        }

        private async void storeMessageOnDataBaseAsync()
        {
            bool owner = isMine;
            ParseObject objClass = new ParseObject("usersChats");
            //sempre guardar com o "menor" nome em ordem alfabética como "user1"
            if (sender.CompareTo(receiver) < 0) {
                objClass["user1"] = ParseObject.CreateWithoutData("_User", sender);
                objClass["user2"] = ParseObject.CreateWithoutData("_User", receiver);
            }
            else {
                objClass["user1"] = ParseObject.CreateWithoutData("_User", receiver);
                objClass["user2"] = ParseObject.CreateWithoutData("_User", sender);
                owner = !owner;
            }

            objClass["createdAt"] = date;
            objClass["isMine"] = owner;
            objClass["message"] = body;

            await objClass.SaveAsync();
        }
    }
}