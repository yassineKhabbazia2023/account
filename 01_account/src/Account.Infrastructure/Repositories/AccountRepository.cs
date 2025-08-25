// <copyright file="AccountRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Data;
using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.ExceptionMiddleware.Exceptions;
using AccountModel = Pulse.Account.Core.Models.Account;

namespace Pulse.Account.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly AccountContext _accountContext;
    private readonly AsyncRetryPolicy _retryPolicy;

    public AccountRepository(AccountContext accountContext)
    {
        _accountContext = accountContext;

        _retryPolicy = Policy
                .Handle<SqlException>()
                .WaitAndRetryAsync(
                    retryCount: 1,
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(GlobalConstants.RETRYTIMESPAN));
    }

    public async Task<Paging<AccountModel>> GetAccountsAsync(SearchAccountCriteria criteria, Pagination pagination)
    {
        // Construire la requête de base, en AsNoTracking, avec un premier filtre sur ContactId
        var baseQuery = from a in _accountContext.AccountEntity.AsNoTracking()
                        join r in _accountContext.RoleEntity on a.AccountId equals r.AccountId
                        where r.ContactId == criteria.ContactId &&
                        (criteria.IsFavoriteFilter != true || r.IsFavorite == true) &&
                        (criteria.IsCustomerRelationFilter != true || r.IsCustomerRelation == true)
                        select a;

        // Filtrer par "Search"
        if (!string.IsNullOrWhiteSpace(criteria.Search))
        {
            var search = criteria.Search.Trim();

            // Filtrer par LegalName, AccountNumber, ou par un rôle signataire dont le Contact correspond à la recherche
            baseQuery = baseQuery.Where(a =>
                a.LegalName.Contains(search) ||
                a.AccountNumber.Contains(search) ||
                a.RoleEntity.Any(r =>
                    r.IsSignatory == true &&
                    ((r.Contact.FirstName + " " + r.Contact.LastName).Contains(search)
                        || r.Contact.Email.Contains(search))));
        }

        // Filtrer par DeploymentStatus
        if (criteria.DeploymentStatus.HasValue)
        {
            baseQuery = baseQuery.Where(a => a.DeploymentEntity.Status == criteria.DeploymentStatus.Value);
        }

        // Calcul du nombre total d'éléments (après filtres) pour la pagination
        var totalItems = await baseQuery.CountAsync();
        var totalPages = Paginator.GetTotalPages(totalItems, pagination.PageSize);

        // Sort result
        var query = GetAccountEntitiesSorted(baseQuery, criteria.Sorting);

        // Appliquer le Skip et le Take avant les Includes
        query = query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize);

        // Ajouter les Includes nécessaires pour de meilleures performances
        query = query
            .Include(a => a.RoleEntity)
                .ThenInclude(r => r.Contact)
            .Include(a => a.AddressEntity)
            .Include(a => a.DeploymentEntity);

        // Exécuter la requête et mapper les entités
        var entities = await query.ToListAsync();

        return MapAccountDbToAccountModel.MapToPaginAccounts(
            entities,
            criteria.ContactId,
            pagination.PageNumber,
            totalItems,
            totalPages
        );
    }

    private static IQueryable<AccountEntity> GetAccountEntitiesSorted(IQueryable<AccountEntity> query, Sorting? sorting)
    {
        if (sorting == null || string.IsNullOrEmpty(sorting.Field))
        {
            return query.OrderBy(x => x.LegalName);
        }

        switch (sorting.Field)
        {
            case SortingConstants.COMPANYNAME:
                query = sorting.Descending
                    ? query.OrderByDescending(x => x.LegalName)
                    : query.OrderBy(x => x.LegalName);
                break;

            case SortingConstants.CUSTOMERCODE:
                query = sorting.Descending
                    ? query.OrderByDescending(x => x.AccountNumber)
                    : query.OrderBy(x => x.AccountNumber);
                break;

            case SortingConstants.LEADER:
                query = sorting.Descending
                    ? query.OrderByDescending(x => x.RoleEntity
                        .Where(r => r.IsSignatory == true)
                        .Select(r => r.Contact.FirstName + r.Contact.LastName)
                        .FirstOrDefault())
                    : query.OrderBy(x => x.RoleEntity
                        .Where(r => r.IsSignatory == true)
                        .Select(r => r.Contact.FirstName + r.Contact.LastName)
                        .FirstOrDefault());
                break;

            case SortingConstants.EMAIL:
                query = sorting.Descending
                    ? query.OrderByDescending(x => x.RoleEntity
                        .Where(r => r.IsSignatory == true)
                        .Select(r => r.Contact.Email)
                        .FirstOrDefault())
                    : query.OrderBy(x => x.RoleEntity
                        .Where(r => r.IsSignatory == true)
                        .Select(r => r.Contact.Email)
                        .FirstOrDefault());
                break;

            case SortingConstants.CITY:
                query = sorting.Descending
                    ? query.OrderByDescending(x => x.AddressEntity
                        .Select(a => a.City)
                        .FirstOrDefault())
                    : query.OrderBy(x => x.AddressEntity
                        .Select(a => a.City)
                        .FirstOrDefault());
                break;

            case SortingConstants.STATUS:
                query = sorting.Descending
                    ? query.OrderByDescending(x => x.DeploymentEntity.Status)
                    : query.OrderBy(x => x.DeploymentEntity.Status);
                break;

            default:
                throw new BadRequestException(
                    Errors.BadRequestContactsAccountCode,
                    string.Format(Errors.BadRequestContactsAccountMessage, sorting.Field));
        }

        return query;
    }

    public async Task<Paging<AccountModel>> GetAllAccountsAsync(string? accountNumber, Pagination pagination, SearchAccountCriteria criteria)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            IQueryable<AccountEntity> query = GetAccountQueryByContactId(null);

            if (!string.IsNullOrWhiteSpace(accountNumber))
            {
                query = from n in query
                        where n.AccountNumber.Contains(accountNumber)
                        select n;
            }

            query = GetAccountEntitiesSorted(query, criteria.Sorting);

            var totalItems = await query.CountAsync();
            var totalPages = Paginator.GetTotalPages(totalItems, pagination!.PageSize);

            query = query.Skip((pagination!.PageNumber - 1) * pagination!.PageSize);
            query = query.Take(pagination!.PageSize);

            return MapAccountDbToAccountModel.MapToPaginAccounts(
                await query.ToListAsync(),
                null,
                pagination!.PageNumber,
                totalItems,
                totalPages);
        });
    }

    public async Task<AccountModel?> GetAccountSummaryAsync(int accountId)
    {
        AccountEntity? account = await _retryPolicy.ExecuteAsync(async () =>
        {
            return await _accountContext.AccountEntity
                    .AsNoTracking()
                    .Include(a => a.DeploymentEntity)
                    .Include(x => x.Office)
                    .ThenInclude(x => x.Address)
                    .FirstOrDefaultAsync(a => a.AccountId == accountId);
        });

        if (account == null)
        {
            throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, accountId));
        }

        return account.MapToAccount();
    }

    public async Task<AccountDetail?> GetAccountAsync(int accountId)
    {
        AccountEntity? account = await _retryPolicy.ExecuteAsync(async () =>
        {
            return await _accountContext.AccountEntity
                    .AsNoTracking()
                    .Include(a => a.DeploymentEntity)
                    .Include(h => h.Hub)
                    .FirstOrDefaultAsync(a => a.AccountId == accountId);
        });

        if (account == null)
        {
            throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, accountId));
        }

        return account.MapToAccountDetail();
    }

    public async Task<AccountDetail?> GetAccountDetailAsync(int accountId)
    {
        AccountEntity? account = await _retryPolicy.ExecuteAsync(async () =>
        {
            return await _accountContext.AccountEntity
                    .AsNoTracking()
                    .Include(x => x.RoleEntity)
                    .ThenInclude(r => r.Contact)
                    .Include(a => a.AddressEntity)
                    .Include(x => x.DeploymentEntity)
                    .Include(x => x.Hub)
                    .Include(x => x.Naf)
                    .Include(x => x.Office)
                    .Include(x => x.Office.Address)
                    .Include(x => x.PhoneEntity)
                    .FirstOrDefaultAsync(a => a.AccountId == accountId);
        });

        if (account == null)
        {
            throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, accountId));
        }

        return account.MapToAccountDetail();
    }

    public async Task<AccountDetail> UpdateAccountAsync(int accountId, AccountDetail accountDetail)
    {
        AccountDetail? toReturn = null!;
        await _retryPolicy.ExecuteAsync(async () =>
        {
            var existingAccount = await _accountContext.AccountEntity.Include(a => a.DeploymentEntity).FirstOrDefaultAsync(x => x.AccountId == accountId);
            if (existingAccount == null)
            {
                throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, accountId));
            }

            existingAccount.MapToUpdatedAccount(accountDetail);
            _accountContext.AccountEntity.Update(existingAccount);
            toReturn = existingAccount.MapToAccountDetail();
            await _accountContext.SaveChangesAsync();
        });

        return toReturn!;
    }

    public async Task<Paging<Contact>> GetContactsAccountAsync(int accountId, SearchContactsAccountCriteria criteria, Pagination pagination)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            IQueryable<ContactEntity> query = GetContactEntitiesByAccountId(accountId, criteria.IsCustomerRelation);

            if (!string.IsNullOrWhiteSpace(criteria.Search))
            {
                query = from n in query
                        where n.Email.Contains(criteria.Search)
                                || n.FirstName.Contains(criteria.Search)
                                || n.LastName.Contains(criteria.Search)
                                || n.PersonaName.Contains(criteria.Search)
                                || ((n.Office != null && n.Office.Contains(criteria.Search))
                                || (!string.IsNullOrWhiteSpace(n.Status) && n.Status.Contains(criteria.Search)))
                        select n;
            }

            if (criteria.Type != null && Enum.IsDefined(typeof(ContactType), criteria.Type))
            {
                query = query.Where(x => x.Type.Equals(criteria.Type.ToString()));
            }

            query = GetContactEntitiesSorted(query, criteria.Sorting);

            if (criteria.Type == ContactType.Collaborator)
            {
                query = query.OrderByDescending(x => x.RoleEntity.Where(x => x.AccountId == accountId).Select(r => r.ActionLevel).FirstOrDefault())
                .ThenBy(cnt => cnt.FirstName);
            }

            var totalItems = await query.CountAsync();

            var totalPages = Paginator.GetTotalPages(totalItems, pagination!.PageSize);

            query = query.Skip((pagination!.PageNumber - 1) * pagination!.PageSize);
            query = query.Take(pagination!.PageSize);

            var result = await query.ToListAsync();

            var contacts = result.Select(c => c.MapToContact(accountId));

            return contacts!.MapToPagingContact(
                pagination!.PageNumber,
                totalItems,
                totalPages);
        });
    }

    public async Task<Paging<Contact>> GetAssociatedContactsAsync(int contactId, GetAssociatedContactsRequest request, Pagination pagination)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            IQueryable<int> accountIds = GetAccountQueryByContactId(contactId).Select(account => account.AccountId);
            if (accountIds?.Any() != true)
            {
                throw new NotFoundException(Errors.NotFoundRoleContactCode, string.Format(Errors.NotFoundRoleContactMessage, contactId));
            }

            var query = _accountContext.RoleEntity
                    .AsNoTracking()
                    .Include(x => x.Contact)
                    .Where(x => accountIds.Contains(x.AccountId)
                    && x.Contact.Type == request.ContactType.ToString());

            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(role => role.Contact.Email.Contains(request.Search!)
                                                            || role.Contact.FirstName.Contains(request.Search)
                                                            || role.Contact.LastName.Contains(request.Search)
                                                            || role.Contact.PersonaName.Contains(request.Search)
                                                            || (role.Contact.Office != null && role.Contact.Office.Contains(request.Search)));
            }

            query = query.GroupBy(x => x.ContactId).Select(g => g.First());
            var totalItems = await query.CountAsync();
            var totalPages = Paginator.GetTotalPages(totalItems, pagination!.PageSize);

            var contacts = (await query
                    .ToListAsync())
                    .MapToContacts();

            var contactsSorted = GetContactAssociateSorted(contacts, request.Sorting).AsQueryable();
            contactsSorted = contactsSorted.Skip((pagination!.PageNumber - 1) * pagination!.PageSize);
            contactsSorted = contactsSorted.Take(pagination!.PageSize);

            return contactsSorted.AsEnumerable().MapToPagingContact(pagination!.PageNumber, totalItems, totalPages);
        });
    }

    private IQueryable<AccountEntity> GetAccountQueryByContactId(int? contactId)
    {
        IQueryable<AccountEntity> query = _accountContext.AccountEntity
                        .Include(x => x.RoleEntity)
                        .ThenInclude(r => r.Contact)
                        .Include(a => a.AddressEntity)
                        .Include(x => x.DeploymentEntity)
                        .Include(x => x.Hub)
                        .AsNoTracking();

        if (contactId != null)
        {
            query = query.Where(a => a.RoleEntity.Any(r => r.ContactId == contactId));
        }

        return query.OrderBy(a => a.LegalName);
    }

    private IQueryable<ContactEntity> GetContactEntitiesByAccountId(int accountId, bool? isCustomerRelation)
    {
        var query = _accountContext.ContactEntity.AsNoTracking()
                    .Include(c => c.RoleEntity)
                    .Include(c => c.RoleLabelEntityContact)
                    .ThenInclude(r => r.Label)
                    .Where(c => c.RoleEntity
                                    .Any(r => r.AccountId == accountId &&
                                            (isCustomerRelation == null || r.IsCustomerRelation == isCustomerRelation)));

        return query;
    }

    private static IQueryable<ContactEntity> GetContactEntitiesSorted(IQueryable<ContactEntity> query, Sorting? sorting)
    {
        if (sorting != null)
        {
            Expression<Func<ContactEntity, object>> exp = null!;
            switch (sorting.Field)
            {
                case SortingConstants.NAME:
                    exp = c => c.FirstName + c.LastName;
                    break;

                case SortingConstants.EMAIL:
                    exp = c => c.Email;
                    break;

                case SortingConstants.PERSONA:
                    exp = c => c.PersonaName;
                    break;

                case SortingConstants.OFFICE:
                    exp = c => c.Office;
                    break;

                case SortingConstants.STATUS:
                    exp = c => c.Status;
                    break;

                case SortingConstants.DATE:
                    exp = c => c.CreationDate;
                    break;

                default:
                    throw new BadRequestException(Errors.BadRequestContactsAccountCode, string.Format(Errors.BadRequestContactsAccountMessage, sorting.Field));
            }

            if (sorting.Descending)
            {
                query = query.OrderByDescending(exp);
            }
            else
            {
                query = query.OrderBy(exp);
            }
        }
        else
        {
            query = query.OrderBy(x => x.FirstName + x.LastName);
        }

        return query;
    }

    private static IEnumerable<Contact> GetContactAssociateSorted(IEnumerable<Contact> query, Sorting? sorting)
    {
        if (sorting != null)
        {
            Expression<Func<Contact, object>> exp = null!;
            switch (sorting.Field)
            {
                case SortingConstants.NAME:
                    exp = c => c.FirstName + c.LastName;
                    break;

                case SortingConstants.EMAIL:
                    exp = c => c.Email;
                    break;

                case SortingConstants.DATE:
                    exp = c => c.CreationDate;
                    break;

                default:
                    throw new BadRequestException(Errors.BadRequestContactsAccountCode, string.Format(Errors.BadRequestContactsAccountMessage, sorting.Field));
            }

            if (sorting.Descending)
            {
                query = query.AsQueryable().OrderByDescending(exp);
            }
            else
            {
                query = query.AsQueryable().OrderBy(exp);
            }
        }
        else
        {
            query = query.OrderBy(x => x.FirstName + x.LastName);
        }

        return query;
    }
}
