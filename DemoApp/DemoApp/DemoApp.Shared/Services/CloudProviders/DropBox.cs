using DemoApp.Extensions;

using Dropbox.Api;
using Dropbox.Api.Files;

using Microsoft.Graph;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using File = System.IO.File;

namespace DemoApp.Services.CloudProviders
{
    public class DropBox : BaseCloudProvider, ICloudProvider
    {
        #region OAuthSettings

        protected static class DAuthenticationSettings
        {
            public const string AppKey = "d0v588tiq4o5gt9";
            public const string AppSecret = "3xud3y91x2i9k6c";
            public const string RedirectUri = "https://xmodedevs.ebusinesscards/authorize";
        }

        #endregion

        #region Properties

        public static DropboxClient DropBoxClient { get; private set; }
        private string AuthenticationUrl { get; set; }
        private string OAuthToState { get; set; }
        private string AccessToken { get; set; }

        public Action OnAuthenticated;

        #endregion

        #region Constructor(s)

        public DropBox() => AuthenticationUrl = GenerateAuthenticationUrlAsync().GetAwaiter().GetResult();

        #endregion

        #region Helper Method(s)

        private DropboxClient GetDropBoxClient(string accessToken)
        {
            #if __ANDROID__
            return new DropboxClient(AccessToken, new DropboxClientConfig() { HttpClient = new HttpClient(new HttpClientHandler()) });
            #else
            return new DropboxClient(accessToken);
            #endif
        }

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

        private async Task AccountInfo()
        {
            var account = await DropBoxClient.Users.GetCurrentAccountAsync();
            Username = account.Name.DisplayName;
            UserEmail = account.Email;
        }

        private bool CanAuthenticate()
        {
            try
            {
                if (DAuthenticationSettings.AppKey == null)
                {
                    throw new ArgumentNullException("AppKey");
                }
                if (DAuthenticationSettings.AppSecret == null)
                {
                    throw new ArgumentNullException("AppSecret");
                }
                return true;
            }
            catch (Exception exception)
            {
                var crashProperties = new Dictionary<string, string>
                {
                    {"Class", "DropBox.cs" },
                    {"Method", "CanAuthenticate()" },
                    {"Source", exception.Source },
                    {"Message", exception.Message }
                };
                //Crashes.TrackError(exception, crashProperties);

                Debug.WriteLine("Obtaining DropBox Access Token Error " + exception.Message);
                //Application.Current.MainPage.DisplayAlert("Obtaining DropBox Access Token Error", exception.Message, "OK");
                return false;
            }
        }

        private async Task<IList<Metadata>> GetAppFolderAsync()
        {
            try
            {
                if (DropBoxClient == null)
                {
                    DropBoxClient = GetDropBoxClient(AccessToken);
                }
                var list = await DropBoxClient.Files.ListFolderAsync(string.Empty);
                return list.Entries;
            }
            catch (Exception exception)
            {

                throw;
            }
        }

#endregion

        #region Event Handling

        private void DropBoxSignInNavigating(WebView sender, WebViewNavigationStartingEventArgs e)
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

                    OAuth2Response result = DropboxOAuth2Helper.ParseTokenFragment(e.Uri);
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

        public async Task<bool> BackUp(string databaseName)
        {
            try
            {
                await BackupDatabase(databaseName);
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public async Task<bool> Restore(string databaseName)
        {
            try
            {
                await RestoreDatabase(databaseName);
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public async Task<bool> SignInAsync()
        {
            if (!string.IsNullOrEmpty(AccessToken) || !string.IsNullOrWhiteSpace(AccessToken))
            {
                DropBoxClient = GetDropBoxClient(AccessToken);

                await AccountInfo();

                OnAuthenticated?.Invoke();
                return false;
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

                    //WebView for DropBoC:\Users\onots\OneDrive\Documents\ADO\Solo\Solo\SoloInteractive\SoloDbService.ipynbx Sign-in
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

        public Task<bool> SignOutAsync()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Sync Method(s)

        public async Task RestoreDatabase(string databaseName)
        {
            try
            {
                Stream stream = null;
                if (DropBoxClient == null)
                {
                    DropBoxClient = GetDropBoxClient(AccessToken);
                }
                var database = await DropBoxClient.Files.DownloadAsync(databaseName);
                stream = await database.GetContentAsStreamAsync();

                var destinationPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), databaseName));
                using (var databaseDriveItem = File.Create(destinationPath))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    await stream.CopyToAsync(databaseDriveItem);
                }
            }
            catch (Exception exception)
            {

                throw;
            }
        }

        public async Task BackupDatabase(string databaseName)
        {
            try
            {
                var sourcPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), databaseName));
                var databaseData = await File.ReadAllBytesAsync(sourcPath);

                if (DropBoxClient == null)
                {
                    DropBoxClient = GetDropBoxClient(AccessToken);
                }
                using (var stream = new MemoryStream(databaseData))
                {
                    var commitInfo = new CommitInfo(databaseName, WriteMode.Overwrite.Instance, false, DateTime.UtcNow);
                    var syncDatabase = await DropBoxClient.Files.UploadAsync(commitInfo, stream);
                }
               
            }
            catch (Exception exception)
            {
                // await DisplayAlert();
            }
        }

        #endregion
    }
}
