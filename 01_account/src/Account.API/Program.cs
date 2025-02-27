// <copyright file="Program.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Serilog;

namespace Pulse.Account.API
{
    [ExcludeFromCodeCoverage]
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                     .ConfigureLogging((context, loggerConfiguration) =>
                     {
                         loggerConfiguration.AddApplicationInsights();
                     })
                  .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>()
                            .UseDefaultServiceProvider(options => options.ValidateScopes = false);
                    });
        }
    }
}
