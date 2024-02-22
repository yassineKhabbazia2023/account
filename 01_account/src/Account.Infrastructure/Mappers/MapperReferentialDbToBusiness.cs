// <copyright file="MapperReferentialDbToBusiness.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Mappers
{
    public static class MapperReferentialDbToBusiness
    {
        public static IEnumerable<Hub?> MapTHubsToHubs(IEnumerable<THub> source)
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
    }
}
