using Kpmg.AspNetCore.Authentication.ConstellationIdentityService;
using Microsoft.AspNetCore.Authorization;
using Pulse.Account.API.Configuration.Model;

namespace Pulse.Account.API.Configuration
{
    public static class AuthenticationConfiguration
    {
        public static IServiceCollection RegisterAuthenticationAndAuthorization(this IServiceCollection services,
            AuthenticationModel authentication)
        {
            ArgumentNullException.ThrowIfNull(authentication, nameof(authentication));

            var identityServiceOptions = new ConstellationIdentityServiceAuthenticationOptions
            {
                AzureActiveDirectoryClientCredentials =
                        {
                            ClientId = authentication.AuthClientId,
                            ClientSecret = authentication.AuthClientSecret,
                            Scope = authentication.AuthScope,
                            Tenant = authentication.AuthTenant,
                        },
                ServerAddress = new Uri(authentication.AuthServerAdress ?? string.Empty),
            };

            services.AddAuthentication()
            .AddConstellationIdentityService(identityServiceOptions, out string[] schemaNames);

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes(schemaNames)
                    .Build();
            });
            services.AddConstellationHttpClient();

            return services;
        }

        public static void RegisterSystemAuthenticationProvider(
            this IServiceCollection services,
            AuthenticationModel authentication)
        {
            ArgumentNullException.ThrowIfNull(authentication, nameof(authentication));

            services.AddSystemAccountAuthenticationProvider<IConfiguration>((settings, configuration) =>
            {
                settings.Tenant = authentication.AuthTenant;
                settings.ClientId = authentication.AuthClientId;
                settings.Audience = authentication.AuthScope;
                settings.ClientSecret = authentication.AuthClientSecret;
            });
        }
    }
}
