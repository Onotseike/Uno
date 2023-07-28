

using CommunityToolkit.Mvvm.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using System;
using System.Threading.Tasks;

#if __IOS__
using UIKit;
using CoreFoundation;
using BackgroundTasks;
using Foundation;
#endif

#if __ANDROID__
using XamarinBackgroundWorker.Droid;
#endif
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Application = Windows.UI.Xaml.Application;

namespace XamarinBackgroundWorker
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {

        /// <summary>
        /// Gets the current <see cref="App"/> instance in use
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }


        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddTransient<MainPageViewModel>();

            services.AddSingleton<IPermissionHandler, PermissionHandler>();
#if __IOS__
            services.AddSingleton<ILocationBackgroundWorker, LocationBackgroundWorker>();
            services.AddSingleton<IBackgroundWorker, BackgroundWorker>();
            services.AddSingleton<IRegionMonitor, RegionMonitor>();
#elif __ANDROID__
            //services.AddSingleton<ILocationBackgroundWorker, LocationBackgroundWorker>();
            services.AddSingleton<IBackgroundWorker, BackgroundWorker>();
            //services.AddSingleton<IRegionMonitor, RegionMonitor>();
#endif
            return services.BuildServiceProvider();
        }


        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            Services = ConfigureServices();
            InitializeLogging();

            this.InitializeComponent();

#if HAS_UNO || NETFX_CORE
			this.Suspending += OnSuspending;
#endif
        }

        /// <summary>
        /// Gets the main window of the app.
        /// </summary>
        internal static Window MainWindow { get; private set; }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// this.DebugSettings.EnableFrameRateCounter = true;
			}
#endif

#if NET5_0 && WINDOWS && !HAS_UNO
			MainWindow = new Window();
			MainWindow.Activate();
#else
            MainWindow = Windows.UI.Xaml.Window.Current;
#endif

            var rootFrame = MainWindow.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                MainWindow.Content = rootFrame;
            }

#if !(NET5_0 && WINDOWS)
            if (args.PrelaunchActivated == false)
#endif
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), args.Arguments);
                }
                // Ensure the current window is active
                MainWindow.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new InvalidOperationException($"Failed to load {e.SourcePageType.FullName}: {e.Exception}");
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        /// <summary>
        /// Configures global Uno Platform logging
        /// </summary>
        private static void InitializeLogging()
        {
            var factory = LoggerFactory.Create(builder =>
            {
#if __WASM__
				builder.AddProvider(new global::Uno.Extensions.Logging.WebAssembly.WebAssemblyConsoleLoggerProvider());
#elif __IOS__ && !__MACCATALYST__
				builder.AddProvider(new global::Uno.Extensions.Logging.OSLogLoggerProvider());
#elif NETFX_CORE
				builder.AddDebug();
#else
                builder.AddConsole();
#endif

                // Exclude logs below this level
                builder.SetMinimumLevel(LogLevel.Information);

                // Default filters for Uno Platform namespaces
                builder.AddFilter("Uno", LogLevel.Warning);
                builder.AddFilter("Windows", LogLevel.Warning);
                builder.AddFilter("Microsoft", LogLevel.Warning);

                // Generic Xaml events
                // builder.AddFilter("Windows.UI.Xaml", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.VisualStateGroup", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.StateTriggerBase", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.UIElement", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.FrameworkElement", LogLevel.Trace );

                // Layouter specific messages
                // builder.AddFilter("Windows.UI.Xaml.Controls", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Controls.Layouter", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Controls.Panel", LogLevel.Debug );

                // builder.AddFilter("Windows.Storage", LogLevel.Debug );

                // Binding related messages
                // builder.AddFilter("Windows.UI.Xaml.Data", LogLevel.Debug );
                // builder.AddFilter("Windows.UI.Xaml.Data", LogLevel.Debug );

                // Binder memory references tracking
                // builder.AddFilter("Uno.UI.DataBinding.BinderReferenceHolder", LogLevel.Debug );

                // RemoteControl and HotReload related
                // builder.AddFilter("Uno.UI.RemoteControl", LogLevel.Information);

                // Debug JS interop
                // builder.AddFilter("Uno.Foundation.WebAssemblyRuntime", LogLevel.Debug );
            });

            global::Uno.Extensions.LogExtensionPoint.AmbientLoggerFactory = factory;

#if HAS_UNO
			global::Uno.UI.Adapter.Microsoft.Extensions.Logging.LoggingAdapter.Initialize();
#endif
        }

#if __IOS__
        private const string REFRESH_IDENTIFIER = "com.companyname.XamarinBackgroundWorker";

        private IBackgroundWorker _backgroundWorker;
        /// <summary>
        /// Background Worker to Run Background Updates during Perform Fetch
        /// </summary>
        private IBackgroundWorker BackgroundWorker =>
            _backgroundWorker ??= Ioc.Default.GetRequiredService<IBackgroundWorker>();

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
