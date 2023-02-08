using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

using System;
using System.Linq;
using System.Threading.Tasks;

using Uno.UI.MSAL;

namespace UnoMSAL.Authentication
{
    public interface IAuthService
    {
        Task<bool> Authenticate();
        Task SignOut();
        Task<bool> TryRefresh();
    }

    internal class MsalOptions
    {
        public string ClientId { get; set; }
        public string TenantId { get; set; }
        public string AppleRedirectUri { get; set; }
        public string AppleKeyChainGroup { get; set; }
        public string AndroidRedirectUri { get; set; }
        public string B2CSigninSignupAuthority { get; set; }
    }

    public class MSALBasic : IAuthService
    {
        static readonly string[] SCOPES = new[] { "User.Read" };
        readonly MsalOptions options;
        readonly IPublicClientApplication pca;

        public MSALBasic(IConfiguration config)
        {
            this.options = config.GetSection("Msal").Get<MsalOptions>();

            this.pca = PublicClientApplicationBuilder
                .Create(this.options.ClientId)
#if ANDROID
            .WithRedirectUri(this.options.AndroidRedirectUri)
#else
                .WithRedirectUri(this.options.AppleRedirectUri)
#endif
                 //.WithIosKeychainSecurityGroup(this.options.AppleKeyChainGroup)
                 .WithUnoHelpers()
                .Build();
        }


        public async Task<bool> Authenticate()
        {
            try
            {
                var result = await this.pca
                    .AcquireTokenInteractive(SCOPES)

//#if  IOS || MACCATALYST
//                .WithSystemWebViewOptions(new SystemWebViewOptions
//                {
//                    iOSHidePrivacyPrompt = true
//                })
//#endif
                    .WithUnoHelpers()
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                if (result == null)
                    return false;

                this.SetAuth(result);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public async Task<bool> TryRefresh()
        {
            try
            {
                var accts = await this.pca.GetAccountsAsync().ConfigureAwait(false);
                var acct = accts.FirstOrDefault();
                if (acct == null)
                    return false;

                var authResult = await this.pca
                    .AcquireTokenSilent(SCOPES, acct)
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                this.SetAuth(authResult);
                return true;
            }
            catch (MsalUiRequiredException)
            {
                return false;
            }
        }


        public async Task SignOut()
        {
            var accounts = await this.pca
                .GetAccountsAsync()
                .ConfigureAwait(false);

            foreach (var acct in accounts)
                await this.pca.RemoveAsync(acct).ConfigureAwait(false);
        }


        void SetAuth(AuthenticationResult result)
        {
            Console.WriteLine("Token received");
        }
    }

    public class MSALB2C : IAuthService
    {

        static readonly string[] SCOPES = new[] { "User.Read" };
        readonly MsalOptions options;
        readonly IPublicClientApplication pca;
        public MSALB2C(IConfiguration config)
        {

            this.options = config.GetSection("Msal").Get<MsalOptions>();

            this.pca = PublicClientApplicationBuilder
                .Create(this.options.ClientId)
                .WithB2CAuthority(this.options.B2CSigninSignupAuthority)
#if ANDROID
            .WithRedirectUri(this.options.AndroidRedirectUri)
#else
                .WithRedirectUri(this.options.AppleRedirectUri)
#endif
                //.WithIosKeychainSecurityGroup(this.options.AppleKeyChainGroup)
                .WithUnoHelpers()
                .Build();
        }

        public async Task<bool> Authenticate()
        {
            try
            {
                var result = await this.pca
                    .AcquireTokenInteractive(SCOPES)

//#if  IOS || MACCATALYST
//                .WithSystemWebViewOptions(new SystemWebViewOptions
//                {
//                    iOSHidePrivacyPrompt = true
//                })
//#endif
                    .WithB2CAuthority(this.options.B2CSigninSignupAuthority)
                    .WithUnoHelpers()
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                if (result == null)
                    return false;

                this.SetAuth(result);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public async Task<bool> TryRefresh()
        {
            try
            {
                var accts = await this.pca.GetAccountsAsync().ConfigureAwait(false);
                var acct = accts.FirstOrDefault();
                if (acct == null)
                    return false;

                var authResult = await this.pca
                    .AcquireTokenSilent(SCOPES, acct)
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                this.SetAuth(authResult);
                return true;
            }
            catch (MsalUiRequiredException)
            {
                return false;
            }
        }


        public async Task SignOut()
        {
            var accounts = await this.pca
                .GetAccountsAsync()
                .ConfigureAwait(false);

            foreach (var acct in accounts)
                await this.pca.RemoveAsync(acct).ConfigureAwait(false);
        }


        void SetAuth(AuthenticationResult result)
        {
            Console.WriteLine("Token received");
        }

    }

    public class MSALBroker : IAuthService
    {
        static readonly string[] SCOPES = new[] { "User.Read" };
        readonly MsalOptions options;
        readonly IPublicClientApplication pca;

        public MSALBroker(IConfiguration config)
        {
            this.options = config.GetSection("Msal").Get<MsalOptions>();

            this.pca = PublicClientApplicationBuilder
                .Create(this.options.ClientId)
                .WithTenantId(this.options.TenantId)
               // .WithBroker(true)
#if ANDROID
            .WithRedirectUri(this.options.AndroidRedirectUri)
#else
                .WithRedirectUri(this.options.AppleRedirectUri)
#endif
                //.WithIosKeychainSecurityGroup(this.options.AppleKeyChainGroup)
                .WithUnoHelpers()
                .Build();
        }


        public async Task<bool> Authenticate()
        {
            try
            {
                var result = await this.pca
                    .AcquireTokenInteractive(SCOPES)

//#if  IOS || MACCATALYST
//                .WithSystemWebViewOptions(new SystemWebViewOptions
//                {
//                    iOSHidePrivacyPrompt = true
//                })
//#endif
                    .WithUnoHelpers()
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                if (result == null)
                    return false;

                this.SetAuth(result);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


        public async Task<bool> TryRefresh()
        {
            try
            {
                var accts = await this.pca.GetAccountsAsync().ConfigureAwait(false);
                var acct = accts.FirstOrDefault();
                if (acct == null)
                    return false;

                var authResult = await this.pca
                    .AcquireTokenSilent(SCOPES, acct)
                    .ExecuteAsync()
                    .ConfigureAwait(false);

                this.SetAuth(authResult);
                return true;
            }
            catch (MsalUiRequiredException)
            {
                return false;
            }
        }


        public async Task SignOut()
        {
            var accounts = await this.pca
                .GetAccountsAsync()
                .ConfigureAwait(false);

            foreach (var acct in accounts)
                await this.pca.RemoveAsync(acct).ConfigureAwait(false);
        }


        void SetAuth(AuthenticationResult result)
        {
            Console.WriteLine("Token received");
        }

    }
}

