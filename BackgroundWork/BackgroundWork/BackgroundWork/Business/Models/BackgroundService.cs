using System;
using System.Threading;
using System.Threading.Tasks;

#if ANDROID

using Android;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;

using AndroidX.Core.App;
using AndroidX.Core.Content.Resources;

using BackgroundWork.Business.Interfaces;

#endif
namespace BackgroundWork.Business.Models
{
#if ANDROID
	public class BackgroundService : Service
	{
        /// <summary>
        /// The Unique Id of the Service
        /// </summary>
        private const int SERVICE_ID = 5050;

        private const string SERVICE_NOTIFICATION_CHANNEL_ID = "5051";

        private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);
        private readonly IBackgroundWorker _backgroundWorker;

        private bool _workerActive;
        private CancellationTokenSource _cancellationTokenSource;


        public BackgroundService()
		{
            _backgroundWorker = (Microsoft.UI.Xaml.Application.Current as App).Host.Services.GetRequiredService<IBackgroundWorker>();
        }

        public override IBinder? OnBind(Intent? intent)
        {
            return null;
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags,
       int startId)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            _backgroundWorker.WorkerStopped += OnWorkerStopped;
            //  Build the notification for the foreground service
            var notification = BuildNotification();
            StartForeground(SERVICE_ID, notification);

            _ = Task.Run(async () =>
            {
                _workerActive = true;
                while (_workerActive)
                {
                    await Task.Delay(_interval, _cancellationTokenSource.Token);
                    await _backgroundWorker.BackgroundWork();
                }
            });

            //  Return a sticky result so that the service remains running
            return StartCommandResult.Sticky;
        }

        /// <summary>
        /// Worker Stopped
        /// </summary>
        private void OnWorkerStopped(object sender, EventArgs e)
        {
            _workerActive = false;
            _cancellationTokenSource.Cancel();
            _backgroundWorker.WorkerStopped -= OnWorkerStopped;
            StopForeground(removeNotification: true);
            StopSelf();
        }

        /// <summary>
        /// Build Service Notification Tile
        /// </summary>
        /// <returns>Service Notification Tile</returns>
        private Notification BuildNotification()
        {
            // Building intent
            var intent = new Intent(Uno.UI.ContextHelper.Current, typeof(ApplicationActivity));
            intent.AddFlags(ActivityFlags.NoUserAction);
            intent.PutExtra("Title", "Message");

            var pendingIntent = PendingIntent.GetActivity(Uno.UI.ContextHelper.Current, 0, intent, PendingIntentFlags.UpdateCurrent);

            var notificationBuilder = new NotificationCompat.Builder(Uno.UI.ContextHelper.Current, SERVICE_NOTIFICATION_CHANNEL_ID)
                .SetContentTitle("Name of BG Work")
                .SetContentText("User Friendly Description")
                //.SetSmallIcon(Resource.Mipmap.SymDefAppIcon)
                //.SetColor(ResourcesCompat.GetColor(Uno.UI.ContextHelper.Current.Resources, Resource.Color.BackgroundDark, null))
                .SetOngoing(true)
                .SetContentIntent(pendingIntent);

            // Building channel if API version is 26 or above
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                NotificationChannel notificationChannel = new NotificationChannel(SERVICE_NOTIFICATION_CHANNEL_ID,
                    "AppName", NotificationImportance.Low);
                notificationChannel.Importance = NotificationImportance.Low;
                notificationChannel.EnableLights(true);
                notificationChannel.EnableVibration(false);
                notificationChannel.SetShowBadge(true);

                if (Uno.UI.ContextHelper.Current.GetSystemService(NotificationService) is NotificationManager notificationManager)
                {
                    notificationBuilder.SetChannelId(SERVICE_NOTIFICATION_CHANNEL_ID);
                    notificationManager.CreateNotificationChannel(notificationChannel);
                }
            }

            return notificationBuilder.Build();
        }



}
#endif
}

