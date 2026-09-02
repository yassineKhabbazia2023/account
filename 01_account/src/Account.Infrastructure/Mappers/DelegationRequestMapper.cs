// <copyright file="DelegationRequestMapper.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Mappers
{
    public static class DelegationRequestMapper
    {
        public static DelegationRequest ToDelegationRequest(this DelegationRequestEntity entity)
        {
            return new DelegationRequest
            {
                DelegationRequestId = entity.DelegationRequestId,
                RequesterId = entity.RequesterId,
                RecipientId = entity.RecipientId,
                AccountId = entity.AccountId,
                CreatedAt = entity.CreatedAt,
                Status = entity.Status,
                RespondedAt = entity.RespondedAt,
                Requester = entity.Requester != null ? new Contact
                {
                    ContactId = entity.Requester.ContactId,
                    FirstName = entity.Requester.FirstName,
                    LastName = entity.Requester.LastName,
                    Email = entity.Requester.Email
                } : null,
                Recipient = entity.Recipient != null ? new Contact
                {
                    ContactId = entity.Recipient.ContactId,
                    FirstName = entity.Recipient.FirstName,
                    LastName = entity.Recipient.LastName,
                    Email = entity.Recipient.Email
                } : null,
                Account = entity.Account != null ? new Core.Models.Account
                {
                    AccountId = entity.Account.AccountId,
                    AccountNumber = entity.Account.AccountNumber,
                    LegalName = entity.Account.LegalName,
                    AccountType = entity.Account.AccountType
                } : null
            };
        }
    }
}

