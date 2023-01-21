using Microsoft.Identity.Client;
using Microsoft.UI.Xaml.Controls;

using System;
using System.Threading.Tasks;

using UnoMSAL.Models;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UnoMSAL.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PageOne : Page
    {
        public PageOne()
        {
            this.InitializeComponent();
            
            // Initializes the Public Client app and loads any already signed in user from the token cache
            IAccount cachedUserAccount = Task.Run(async () => await MSALClientSingleton.Instance.MSALClientHelper.FetchSignedInUserFromCache()).Result;
            if (cachedUserAccount == null)
            {
                SignInButton.IsEnabled = true;
            }
            //_ = Dispatcher.CurrentPriority.D(async () =>
            //{
            //    if (cachedUserAccount == null)
            //    {
            //        SignInButton.IsEnabled = true;
            //    }
            //    else
            //    {
            //        await Shell.Current.GoToAsync("userview");
            //    }
            //});
        }

        private async void OnSignInClicked(object sender, EventArgs e)
        {
            // Sign-in the user
            MSALClientSingleton.Instance.UseEmbedded = (bool)useEmbedded.IsChecked;

            try
            {
                await MSALClientSingleton.Instance.AcquireTokenSilentAsync();
            }
            catch (MsalClientException ex) when (ex.ErrorCode == MsalError.AuthenticationCanceledError)
            {
                await ShowMessage("Login failed", "User cancelled sign in.");
                return;
            }

            //await Shell.Current.GoToAsync("userview");
        }

        private async Task ShowMessage(string title, string message)
        {
            var dialog = new ContentDialog();
            dialog.Title = title;
            dialog.Content = message;
            dialog.CloseButtonText = "Ok";

            var dialogResult = await dialog.ShowAsync();
            //_ = this.Dispatcher.Dispatch(async () =>
            //{
            //    await DisplayAlert(title, message, "OK").ConfigureAwait(false);
            //});
        }

    }
}
