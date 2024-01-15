// <copyright file="ModelsBuilder.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using AutoFixture;
using Kpmg.Offer.Infrastructure.Entities;

namespace Kpmg.Offer.Infrastructure.Tests
{
    public static class ModelsBuilder
    {
        public static Fixture ConfigurationAutoFixture()
        {
            var fixture = new Fixture();
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            return fixture;
        }

        public static IList<TOffer> GenerateOffers(int offerCount)
        {
            var fixture = ConfigurationAutoFixture();
            var listOffer = new List<TOffer>();

            for (int i = 0; i < offerCount; i++)
            {
                var offer = fixture.Create<TOffer>();
                listOffer.Add(offer);
            }

            return listOffer;
        }

        public static TProductConfiguration GenerateProductConfiguration()
        {
            var fixture = ConfigurationAutoFixture();

            return fixture.Create<TProductConfiguration>();
        }

        public static TProduct GenerateProduct()
        {
            var fixture = ConfigurationAutoFixture();

            return fixture.Create<TProduct>();
        }

        public static TProvider GenerateProvider()
        {
            var fixture = ConfigurationAutoFixture();

            return fixture.Create<TProvider>();
        }
    }
}
