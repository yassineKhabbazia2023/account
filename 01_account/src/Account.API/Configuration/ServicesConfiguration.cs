// <copyright file="ServicesConfiguration.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using Kpmg.Account.Core.Interfaces;
using Kpmg.Account.Core.Services;
using Kpmg.Account.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Infrastructure;
using Pulse.Account.Infrastructure.Context;

namespace Pulse.Account.API.Configuration
{
    [ExcludeFromCodeCoverage]
    public static class ServicesConfiguration
    {
        public static void ServiceRegister(IServiceCollection services, IConfiguration configuration, string environmentName)
        {
            RegisterServices(services);
            if (environmentName != null && !environmentName.Equals("test"))
            {
                RegisterDatabase(services, configuration);
            }
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.AddTransient<IAccountService, AccountService>();

            services.AddScoped<IAccountRepository, AccountRepository>();
        }

        private static void RegisterDatabase(IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration["SqlAccountConnectionString"];
            if (connectionString == null)
            {
                throw new InvalidOperationException(nameof(connectionString));
            }

            services.AddDbContextPool<AccountContext>(
                options => options.UseSqlServer(connectionString, options =>
                options.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery)));

            services.AddHealthChecks()
                .AddSqlServer(connectionString, healthQuery: "SELECT 1;");
        }
    }
}
