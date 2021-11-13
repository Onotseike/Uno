using Demo.Database.Entities;
using Demo.ViewModels;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Demo.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CUInvoice : Page
    {
        public InvoiceVM ViewModel { get; set; }
        public CUInvoice()
        {
            
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (ViewModel == null)
            {
                var invoice = e.Parameter as Invoice;
                ViewModel = new InvoiceVM(invoice, false);
            }
        }

        public static DateTimeOffset GetDateFromNow(double days = 0) => new DateTimeOffset(DateTime.UtcNow.AddDays(days));

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

        }

        private void Additem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

        }
        private void DeleteItem_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

        }

    }
}
