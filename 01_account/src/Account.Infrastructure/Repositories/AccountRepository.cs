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
using Pulse.Account.Infrastructure.Extensions;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Infrastructure.Utils;
using Pulse.ExceptionMiddleware.Exceptions;
using AccountModel = Pulse.Account.Core.Models.Account;

namespace Pulse.Account.Infrastructure.Repositories;

public class AccountRepository(AccountContext accountContext) : IAccountRepository
{
    private static readonly string[] ContactWidgetRoleLabelCodes =
    [
        RoleLabelCodes.AccountManager,
        RoleLabelCodes.CustomerLeadPartner
    ];

    private static readonly Expression<Func<AccountRolePair, string?>> SignatoryNameKey =
        x => x.Account.RoleEntity
            .Where(r => r.IsSignatory == true)
            .Select(r => r.Contact.FirstName + r.Contact.LastName)
            .FirstOrDefault();

    private static readonly Expression<Func<AccountRolePair, string?>> SignatoryEmailKey =
        x => x.Account.RoleEntity
            .Where(r => r.IsSignatory == true)
            .Select(r => r.Contact.Email)
            .FirstOrDefault();

    private readonly AccountContext _accountContext = accountContext;
    private static readonly AsyncRetryPolicy _retryPolicy = Policy
                .Handle<SqlException>()
                .WaitAndRetryAsync(
                    retryCount: 1,
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(GlobalConstants.RETRYTIMESPAN));

    public async Task<AccountDetail> CreateAccountAsync(string currentUser, CreateAccountRequest request)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var accountGlobalUniqueId = Guid.NewGuid();

            int? nafId = null;
            if (!string.IsNullOrWhiteSpace(request.NafCode))
            {
                var naf = await _accountContext.NafEntity.AsNoTracking()
                    .FirstOrDefaultAsync(n => n.NafCode == request.NafCode);

                if (naf == null)
                {
                    throw new NotFoundException(Errors.NotFoundNafCode, string.Format(Errors.NotFoundNafMessage, request.NafCode));
                }

                nafId = naf.NafId;
            }

            var accountEntity = new AccountEntity
            {
                AccountGlobalUniqueId = accountGlobalUniqueId,
                AccountNumber = request.AccountNumber,
                LegalName = request.LegalName,
                Siret = request.Siret,
                AccountType = request.AccountType.ToString(),
                LegalForm = request.LegalForm,
                NafId = nafId,
                CreatedBy = currentUser,
                CreationDate = DateTime.UtcNow,
                IsActive = true,
                DeploymentEntity = new DeploymentEntity
                {
                    Status = (int)DeploymentStatus.ToDeploy,
                    DeploymentDate = DateTime.UtcNow,
                }
            };

            if (request.Address != null)
            {
                accountEntity.AddressEntity.Add(new AddressEntity
                {
                    AddressLine1 = request.Address.Street,
                    City = request.Address.City,
                    Country = request.Address.Country,
                    State = request.Address.Department,
                    ZipCode = request.Address.ZipCode,
                    AddressType = AddressType.Delivery.ToString(),
                });
            }

            _accountContext.AccountEntity.Add(accountEntity);

            await _accountContext.SaveChangesAsync();

