// <copyright file="OfferService.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Kpmg.Offer.Core.Interfaces;
using Kpmg.Offer.Core.Services;

namespace Kpmg.Offer.Core.Services
{
    public class OfferService : IOfferService
    {
        private readonly IOfferRepository _repository;

        public OfferService(IOfferRepository repository)
        {
            _repository = repository;
        }

        public async Task<IReadOnlyCollection<Models.Offer>> GetOffersAsync()
        {
            return await _repository.GetOffersAsync();
        }

        public async Task<Models.Offer> GetOfferByIdAsync(int offerId)
        {
            return await _repository.GetOfferByIdAsync(offerId);
        }
    }
}
