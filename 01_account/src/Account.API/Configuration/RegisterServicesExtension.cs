// <copyright file="RegisterServicesExtension.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Kpmg.Offer.Core.Interfaces;
using Kpmg.Offer.Core.Services;
using Kpmg.Offer.Infrastructure.Repositories;
using Pulse.Account.Core.Interfaces;

namespace Kpmg.Offer.API.Configuration
{
    public static class RegisterServicesExtension
    {
        public static void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IOfferRepository, OfferRepository>();
        }
    }
}
