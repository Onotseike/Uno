using Microsoft.UI.Xaml.Controls;

using Uno.Toolkit.UI;

using UnoMSAL.Views;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UnoMSAL
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void TabBar_SelectionChanged(TabBar sender, TabBarSelectionChangedEventArgs args)
        {
            if (args.NewItem == sender.Items[0])
            {
                ContentFrame.Navigate(typeof(PageOne));
            }
            else if (args.NewItem == sender.Items[1])
            {
                ContentFrame.Navigate(typeof(PageOne));
            }
            else if (args.NewItem == sender.Items[2])
            {
                ContentFrame.Navigate(typeof(PageOne));
            }
            else
            {
                ContentFrame.Navigate(typeof(PageOne));
            }

        }
    }
}
