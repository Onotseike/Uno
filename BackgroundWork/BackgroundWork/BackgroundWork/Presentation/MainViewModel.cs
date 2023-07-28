using BackgroundWork.Business.Interfaces;

using Microsoft.UI.Dispatching;

using System.Collections.ObjectModel;

using Windows.UI.Core;

namespace BackgroundWork.Presentation
{
    public partial class MainViewModel : ObservableObject
    {
        private INavigator _navigator;
        
        protected DispatcherQueue Dispatcher => DispatcherQueue.GetForCurrentThread();

        [ObservableProperty]
        private string? name;

        public ObservableCollection<WorkItem> BackgroundUpdates { get; set; }
        private readonly IBackgroundWorker backgroundWorker;

        public MainViewModel(
            IStringLocalizer localizer,
            IOptions<AppConfig> appInfo,
            INavigator navigator,
            IBackgroundWorker _backgroundWorker)
        {
            _navigator = navigator;
            Title = "Main";
            Title += $" - {localizer["ApplicationName"]}";
            Title += $" - {appInfo?.Value?.Environment}";

            backgroundWorker = _backgroundWorker;
            BackgroundUpdates = new ObservableCollection<WorkItem>();
            GoToSecond = new AsyncRelayCommand(GoToSecondView);

            backgroundWorker.StartWorker(BackgroundWork);
        }

        private async Task BackgroundWork()
        {
            int count = 0;

            //Window.Current.DispatcherQueue.TryEnqueue(() =>
            //{
            //    BackgroundUpdates.Add(new WorkItem
            //    {
            //        Name = $"Background Work {count++}",
            //        Status = "Running",
            //        Started = DateTime.Now
            //    });
            //});
           await DispatchAsync(() =>
            {
                BackgroundUpdates.Add(new WorkItem
                {
                    Name = $"Background Work {count++}",
                    Status = "Running",
                    Started = DateTime.Now.ToShortTimeString()
                });
            });
        }

        public string? Title { get; }

        public ICommand GoToSecond { get; }


        private async Task GoToSecondView()
        {
            await _navigator.NavigateViewModelAsync<SecondViewModel>(this, data: new Entity(Name!));
        }

        // Insert DispatchAsync below here
        protected async Task DispatchAsync(DispatcherQueueHandler callback)
        {
            var hasThreadAccess =
#if __WASM__
        true;
#else
            Dispatcher.HasThreadAccess;
#endif

            if (hasThreadAccess)
            {
                callback.Invoke();
            }
            else
            {
                var completion = new TaskCompletionSource();
                Dispatcher.TryEnqueue(() =>
                {
                    callback();
                    completion.SetResult();
                });
                await completion.Task;
            }
        }

    }

    public class WorkItem
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public string Started { get; set; }
    }
}