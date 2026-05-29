// <copyright file="DelegationRequestEntityMapper.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Mappers
{
    public static class DelegationRequestEntityMapper
    {
        public static DelegationRequestEntity ToEntity(int requesterId, int recipientId, int accountId, string status)
        {
            return new DelegationRequestEntity
            {
                RequesterId = requesterId,
                RecipientId = recipientId,
                AccountId = accountId,
                CreatedAt = DateTime.UtcNow,
                Status = status,
                RespondedAt = null
            };
        }

        public static List<DelegationRequestEntity> ToEntities(int requesterId, int accountId, int[] recipientIds, string status)
        {
            return recipientIds.Select(recipientId => ToEntity(requesterId, recipientId, accountId, status)).ToList();
        }
    }
}

