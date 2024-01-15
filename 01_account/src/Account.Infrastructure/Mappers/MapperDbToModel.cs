// <copyright file="MapperDbToModel.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Kpmg.Offer.Core.Models;
using Kpmg.Offer.Infrastructure.Entities;
using OfferModel = Kpmg.Offer.Core.Models.Offer;

namespace Kpmg.Offer.Infrastructure.Mappers
{
    public static class MapperDbToModel
    {
        public static IReadOnlyCollection<OfferModel> MapOffers(ICollection<TOffer> source) =>
        source?.Select(item => MapLightOffer(item)).ToList() ?? new List<OfferModel>();

        public static OfferModel MapLightOffer(TOffer source) =>
            source == null
                ? new OfferModel()
                : new OfferModel
                {
                    OfferId = source.OfferId,
                    OfferName = source.OfferName,
                    OfferLabel = source.OfferLabel,
                    OfferPriceType = source.OfferPriceType,
                    OfferPrice = source.OfferPrice,
                    LogoName = source.LogoName,
                };

        public static OfferModel MapOffer(TOffer source)
        {
            if (source == null)
            {
                return new OfferModel();
            }

            var offer = new OfferModel
            {
                OfferId = source.OfferId,
                GlobalOfferId = source.GlobalOfferId,
                OfferName = source.OfferName,
                OfferLabel = source.OfferLabel,
                OfferDescription = source.OfferDescription,
                OfferPriceType = source.OfferPriceType,
                OfferPrice = source.OfferPrice,
                OffersBookLink = source.OffersBookLink,
                IntranetLink = source.IntranetLink,
                ApprovalRequired = source.ApprovalRequired,
                Tags = source.Tags,
                LogoName = source.LogoName,
            };

            if (source.TProductConfiguration != null)
            {
                offer.Providers = source.TProductConfiguration
                    .GroupBy(config => config.TProduct.TProvider.ProviderName)
                    .Select(group => new Provider
                    {
                        ProviderName = group.Key,
                        ProviderCategory = group.First().TProduct.TProvider.ProviderCategory,
                        ProviderEditor = group.First().TProduct.TProvider.ProviderEditor,
                        LogoName = group.First().TProduct.TProvider.LogoName,
                        Products = group.Select(config => new Product
                        {
                            ProductName = config.TProduct.ProductName,
                            ProductLabel = config.TProduct.ProductLabel,
                            Enabled = config.TProduct.Enabled,
                            IsOptionnal = config.IsOptionnal,
                            PriceType = config.PriceType,
                            Price = config.Price,
                            GlobalProductId = config.GlobalProductConfigurationId
                        }).ToList()
                    })
                    .ToList();
            }

            return offer;
        }
    }
}
