// <copyright file="RegisterInfrastructureModule.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Kpmg.Account.Core.Services;
using Pulse.Account.Core.Interfaces;

namespace Pulse.Account.API.Configuration
{
    [ExcludeFromCodeCoverage]
    public static class ServicesConfiguration
    {
        public static void ServiceRegister(IServiceCollection services, IConfiguration configuration, string environmentName)
        {
            RegisterServices(services);
            if (!environmentName.Equals("test"))
            {
                RegisterDatabase(services, configuration);
            }
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IAccountService, AccountService>();
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
