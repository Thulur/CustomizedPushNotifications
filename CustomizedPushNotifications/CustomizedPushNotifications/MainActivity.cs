using Android.App;
using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

using Newtonsoft.Json;
using System;
using System.Threading;

using CustomizedPushNotifications.API;
using CustomizedPushNotifications.API.Twitch;

using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;
using System.IO;

namespace CustomizedPushNotifications
{
    [Activity(Label = "Customized Push Notifications", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private static readonly ApiService service = new ApiService();
        private static ISharedPreferences preferences;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            preferences = GetPreferences(FileCreationMode.Private);

            var activeSwitch = FindViewById<Switch>(Resource.Id.activeSwitch);
            activeSwitch.CheckedChange += SwitchToggled;
            activeSwitch.Checked = preferences.GetBoolean("notifications_active", false);
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
        private string[] items;
        private Activity context;

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



    [Service(Name="thulur.CustomizedPushNotifications.ApiService", Exported=true, Process= ":ServiceProcess")]
    public class ApiService : Service
    {
        private static readonly int ButtonClickNotificationId = 1000;
        private static readonly int TimerWait = 30000;
        private static Configuration configuration;
        private Timer timer;
        private DateTime startTime;
        private bool isStarted = false;

        public override void OnCreate()
        {
            base.OnCreate();

            using (StreamReader sr = new StreamReader(Assets.Open("Configuration.json")))
            {
                configuration = JsonConvert.DeserializeObject<Configuration>(sr.ReadToEnd());
            }
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (!isStarted)
            {
                startTime = DateTime.UtcNow;
                timer = new Timer(HandleTimerCallback, startTime, 0, TimerWait);
                isStarted = true;
            }

            return StartCommandResult.Sticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }


        public override void OnDestroy()
        {
            timer.Dispose();
            timer = null;
            isStarted = false;

            base.OnDestroy();
        }

        void HandleTimerCallback(object state)
        {
            var streamersOnline = Twitch.StreamersOnline(new string[] { "laraloft", "gronkh" }, configuration);

            foreach (var entry in streamersOnline)
            {
                if (!entry.Value) continue;

                Bundle valuesForActivity = new Bundle();
                Intent resultIntent = new Intent(this, typeof(MainActivity));
                resultIntent.PutExtras(valuesForActivity);

                TaskStackBuilder stackBuilder = TaskStackBuilder.Create(this);
                stackBuilder.AddParentStack(Java.Lang.Class.FromType(typeof(MainActivity)));
                stackBuilder.AddNextIntent(resultIntent);

                PendingIntent resultPendingIntent = stackBuilder.GetPendingIntent(0, (int)PendingIntentFlags.UpdateCurrent);

                NotificationCompat.Builder builder = new NotificationCompat.Builder(this)
                    .SetAutoCancel(true)
                    .SetContentIntent(resultPendingIntent)
                    .SetContentTitle("Streamer online")
                    .SetSmallIcon(Resource.Drawable.Icon)
                    .SetContentText(entry.Key + " is streaming now");

                NotificationManager notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.Notify(ButtonClickNotificationId, builder.Build());
            }
        }
    }
}

