namespace CameraPreview.Presentation
{
    public partial class MainViewModel : ObservableObject
    {
        private INavigator _navigator;

        [ObservableProperty]
        private string? name;

        [ObservableProperty]
        private List<CarouselObject> carouselObjects;

        public MainViewModel(
            IStringLocalizer localizer,
            IOptions<AppConfig> appInfo,
            INavigator navigator)
        {
            _navigator = navigator;
            Title = "Main";
            Title += $" - {localizer["ApplicationName"]}";
            Title += $" - {appInfo?.Value?.Environment}";
            GoToSecond = new AsyncRelayCommand(GoToSecondView);

            CarouselObjects = new List<CarouselObject>
            {
                new CarouselObject
                {
                    ImageSource = "Assets/Tutorials/clients.png",
                    TextString = "Clients"
                },
                new CarouselObject
                {
                    ImageSource = "Assets/Tutorials/data.png",
                    TextString = "Adata"
                },
                new CarouselObject
                {
                    ImageSource = "Assets/Tutorials/freelance.png",
                    TextString = "free lance"
                },
            };
        }
        public string? Title { get; }

        public ICommand GoToSecond { get; }


        private async Task GoToSecondView()
        {
            await _navigator.NavigateViewModelAsync<SecondViewModel>(this, data: new Entity(Name!));
        }


        public class CarouselObject
        {
            public string ImageSource { get; set; }
            public string  TextString { get; set; }
        }

    }
}