using Demo.Database.Entities;
using Demo.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Demo.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Invoices : Page
    {
        public Invoices()
        {
            this.InitializeComponent();
        }

        
        
        public static string PriceOfItems(List<ItemBlob> items, string currency) => $"{currency} {items.Sum(item => item.Price)}";

        public static string EnumToString(Enum enumObject) => enumObject.ToString();

        public static string DateFormat(DateTime dateTime, bool isIssueDate) => isIssueDate ? $"Issued : {dateTime.ToString("d")}" : $"Due : {dateTime.ToString("d")}";

        public static string IndexOf(Invoice invoice)
        {
            return "";
        }

        private void GridView_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void ViewClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var invoice = button.DataContext as Invoice;
        }

        private void EditClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var invoice = button.DataContext as Invoice;
            
            Frame.Navigate(typeof(CUInvoice), invoice);
        }

        private void DeleteClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var invoice = button.DataContext as Invoice;
            var vm = DataContext as InvoicesVM;
            vm.DeleteEntity(invoice);
        }
    }
}
