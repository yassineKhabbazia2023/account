// <copyright file="RegisterServicesExtension.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Kpmg.Account.Core.Services;
using Pulse.Account.Core.Interfaces;

namespace Kpmg.Account.API.Configuration
{
    public static class RegisterServicesExtension
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IAccountService, AccountService>();
        }
    }
}
