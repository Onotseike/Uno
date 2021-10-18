using DemoApp.Extensions;

using Microsoft.Graph;
using Microsoft.Identity.Client;

using SQLite;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Windows.UI.Xaml.Controls;
using File = System.IO.File;

#if NETFX_CORE
using _Popup = Windows.UI.Xaml.Controls.Primitives.Popup;

#else
using _Popup = Windows.UI.Xaml.Controls.Popup;
#endif

namespace DemoApp.Services.CloudProviders
{
    public class OneDrive : BaseCloudProvider, ICloudProvider
    {
        #region OAuthSettings

        protected static class OAuthenticationSettings
        {
            public const string ApplicationId = "4f554894-133f-44c9-92fe-bdcb164ddaa0";
            public const string RedirectUri = "soloApp://redirect";
            public readonly static string[] Scopes = new string[] { "Files.ReadWrite.AppFolder", "User.Read", "Device.Read" };
        }

        #endregion

        #region Property(ies)

        // Microsoft Authentication client for native/mobile apps
        private IPublicClientApplication PCA { get; set; }

        // UIParent used by Android version of the app
        public object AuthenticationUIParent { get; private set; }

        // Keychain security group used by iOS version of the app
        public string IOSKeychainSecurityGroup { get; private set; }

        // Microsoft Graph client
        private GraphServiceClient GraphClient { get; set; }
                
        #endregion

        #region Constructor(s)

        public OneDrive()
        {
            var builder = PublicClientApplicationBuilder.Create(OAuthenticationSettings.ApplicationId).WithRedirectUri(OAuthenticationSettings.RedirectUri);

            if (!string.IsNullOrEmpty(IOSKeychainSecurityGroup))
            {
                builder = builder.WithIosKeychainSecurityGroup(IOSKeychainSecurityGroup);
            }

            PCA = builder.Build();
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
            // First, attempt silent sign in
            // If the user's information is already in the app's cache,
            // they won't have to sign in again.

            try
            {
                var accounts = await PCA.GetAccountsAsync();

                var enumerable = accounts.ToList();
                if (enumerable.Any())
                {
                    var silentAuthResult = await PCA
                        .AcquireTokenSilent(OAuthenticationSettings.Scopes, enumerable.FirstOrDefault())
                        .ExecuteAsync();

                    Debug.WriteLine("User already signed in.");
                    Debug.WriteLine($"Access token: {silentAuthResult.AccessToken}");
                    // AccessToken = silentAuthResult.AccessToken;
                }

                IsSignedIn = true;
                return IsSignedIn;
            }
            catch (MsalUiRequiredException exception)
            {
                Debug.WriteLine("Silent token request failed, user needs to sign-in: " + exception.Message);
                // Prompt the user to sign-in

                var interactiveRequest = PCA.AcquireTokenInteractive(OAuthenticationSettings.Scopes);

                if (AuthenticationUIParent != null)
                {
                    interactiveRequest = interactiveRequest
                        .WithParentActivityOrWindow(AuthenticationUIParent);
                }

                var authResult = await interactiveRequest.ExecuteAsync();
                Debug.WriteLine($"Successful interactive authentication for: {authResult.Account.Username}");
                Debug.WriteLine($"Access token: {authResult.AccessToken}");
                //AccessToken = authResult.AccessToken;
                IsSignedIn = true;
                return IsSignedIn;


            }
            catch (Exception ex)
            {
                Debug.WriteLine("Silent token request failed, user needs to sign-in");
                Debug.WriteLine("Sign In Error: " + ex.Message);
                var alert = new ContentDialog
                {
                    Title = "OnedDrive Sign-In Error",
                    Content = "Silent token request failed, user needs to sign-in"
                }.SetPrimaryButton("OK");
                await alert.ShowOneAtATimeAsync();
                IsSignedIn = false;
                return false;
            }
        }

        public async Task<bool> SignOutAsync()
        {
            // Get all cached accounts for the app
            // (Should only be one)
            var accounts = await PCA.GetAccountsAsync();
            var enumerable = accounts.ToList();
            while (enumerable.Any())
            {
                // Remove the account info from the cache
                await PCA.RemoveAsync(enumerable.First());
                accounts = await PCA.GetAccountsAsync();
            }

            Username = string.Empty;
            UserEmail = string.Empty;
            IsSignedIn = false;
            return IsSignedIn;
        }

        #endregion

        #region Helper Method(s)

        public async void InitializeGraphClient()
        {
            var accounts = await PCA.GetAccountsAsync();

            try
            {
                if (accounts.Count() > 0)
                {
                    // Initialize Graph client
                    GraphClient = new GraphServiceClient(new DelegateAuthenticationProvider(
                        async (requestMessage) =>
                        {
                            var result = await PCA.AcquireTokenSilent(OAuthenticationSettings.Scopes, accounts.FirstOrDefault()).ExecuteAsync();

                            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.AccessToken);
                        }));

                    await AccountInfo();

                    IsSignedIn = true;
                }
                else
                {
                    IsSignedIn = false;
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Failed to initialized graph client.");
                Debug.WriteLine($"Accounts in the msal cache: {accounts.Count()}.");
                Debug.WriteLine($"See exception message for details: {exception.Message}");
            }

        }

        private async Task<DriveItem> GetAppFolderAsync()
        {
            try
            {
                if (GraphClient != null)
                {
                    return await GraphClient.Me.Drive.Special.AppRoot.Request().GetAsync();
                }
                else
                {
                    InitializeGraphClient();
                    return await GraphClient.Me.Drive.Special.AppRoot.Request().GetAsync();
                }


            }
            catch (Exception exception)
            {
                Debug.Write("GetAppsFolder Error: " + exception.Message);
                var alert = new ContentDialog
                {
                    Title = "OneDrive GetAppsFolder Error",
                    Content = exception.Message
                }.SetPrimaryButton("OK");
                await alert.ShowOneAtATimeAsync();
                return null;
            }
        }

        private async Task AccountInfo()
        {
            var account = await GraphClient.Me.Request().Select(user => new
            {
                user.DisplayName,
                user.Mail,
                user.UserPrincipalName
            }).GetAsync();

            Username = account.DisplayName;
            UserEmail = String.IsNullOrEmpty(account.Mail) ? account.UserPrincipalName : account.Mail;
        }

        #endregion

        #region Sync Method(s)

        private async Task RestoreDatabase(string databaseName)
        {
            try
            {
                Stream stream = null;
                var databasePath = await GraphClient.Me.Drive.Special.AppRoot.ItemWithPath(databaseName).Request().GetAsync();

                if (databasePath != null)
                {
                    stream = await GraphClient.Me.Drive.Special.AppRoot.ItemWithPath(databaseName).Content.Request().GetAsync();

                    var destinationPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), databaseName));
                    using (var databaseDriveItem = File.Create(destinationPath))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        await stream.CopyToAsync(databaseDriveItem);
                    }
                }
            }
            catch ( Exception exception)
            {
                // await DisplayAlert();
            }            
        }

        private async Task BackupDatabase(string databaseName)
        {
            try
            {
                var sourcPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), databaseName));
                var databaseData = await File.ReadAllBytesAsync(sourcPath);
                var stream = new MemoryStream(databaseData);

                await GraphClient.Me.Drive.Special.AppRoot.ItemWithPath(databaseName).Content.Request().PutAsync<DriveItem>(stream);
            }
            catch (Exception exception)
            {
                // await DisplayAlert();
            }
        }

        #endregion
    }
}
