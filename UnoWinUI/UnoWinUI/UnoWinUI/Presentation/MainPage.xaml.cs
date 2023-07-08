using Microsoft.UI.Xaml.Input;

namespace UnoWinUI.Presentation
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);           
        }

        private void RectItem_Loaded(object sender, RoutedEventArgs e)
        {

        }

        public void itemTapped(object sender, TappedRoutedEventArgs e)
        {
            var listView = sender as ListView;
            var viewModel = listView?.DataContext as MainViewModel;

            if(viewModel != null)
            {

                if (viewModel.IsLoading)
                {
                    listView.ItemTemplate = (DataTemplate)Resources["LoadingTemplate"];
                }
                else
                {
                    listView.ItemTemplate = (DataTemplate)Resources["LoadedTemplate"];
                }

                viewModel.IsLoading = !viewModel.IsLoading;
            }            
        }
    }
}