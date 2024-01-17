// <copyright file="IOfferService.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

namespace Kpmg.Offer.Core.Interfaces
{
    public interface IOfferService
    {
        Task<IReadOnlyCollection<Models.Offer>> GetOffersAsync();

        Task<Models.Offer> GetOfferByIdAsync(int offerId);
    }
}
