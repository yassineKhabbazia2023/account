// <copyright file="ModelsBuilder.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using AutoFixture;

namespace Kpmg.Offer.Core.Tests
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

        public static IReadOnlyCollection<Models.Offer> GenerateOffers()
        {
            var fixture = ConfigurationAutoFixture();
            var pennylaneOffer = fixture.Create<Models.Offer>();
            var otherOffer = fixture.Create<Models.Offer>();

            return new List<Models.Offer>() { pennylaneOffer, otherOffer };
        }
    }
}
