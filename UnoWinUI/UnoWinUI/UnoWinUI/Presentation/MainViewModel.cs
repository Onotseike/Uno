using System.Collections.ObjectModel;

namespace UnoWinUI.Presentation
{
    public class Item
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
    }

    public partial class MainViewModel : ObservableObject
    {
        private INavigator _navigator;

        [ObservableProperty]
        private string? name;

        [ObservableProperty]
        private bool isLoading;

        public ObservableCollection<Item> MainItems { get; set; }

        public MainViewModel(
            IOptions<AppConfig> appInfo,
            INavigator navigator)
        {
            _navigator = navigator;
            Title = "Main";
            Title += $" - {appInfo?.Value?.Environment}";
            GoToSecond = new AsyncRelayCommand(GoToSecondView);
            MainItems = new ObservableCollection<Item>()
        {
            new Item { Id = Guid.NewGuid().ToString(), Text = "First item", Description="This is an item description." },
            new Item { Id = Guid.NewGuid().ToString(), Text = "Second item", Description="This is an item description." },
            new Item { Id = Guid.NewGuid().ToString(), Text = "Third item", Description = "This is an item description." },
            new Item { Id = Guid.NewGuid().ToString(), Text = "Fourth item", Description = "This is an item description." },
            new Item { Id = Guid.NewGuid().ToString(), Text = "Fifth item", Description = "This is an item description." },
            new Item { Id = Guid.NewGuid().ToString(), Text = "Sixth item", Description = "This is an item description." }
        };
        }
        public string? Title { get; }

        public ICommand GoToSecond { get; }


        private async Task GoToSecondView()
        {
            await _navigator.NavigateViewModelAsync<SecondViewModel>(this, data: new Entity(Name!));
        }

    }
}