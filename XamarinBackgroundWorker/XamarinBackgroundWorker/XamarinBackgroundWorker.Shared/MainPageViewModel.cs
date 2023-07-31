using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Windows.System;

using Xamarin.Essentials;


namespace XamarinBackgroundWorker
{
    public partial class MainPageViewModel : ObservableObject
    {
        //private readonly ILocationBackgroundWorker _locationBackgroundWorker;
        //private readonly IRegionMonitor _regionMonitor;
        private readonly IBackgroundWorker _backgroundWorker;
        private readonly IPermissionHandler _permissionHandler;

        [ObservableProperty]
        private ObservableCollection<string> locationUpdates;

        protected DispatcherQueue Dispatcher => DispatcherQueue.GetForCurrentThread();

        public MainPageViewModel(
            IBackgroundWorker backgroundWorker,
            IPermissionHandler permissionHandler)
        {
            //_locationBackgroundWorker = locationBackgroundWorker;
            //_regionMonitor = regionMonitor;
            _backgroundWorker = backgroundWorker;
            _permissionHandler = permissionHandler;

            //_locationBackgroundWorker.LocationUpdated += LocationBackgroundWorkerOnLocationUpdated;
            //_regionMonitor.MonitorNotifications += RegionMonitorOnMonitorNotifications;

            //StartUpdatesCommand = new Command(ExecuteStartUpdates);
            LocationUpdates = new ObservableCollection<string>();

            _backgroundWorker.StartWorker(BackgroundWork);
        }

        private void RegionMonitorOnMonitorNotifications(object sender, string e)
        {
            if (Dispatcher.HasThreadAccess)
            {
                LocationUpdates.Add(e);
            }
            else
            {
                Dispatcher.TryEnqueue(() =>
                {
                    LocationUpdates.Add(e);
                });
            }
        }

        private async Task BackgroundWork()
        {
            if (Dispatcher.HasThreadAccess)
            {
                LocationUpdates.Add($"Background Work Update {DateTime.Now.ToString("hh:mm:ss")}");
            }
            else
            {
                Dispatcher.TryEnqueue(() =>
                {
                    LocationUpdates.Add($"Background Work Update {DateTime.Now.ToString("hh:mm:ss")}");
                });
            }
            await Task.CompletedTask;
        }

        private void LocationBackgroundWorkerOnLocationUpdated(object sender, Location e)
        {
            //Also we may send from Event Time when it was raised, I don't make it in my example 
            //and log it here, but you can change that and send in even immediately ;)
            if (Dispatcher.HasThreadAccess)
            {
                LocationUpdates.Add($"Location Updated {e.Latitude:N6} {e.Longitude:N6} {DateTime.Now.ToString("hh:mm:ss")}");
            }
            else
            {
                Dispatcher.TryEnqueue(() =>
                {
                    LocationUpdates.Add($"Location Updated {e.Latitude:N6} {e.Longitude:N6} {DateTime.Now.ToString("hh:mm:ss")}");
                });
            }
        }

        [RelayCommand]
        private void StartUpdates()
        {
            //StartUpdatesCommand = null;
            _permissionHandler.RequestPermission<Permissions.LocationAlways>();
            //_regionMonitor.StartRegionUpdates();
            //_locationBackgroundWorker.StartLocationUpdates(TimeSpan.FromSeconds(5));
        }
    }
}
