using DemoApp.Extensions;

using Microsoft.Graph;
using Microsoft.Identity.Client;

using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using Windows.UI.Xaml.Controls;

#if NETFX_CORE
using _Popup = Windows.UI.Xaml.Controls.Primitives.Popup;
#else
using _Popup = Windows.UI.Xaml.Controls.Popup;
#endif

namespace DemoApp.Services.CloudProviders
{
    public class OneDrive : BaseCloudProvider, ICloudProvider
    {
        protected static class OAuthenticationSettings
        {
            public const string ApplicationId = "4f554894-133f-44c9-92fe-bdcb164ddaa0";
            public const string RedirectUri = "soloApp://redirect";
            public readonly static string[] Scopes = new string[] { "Files.ReadWrite.AppFolder", "User.Read" };
        }

        #region Property(ies)

        // Microsoft Authentication client for native/mobile apps
        private IPublicClientApplication PCA { get; set; }

        // UIParent used by Android version of the app
        public object AuthenticationUIParent { get; private set; }

        // Keychain security group used by iOS version of the app
        public string IOSKeychainSecurityGroup { get; private set; }

        // Microsoft Graph client
        private GraphServiceClient GraphClient { get; set; }

        private bool isSignedIn;
        public bool IsSignedIn
        {
            get => isSignedIn;
            set => SetProperty(ref isSignedIn, value);
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

        public Task<bool> BackUp(string databaseName)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Restore(string databaseName)
        {
            throw new NotImplementedException();
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
    }
}
