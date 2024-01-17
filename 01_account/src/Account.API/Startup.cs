// <copyright file="Startup.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using Kpmg.AspNetCore.Authentication.ConstellationIdentityService;
using Kpmg.ExceptionMiddleware;
using Kpmg.Offer.API.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Kpmg.Offer.API
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private const int MaxAgeConfHsts = 365;
        private readonly IConfiguration _configuration;
        private readonly SwaggerConfiguration? _swaggerConfiguration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
            _swaggerConfiguration = _configuration.GetSection("Swagger").Get<SwaggerConfiguration>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(_configuration);

            RegisterAuthenticationAndAuthorization(services, _configuration);

            services.AddCors(options =>
            {
                options.AddPolicy(
                    name: "CorsPolicy",
                    builder =>
                    {
                        builder.AllowAnyHeader()
                                .AllowAnyMethod()
                                .AllowCredentials()
                                .SetIsOriginAllowed(_ => true)
                                .WithExposedHeaders("content-range", "content-type", "accept-ranges", "link");
                    });
            });

            services.AddHsts(options =>
            {
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(MaxAgeConfHsts);
            });
            services.AddControllers().AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase).AddControllersAsServices()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.DateParseHandling = DateParseHandling.None;
                });

            if (!string.IsNullOrEmpty(_configuration["OfferApplicationInsightConnectionString"]))
            {
                services.AddApplicationInsightsTelemetry(options =>
                {
                    options.ConnectionString = _configuration["OfferApplicationInsightConnectionString"];
                });
            }

            HealthCheckExtension.ConfigureHealthCheckService(services, _configuration);
            RegisterServicesExtension.RegisterServices(services);
            RegisterInfrastructureModule.Register(services, _configuration);
            SwaggerExtension.ConfigureSwaggerService(services, _swaggerConfiguration);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                IdentityModelEventSource.ShowPII = true;
            }
            else
            {
                app.UseHsts();
            }

            app.UseExceptionMiddleware();
            app.UseRouting();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCors("CorsPolicy");
            app.UseSwagger(option =>
            {
                option.RouteTemplate = "/api/{documentName}/api.json";
            });
            app.UseSwaggerUI(c =>
            {
                c.EnableTryItOutByDefault();
                c.SwaggerEndpoint("/api/v1/api.json", "Account V1");
                c.RoutePrefix = "api";
            });

            SwaggerExtension.UseSwagger(app, _swaggerConfiguration);
            HealthCheckExtension.UseHealthcheckUI(app);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static void RegisterAuthenticationAndAuthorization(IServiceCollection services, IConfiguration configuration)
        {
            var identityServiceOptions = new ConstellationIdentityServiceAuthenticationOptions
            {
                AzureActiveDirectoryClientCredentials =
                        {
                            ClientId = configuration["AuthClientId"],
                            ClientSecret = configuration["AuthClientSecret"],
                            Scope = configuration["AuthScope"],
                            Tenant = configuration["AuthTenant"],
                        },
                ServerAddress = new Uri(configuration["AuthServerAdress"] ?? string.Empty),
            };

            //services.AddAuthentication()
            //.AddConstellationIdentityService(identityServiceOptions, out string[] schemaNames);

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddAuthenticationSchemes([])
                    .Build();
            });
            services.AddConstellationHttpClient();
        }
    }
}
