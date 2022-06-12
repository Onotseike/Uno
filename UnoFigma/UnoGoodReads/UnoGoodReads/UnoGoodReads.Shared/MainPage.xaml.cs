using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;

using UnoGoodReads.Views;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UnoGoodReads
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

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var item = args.InvokedItem as NavigationViewItem;
            
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var item = sender.SelectedItem as NavigationViewItem;
            Type pageType = typeof(HomePage);
            if (item.Tag.Equals("Home"))
            {
                pageType = typeof(HomePage);
            }
            else if (item.Tag.Equals("Author"))
            {
                pageType = typeof(AuthorPage);
            }
            else if (item.Tag.Equals("Book"))
            {
                pageType = typeof(BookPage);
            }
            contentFrame.Navigate(pageType, null);
        }
    }
}