            return accountEntity.MapToAccountDetail()!;
        });
    }

    public async Task<Paging<AccountModel>> GetAccountsAsync(SearchAccountCriteria criteria, Pagination pagination, bool sortByLastActivity = true)
    {
        var baseQuery = _accountContext.AccountEntity.AsNoTracking()
            .Join(_accountContext.RoleEntity, a => a.AccountId, r => r.AccountId, (a, r) => new AccountRolePair { Account = a, Role = r })
            .Where(x => x.Role.ContactId == criteria.ContactId)
            .ApplyFavorite(criteria.IsFavoriteFilter)
            .ApplyCustomerRelation(criteria.IsCustomerRelationFilter)
            .ApplyLastActivityRange(criteria.LastActivityDateFrom, criteria.LastActivityDateTo)
            .ApplySearch(criteria.Search)
            .ApplyDeploymentStatus(criteria.DeploymentStatus)
            .ApplyMissionType(criteria.MissionType);

        // Calcul du nombre total d'éléments (après filtres) pour la pagination
        var totalItems = await baseQuery.CountAsync();
        var totalPages = Paginator.GetTotalPages(totalItems, pagination.PageSize);

        var pageAccountIds = await GetAccountRolePairsSorted(baseQuery, criteria.Sorting, sortByLastActivity)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(x => x.Account.AccountId)
            .ToListAsync();

        var entities = await _accountContext.AccountEntity.AsNoTracking()
            .Where(a => pageAccountIds.Contains(a.AccountId))
            .Include(a => a.RoleEntity)
                .ThenInclude(r => r.Contact)
            .Include(a => a.AddressEntity)
            .Include(a => a.DeploymentEntity)
            .ToListAsync();

        var orderedEntities = pageAccountIds
            .Select(accountId => entities.First(e => e.AccountId == accountId))
            .ToList();

        return MapAccountDatabaseToAccountModel.MapToPaginAccounts(
        orderedEntities,
        criteria.ContactId,
        pagination.PageNumber,
        totalItems,
        totalPages
    );
    }

    private static IQueryable<AccountRolePair> GetAccountRolePairsSorted(IQueryable<AccountRolePair> query, Sorting? sorting, bool sortByLastActivity)
    {
        var sorted = sorting == null || string.IsNullOrEmpty(sorting.Field)
            ? SortByDefault(query, sortByLastActivity)
            : sorting.Field switch
            {
                SortingConstants.COMPANYNAME => query.OrderByDirection(x => x.Account.LegalName, sorting.Descending),
                SortingConstants.CUSTOMERCODE => query.OrderByDirection(x => x.Account.AccountNumber, sorting.Descending),
                SortingConstants.LEADER => query.OrderByDirection(SignatoryNameKey, sorting.Descending),
                SortingConstants.EMAIL => query.OrderByDirection(SignatoryEmailKey, sorting.Descending),
                SortingConstants.CITY => query.OrderByDirection(x => x.Account.AddressEntity.Select(a => a.City).FirstOrDefault(), sorting.Descending),
                SortingConstants.STATUS => query.OrderByDirection(x => x.Account.DeploymentEntity.Status, sorting.Descending),
                SortingConstants.LASTACTIVITYDATE => query
                    .OrderByDirection(x => x.Role.LastActivityDate, sorting.Descending)
                    .ThenBy(x => x.Account.LegalName),
                SortingConstants.ISFAVORITE => sortByLastActivity
                    ? query.OrderByDirection(x => x.Role.IsFavorite == true, sorting.Descending)
                        .ThenByDescending(x => x.Role.LastActivityDate)
                        .ThenBy(x => x.Account.LegalName)
                    : query.OrderByDirection(x => x.Role.IsFavorite == true, sorting.Descending)
                        .ThenBy(x => x.Account.LegalName),
                _ => throw new BadRequestException(
                    Errors.BadRequestContactsAccountCode,
                    string.Format(Errors.BadRequestContactsAccountMessage, sorting.Field)),
            };

        return sorted.ThenBy(x => x.Account.AccountId);
    }

    private static IOrderedQueryable<AccountRolePair> SortByDefault(IQueryable<AccountRolePair> query, bool sortByLastActivity) =>
        sortByLastActivity
            ? query
                .OrderByDescending(x => x.Role.LastActivityDate)
                .ThenBy(x => x.Account.LegalName)
            : query.OrderBy(x => x.Account.LegalName);

    private static IQueryable<AccountEntity> GetAccountEntitiesSorted(IQueryable<AccountEntity> query, Sorting? sorting)
    {
        if (sorting == null || string.IsNullOrEmpty(sorting.Field))
        {
            return query.OrderBy(x => x.LegalName);
        }

        return sorting.Field switch
        {
            SortingConstants.COMPANYNAME => query.OrderByDirection(x => x.LegalName, sorting.Descending),
            SortingConstants.CUSTOMERCODE => query.OrderByDirection(x => x.AccountNumber, sorting.Descending),
            SortingConstants.LEADER => query.OrderByDirection(
                x => x.RoleEntity
                    .Where(r => r.IsSignatory == true)
                    .Select(r => r.Contact.FirstName + r.Contact.LastName)
                    .FirstOrDefault(),
                sorting.Descending),
            SortingConstants.EMAIL => query.OrderByDirection(
                x => x.RoleEntity
                    .Where(r => r.IsSignatory == true)
                    .Select(r => r.Contact.Email)
                    .FirstOrDefault(),
                sorting.Descending),
            SortingConstants.CITY => query.OrderByDirection(x => x.AddressEntity.Select(a => a.City).FirstOrDefault(), sorting.Descending),
            SortingConstants.STATUS => query.OrderByDirection(x => x.DeploymentEntity.Status, sorting.Descending),
            SortingConstants.LASTACTIVITYDATE => query
                .OrderByDirection(x => x.RoleEntity.Select(r => r.LastActivityDate).FirstOrDefault(), sorting.Descending)
                .ThenBy(x => x.LegalName),
            _ => throw new BadRequestException(
                Errors.BadRequestContactsAccountCode,
                string.Format(Errors.BadRequestContactsAccountMessage, sorting.Field)),
        };
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

            return MapAccountDatabaseToAccountModel.MapToPaginAccounts(
                await query.ToListAsync(),
                null,
                pagination!.PageNumber,
                totalItems,
                totalPages);
        });
    }

    public async Task<Paging<AccountSearchResult>> SearchAccountsAsync(string? keyword, Pagination pagination)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var baseQuery = _accountContext.AccountEntity.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var trimmedKeyword = keyword.Trim();
                baseQuery = baseQuery.Where(a =>
                    a.LegalName.Contains(trimmedKeyword) ||
                    a.AccountNumber.Contains(trimmedKeyword));
            }

            int totalItems = await baseQuery.CountAsync();
            int totalPages = Paginator.GetTotalPages(totalItems, pagination.PageSize);

            var results = await baseQuery
                .OrderBy(a => a.LegalName)
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(a => new AccountSearchResult
                {
                    AccountId = a.AccountId,
                    LegalName = a.LegalName,
                    AccountNumber = a.AccountNumber,
                    Siret = a.Siret,
                    DirectorEmail = a.RoleEntity
                        .Where(r => r.IsSignatory == true)
                        .Select(r => r.Contact.Email)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return results.MapToPagingAccountSearchResult(
                pagination.PageNumber,
                totalItems,
                totalPages);
        });
    }

    public async Task<AccountModel?> GetAccountSummaryAsync(int contactId, int accountId)
    {
        AccountEntity? account = await _retryPolicy.ExecuteAsync(async () =>
        {
            return await _accountContext.ActiveAccounts
                    .AsNoTracking()
                    .Include(a => a.DeploymentEntity)
                    .Include(a => a.OfferEligibilityEntity)
                    .Include(a => a.RoleEntity)
                    .Include(x => x.Office)
                    .ThenInclude(x => x.Address)
                    .FirstOrDefaultAsync(a => a.AccountId == accountId);
        });

        if (account == null)
        {
            throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, accountId));
        }

        return account.MapToAccountSummary(contactId);
    }

    public async Task<AccountDetail> GetAccountAsync(int accountId)
    {
        AccountEntity? account = await _retryPolicy.ExecuteAsync(async () =>
        {
            return await _accountContext.ActiveAccounts
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
            return await _accountContext.ActiveAccounts
                    .AsNoTracking()
                    .Include(x => x.RoleEntity)
                    .ThenInclude(r => r.Contact)
                    .Include(a => a.AddressEntity)
                    .Include(x => x.DeploymentEntity)
                    .Include(x => x.OfferEligibilityEntity)
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

    public async Task<AccountDetail> UpdateAccountAsync(int accountId, AccountDetail accountDetail, bool includeProspects = false)
    {
        AccountDetail? toReturn = null!;
        await _retryPolicy.ExecuteAsync(async () =>
        {
            IQueryable<AccountEntity> accounts = includeProspects ? _accountContext.ActiveAccounts : _accountContext.AccountEntity;
            var existingAccount = await accounts.Include(a => a.DeploymentEntity).FirstOrDefaultAsync(x => x.AccountId == accountId);
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

    public async Task<Paging<Contact>> GetContactsAccountAsync(int accountId, SearchContactsAccountCriteria criteria, Pagination pagination, bool includeProspects = false)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            IQueryable<ContactEntity> query = GetContactEntitiesByAccountId(accountId, criteria.IsCustomerRelation, includeProspects);

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

            query = GetContactEntitiesSorted(query, criteria.Sorting, criteria.Type == ContactType.Collaborator, accountId);

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

    /// <inheritdoc/>
    public async Task<bool> IsContactProspectOnlyAsync(int contactId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var contactExists = await _accountContext.ContactEntity
                .AsNoTracking()
                .AnyAsync(contact => contact.ContactId == contactId);

            if (!contactExists)
            {
                throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId));
            }

            var rolesForContact = _accountContext.RoleEntity
                .AsNoTracking()
                .Where(role => role.ContactId == contactId);

            var hasAnyRole = await rolesForContact.AnyAsync();
            if (!hasAnyRole)
            {
                return false;
            }

            var hasNonProspectRole = await (
                from role in rolesForContact
                join account in _accountContext.AccountEntity.IgnoreQueryFilters().AsNoTracking()
                    on role.AccountId equals account.AccountId
                where account.AccountType != GlobalConstants.ProspectAccountType
                select role)
                .AnyAsync();

            return !hasNonProspectRole;
        });
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Contact>> GetAccountContactWidgetContactsAsync(int accountId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var contacts = await _accountContext.ContactEntity.AsNoTracking()
                .Include(c => c.RoleEntity)
                .Include(c => c.RoleLabelEntityContact)
                    .ThenInclude(roleLabel => roleLabel.Label)
                .Where(contact =>
                    contact.Type == ContactType.Collaborator.ToString()
                    && contact.RoleEntity.Any(role => role.AccountId == accountId)
                    && contact.RoleLabelEntityContact.Any(roleLabel =>
                        roleLabel.AccountId == accountId
                        && ContactWidgetRoleLabelCodes.Contains(roleLabel.Label.Code)))
                .OrderBy(contact => contact.LastName)
                .ThenBy(contact => contact.FirstName)
                .ToListAsync();

            return contacts.Select(contact => contact.MapToContact(accountId)!);
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

    private IQueryable<ContactEntity> GetContactEntitiesByAccountId(int accountId, bool? isCustomerRelation, bool includeProspects = false)
    {
        IQueryable<AccountEntity> accountSet = includeProspects
            ? _accountContext.ActiveAccounts
            : _accountContext.AccountEntity;

        var contactQuery = includeProspects
            ? _accountContext.ContactEntity.IgnoreQueryFilters().Where(c => c.IsActive).AsNoTracking()
            : _accountContext.ContactEntity.AsNoTracking();

        return contactQuery
                    .Include(c => c.RoleEntity)
                    .Include(c => c.RoleLabelEntityContact)
                    .ThenInclude(r => r.Label)
                    .Where(c => c.RoleEntity
                                    .Any(r => r.AccountId == accountId &&
                                            accountSet.Any(a => a.AccountId == r.AccountId) &&
                                            (isCustomerRelation == null || r.IsCustomerRelation == isCustomerRelation)));
    }

    private static IQueryable<ContactEntity> GetContactEntitiesSorted(IQueryable<ContactEntity> query, Sorting? sorting, bool isCollab, int accountId)
    {
        if (sorting != null)
        {
            Expression<Func<ContactEntity, object>> exp = null!;
            bool sortingApplied = false;
            switch (sorting.Field)
            {
                case SortingConstants.NAME:
                    exp = c => c.LastName;
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

                case SortingConstants.LABEL:
                    if (isCollab)
                    {
                        query = sorting.Descending
                            ? query.OrderByDescending(x => x.RoleLabelEntityContact.Any(rl => rl.AccountId == accountId))
                            : query.OrderBy(x => x.RoleLabelEntityContact.Any(rl => rl.AccountId == accountId));
                    }
                    else
                    {
                        query = sorting.Descending
                            ? query.OrderByDescending(x => x.RoleEntity.Where(r => r.AccountId == accountId).Select(r => r.IsSignatory).FirstOrDefault())
                                   .ThenByDescending(x => x.RoleEntity.Where(r => r.AccountId == accountId).Select(r => r.ContactFlagPortailFactures).FirstOrDefault())
                            : query.OrderBy(x => x.RoleEntity.Where(r => r.AccountId == accountId).Select(r => r.IsSignatory).FirstOrDefault())
                                   .ThenBy(x => x.RoleEntity.Where(r => r.AccountId == accountId).Select(r => r.ContactFlagPortailFactures).FirstOrDefault());
                    }
                    sortingApplied = true;
                    break;

                default:
                    throw new BadRequestException(Errors.BadRequestContactsAccountCode, string.Format(Errors.BadRequestContactsAccountMessage, sorting.Field));
            }

            if (!sortingApplied)
            {
                if (sorting.Descending)
                {
                    query = query.OrderByDescending(exp);
                }
                else
                {
                    query = query.OrderBy(exp);
                }
            }
        }
        else
        {
            if (isCollab)
            {
                query = query.OrderByDescending(x => x.RoleEntity.Where(x => x.AccountId == accountId).Select(r => r.ActionLevel).FirstOrDefault())
                .ThenBy(cnt => cnt.LastName);
            }
            else
            {
                query = query.OrderBy(x => x.LastName);
            }
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

    public async Task<AccountDetail> GetAccountProspectIncludedAsync(int accountId)
    {
        AccountEntity? account = await _retryPolicy.ExecuteAsync(async () =>
        {
            return await _accountContext.ActiveAccounts
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
}
