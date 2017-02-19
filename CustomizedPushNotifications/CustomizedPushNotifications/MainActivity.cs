using Android.App;
using Android.Widget;
using Android.OS;
using Android.Content;
using System;
using CustomizedPushNotifications.API.Twitch;
using Android.Views;

//using CustomizedPushNotifications.API.Twitch;

namespace CustomizedPushNotifications
{
    [Activity(Label = "Customized Push Notifications", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            var listView1 = FindViewById<ListView>(Resource.Id.listView1);
            string[] items = { "laraloft: " + Twitch.StreamerOnline("laraloft"), "gronkh: " + Twitch.StreamerOnline("gronkh") };
            listView1.Adapter = new StringListViewAdapter(this, items);
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
                view = context.LayoutInflater.Inflate(Android.Resource.Layout.SimpleListItem1, null);
            }
            view.FindViewById<TextView>(Android.Resource.Id.Text1).Text = items[position];
            return view;
        }
    }



    /*[Service]
    public class ApiService : Service
    {
        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }

        public override StartCommandResult OnStartCommand(Android.Content.Intent intent, StartCommandFlags flags, int startId)
        {
            var t = new Thread(() => {
                Log.Debug("DemoService", "Doing work");
                Thread.Sleep(5000);
                Log.Debug("DemoService", "Work complete");
                StopSelf();
            }
            );
            t.Start();
            return StartCommandResult.Sticky;
        }
    }*/
}

