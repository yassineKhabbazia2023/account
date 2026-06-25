// <copyright file="OmitNavigationCollectionsSpecimenBuilder.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Collections;
using System.Reflection;
using AutoFixture.Kernel;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Tests.Helpers
{
    /// <summary>
    /// Empêche AutoFixture de peupler les collections de navigation EF (virtual ICollection&lt;Entity&gt;).
    /// Sans cela, AutoFixture génère des graphes d'entités avec des clés aléatoires qui finissent
    /// par entrer en collision lors du suivi par le DbContext, provoquant l'erreur intermittente
    /// "another instance with the same key value is already being tracked".
    /// Les collections sont laissées à leur valeur par défaut (liste vide définie par l'entité).
    /// </summary>
    public class OmitNavigationCollectionsSpecimenBuilder : ISpecimenBuilder
    {
        private static readonly string EntitiesNamespace = typeof(AccountEntity).Namespace!;

        public object Create(object request, ISpecimenContext context)
        {
            if (request is PropertyInfo pi
                && pi.PropertyType != typeof(string)
                && pi.PropertyType.IsGenericType
                && typeof(IEnumerable).IsAssignableFrom(pi.PropertyType))
            {
                var itemType = pi.PropertyType.GetGenericArguments()[0];
                if (itemType.Namespace == EntitiesNamespace)
                {
                    return new OmitSpecimen();
                }
            }

            return new NoSpecimen();
        }
    }
}
