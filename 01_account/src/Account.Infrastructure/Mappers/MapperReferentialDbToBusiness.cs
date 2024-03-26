// <copyright file="MapperReferentialDbToBusiness.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Mappers
{
    public static class MapperReferentialDbToBusiness
    {
        public static IEnumerable<Hub?> MapHubEntitiesToHubs(this IEnumerable<HubEntity> source)
        {
            return source?.Select(s => s.MapHubEntityToHub()) ?? Enumerable.Empty<Hub>();
        }

        public static Hub? MapHubEntityToHub(this HubEntity source)
        {
            return source == null ? null :
                new Hub
                {
                    HubId = source.HubId,
                    HubName = source.HubName,
                };
        }

        public static Paging<Naf> MapToPagingNaf(this IEnumerable<NafEntity?> source, int pageNumber, int totalRows, int totalPageCalcul)
        {
            return new Paging<Naf>
            {
                Items = source?.MapNafEntitiesToNafs() ?? Enumerable.Empty<Naf>(),
                CurrentPage = pageNumber,
                TotalItems = totalRows,
                TotalPage = totalPageCalcul
            };
        }

        public static IEnumerable<Naf> MapNafEntitiesToNafs(this IEnumerable<NafEntity?> source)
        {
            return source?.Select(s => s.MapNafEntityToNaf() !) ?? Enumerable.Empty<Naf>();
        }

        public static Naf? MapNafEntityToNaf(this NafEntity source)
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
