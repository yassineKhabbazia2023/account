// <copyright file="OfferRepositoryTests.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using AutoFixture;
using Kpmg.Offer.Infrastructure.Context;
using Kpmg.Offer.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Kpmg.Offer.Infrastructure.Tests.Repositories
{
    public class OfferRepositoryTests
    {
        private readonly DbContextOptions<OfferContext> _options;
        private readonly Fixture _fixture;

        public OfferRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<OfferContext>()
                                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                                .Options;
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task Should_GetOffersAsync_Return_Offers()
        {
            using (var context = new OfferContext(_options))
            {
                var expected = _fixture.CreateMany<Entities.TOffer>(2).ToList();
                var repository = new OfferRepository(context);

                context.TOffer.AddRange(expected);
                await context.SaveChangesAsync();

                var result = await repository.GetOffersAsync();

                Assert.NotNull(result);
                Assert.Equal(2, result.Count);

                for (int i = 0; i < expected.Count; i++)
                {
                    var tOffer = expected.ElementAt(i);
                    var offer = result.ElementAt(i);

                    Assert.Equal(tOffer.OfferId, offer.OfferId);
                    Assert.Equal(tOffer.OfferName, offer.OfferName);
                    Assert.Equal(tOffer.OfferLabel, offer.OfferLabel);
                    Assert.Equal(tOffer.OfferPrice, offer.OfferPrice);
                    Assert.Equal(tOffer.LogoName, offer.LogoName);
                }
            }
        }

        [Fact]
        public async Task Should_GetOfferById_Return_Offer()
        {
            using (var context = new OfferContext(_options))
            {
                var expected = _fixture.Create<Entities.TOffer>();
                var repository = new OfferRepository(context);

                context.TOffer.AddRange(expected);
                await context.SaveChangesAsync();

                var result = await repository.GetOfferByIdAsync(expected.OfferId);

                Assert.NotNull(result);
                Assert.NotNull(result.Providers);
                Assert.NotEmpty(result.Providers);

                foreach (var provider in result.Providers)
                {
                    Assert.NotNull(provider.Products);
                }
            }
        }
    }
}
