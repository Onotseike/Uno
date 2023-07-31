using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using AndroidX.Core.App;
using AndroidX.Core.Content.Resources;

using Microsoft.Extensions.DependencyInjection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XamarinBackgroundWorker.Droid
{
    public class BackgroundWorker : IBackgroundWorker
    {
        public event EventHandler WorkerStopped;
        public Func<Task> BackgroundWork { get; private set; }
        public void StartWorker(Func<Task> backgroundWork)
        {
            BackgroundWork = backgroundWork;
            var intent = new Intent(Uno.UI.ContextHelper.Current, typeof(BackgroundService));

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                Uno.UI.ContextHelper.Current.StartForegroundService(intent);
            }
            else
            {
                Uno.UI.ContextHelper.Current.StartService(intent);
            }
        }

        public void StopWorker()
        {
            WorkerStopped?.Invoke(this, EventArgs.Empty);
        }

        public Task<bool> IsDataAvailableToSync()
        {
            //todo check if we need to run our BG Work
            //we need to run if any data to sync or download 
            //is available
            return Task.FromResult(true);//true - data available, false - run BG Work is not required
        }
    }

    /// <summary>
    /// Android Native Background Service
    /// </summary>
    [Service]
    public class BackgroundService : Service
    {
        /// <summary>
        /// The Id of the Service
        /// </summary>
        private const int SERVICE_ID = 5050;

        /// <summary>
        /// The string identifier for the service notification
        /// </summary>
        private const string SERVICE_NOTIFICATION_CHANNEL_ID = "5051";

        private readonly TimeSpan _interval = TimeSpan.FromSeconds(5);
        private readonly IBackgroundWorker _backgroundWorker;

        private bool _workerActive;
        private CancellationTokenSource _cancellationTokenSource;

        public BackgroundService()
        {
            _backgroundWorker = App.Current.Services.GetRequiredService<IBackgroundWorker>();
        }

        public override IBinder OnBind(Intent intent)
        {
            //  This service is not bindable by an external app so
            //  return null
            return null;
        }

        /// <summary>
        /// On Start Service Command
        /// </summary>
        /// <returns>Start Command Result</returns>
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
            var intent = new Intent(Uno.UI.ContextHelper.Current, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.NoUserAction);
            intent.PutExtra("Title", "Message");

            var pendingIntentFlags = (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Mutable
                : PendingIntentFlags.UpdateCurrent;

            var pendingIntent = PendingIntent.GetActivity(Uno.UI.ContextHelper.Current, 0, intent, pendingIntentFlags);

            var notificationBuilder = new NotificationCompat.Builder(Uno.UI.ContextHelper.Current, SERVICE_NOTIFICATION_CHANNEL_ID)
                .SetContentTitle("Name of BG Work")
                .SetContentText("User Friendly Description")
                .SetOngoing(true)
                .SetContentIntent(pendingIntent);
            //.SetSmallIcon(Resource.Mipmap.icon)
            //.SetColor(ResourcesCompat.GetColor(Uno.UI.ContextHelper.Current.Resources, Resource.Color.colorAccent, null))

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
}