// <copyright file="AuthenticationConfiguration.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

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
            ArgumentNullException.ThrowIfNull(authentication);

            var identityServiceOptions = new ConstellationIdentityServiceAuthenticationOptions
            {
                AzureActiveDirectoryClientCredentials =
                        {
                            ClientId = authentication?.AuthClientId,
                            ClientSecret = authentication?.AuthClientSecret,
                            Scope = authentication?.AuthScope,
                            Tenant = authentication?.AuthTenant,
                        },
                ServerAddress = new Uri(authentication?.AuthServerAdress ?? string.Empty),
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
            ArgumentNullException.ThrowIfNull(authentication);

            services.AddSystemAccountAuthenticationProvider<IConfiguration>((settings, configuration) =>
            {
                settings.Tenant = authentication?.AuthTenant;
                settings.ClientId = authentication?.AuthClientId;
                settings.Audience = authentication?.AuthScope;
                settings.ClientSecret = authentication?.AuthClientSecret;
            });
        }
    }
}
