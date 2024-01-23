// <copyright file="RegisterServicesExtension.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Kpmg.Account.Core.Services;
using Pulse.Account.Core.Interfaces;

namespace Kpmg.Account.API.Configuration
{

    [ExcludeFromCodeCoverage]
    public static class RegisterServicesExtension
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IAccountService, AccountService>();
        }

        public static void Register(IServiceCollection services, IConfiguration configuration)
        {
            RegisterDatabase(services, configuration);
        }

        private static void RegisterDatabase(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration["SqlAccountConnectionString"];
            if (connectionString == null)
            {
                throw new InvalidOperationException(nameof(connectionString));
            }

            services.AddHealthChecks()
                .AddSqlServer(connectionString, healthQuery: "SELECT 1;");
        }
    }
}
