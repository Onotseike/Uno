
using System;
using System.Collections.Generic;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Essentials;

namespace XamarinBackgroundWorker
{
    public interface IBackgroundWorker
    {
        /// <summary>
        /// Event Raised when BG Worker has been Stopped
        /// </summary>
        event EventHandler WorkerStopped;

        /// <summary>
        /// Represent BG Work
        /// </summary>
        Func<Task> BackgroundWork { get; }

        /// <summary>
        /// Start Specific BG Work
        /// </summary>
        void StartWorker(Func<Task> backgroundWork);

        /// <summary>
        /// Force Stop BG Work Execution
        /// </summary>
        void StopWorker();

        /// <summary>
        /// Identify if BG Work has any data to upload or download
        /// </summary>
        Task<bool> IsDataAvailableToSync();
    }

    public interface ILocationBackgroundWorker
    {
        /// <summary>
        /// Raised when only Work with Result Completed
        /// </summary>
        event EventHandler<Location> LocationUpdated;

        /// <summary>
        /// Event Raised when BG Worker has been Stopped
        /// </summary>
        public event EventHandler WorkerStopped;

        /// <summary>
        /// Update Interval for native platform implementation
        /// </summary>
        TimeSpan Interval { get; }

        void StartLocationUpdates(TimeSpan interval);

        /// <summary>
        /// Force Stop Location Execution
        /// </summary>
        void StopWorker();
    }

    public interface IRegionMonitor
    {
        /// <summary>
        /// Raised when we got any region changes
        /// </summary>
        event EventHandler<string> MonitorNotifications;
        //it's better for you to replace string to something
        //more specific like class with all required fields,
        //I will simplify that to just string because it's ok
        //according to your requirements ;)

        void StartRegionUpdates();

        /// <summary>
        /// Monitor Region Enter or Exit Actions
        /// </summary>
        void MonitorRegion(Location location);
    }


    public interface IPermissionHandler
    {
        Task<PermissionStatus> RequestPermission<TPermission>()
            where TPermission : Permissions.BasePlatformPermission, new();
    }

    /// <summary>
    /// Permission Handler
    /// </summary>
    public class PermissionHandler : IPermissionHandler
    {
        /// <summary>
        /// Request Permission
        /// </summary>
        /// <typeparam name="TPermission">Base Platform Permission</typeparam>
        /// <returns>Permission Status</returns>
        public async Task<PermissionStatus> RequestPermission<TPermission>()
            where TPermission : Permissions.BasePlatformPermission, new()
        {
            var status = await Permissions.CheckStatusAsync<TPermission>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<TPermission>();
            }

            return status;
        }
    }
}
