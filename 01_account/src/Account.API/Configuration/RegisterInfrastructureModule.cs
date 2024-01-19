// <copyright file="RegisterInfrastructureModule.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Kpmg.Account.Core.Services;

namespace Kpmg.Account.API.Configuration
{
    [ExcludeFromCodeCoverage]
    public static class RegisterInfrastructureModule
    {
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
                .AddCheck<AccountHealthCheckService>(nameof(AccountHealthCheckService))
                .AddSqlServer(connectionString, healthQuery: "SELECT 1;");
        }
    }
}
