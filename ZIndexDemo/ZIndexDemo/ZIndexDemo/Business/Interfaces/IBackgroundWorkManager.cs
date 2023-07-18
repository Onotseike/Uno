using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZIndexDemo.Business.Interfaces
{
    internal interface IBackgroundWorkManager
    {
        /// <summary>
        /// Get Background Work
        /// </summary>
        Func<Task> GetBackgroundWork();

        /// <summary>
        /// Set Background Work
        /// </summary>
        void SetBackgroundWork(Func<Task> bgWork);
    }

    /// <summary>
    /// Native Background Worker Interface
    /// </summary>
    internal interface IBackgroundWorker
    {
        /// <summary>
        /// Worker Stopped Event
        /// </summary>
        public event EventHandler WorkerStopped;

        /// <summary>
        /// Actual Background Work
        /// </summary>
        public Func<Task> BackgroundWork { get; }

        /// <summary>
        /// Start Native Background or Foreground Service
        /// </summary>
        void StartWorker(Func<Task> backgroundWork);

        /// <summary>
        /// Force stop worked foreground or background service
        /// </summary>
        void StopWorker();
    }


    /// <summary>
    /// Native Background Worker
    /// </summary>
    public class BackgroundWorker : IBackgroundWorker
    {
        // https://github.com/bbenetskyy/ios-bg-worker/blob/master/sample/sample/sample.iOS/BackgroundWorker.cs
        /// <summary>
        /// Worker Stopped Event
        /// </summary>
        public event EventHandler WorkerStopped;

        /// <summary>
        /// Actual Background Work
        /// </summary>
        public Func<Task> BackgroundWork { get; private set; }

        #region Public Methods

        /// <summary>
        /// Start Native Background or Foreground Service
        /// </summary>
        public void StartWorker(Func<Task> backgroundWork)
        {
            //BackgroundWork = backgroundWork;
            //var intent = new Intent(Application.Context, typeof(BackgroundService));

            //if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            //{
            //    Application.Context.StartForegroundService(intent);
            //}
            //else
            //{
            //    Application.Context.StartService(intent);
            //}
        }

        /// <summary>
        /// Force stop worked foreground or background service
        /// </summary>
        public void StopWorker()
        {
            WorkerStopped?.Invoke(this, EventArgs.Empty);
        }

        #endregion Public Methods
    }
}
