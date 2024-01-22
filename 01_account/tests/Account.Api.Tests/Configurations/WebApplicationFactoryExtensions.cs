using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using Kpmg.Account.Core.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Pulse.Account.Core.Interfaces;

namespace Account.Api.Tests.Configurations
{
    [ExcludeFromCodeCoverage]
    public static class WebApplicationFactoryExtensions
    {
        /// <summary>
        /// When you use [Authorize(AuthenticationSchemes = "Scheme1,Scheme2, ...")] on an action/controller, 
        /// the default authorization policy is combined before evaluation. In your test setup, 
        /// you can combine an empty policy with Test scheme with the default policy from the actual code to make sure your Test scheme always runs.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="factory"></param>
        /// <returns></returns>
        public static WebApplicationFactory<T> WithAuthentication<T>(this WebApplicationFactory<T> factory, bool useSystemToken = false)
            where T : class
        {
            return factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll(typeof(IHostedService));
                    services.AddScoped<IAccountService, AccountService>();
                    services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                    services.AddSingleton<IAuthenticationContext, AuthenticationContextMOCK>();
                    services.AddAuthentication(o =>
                    {
                        o.DefaultAuthenticateScheme = "Test";
                        o.DefaultChallengeScheme = "Test";
                    }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });

                    services.AddAuthorization(opt =>
                    {
                        opt.DefaultPolicy = new AuthorizationPolicyBuilder()
                            .AddAuthenticationSchemes("Test")
                            .Combine(opt.DefaultPolicy)
                            .Build();
                    });
                });
                builder.UseEnvironment("test");
            });
        }

        public static HttpClient CreateClientWithTestAuth<T>(this WebApplicationFactory<T> factory, bool useSystemToken = false) where T : class
        {
            var client = factory.WithAuthentication(useSystemToken).CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
            });

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

            return client;
        }
    }
}
