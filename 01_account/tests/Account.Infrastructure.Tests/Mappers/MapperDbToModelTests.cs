// <copyright file="MapperDbToModelTests.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>
using Kpmg.Offer.Infrastructure.Entities;
using Kpmg.Offer.Infrastructure.Mappers;
using Moq;

namespace Kpmg.Offer.Infrastructure.Tests.Mappers
{
    public class MapperDbToModelTests
    {
        [Fact]
        public void Should_MapOffer()
        {
            var expectedFirstProduct = new TProductConfiguration
            {
                TProduct = new TProduct
                {
                    TProvider = new TProvider
                    {
                        ProviderName = "MEG",
                        ProviderCategory = "PULSE",
                        ProviderEditor = "RCA",
                        LogoName = "MEG-LOGO",
                    },
                    ProductName = "Accès MEG",
                    ProductLabel = "Accès à MEG",
                    Enabled = true,
                },
                IsOptionnal = true,
                PriceType = "exact",
                Price = 99.99m
            };

            var expctedSecondProduct = new TProductConfiguration
            {
                Date = new DateTime(2023, 10, 12),
                Enabled = true,
                OfferId = 125,
                ProductConfigurationId = 122500145,
                ProductId = 1,
                TProduct = new TProduct
                {
                    TProvider = new TProvider
                    {
                        ProviderName = "MEC",
                        ProviderCategory = "PULSE",
                        ProviderEditor = "RDA",
                        LogoName = "MEC-LOGO",
                    },
                    ProductName = "GED Compta",
                    ProductLabel = "La GED Compta",
                    Enabled = true,
                },
                IsOptionnal = false,
                PriceType = "exact",
                Price = 99.99m
            };

            var expected = new TOffer
            {
                OfferId = 1,
                OfferName = "Smart Meg",
                GlobalOfferId = new Guid("21975301-921E-4989-909D-C019747E4281"),
                Tags = "Mon tag",
                OfferLabel = "Smart Meg Premium",
                OfferDescription = "Offre MEG",
                OfferPriceType = "exact",
                OfferPrice = 99.99m,
                OffersBookLink = "TestBookLink",
                IntranetLink = "TestIntranetLink",
                ApprovalRequired = true,
                LogoName = "MEG-LOGO",
                TProductConfiguration = new List<TProductConfiguration>
                {
                    expectedFirstProduct,
                    expctedSecondProduct
                }
            };

            // Act
            var result = MapperDbToModel.MapOffer(expected);

            // Assert
            Assert.Equal(expected.OfferId, result.OfferId);
            Assert.Equal(expected.GlobalOfferId, result.GlobalOfferId);
            Assert.Equal(expected.OfferName, result.OfferName);
            Assert.Equal(expected.Tags, result.Tags);
            Assert.Equal(expected.OfferLabel, result.OfferLabel);
            Assert.Equal(expected.OfferDescription, result.OfferDescription);
            Assert.Equal(expected.OfferPriceType, result.OfferPriceType);
            Assert.Equal(expected.OfferPrice, result.OfferPrice);
            Assert.Equal(expected.OffersBookLink, result.OffersBookLink);
            Assert.Equal(expected.IntranetLink, result.IntranetLink);
            Assert.Equal(expected.ApprovalRequired, result.ApprovalRequired);
            Assert.Equal(expected.LogoName, result.LogoName);

            Assert.Equal(2, result.Providers.Count());

            var resultProvider = result.Providers.FirstOrDefault();
            var expectedProduct = expectedFirstProduct.TProduct;

            Assert.Equal(expectedProduct.TProvider.ProviderName, resultProvider.ProviderName);
            Assert.Equal(expectedProduct.TProvider.ProviderCategory, resultProvider.ProviderCategory);
            Assert.Equal(expectedProduct.TProvider.ProviderEditor, resultProvider.ProviderEditor);
            Assert.Equal(expectedProduct.TProvider.LogoName, resultProvider.LogoName);

            var resultProduct = resultProvider.Products.FirstOrDefault();

            Assert.Equal(expectedFirstProduct.IsOptionnal, resultProduct.IsOptionnal);
            Assert.Equal(expectedFirstProduct.PriceType, resultProduct.PriceType);
            Assert.Equal(expectedFirstProduct.Price, resultProduct.Price);
            Assert.Equal(expectedFirstProduct.Price, resultProduct.Price);
            Assert.Equal(expectedProduct.ProductName, resultProduct.ProductName);
            Assert.Equal(expectedProduct.ProductLabel, resultProduct.ProductLabel);
            Assert.Equal(expectedProduct.Enabled, resultProduct.Enabled);
        }

        [Fact]
        public void Should_MapOffers()
        {
            var firstOffer = new TOffer
            {
                OfferId = 1,
                OfferName = "Smart Meg",
                GlobalOfferId = new Guid("21975301-921E-4989-909D-C019747E4281"),
                Tags = "Mon Tag",
                OfferLabel = "Smart Meg Premium",
                OfferDescription = "Offre MEG",
                OfferPriceType = "exact",
                OfferPrice = 99.99m,
                OffersBookLink = "TestBookLink",
                IntranetLink = "TestIntranetLink",
                ApprovalRequired = true,
                LogoName = "MEG-LOGO",
                TProductConfiguration = new List<TProductConfiguration>()
                    {
                        new TProductConfiguration(),
                    }
            };
            var secondOffer = new TOffer
            {
                OfferId = 2,
                OfferName = "Smart Meg Premium",
                GlobalOfferId = new Guid("21975301-921E-4989-909D-C019747E4251"),
                Tags = "Mon Tag 2",
                OfferLabel = "Smart Meg Premium 2",
                OfferDescription = "Offre MEG 2",
                OfferPriceType = "exact",
                OfferPrice = 99.99m,
                OffersBookLink = "TestBookLink2",
                IntranetLink = "TestIntranetLink2",
                ApprovalRequired = true,
                LogoName = "MEG-LOGO",
                TProductConfiguration = new List<TProductConfiguration>()
                    {
                        new TProductConfiguration(),
                    }
            };

            var expected = new List<TOffer>
            {
                firstOffer,
                secondOffer
            };

            // Act
            var result = MapperDbToModel.MapOffers(expected);

            // Assert
            Assert.Equal(2, result.Count);
            var resultFirst = result.FirstOrDefault();
            Assert.Equal(firstOffer.OfferId, resultFirst.OfferId);
            Assert.Equal(firstOffer.OfferName, resultFirst.OfferName);
            Assert.Equal(firstOffer.OfferLabel, resultFirst.OfferLabel);
            Assert.Equal(firstOffer.OfferPrice, resultFirst.OfferPrice);
            Assert.Equal(firstOffer.OfferPriceType, resultFirst.OfferPriceType);
            Assert.Equal(firstOffer.LogoName, resultFirst.LogoName);
            Assert.Null(resultFirst.GlobalOfferId);
            Assert.Null(resultFirst.Tags);
            Assert.Null(resultFirst.OffersBookLink);
            Assert.Null(resultFirst.OfferDescription);
            Assert.Null(resultFirst.IntranetLink);
            Assert.Null(resultFirst.ApprovalRequired);
            Assert.Null(resultFirst.Providers);
        }
    }
}
