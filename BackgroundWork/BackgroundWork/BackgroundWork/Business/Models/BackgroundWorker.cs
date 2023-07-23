using System;
using BackgroundWork.Business.Interfaces;
using Microsoft.UI.Xaml.Documents;


#if IOS
using Foundation;
using UIKit;

#elif ANDROID
using Android.Content;
using Android.OS;
#endif

namespace BackgroundWork.Business.Models
{
    public class BackgroundWorker : IBackgroundWorker
    {
        public event EventHandler WorkerStopped;
        public Func<Task> BackgroundWork { get; private set; }

        #region iOS Specific Properties

#if IOS
        /// <summary>
        /// Timer Cooldown/Delay
        /// </summary>
        public const double TIMER_COOLDOWN = 5; //5 seconds
                                                //best time according to iOS is from 5 to 15 minutes,
                                                //but with 30 minutes it may also work

        private NSTimer _timer;

#endif
        #endregion

        public Task<bool> IsDataAvailableToSync()
        {
            // todo check if we need to run our BG Work
            //we need to run if any data to sync or download 
            //is available
            return Task.FromResult(true);//true - data available, false - run BG Work is not required

        }

        public void StartWorker(Func<Task> backgroundWork)
        {
#if ANDROID
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

#elif IOS
 BackgroundWork = backgroundWork;

        if (_timer is not null)
        {
            StopWorker();
        }

        _timer = NSTimer.CreateRepeatingScheduledTimer(TIMER_COOLDOWN, async _ =>
        {
            nint taskId = 0;
            taskId = UIApplication.SharedApplication.BeginBackgroundTask(() =>
            {
                // Time execution limit reached. Stopping the background task
                UIApplication.SharedApplication.EndBackgroundTask(taskId);
            });
 
            await BackgroundWork();
 
            UIApplication.SharedApplication.EndBackgroundTask(taskId);
        });
#endif

        }

        public void StopWorker()
        {
#if ANDROID
WorkerStopped?.Invoke(this, EventArgs.Empty);
#elif IOS
            if (_timer is not null)
            {
                OnWorkerStopped();
                _timer.Invalidate();
                _timer.Dispose();
                _timer = null;
            }
#endif
        }

#if IOS
        protected virtual void OnWorkerStopped()
        {
            WorkerStopped?.Invoke(this, EventArgs.Empty);
        }
#endif
    }
}

