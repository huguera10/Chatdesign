using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    [Activity(Label = "Register")]
    public class RegisterActivity : Activity
    {
        private EditText mUsernameView;
        private EditText mPasswordView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Register);

            mUsernameView = (EditText)FindViewById(Resource.Id.register_username);
            mPasswordView = (EditText)FindViewById(Resource.Id.register_password);

            Button mRegisterButton = (Button)FindViewById(Resource.Id.register);
            mRegisterButton.Click += async delegate
            {
                await SignUpButton_ClickAsync();
            };

        }


        public async Task SignUpButton_ClickAsync()
        {
            bool validationError = false;
            StringBuilder validationErrorMenssage = new StringBuilder("Please ");

            if (isEmpty(mUsernameView))
            {
                validationError = true;
                validationErrorMenssage.Append("enter a username");
            }
            if (isEmpty(mPasswordView))
            {
                if (validationError)
                {
                    validationErrorMenssage.Append(", and ");
                }
                validationError = true;
                validationErrorMenssage.Append("enter a password");
            }
            validationErrorMenssage.Append(".");

            if (validationError)
            {
                Toast.MakeText(this, validationErrorMenssage.ToString(), ToastLength.Long).Show();
                return;
            }

            //Toast.MakeText(this, "CHEGOU ATÉ AQUI "+ mUsernameView.Text +"    " + mPasswordView.Text, ToastLength.Long).Show();

            try
            {
                var user = new ParseUser()
                {
                    Username = mUsernameView.Text,
                    Password = mPasswordView.Text,
                    Email = mUsernameView + "@example.com"
                };

                await user.SignUpAsync();
                StartActivity(typeof(ContactsActivity));
            }
            catch (Exception e)
            {
                Toast.MakeText(this, "Please try again", ToastLength.Long).Show();
            }
        }

        private bool isEmpty(EditText etText)
        {
            if (etText.Text.Trim().Length > 0)
            {
                return false;
            }
            else
                return true;
        }

    }
}