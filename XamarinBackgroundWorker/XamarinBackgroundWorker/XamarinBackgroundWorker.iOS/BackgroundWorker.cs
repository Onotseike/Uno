using CoreLocation;

using Foundation;

using System;
using System.Linq;
using System.Threading.Tasks;

using UIKit;

using Xamarin.Essentials;

namespace XamarinBackgroundWorker
{
    public class BackgroundWorker : IBackgroundWorker
    {
        /// <summary>
        /// Timer Cooldown/Delay
        /// </summary>
        public const double TIMER_COOLDOWN = 5; //5 seconds
                                                //best time according to iOS is from 5 to 15 minutes,
                                                //but with 30 minutes it may also work

        public event EventHandler WorkerStopped;

        private NSTimer _timer;

        public Func<Task> BackgroundWork { get; private set; }


        public void StartWorker(Func<Task> backgroundWork)
        {
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
        }

        public void StopWorker()
        {
            if (_timer is not null)
            {
                OnWorkerStopped();
                _timer.Invalidate();
                _timer.Dispose();
                _timer = null;
            }
        }

        public Task<bool> IsDataAvailableToSync()
        {
            //todo check if we need to run our BG Work
            //we need to run if any data to sync or download 
            //is available
            return Task.FromResult(true);//true - data available, false - run BG Work is not required
        }

        protected virtual void OnWorkerStopped()
        {
            WorkerStopped?.Invoke(this, EventArgs.Empty);
        }
    }

    public class LocationBackgroundWorker : ILocationBackgroundWorker
    {
        /// <summary>
        /// Location Manager Desired Accuracy in meters
        /// </summary>
        public const int LOC_MGR_DESIRED_ACCURACY = 100;

        /// <summary>
        /// Last Known Location from iOS
        /// </summary>
        private CLLocation _lastKnownLocation;

        /// <summary>
        /// Last Updated Time
        /// </summary>
        private DateTime _lastUpdatedTime;

        /// <summary>
        /// Local Location Manager
        /// </summary>
        private CLLocationManager _locMgr;

        /// <summary>
        /// Raised when only Work with Result Completed
        /// </summary>
        public event EventHandler<Location> LocationUpdated;

        /// <summary>
        /// Event Raised when BG Worker has been Stopped
        /// </summary>
        public event EventHandler WorkerStopped;

        public TimeSpan Interval { get; private set; }

        public void StartLocationUpdates(TimeSpan interval)
        {
            Interval = interval;
            _locMgr = new CLLocationManager();
            _locMgr.PausesLocationUpdatesAutomatically = false;

            // iOS 8 has additional permissions requirements
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                _locMgr.RequestAlwaysAuthorization(); // works in background
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                _locMgr.AllowsBackgroundLocationUpdates = true;
            }

            if (CLLocationManager.LocationServicesEnabled)
            {
                _ = Task.Run(() =>
                {
                    //set the desired accuracy, in meters
                    _locMgr.DesiredAccuracy = LOC_MGR_DESIRED_ACCURACY;
                    _locMgr.LocationsUpdated += (_, args) =>
                    {
                        _lastKnownLocation = args.Locations.Last();
                        if (DateTime.UtcNow - _lastUpdatedTime > Interval)
                        {
                            _lastUpdatedTime = DateTime.UtcNow;
                            OnLocationUpdated(new(
                                _lastKnownLocation.Coordinate.Latitude,
                                _lastKnownLocation.Coordinate.Longitude));
                        }
                    };
                    _locMgr.StartUpdatingLocation();
                });
            }
        }

        public void StopWorker()
        {
            _locMgr.StopUpdatingLocation();
        }

        protected virtual void OnLocationUpdated(Location e)
        {
            LocationUpdated?.Invoke(this, e);
        }

        protected virtual void OnWorkerStopped()
        {
            WorkerStopped?.Invoke(this, EventArgs.Empty);
        }
    }

    public class RegionMonitor : IRegionMonitor
    {
        public event EventHandler<string> MonitorNotifications;

        /// <summary>
        /// Local Location Manager
        /// </summary>
        private CLLocationManager _locMgr;

        private CLCircularRegion _region;


        public void StartRegionUpdates()
        {
            _locMgr = new CLLocationManager();
            _locMgr.PausesLocationUpdatesAutomatically = false;

            // iOS 8 has additional permissions requirements
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                _locMgr.RequestAlwaysAuthorization(); // works in background
            }

            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                _locMgr.AllowsBackgroundLocationUpdates = true;
            }

            if (CLLocationManager.LocationServicesEnabled && CLLocationManager.IsMonitoringAvailable(typeof(CLCircularRegion)))
            {
                _ = Task.Run(() =>
                {
                    //set the desired accuracy, in meters
                    _locMgr.DesiredAccuracy = LocationBackgroundWorker.LOC_MGR_DESIRED_ACCURACY;

                    _locMgr.RegionEntered += OnRegionEntered;
                    _locMgr.RegionLeft += OnRegionLeft;
                    _locMgr.LocationsUpdated += OnLocationsUpdated;

                    _locMgr.StartUpdatingLocation();
                }
                );
            }
        }

        public void MonitorRegion(Location location)
        {
            //this is not the best interface, because
            //ios is quite different from android
            //and for ios we don't need that at all
        }

        protected virtual void OnMonitorNotifications(string e)
        {
            MonitorNotifications?.Invoke(this, e);
        }

        private void OnLocationsUpdated(object sender, CLLocationsUpdatedEventArgs e)
        {
            var radius = 100;//radius of region circle to monitor
            var location = e.Locations.First();
            //create region outside our location to trigger enter region event
            location = new CLLocation(location.Coordinate.Latitude + 0.01, location.Coordinate.Longitude - 0.01);

            OnMonitorNotifications($"Create Region {location.Coordinate.Latitude:N6} {location.Coordinate.Longitude:N6} {DateTime.Now.ToString("hh:mm:ss")}");

            //todo this region should be from your API
            //just to test that it works I create from first location I've get
            _region = new CLCircularRegion(location.Coordinate, radius, "YourIdentifier");

            _locMgr.StartMonitoring(_region);
            _locMgr.StopUpdatingLocation();

            //todo to stop monitoring call _locMgr.StopMonitoring(_region);
            //and don't forget to ensure _region is not null before stop 
        }

        private void OnRegionEntered(object sender, CLRegionEventArgs e)
        {
            OnMonitorNotifications($"On Region Entered {e.Region.Identifier} {e.Region.Center.Latitude:N6} {e.Region.Center.Longitude:N6} {DateTime.Now.ToString("hh:mm:ss")}");
        }

        private void OnRegionLeft(object sender, CLRegionEventArgs e)
        {
            OnMonitorNotifications($"On Region Left {e.Region.Identifier} {e.Region.Center.Latitude:N6} {e.Region.Center.Longitude:N6} {DateTime.Now.ToString("hh:mm:ss")}");
        }
    }
}