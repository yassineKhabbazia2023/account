// <copyright file="MapDelegationDbToDelegationModel.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Mappers;

public static class MapDelegationDbToDelegationModel
{
    public static IReadOnlyCollection<Delegation> ToDelegations(this ICollection<DelegationEntity> source)
    {
        return source?.Select(d => d.ToDelegation() !).ToList() ?? new List<Delegation>();
    }

    public static Delegation? ToDelegation(this DelegationEntity source)
    {
        return source == null ? null :
            new Delegation
            {
                DelegationId = source.DelegationId,
                CreationDate = source.CreationDate,
                StartDate = source.StartDate,
                EndDate = source.EndDate,
                Status = source.Status,
                Note = source.Note,
                Account = source.Account?.ToAccount(),
                Delegatee = source.Delegatee?.ToContact(),
                Delegator = source.Delegator?.ToContact(),
            };
    }

    public static Core.Models.Account? ToAccount(this AccountEntity source)
    {
        return source == null ? null :
             new Core.Models.Account
             {
                 AccountId = source.AccountId,
                 AccountNumber = source.AccountNumber,
                 LegalName = source.LegalName,
                 Address = source.AddressEntity.ToAddress(),
             };
    }

    public static Contact? ToContact(this ContactEntity source)
    {
        return source == null ? null :
            new Contact
            {
                ContactId = source.ContactId,
                GlobalContactId = source.ContactGlobalUniqueId,
                Email = source.Email,
                FirstName = source.FirstName,
                LastName = source.LastName,
                Type = source.Type,
                Status = source.Status,
                PersonaName = source.PersonaName,
                CreationDate = source.CreationDate,
            };
    }

    public static ICollection<Address> ToAddress(this ICollection<AddressEntity> source)
    {
        return source?.Select(d => d.ToAddress() !).ToList() ?? new List<Address>();
    }

    private static Address? ToAddress(this AddressEntity source)
    {
        return source == null ? null :
            new Address
            {
                AddressId = source.AddressId,
                AddressLine1 = source.AddressLine1,
                ZipCode = source.ZipCode,
                City = source.City,
                State = source.State,
                Country = source.Country,
                AddressType = source.AddressType,
            };
    }
}
