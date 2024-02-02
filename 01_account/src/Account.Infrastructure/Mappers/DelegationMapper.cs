// <copyright file="DelegationMapper.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Mappers;

public static class DelegationMapper
{
    public static IReadOnlyCollection<Delegation> ToDelegationList(this ICollection<TDelegation> source)
    {
        return source?.Select(d => d.ToDelegation()).ToList() ?? new List<Delegation>();
    }

    public static Delegation ToDelegation(this TDelegation source)
    {
        return
            new Delegation
            {
                DelegationId = source.DelegationId,
                CreationDate = source.CreationDate,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                Status = (DelegationStatus)source.Status,
                Note = source.Note,
                Account = source.Account?.ToAccount(),
                Delegatee = source.Delegatee?.ToContact(),
                Delegator = source.Delegator?.ToContact(),
            };
    }

    public static Pulse.Account.Core.Models.Account ToAccount(this TAccount source)
    {
        return
            new Pulse.Account.Core.Models.Account
            {
                AccountId = source.AccountId,
                AccountNumber = source.AccountNumber,
                LegalName = source.LegalName,
                Address = new Address
                {
                    City = source.TAddress.FirstOrDefault()?.City,
                },
            };
    }

    public static Contact ToContact(this TContact source)
    {
        return
            new Contact
            {
                ContactId = source.ContactId,
                GlobalContactId = source.ContactGlobalUniqueId,
                ContactEmail = source.ContactEmail,
                FirstName = source.FirstName,
                LastName = source.LastName,
                Type = source.Type,
            };
    }
}
