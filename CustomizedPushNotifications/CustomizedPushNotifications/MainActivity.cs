using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Views;
using Android.Widget;

using System;
using System.Collections.Generic;

namespace CustomizedPushNotifications
{
    [Activity(Label = "Customized Push Notifications", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private static readonly ApiService service = new ApiService();
        private static ISharedPreferences preferences;
        private ArrayAdapter streamerAdapter;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            preferences = PreferenceManager.GetDefaultSharedPreferences(this);

            // The switch enables/disables the apps push notifications
            var activeSwitch = FindViewById<Switch>(Resource.Id.activeSwitch);
            activeSwitch.CheckedChange += SwitchToggled;
            activeSwitch.Checked = preferences.GetBoolean("notifications_active", false);

            var addButton = FindViewById<Button>(Resource.Id.addButton);
            addButton.Click += AddButtonClicked;

            var testButton = FindViewById<Button>(Resource.Id.removeButton);
            testButton.Click += RemoveButtonClicked;

            var listView = FindViewById<ListView>(Resource.Id.listView1);
            var items = preferences.GetString("streamer_names", "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            streamerAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleListItemMultipleChoice, new List<string>(items));
            listView.Adapter = streamerAdapter;
        }

        private void RemoveButtonClicked(object sender, EventArgs e)
        {
            var streamers = FindViewById<ListView>(Resource.Id.listView1).CheckedItemPositions;
            var items = preferences.GetString("streamer_names", "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            for (var i = 0; i < streamers.Size(); i++)
            {
                if (streamers.ValueAt(i) == false) continue;

                streamerAdapter.Remove(items[streamers.KeyAt(i)]);
                items[streamers.KeyAt(i)] = null;
            }

            ISharedPreferencesEditor editor = preferences.Edit();
            editor.PutString("streamer_names", String.Join(";", items));
            editor.Apply();

            
            streamerAdapter.NotifyDataSetChanged();
        }

        /// <summary>
        /// Adds a new streamer.
        /// </summary>
        private void AddButtonClicked(object sender, EventArgs e)
        {
            var newStreamerText = FindViewById<EditText>(Resource.Id.newStreamerText);
            var streamerNames = preferences.GetString("streamer_names", "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            ISharedPreferencesEditor editor = preferences.Edit();
            Array.Resize(ref streamerNames, streamerNames.Length + 1);
            streamerNames[streamerNames.Length - 1] = newStreamerText.Text;                
            editor.PutString("streamer_names", String.Join(";", streamerNames));
            editor.Apply();
            streamerAdapter.Insert(newStreamerText.Text, streamerAdapter.Count);
            streamerAdapter.NotifyDataSetChanged();
        }

        private void SwitchToggled(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (e.IsChecked)
            {
                ApplicationContext.StartService(new Intent(this, typeof(ApiService)));

                ISharedPreferencesEditor editor = preferences.Edit();
                editor.PutBoolean("notifications_active", true);
                editor.Apply();
            }
            else
            {
                ApplicationContext.StopService(new Intent(this, typeof(ApiService)));

                ISharedPreferencesEditor editor = preferences.Edit();
                editor.PutBoolean("notifications_active", false);
                editor.Apply();
            }
        }
    }

    public class StringListViewAdapter : BaseAdapter<string>
    {
        string[] items;
        Activity context;

        public StringListViewAdapter(Activity context, string[] items) : base()
        {
            this.context = context;
            this.items = items;
        }

        // Set our view from the "main" layout resource
        // SetContentView (Resource.Layout.Main);
        public override long GetItemId(int position)
        {
            return position;
        }

        public override string this[int position]
        {
            get { return items[position]; }
        }

        public override int Count
        {
            get { return items.Length; }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            View view = convertView; // re-use an existing view, if one is available
            if (view == null)
            { // otherwise create a new one
                view = context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItemMultipleChoice, null);
            }
            view.FindViewById<TextView>(Android.Resource.Id.Text1).Text = items[position];
            return view;
         }
     }
}

