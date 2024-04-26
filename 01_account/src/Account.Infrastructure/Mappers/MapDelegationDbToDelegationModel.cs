// <copyright file="MapDelegationDbToDelegationModel.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
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
                Accounts = source.Account.ToAccounts(),
                Delegatee = source.Delegatee?.ToContact(),
                Delegator = source.Delegator?.ToContact(),
                IsFullDelegation = source.IsFullDelegation,
                IsAutomaticDelegation = source.IsAutomaticDelegation
            };
    }

    public static IEnumerable<Core.Models.Account> ToAccounts(this IEnumerable<AccountEntity> source)
    {
        return source?.Select(a => a.ToAccount()) ?? Enumerable.Empty<Core.Models.Account>();
    }

    public static Core.Models.Account? ToAccount(this AccountEntity source)
    {
        return source == null ? null :
             new Core.Models.Account
             {
                 AccountId = source.AccountId,
                 AccountNumber = source.AccountNumber,
                 LegalName = source.LegalName,
             };
    }

    public static Contact? ToContact(this ContactEntity source)
    {
        return source == null ? null :
            new Contact
            {
                ContactId = source.ContactId,
                ContactGlobalUniqueId = source.ContactGlobalUniqueId,
                Email = source.Email,
                FirstName = source.FirstName,
                LastName = source.LastName,
            };
    }

    public static Paging<Delegation> MapToPagingDelegations(this ICollection<DelegationEntity> source,
        int pageNumber,
        int totalRows,
        int totalPageCalcul)
    {
        return new Paging<Delegation>
        {
            Items = source.ToDelegations(),
            CurrentPage = pageNumber,
            TotalItems = totalRows,
            TotalPage = totalPageCalcul
        };
    }
}
