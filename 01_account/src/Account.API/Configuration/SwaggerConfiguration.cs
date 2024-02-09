// <copyright file="SwaggerConfiguration.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Reflection;
using Microsoft.OpenApi.Models;
using Pulse.Account.API.Configuration.Model;

namespace Pulse.Account.API.Configuration
{
    public static class SwaggerConfiguration
    {
        public static void ConfigureSwaggerService(this IServiceCollection services, SwaggerModel? swaggerConfiguration)
        {
            services.AddSwaggerGen(swaggerGenOptions =>
            {
                swaggerGenOptions.AddServer(new OpenApiServer()
                {
                    Url = "/",
                });
                swaggerGenOptions.AddServer(new OpenApiServer()
                {
                    Url = "/account",
                });
                swaggerGenOptions.SwaggerDoc(swaggerConfiguration?.Version, new OpenApiInfo
                {
                    Title = swaggerConfiguration?.Title,
                    Version = swaggerConfiguration?.Version,
                    Description = swaggerConfiguration?.Description,
                    TermsOfService = swaggerConfiguration?.TermsOfService,
                    Contact = new OpenApiContact
                    {
                        Name = swaggerConfiguration?.ContactName,
                        Email = swaggerConfiguration?.ContactEmail,
                    },
                    License = new OpenApiLicense
                    {
                        Name = swaggerConfiguration?.LicenseName,
                    },
                });

                var xmlFile = $"{Assembly.GetAssembly(typeof(Program)) !.GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                swaggerGenOptions.IncludeXmlComments(xmlPath);
            });
        }

        public static void UseSwagger(IApplicationBuilder app, SwaggerModel? swaggerConfiguration)
        {
            app.UseSwagger(option =>
            {
                option.RouteTemplate = swaggerConfiguration?.RouteTemplate;
            });
            app.UseSwaggerUI(swaggerUiOptions =>
            {
                swaggerUiOptions.SwaggerEndpoint(swaggerConfiguration?.JsonEndpoint, swaggerConfiguration?.Title);
                swaggerUiOptions.RoutePrefix = swaggerConfiguration?.UiEndpoint;
                swaggerUiOptions.EnableTryItOutByDefault();
            });
        }
    }
}
