// <copyright file="MapDelegationBusinessToDelegationDb.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Mappers
{
    public static class MapDelegationBusinessToDelegationDb
    {
        public static IEnumerable<DelegationEntity> MapDelegationRequestToDelegationsDb(this CreateDelegationRequest delegation, IEnumerable<AccountEntity> accounts)
        {
            if (delegation?.DelegationDetails?.Any() != true)
            {
                return Enumerable.Empty<DelegationEntity>();
            }

            var delegationEntities = new List<DelegationEntity>();

            foreach (var detail in delegation.DelegationDetails)
            {
                var delegationEntity = new DelegationEntity
                {
                    CreationDate = DateTime.UtcNow,
                    DelegatorId = delegation.DelegatorId,
                    DelegateeId = detail.DelegateeId,
                    StartDate = detail.StartDate!.Value,
                    EndDate = detail.EndDate,
                    Status = detail.Status,
                    Note = detail.Note,
                    Account = accounts.ToList(),
                };

                delegationEntities.Add(delegationEntity);
            }

            return delegationEntities;
        }
    }
}
