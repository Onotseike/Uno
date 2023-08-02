using Windows.System;

using System.Collections.ObjectModel;

using UnoBackgroundWorker.Business;

namespace UnoBackgroundWorker.Presentation
{
    public partial class MainViewModel : ObservableObject
    {
        private INavigator _navigator;
        private readonly IBackgroundWorker _backgroundWorker;
        
        [ObservableProperty]
        private ObservableCollection<string> locationUpdates;

        protected DispatcherQueue Dispatcher => DispatcherQueue.GetForCurrentThread();

        [ObservableProperty]
        private string? name;

        public MainViewModel(
            IStringLocalizer localizer,
            IOptions<AppConfig> appInfo,
            INavigator navigator,
            IBackgroundWorker backgroundWorker)
        {
            _navigator = navigator;
            _backgroundWorker = backgroundWorker;
            Title = "Main";
            Title += $" - {localizer["ApplicationName"]}";
            Title += $" - {appInfo?.Value?.Environment}";
            GoToSecond = new AsyncRelayCommand(GoToSecondView);

            LocationUpdates = new ObservableCollection<string>();
            _backgroundWorker.StartWorker(BackgroundWork);
        }
        public string? Title { get; }

        public ICommand GoToSecond { get; }


        private async Task GoToSecondView()
        {
            await _navigator.NavigateViewModelAsync<SecondViewModel>(this, data: new Entity(Name!));
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

    }
}