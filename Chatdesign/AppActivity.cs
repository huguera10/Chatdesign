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
    [Activity(Label = "Chat", MainLauncher = true, Icon = "@drawable/icon")]
    public class AppActivity : Activity
    {
        private EditText mUsernameView;
        private EditText mPasswordView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.App);
            // Create your application here

            ParseClient.Initialize(new ParseClient.Configuration
            {
                ApplicationId = GetString(Resource.String.back4app_app_id),
                WindowsKey = GetString(Resource.String.back4app_dotnet_key),
                Server = "https://parseapi.back4app.com/"

            });

            ParseInstallation.CurrentInstallation.SaveAsync();

            Button btnSignIn = (Button)FindViewById(Resource.Id.btnSignIn);
            btnSignIn.Click += async delegate
            {
                try
                {
                    mUsernameView = (EditText)FindViewById(Resource.Id.et_username);
                    mPasswordView = (EditText)FindViewById(Resource.Id.et_password);
                    if (!mUsernameView.Text.Equals("") && !mPasswordView.Text.Equals(""))
                    {
                        await ParseUser.LogInAsync(mUsernameView.Text, mPasswordView.Text);
                        StartActivity(typeof(ContactsActivity));
                    }
                    //Login was successful.
                }
                catch (Exception e)
                {
                    Toast.MakeText(this, "Invalid email or password.", ToastLength.Long).Show();
                }
            };

            Button btnSignUp = (Button)FindViewById(Resource.Id.btnSignUp);
            btnSignUp.Click += delegate
            {
                StartActivity(typeof(RegisterActivity));
            };

        }
        
    }
}