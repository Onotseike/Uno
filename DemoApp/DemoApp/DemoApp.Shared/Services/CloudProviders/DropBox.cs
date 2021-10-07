using DemoApp.Extensions;

using Dropbox.Api;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace DemoApp.Services.CloudProviders
{
    public class DropBox : BaseCloudProvider, ICloudProvider
    {
        protected static class DAuthenticationSettings
        {
            public const string AppKey = "d0v588tiq4o5gt9";
            public const string AppSecret = "3xud3y91x2i9k6c";
            public const string RedirectUri = "https://xmodedevs.ebusinesscards/authorize";
            

        }

        #region Properties

        public static DropboxClient DropBoxClient { get; private set; }
        private string AuthenticationUrl { get; set; }
        private string OAuthToState { get; set; }
        private string AccessToken { get; set; }

        public Action OnAuthenticated;

        private bool isSignedIn;
        public bool IsSignedIn
        {
            get => isSignedIn;
            set => SetProperty(ref isSignedIn, value);
        }

        private bool isBackedUp;
        public bool IsBackedUp
        {
            get => isBackedUp;
            set => SetProperty(ref isBackedUp, value);
        }
        // The user's display name
        private string userName;
        public string Username
        {
            get => userName;
            set => SetProperty(ref userName, value);
        }

        // The user's email
        private string userEmail;
        public string UserEmail
        {
            get => userEmail;
            set => SetProperty(ref userEmail, value);
        }

        #endregion

        #region Constructor(s)

        public DropBox() => AuthenticationUrl = GenerateAuthenticationUrlAsync().GetAwaiter().GetResult();

        #endregion

        #region Helper Method(s)

        private DropboxClient GetDropBoxClient(string accessToken) => new DropboxClient(accessToken);

        private async Task<string> GenerateAuthenticationUrlAsync()
        {
            try
            {
                OAuthToState = Guid.NewGuid().ToString("N");
                Uri authorizeUri = DropboxOAuth2Helper.GetAuthorizeUri(OAuthResponseType.Token, DAuthenticationSettings.AppKey, new Uri(DAuthenticationSettings.RedirectUri), OAuthToState);
                AuthenticationUrl = authorizeUri.AbsoluteUri;
                return AuthenticationUrl;


            }
            catch (Exception exception)
            {
                var crashProperties = new Dictionary<string, string>
                {
                    {"Class", "DropBox.cs" },
                    {"Method", "GenerateAuthenticationUrl()" },
                    {"Source", exception.Source },
                    {"Message", exception.Message }
                };
                //Crashes.TrackError(exception, crashProperties);

                Debug.WriteLine("Obtaining DropBox Authentication URL Error " + exception.Message);
                var alert = new ContentDialog
                {
                    Title = "Obtaining DropBox Authentication URL Error",
                    Content = exception.Message
                }.SetPrimaryButton("OK");
                await alert.ShowOneAtATimeAsync();                
                return null;
            }
        }

        private async Task DropBoxAuthorize()
        {
            if (!string.IsNullOrEmpty(AccessToken) || !string.IsNullOrWhiteSpace(AccessToken))
            {
                DropBoxClient = GetDropBoxClient(AccessToken);                

                await GetUserInfo();

                OnAuthenticated?.Invoke();
                return;
            }            

            //Authentication of Dropbox
            try
            {
                if (CanAuthenticate())
                {
                    if (string.IsNullOrEmpty(AuthenticationUrl))
                    {
                        throw new Exception("AuthenticationURL is not generated !");

                    }

                    //WebView for DropBox Sign-in
                    var webView = new WebView
                    {
                        Source = new Uri(!string.IsNullOrEmpty(AuthenticationUrl) ? AuthenticationUrl : GenerateAuthenticationUrlAsync().GetAwaiter().GetResult())

                    };
                    webView.NavigationStarting += DropBoxSignInNavigating; ;
                    //webView.Navigated += DropBoxSignInNavigated;
                    //var contentPage = new ContentPage { Content = webView };
                    //Application.Current.content
                }
            }
            catch (Exception exception)
            {
                var crashProperties = new Dictionary<string, string>
                {
                    {"Class", "DropBox.cs" },
                    {"Method", "DropBoxAuthorize()" },
                    {"Source", exception.Source },
                    {"Message", exception.Message }
                };
                //Crashes.TrackError(exception, crashProperties);

                Debug.WriteLine("DropBox Authentication Error " + exception.Message);
                //await Application.Current.MainPage.DisplayAlert("DropBox Authentication Error", exception.Message, "OK");
            }
        }

        
        #endregion

        #region Event Handling
        private async void DropBoxSignInNavigating(WebView sender, WebViewNavigationStartingEventArgs e)
        {
            var url = "https://www.dropbox.com/oauth2/authorize";
            
            if (!e.Uri.ToString().StartsWith(DAuthenticationSettings.RedirectUri, StringComparison.OrdinalIgnoreCase) && !e.Uri.ToString().StartsWith(url, StringComparison.OrdinalIgnoreCase))
            {
                // we need to ignore all navigation that isn't to the redirect uri.  
                return;
            }

            if (e.Uri.ToString().StartsWith(DAuthenticationSettings.RedirectUri, StringComparison.OrdinalIgnoreCase))
            {
                try
                {

                    OAuth2Response result = DropboxOAuth2Helper.ParseTokenFragment(new Uri(e.Url));
                    if (result.State != OAuthToState)
                    {
                        return;
                    }

                    AccessToken = result.AccessToken;
                    DropBoxClient = GetDropBoxClient(AccessToken);
                    //await SaveDropBoxTokenInSettings(AccessToken);
                    OnAuthenticated?.Invoke();
                }

                catch (Exception exception)
                {
                    var crashProperties = new Dictionary<string, string>
                {
                    {"Class", "DropBox.cs" },
                    {"Method", "DropBoxSigninNavigating()" },
                    {"Source", exception.Source },
                    {"Message", exception.Message }
                };
                    //Crashes.TrackError(exception, crashProperties);

                    Debug.WriteLine("DropBox WebView Navigating Error " + exception.Message);
                    //await Application.Current.MainPage.DisplayAlert("DropBox WebView Navigating Error", exception.Message, "OK");
                }

                finally
                {
                    e.Cancel = true;
                    IsSignedIn = true;

                    //Service Id, Save Settings & IsConnected Settings
                    //App.SecureStorages.AccessToken = App.DropBoxService.AccessToken ?? "";
                    //App.SecureStorages.CloudServiceId = (int)ServicesId.DropBox;
                    //App.SecureStorages.CloudServiceName = Enum.GetName(typeof(ServicesId), 1);
                    //App.SecureStorages.WasPreviouslyConnected = true;

                    //App.SecureStorages.DropBoxAccessToken = App.DropBoxService.AccessToken ?? "";

                    //await GetUserInfo();

                    //App.SecureStorages.Username = App.DropBoxService.Username ?? "";
                    //App.SecureStorages.Email = App.DropBoxService.UserEmail ?? "";

                    //App.SecureStorages.OnUnloadCommand.Execute(null);

                    //IsBackedUp = await BackUpDatabase();
                    //await Shell.Current.Navigation.PopModalAsync();
                }
            }

        }

        #endregion

        #region ICloudProvider Implementation(s)

        public Task<bool> BackUp(string databaseName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Restore(string databaseName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> SignInAsync()
        {
            throw new NotImplementedException();
        }

        public Task<bool> SignOutAsync()
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}
