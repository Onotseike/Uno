using Microsoft.UI.Xaml;

using Uno.Resizetizer;
using UnoBackgroundWorker.Business;
using Microsoft.Extensions.DependencyInjection;
#if __IOS__
using UIKit;
using CoreFoundation;
using BackgroundTasks;
using Foundation;
#endif
namespace UnoBackgroundWorker
{
    public sealed partial class AppHead : App
    {
        /// <summary>
        /// Initializes the singleton application object. This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public AppHead()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            MainWindow.SetWindowIcon();
        }


#if __IOS__
        private const string REFRESH_IDENTIFIER = "com.companyname.UnoBackgroundWorker";

        private IBackgroundWorker _backgroundWorker;
        /// <summary>
        /// Background Worker to Run Background Updates during Perform Fetch
        /// </summary>
        private IBackgroundWorker BackgroundWorker =>
            _backgroundWorker ??= (Application.Current as App).Host.Services.GetRequiredService<IBackgroundWorker>();

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {


            BGTaskScheduler.Shared.Register(REFRESH_IDENTIFIER, DispatchQueue.CurrentQueue, task =>
            {
                var queue = NSOperationQueue.CurrentQueue;
                task.ExpirationHandler = () =>
                {
                    queue.CancelAllOperations();
                };
                queue.AddOperation(() => _ = StartSynchronisationWork());
            });
            //BGAppRefreshTask
            UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(UIApplication.BackgroundFetchIntervalMinimum);

            return base.FinishedLaunching(app, options);
        }

        /// <summary>
        /// Perform App Background Fetch
        /// </summary>
        public override async void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
        {
            var dataAvailableToSync = await StartSynchronisationWork();
            completionHandler?.Invoke(dataAvailableToSync
                ? UIBackgroundFetchResult.NewData
                : UIBackgroundFetchResult.NoData);
        }

        /// <summary>
        /// App Just Enter Background
        /// </summary>
        public override void DidEnterBackground(UIApplication uiApplication)
        {
            //base.DidEnterBackground(uiApplication);

            var request = new BGAppRefreshTaskRequest(REFRESH_IDENTIFIER);
            request.EarliestBeginDate = NSDate.FromTimeIntervalSinceNow(5);

            try
            {
                BGTaskScheduler.Shared.Submit(request, out var error);
                //if you try this on simulator you will get net error:
                //Error Domain=BGTaskSchedulerErrorDomain Code=1 "(null)"
                //but in real device with proper certificates you should get no error!!!
                if (error is not null)
                {
                    //todo Logger.LogError(new Exception(error.Description));
                }
            }
            catch (Exception ex)
            {
                //todo Logger.LogError(ex);
            }
        }


        /// <summary>
        /// Check if we need to start BG Work and start if any data is available
        /// </summary>
        private async Task<bool> StartSynchronisationWork()
        {
            var dataAvailableToSync = await BackgroundWorker.IsDataAvailableToSync();
            if (dataAvailableToSync)
            {
                _ = BackgroundWorker.BackgroundWork?.Invoke();
            }

            return dataAvailableToSync;
        }
#endif
    }
}