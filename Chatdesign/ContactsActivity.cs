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
    [Activity(Label = "Contacts")]
    public class ContactsActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Contacts);

            // Create your application here

            loadContactsListAsync();
        }

        private async Task loadContactsListAsync()
        {
            string[] contacts = await getContactsAsync();
            ListView listView = (ListView)FindViewById(Resource.Id.contactsListView);

            ArrayAdapter<String> listViewAdapter = new ArrayAdapter<string>
                (
                this,
                Resource.Layout.list_view_item,
                contacts
                );

            listView.TranscriptMode = TranscriptMode.AlwaysScroll;
            listView.Adapter = listViewAdapter;

            listView.ItemClick += (parent, args) =>
            {
                var intent = new Intent(this, typeof(ChatActivity));
                intent.PutExtra("contactUsername", contacts[args.Position]);
                StartActivity(intent);
            };
        }

        private async Task<string[]> getContactsAsync()
        {
            ParseUser parseUser = ParseUser.CurrentUser;
            string currentUsername = parseUser.Username;

            IEnumerable<ParseObject> users = await ParseUser.Query
                .WhereNotEqualTo("username", currentUsername)
                .FindAsync();             

            string[] contacts = new string[users.Count()];
            int count = 0;
            foreach (var user in users)
            {
                contacts[count++] = user.Get<String>("username");
            }

            return contacts;
        }
    }
}