// <copyright file="Program.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Serilog;
using Serilog.Enrichers.Span;

namespace Pulse.Account.API
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                Log.Information("Starting Pulse.Back.Account microservice");
                CreateHostBuilder(args).Build().Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
                throw;
            }
            finally
            {
                Log.Information("Shutting down Pulse.Back.Account microservice");
                Log.CloseAndFlush();
            }
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                     .UseSerilog((context, services, configuration) =>
                     {
                         configuration
                             .ReadFrom.Configuration(context.Configuration)
                             .ReadFrom.Services(services)
                             .Enrich.FromLogContext()
                             .Enrich.WithSpan()
                             .Enrich.WithProperty("Application", "Pulse.Back.Account")
                             .Enrich.WithProperty("Layer", "WebApi")
                             .Enrich.WithProperty("Domain", "account");
                     }, writeToProviders: true)
                  .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>()
                            .UseDefaultServiceProvider(options => options.ValidateScopes = false);
                    });
        }
    }
}
