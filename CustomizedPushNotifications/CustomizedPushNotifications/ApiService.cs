using CustomizedPushNotifications.API;
using CustomizedPushNotifications.API.Twitch;

using Android.App;
using Android.Content;
using Android.Support.V4.App;
using Android.OS;

using Newtonsoft.Json;

using System;
using System.IO;
using System.Threading;

using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;
using Android.Preferences;

namespace CustomizedPushNotifications
{
    /// <summary>
    /// Provides a background service, which ckecks for updated statuses in given intervals.
    /// </summary>
    [Service(Name = "thulur.CustomizedPushNotifications.ApiService", Exported = true, Process = ":ServiceProcess")]
    public class ApiService : Service
    {
        private static readonly int ButtonClickNotificationId = 1000;
        // Number of seconds between to checks for new notifications
        private static readonly int TimerWait = 5000;
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
            var preferences = PreferenceManager.GetDefaultSharedPreferences(this);
            var streamerNames = preferences.GetString("streamer_names", "").Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var streamersOnline = Twitch.StreamersOnline(streamerNames, configuration);

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