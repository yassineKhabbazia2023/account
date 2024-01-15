// <copyright file="OfferRepository.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Kpmg.Offer.Core.Exceptions;
using Kpmg.Offer.Core.Interfaces;
using Kpmg.Offer.Infrastructure.Context;
using Kpmg.Offer.Infrastructure.Entities;
using Kpmg.Offer.Infrastructure.Mappers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;

namespace Kpmg.Offer.Infrastructure.Repositories
{
    public class OfferRepository : IOfferRepository
    {
        private readonly OfferContext _offerContext;
        private readonly AsyncRetryPolicy _retryPolicy;

        public OfferRepository(OfferContext context)
        {
            _offerContext = context;

            _retryPolicy = Policy.Handle<SqlException>()
                .WaitAndRetryAsync(
                    retryCount: 1,
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(3000));
        }

        public async Task<IReadOnlyCollection<Core.Models.Offer>> GetOffersAsync()
        {
            List<TOffer> offers = new();

            await _retryPolicy.ExecuteAsync(async () =>
            {
                offers = await _offerContext.TOffer.AsNoTracking().ToListAsync();
            });

            return MapperDbToModel.MapOffers(offers);
        }

        public async Task<Core.Models.Offer> GetOfferByIdAsync(int offerId)
        {
            var offer = new TOffer();

            await _retryPolicy.ExecuteAsync(async () =>
            {
                offer = await _offerContext.TOffer.AsNoTracking()
                    .Include(offr => offr.TProductConfiguration)
                    .ThenInclude(conf => conf.TProduct)
                    .ThenInclude(prod => prod.TProvider)
                    .Where(x => offerId.Equals(x.OfferId))
                    .FirstAsync();
            });

            if (offer == null)
            {
                throw new NotFoundException(Errors.NotFoundOfferCode, Errors.NotFoundOfferMessage);
            }

            return MapperDbToModel.MapOffer(offer);
        }
    }
}
