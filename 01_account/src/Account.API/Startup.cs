// <copyright file="Startup.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pulse.Account.API.Configuration;
using Pulse.Account.API.Configuration.Model;
using Pulse.Back.ExceptionMiddleware;
using Pulse.ExceptionMiddleware;

namespace Pulse.Account.API
{
    [ExcludeFromCodeCoverage]
    public class Startup
    {
        private const int MaxAgeConfHsts = 365;
        private readonly IConfiguration? _configuration;
        private readonly SwaggerModel? _swaggerConfiguration;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            if (configuration != null)
            {
                _configuration = configuration;
                _swaggerConfiguration = _configuration.GetSection("Swagger").Get<SwaggerModel>();
            }
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(_configuration);
            services.AddMemoryCache();
            services.AddHsts(options =>
            {
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(MaxAgeConfHsts);
            });
            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            })
            .AddControllersAsServices()
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver { NamingStrategy = new CamelCaseNamingStrategy() };
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                options.SerializerSettings.DateParseHandling = DateParseHandling.None;
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            });

            services.RegisterCors();
            services.ConfigureSwaggerService(_swaggerConfiguration);
            services.RegisterApplicationInsights(_configuration);
            services.RegisterServices();
            services.RegisterBrokerServices(_configuration);
            services.RegisterDatabase(_configuration!);
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();

                endpoints.MapGet("/", async context =>
                {
                    context.Response.ContentType = "text/html";
                    await context.Response.SendFileAsync(Path.Combine(env.WebRootPath, "index.html"));
                });
            });
        }
    }
}
