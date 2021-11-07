using Demo.Database.Enums;
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
    public sealed partial class Settings : Page
    {
        public MainVM ViewModel { get; set; }
        
        public Settings()
        {
            this.InitializeComponent();            
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (ViewModel == null)
            {
                ViewModel = DataContext as MainVM;
            }            
        }

        private async void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            ViewModel = DataContext as MainVM;
            var toggleSwitch = sender as ToggleSwitch;

            if (toggleSwitch?.IsOn == true)
            {
                await ViewModel.OneDriveSetupAsync();
            }
            else if(toggleSwitch?.IsOn == false)
            {
                await ViewModel.LogOutAsync();
            }
        }

        private async void BackUpButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                ViewModel = DataContext as MainVM;
            }

            await ViewModel.BackUp();
        }


        private async void RestoreButtonClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                ViewModel = DataContext as MainVM;
            }

            await ViewModel.Restore();
        }

        private async void SaveAccountDetailsClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                ViewModel = DataContext as MainVM;
            }

            await ViewModel.SaveAccount(ViewModel.UserAccount);
        }

        private async void SaveAddressDetailsClicked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                ViewModel = DataContext as MainVM;
            }

            await ViewModel.SaveAddress(ViewModel.UserAddress);
        }

        private async void AddressChecked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                ViewModel = DataContext as MainVM;
            }
#if NETFX_CORE
            if(ViewModel == null) { return; }
#endif
            ViewModel.UserAddress.Type = AddressType.Billing;
        }

        private async void AddressUnChecked(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                ViewModel = DataContext as MainVM;
            }
#if NETFX_CORE
            if (ViewModel == null) { return; }
#endif
            ViewModel.UserAddress.Type = AddressType.Shipping;
        }

        private async void AddressIndeterminate(object sender, RoutedEventArgs e)
        {
            if (ViewModel == null)
            {
                ViewModel = DataContext as MainVM;
            }
#if NETFX_CORE
            if (ViewModel == null) { return; }
#endif
            ViewModel.UserAddress.Type = AddressType.Both;
        }
    }
}
