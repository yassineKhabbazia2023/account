// <copyright file="ContactEntityTypeSpecimenBuilder.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Reflection;
using AutoFixture.Kernel;

namespace Pulse.Account.Infrastructure.Tests.Helpers
{
    public class ContactEntityTypeSpecimenBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is PropertyInfo pi && pi.PropertyType == typeof(string) && pi.Name == "Type")
            {
                var types = new[] { "collaborator", "customer" };
                return types[new Random().Next(types.Length)];
            }

            return new NoSpecimen();
        }
    }
}
