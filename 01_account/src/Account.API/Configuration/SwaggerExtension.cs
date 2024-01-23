// <copyright file="SwaggerExtension.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Microsoft.OpenApi.Models;
using Pulse.Account.API.Configuration.Model;

namespace Kpmg.Account.API.Configuration
{
    public static class SwaggerExtension
    {
        public static void ConfigureSwaggerService(this IServiceCollection services, SwaggerModel? swaggerConfiguration)
        {
            services.AddSwaggerGen(swaggerGenOptions =>
            {
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

                swaggerGenOptions.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme.",
                    Type = SecuritySchemeType.Http, // We set the scheme type to http since we're using bearer authentication
                    Scheme = "bearer", // The name of the HTTP Authorization scheme to be used in the Authorization header. In this case "bearer".
                });
                swaggerGenOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme,
                        },
                    },
                    new List<string>()
                },
                });
            });
        }

        public static void UseSwagger(IApplicationBuilder app, SwaggerModel? swaggerConfiguration)
        {
            app.UseSwagger();
            app.UseSwaggerUI(swaggerUiOptions =>
            {
                swaggerUiOptions.SwaggerEndpoint(string.Format(swaggerConfiguration?.JsonEndpoint ?? string.Empty, swaggerConfiguration?.Version), swaggerConfiguration?.Title);
            });
        }
    }
}
