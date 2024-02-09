// <copyright file="DelegationMapper.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Mappers;

public static class DelegationMapper
{
    public static IReadOnlyCollection<Delegation> ToDelegations(this ICollection<TDelegation> source)
    {
        return source?.Select(d => d.ToDelegation() !).ToList() ?? new List<Delegation>();
    }

    public static Delegation? ToDelegation(this TDelegation source)
    {
        return source == null ? null :
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

    public static Core.Models.Account? ToAccount(this TAccount source)
    {
        return source == null ? null :
             new Core.Models.Account
             {
                 AccountId = source.AccountId,
                 AccountNumber = source.AccountNumber,
                 LegalName = source.LegalName,
                 Address = source.TAddress.ToAddress(),
             };
    }

    public static Contact? ToContact(this TContact source)
    {
        return source == null ? null :
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

    public static IReadOnlyCollection<Address> ToAddress(this ICollection<TAddress> source)
    {
        return source?.Select(d => d.ToAddress() !).ToList() ?? new List<Address>();
    }

    private static Address? ToAddress(this TAddress source)
    {
        return source == null ? null :
            new Address
            {
                AddressId = source.AddressId,
                Street = source.Street,
                ZipCode = source.ZipCode,
                City = source.City,
                State = source.State,
                Country = source.Country,
                AddressType = source.AddressType,
            };
    }
}
