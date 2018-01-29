using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.IO;
using Parse;

namespace Chatdesign
{
    class ChatMessage
    {
        public String body, sender, receiver, senderName;
        public DateTime date;
        public Boolean isMine;// Did I send the message.
        private ParseFile parseFile;
        private byte[] byteFile = null;


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
                if (!body.Equals("")) {
                    storeMessageOnDataBaseAsync();
                }
            }
        }

        public async void storeAttachmentOnDataBaseAsync()
        {

            System.Console.Write("CHEGOU");
            await parseFile.SaveAsync();
            System.Console.Write("CHEGOU 2222222");

            bool owner = isMine;
            ParseObject objClass = new ParseObject("usersChats");
            //sempre guardar com o "menor" nome em ordem alfabética como "user1"
            if (sender.CompareTo(receiver) < 0)
            {
                objClass["user1"] = ParseObject.CreateWithoutData("_User", sender);
                objClass["user2"] = ParseObject.CreateWithoutData("_User", receiver);
            }
            else
            {
                objClass["user1"] = ParseObject.CreateWithoutData("_User", receiver);
                objClass["user2"] = ParseObject.CreateWithoutData("_User", sender);
                owner = !owner;
            }

            objClass["createdAt"] = date;
            objClass["isMine"] = owner;
            objClass["attach"] = parseFile;
            objClass["message"] = "";

            await objClass.SaveAsync();
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

        public void setByteFile(byte[] byteFile)
        {
            this.byteFile = byteFile;
        }

        public byte[] getByteFile()
        {
            return byteFile;
        }

        internal void setParseFile(ParseFile parseFile)
        {
            this.parseFile = parseFile;
        }
    }
}