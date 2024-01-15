// <copyright file="Program.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Serilog;

namespace Kpmg.Offer.API
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>

            Host.CreateDefaultBuilder(args)
                .UseSerilog((context, loggerConfiguration) =>
                {
                    loggerConfiguration.ReadFrom.Configuration(context.Configuration);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .UseDefaultServiceProvider(options => options.ValidateScopes = false);
                });
    }
}
