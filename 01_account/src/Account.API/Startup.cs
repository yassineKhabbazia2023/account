// <copyright file="Startup.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Kpmg.Account.API.Configuration;
using Kpmg.ExceptionMiddleware;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pulse.Account.API.Configuration;
using Pulse.Account.API.Configuration.Model;

namespace Kpmg.Account.API
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private const int MaxAgeConfHsts = 365;
        private readonly IConfiguration _configuration;
        private readonly SwaggerModel? _swaggerConfiguration;
        private readonly AuthenticationModel _authenticationConfiguration;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            if (_configuration != null)
            {
                _swaggerConfiguration = _configuration.GetSection("Swagger").Get<SwaggerModel>();
                _authenticationConfiguration = _configuration.GetSection("Authentication").Get<AuthenticationModel>();
            }

            EnvironmentName = environment != null ? environment.EnvironmentName : string.Empty;
        }

        public string EnvironmentName { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            HealthCheckConfiguration.ConfigureHealthCheckService(services, _configuration);
            ServicesConfiguration.ServiceRegister(services, _configuration, this.EnvironmentName);
            SwaggerConfiguration.ConfigureSwaggerService(services, _swaggerConfiguration);

            if(!EnvironmentName.Equals("test"))
            {
                services.RegisterAuthenticationAndAuthorization(_authenticationConfiguration)
                   .RegisterSystemAuthenticationProvider(_authenticationConfiguration);
            }

            services.AddMemoryCache();
            services.AddApplicationInsightsTelemetry(_configuration);

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

            if (!string.IsNullOrEmpty(_configuration["AccountApplicationInsightConnectionString"]))
            {
                services.AddApplicationInsightsTelemetry(options =>
                {
                    options.ConnectionString = _configuration["AccountApplicationInsightConnectionString"];
                });
            }
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

            SwaggerConfiguration.UseSwagger(app, _swaggerConfiguration);
            HealthCheckConfiguration.UseHealthcheckUI(app);

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
