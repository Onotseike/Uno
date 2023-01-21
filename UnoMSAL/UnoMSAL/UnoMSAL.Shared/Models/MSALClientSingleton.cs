using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.CallRecords;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;
using Microsoft.IdentityModel.Abstractions;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnoMSAL.Models
{
    public class MSALClientSingleton
    {

        public static MSALClientSingleton Instance { get; private set; } = new MSALClientSingleton();

        private static IConfiguration AppConfiguration;

        public MSALClientHelper MSALClientHelper { get; }

        public MSGraphHelper MSGraphHelper { get; }

        public bool UseEmbedded { get; set; } = false;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private MSALClientSingleton()
        {
            // Load config
            var assembly = Assembly.GetExecutingAssembly();
            string embeddedConfigfilename = $"{Assembly.GetCallingAssembly().GetName().Name}.appsettings.json";
            using var stream = assembly.GetManifestResourceStream(embeddedConfigfilename);
            AppConfiguration = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            AzureADConfig azureADConfig = AppConfiguration.GetSection("AzureAD").Get<AzureADConfig>();
            this.MSALClientHelper = new MSALClientHelper(azureADConfig);

            MSGraphApiConfig graphApiConfig = AppConfiguration.GetSection("MSGraphApi").Get<MSGraphApiConfig>();
            this.MSGraphHelper = new MSGraphHelper(graphApiConfig, this.MSALClientHelper);
        }


        public async Task<string> AcquireTokenSilentAsync()
        {
            return await this.AcquireTokenSilentAsync(this.GetScopes()).ConfigureAwait(false);
        }

        public async Task<string> AcquireTokenSilentAsync(string[] scopes)
        {
            return await this.MSALClientHelper.SignInUserAndAcquireAccessToken(scopes).ConfigureAwait(false);
        }

        internal async Task<AuthenticationResult> AcquireTokenInteractiveAsync(string[] scopes)
        {
            this.MSALClientHelper.UseEmbedded = this.UseEmbedded;
            return await this.MSALClientHelper.SignInUserInteractivelyAsync(scopes).ConfigureAwait(false);
        }

        internal async Task SignOutAsync()
        {
            await this.MSALClientHelper.SignOutUserAsync().ConfigureAwait(false);
        }

        internal string[] GetScopes()
        {
            return this.MSGraphHelper.MSGraphApiConfig.ScopesArray;
        }


    }

    public class MSALClientHelper
    {
        public AzureADConfig AzureADConfig;

        public AuthenticationResult AuthResult { get; private set; }

        public bool IsBrokerInitialized { get; private set; }
        
        public IPublicClientApplication PublicClientApplication { get; private set; }
        
        public bool UseEmbedded { get; set; } = false;
        
        private PublicClientApplicationBuilder PublicClientApplicationBuilder;

        // Token Caching setup - Mac
        public static readonly string KeyChainServiceName = "Contoso.MyProduct";

        public static readonly string KeyChainAccountName = "MSALCache";

        // Token Caching setup - Linux
        public static readonly string LinuxKeyRingSchema = "com.contoso.msaltokencache";

        public static readonly string LinuxKeyRingCollection = MsalCacheHelper.LinuxKeyRingDefaultCollection;
        public static readonly string LinuxKeyRingLabel = "MSAL token cache for Contoso.";
        public static readonly KeyValuePair<string, string> LinuxKeyRingAttr1 = new KeyValuePair<string, string>("Version", "1");
        public static readonly KeyValuePair<string, string> LinuxKeyRingAttr2 = new KeyValuePair<string, string>("ProductGroup", "Contoso");

        private static string PCANotInitializedExceptionMessage = "The PublicClientApplication needs to be initialized before calling this method. Use InitializePublicClientAppAsync() or InitializePublicClientAppForWAMBrokerAsync() to initialize.";

        public MSALClientHelper(AzureADConfig azureADConfig)
        {
            AzureADConfig = azureADConfig;

            this.InitializePublicClientApplicationBuilder();
        }

        private void InitializePublicClientApplicationBuilder()
        {
            this.PublicClientApplicationBuilder = PublicClientApplicationBuilder.Create(AzureADConfig.ClientId)
            .WithAuthority(string.Format(AzureADConfig.Authority, AzureADConfig.TenantId))
            .WithExperimentalFeatures() // this is for upcoming logger
            .WithLogging(new IdentityLogger(EventLogLevel.Warning), enablePiiLogging: false)    // This is the currently recommended way to log MSAL message. For more info refer to https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/wiki/logging. Set Identity Logging level to Warning which is a middle ground
                .WithClientCapabilities(new string[] { "cp1" })                                     // declare this client app capable of receiving CAE events- https://aka.ms/clientcae

                .WithIosKeychainSecurityGroup("com.microsoft.adalcache");
        }

        public async Task<IAccount> InitializePublicClientAppAsync()
        {
            // Initialize the MSAL library by building a public client application
            this.PublicClientApplication = this.PublicClientApplicationBuilder
                .WithRedirectUri(PlatformConfig.Instance.RedirectUri)   // redirect URI is set later in PlatformConfig when the platform has been decided
                .Build();

            await AttachTokenCache();
            return await FetchSignedInUserFromCache().ConfigureAwait(false);
        }

        /// <summary>
        /// Initializes the public client application of MSAL.NET with the required information to correctly authenticate the user using the WAM broker.
        /// </summary>
        /// <returns>An IAccount of an already signed-in user (if available)</returns>
        public async Task<IAccount> InitializePublicClientAppForWAMBrokerAsync()
        {
            // Initialize the MSAL library by building a public client application
            this.PublicClientApplication = this.PublicClientApplicationBuilder
                .WithRedirectUri(PlatformConfig.Instance.RedirectUri)   // redirect URI is set later in PlatformConfig when the platform is decided
                .WithBroker()
                .WithParentActivityOrWindow(() => PlatformConfig.Instance.ParentWindow)   // This is required when using the WAM broker and is set later in PlatformConfig when the platform has been decided
                .Build();

            this.IsBrokerInitialized = true;

            await AttachTokenCache();
            return await FetchSignedInUserFromCache().ConfigureAwait(false);
        }

        /// <summary>
        /// Attaches the token cache to the Public Client app.
        /// </summary>
        /// <returns>IAccount list of already signed-in users (if available)</returns>
        private async Task<IEnumerable<IAccount>> AttachTokenCache()
        {
#if WINDOWS
            return null;
#endif
            

            // Cache configuration and hook-up to public application. Refer to https://github.com/AzureAD/microsoft-authentication-extensions-for-dotnet/wiki/Cross-platform-Token-Cache#configuring-the-token-cache
            var storageProperties = new StorageCreationPropertiesBuilder(AzureADConfig.CacheFileName, AzureADConfig.CacheDir)
                    .Build();

            var msalcachehelper = await MsalCacheHelper.CreateAsync(storageProperties);
            msalcachehelper.RegisterCache(PublicClientApplication.UserTokenCache);

            // If the cache file is being reused, we'd find some already-signed-in accounts
            return await PublicClientApplication.GetAccountsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Signs in the user and obtains an Access token for a provided set of scopes
        /// </summary>
        /// <param name="scopes"></param>
        /// <returns> Access Token</returns>
        public async Task<string> SignInUserAndAcquireAccessToken(string[] scopes)
        {
            Exception<NullReferenceException>.ThrowOn(() => this.PublicClientApplication == null, PCANotInitializedExceptionMessage);

            var existingUser = await FetchSignedInUserFromCache().ConfigureAwait(false);

            try
            {
                // 1. Try to sign-in the previously signed-in account
                if (existingUser != null)
                {
                    this.AuthResult = await this.PublicClientApplication
                        .AcquireTokenSilent(scopes, existingUser)
                        .ExecuteAsync()
                        .ConfigureAwait(false);
                }
                else
                {
                    if (this.IsBrokerInitialized)
                    {
                        Console.WriteLine("No accounts found in the cache. Trying Window's default account.");

                        this.AuthResult = await this.PublicClientApplication
                            .AcquireTokenSilent(scopes, Microsoft.Identity.Client.PublicClientApplication.OperatingSystemAccount)
                            .ExecuteAsync()
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        this.AuthResult = await SignInUserInteractivelyAsync(scopes);
                    }
                }
            }
            catch (MsalUiRequiredException ex)
            {
                // A MsalUiRequiredException happened on AcquireTokenSilentAsync. This indicates you need to call AcquireTokenInteractive to acquire a token interactively
                Debug.WriteLine($"MsalUiRequiredException: {ex.Message}");

                this.AuthResult = await this.PublicClientApplication
                    .AcquireTokenInteractive(scopes)
                    .WithLoginHint(existingUser?.Username ?? String.Empty)
                    .ExecuteAsync()
                    .ConfigureAwait(false);
            }
            catch (MsalException msalEx)
            {
                Debug.WriteLine($"Error Acquiring Token interactively:{Environment.NewLine}{msalEx}");
                throw msalEx;
            }

            return this.AuthResult.AccessToken;
        }

        /// <summary>
        /// Signs the in user and acquire access token for a provided set of scopes.
        /// </summary>
        /// <param name="scopes">The scopes.</param>
        /// <param name="extraclaims">The extra claims, usually from CAE. We basically handle CAE by sending the user back to Azure AD for
        /// additional processing and requesting a new access token for Graph</param>
        /// <returns></returns>
        public async Task<String> SignInUserAndAcquireAccessToken(string[] scopes, string extraclaims)
        {
            Exception<NullReferenceException>.ThrowOn(() => this.PublicClientApplication == null, PCANotInitializedExceptionMessage);

            try
            {
                // Send the user to Azure AD for re-authentication as a silent acquisition wont resolve any CAE scenarios like an extra claims request
                this.AuthResult = await this.PublicClientApplication.AcquireTokenInteractive(scopes)
                        .WithClaims(extraclaims)
                        .ExecuteAsync()
                        .ConfigureAwait(false);
            }
            catch (MsalException msalEx)
            {
                Debug.WriteLine($"Error Acquiring Token:{Environment.NewLine}{msalEx}");
            }

            return this.AuthResult.AccessToken;
        }

        /// <summary>
        /// Shows a pattern to sign-in a user interactively in applications that are input constrained and would need to fall-back on device code flow.
        /// </summary>
        /// <param name="scopes">The scopes.</param>
        /// <param name="existingAccount">The existing account.</param>
        /// <returns></returns>
        public async Task<AuthenticationResult> SignInUserInteractivelyAsync(string[] scopes, IAccount existingAccount = null)
        {
            Exception<NullReferenceException>.ThrowOn(() => this.PublicClientApplication == null, PCANotInitializedExceptionMessage);

            if (this.PublicClientApplication == null)
                throw new NullReferenceException();

            // If the operating system has UI
            if (this.PublicClientApplication.IsUserInteractive())
            {
                if (MSALClientSingleton.Instance.UseEmbedded)
                {
                    return await this.PublicClientApplication.AcquireTokenInteractive(scopes)
                        .WithLoginHint(existingAccount?.Username ?? String.Empty)
                        .WithUseEmbeddedWebView(true)
                        .WithParentActivityOrWindow(PlatformConfig.Instance.ParentWindow)
                        .ExecuteAsync()
                        .ConfigureAwait(false);
                }
                else
                {
                    SystemWebViewOptions systemWebViewOptions = new SystemWebViewOptions();
#if IOS
                    // Hide the privacy prompt in iOS
                    systemWebViewOptions.iOSHidePrivacyPrompt = true;
#endif
                    return await this.PublicClientApplication.AcquireTokenInteractive(scopes)
                        .WithLoginHint(existingAccount?.Username ?? String.Empty)
                        .WithSystemWebViewOptions(systemWebViewOptions)
                        .WithParentActivityOrWindow(PlatformConfig.Instance.ParentWindow)
                        .ExecuteAsync()
                        .ConfigureAwait(false);
                }
            }

            // If the operating system does not have UI (e.g. SSH into Linux), you can fallback to device code, however this
            // flow will not satisfy the "device is managed" CA policy.
            return await this.PublicClientApplication.AcquireTokenWithDeviceCode(scopes, (dcr) =>
            {
                Console.WriteLine(dcr.Message);
                return Task.CompletedTask;
            }).ExecuteAsync().ConfigureAwait(false);
        }

        public async Task SignOutUserAsync()
        {
            var existingUser = await FetchSignedInUserFromCache().ConfigureAwait(false);
            await this.SignOutUserAsync(existingUser).ConfigureAwait(false);
        }

        public async Task SignOutUserAsync(IAccount user)
        {
            if (this.PublicClientApplication == null) return;

            await this.PublicClientApplication.RemoveAsync(user).ConfigureAwait(false);
        }

        public async Task<IAccount> FetchSignedInUserFromCache()
        {
            Exception<NullReferenceException>.ThrowOn(() => this.PublicClientApplication == null, PCANotInitializedExceptionMessage);

            // get accounts from cache
            IEnumerable<IAccount> accounts = await this.PublicClientApplication.GetAccountsAsync().ConfigureAwait(false);

            if (accounts.Count() > 1)
            {
                foreach (var acc in accounts)
                {
                    await this.PublicClientApplication.RemoveAsync(acc);
                }

                return null;
            }

            return accounts.SingleOrDefault();
        }
    }

    public class MSGraphHelper
    {
        public readonly MSGraphApiConfig MSGraphApiConfig;

        public MSALClientHelper MSALClient { get; }
        private GraphServiceClient _graphServiceClient;

        private string[] GraphScopes;
        private string MSGraphBaseUrl = "https://graph.microsoft.com/v1.0";

        public MSGraphHelper(MSGraphApiConfig graphApiConfig, MSALClientHelper msalClientHelper)
        {
            if (msalClientHelper == null)
            {
                throw new ArgumentNullException(nameof(msalClientHelper));
            }
            this.MSGraphApiConfig = graphApiConfig;

            this.MSALClient = msalClientHelper;
            this.GraphScopes = this.MSGraphApiConfig.ScopesArray;
            this.MSGraphBaseUrl = this.MSGraphApiConfig.MSGraphBaseUrl;
        }

        public async Task<User> GetMeAsync()
        {
            if (this._graphServiceClient == null)
            {
                await SignInAndInitializeGraphServiceClient();
            }

            User graphUser = null;

            // Call /me Api

            try
            {
                graphUser = await _graphServiceClient.Me.Request().GetAsync();
            }
            catch (ServiceException ex) when (ex.Message.Contains("Continuous access evaluation resulted in claims challenge"))
            {
                this._graphServiceClient = await SignInAndInitializeGraphServiceClientPostCAE(ex);

                // Call the /me endpoint of Graph again with a fresh token
                graphUser = await _graphServiceClient.Me.Request().GetAsync();
            }
            return graphUser;
        }

        
        public async Task<Stream> GetMyPhotoAsync()
        {
            if (this._graphServiceClient == null)
            {
                await SignInAndInitializeGraphServiceClient();
            }

            Stream userPhoto = null;

            // Call /me/Photo Api

            try
            {
                userPhoto = await _graphServiceClient.Me.Photo.Content.Request().GetAsync();
            }
            catch (ServiceException ex) when (ex.Message.Contains("Continuous access evaluation resulted in claims challenge"))
            {
                this._graphServiceClient = await SignInAndInitializeGraphServiceClientPostCAE(ex);

                // Call the /me endpoint of Graph again with a fresh token
                userPhoto = await _graphServiceClient.Me.Photo.Content.Request().GetAsync();
            }
            return userPhoto;
        }

        private async Task<GraphServiceClient> SignInAndInitializeGraphServiceClient()
        {
            string token = await this.MSALClient.SignInUserAndAcquireAccessToken(this.GraphScopes);
            return await InitializeGraphServiceClientAsync(token);
        }

        private async Task<GraphServiceClient> SignInAndInitializeGraphServiceClientPostCAE(ServiceException ex)
        {
            // Get challenge from response of Graph API
            var claimChallenge = WwwAuthenticateParameters.GetClaimChallengeFromResponseHeaders(ex.ResponseHeaders);

            string token = await this.MSALClient.SignInUserAndAcquireAccessToken(this.GraphScopes, claimChallenge);
            return await InitializeGraphServiceClientAsync(token);
        }

        private async Task<GraphServiceClient> InitializeGraphServiceClientAsync(string token)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            this._graphServiceClient = new GraphServiceClient(client);

            return await Task.FromResult(this._graphServiceClient);
        }
    }

    public class AzureADConfig
    {
        public string Authority { get; set; }

        public string ClientId { get; set; }

        public string TenantId { get; set; }

        public string RedirectURI { get; set; }

        public string CacheFileName { get; set; }

        public string CacheDir { get; set; }

        public string AndroidRedirectUri { get; set; }

        public string iOSRedirectUri { get; set; }
    }

    public class MSGraphApiConfig
    {
        public string MSGraphBaseUrl { get; set; }

        public string Scopes { get; set; }

        public string[] ScopesArray
        {
            get
            {
                return Scopes.Split(' ');
            }
        }
    }

    public static class Exception<TException> where TException : Exception, new()
    {
        public static void ThrowOn(Func<bool> predicate, string message = null)
        {
            if (predicate())
            {
                TException toThrow = Activator.CreateInstance(typeof(TException), message) as TException;
                throw toThrow;
            }
        }
    }

    public class PlatformConfig
    {
        public static PlatformConfig Instance { get; } = new PlatformConfig();

        public string RedirectUri { get; set; }

        public object ParentWindow { get; set; }

        private PlatformConfig()
        {
        }
    }

    public class IdentityLogger : IIdentityLogger
    {
        private EventLogLevel _minLogLevel = EventLogLevel.LogAlways;

        public IdentityLogger(EventLogLevel minLogLevel = EventLogLevel.LogAlways)
        {
            _minLogLevel = minLogLevel;
        }

        public bool IsEnabled(EventLogLevel eventLogLevel)
        {
            return eventLogLevel >= _minLogLevel;
        }

        public void Log(LogEntry entry)
        {
            Debug.WriteLine($"MSAL: EventLogLevel: {entry.EventLogLevel}, Message: {entry.Message} ");
        }
    }

}
