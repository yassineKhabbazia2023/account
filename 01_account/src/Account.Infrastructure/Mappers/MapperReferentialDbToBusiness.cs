// <copyright file="MapperReferentialDbToBusiness.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Mappers
{
    public static class MapperReferentialDbToBusiness
    {
        public static IEnumerable<Hub?> MapHubEntitiesToHubs(this IEnumerable<THub> source)
        {
            return source?.Select(s => s.MapHubEntityToHub()) ?? Enumerable.Empty<Hub>();
        }

        public static Hub? MapHubEntityToHub(this THub source)
        {
            return source == null ? null :
                new Hub
                {
                    HubId = source.HubId,
                    HubName = source.HubName,
                };
        }

        public static IEnumerable<Naf?> MapNafEntitiesToNafs(this IEnumerable<TNaf> source)
        {
            return source?.Select(s => s.MapNafEntityToNaf()) ?? Enumerable.Empty<Naf>();
        }

        public static Naf? MapNafEntityToNaf(this TNaf source)
        {
            return (source == null) ? null :
                new Naf
                {
                    NafId = source.NafId,
                    NafCode = source.NafCode,
                    NafLabel = source.NafLabel,
                };
        }
    }
}
