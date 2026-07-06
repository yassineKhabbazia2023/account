// <copyright file="StatisticsRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Repositories;

public class StatisticsRepository(AccountContext accountContext) : IStatisticsRepository
{
    private readonly AccountContext _accountContext = accountContext;
    private static readonly AsyncRetryPolicy _retryPolicy = Policy
                .Handle<SqlException>()
                .WaitAndRetryAsync(
                    retryCount: 1,
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(GlobalConstants.RETRYTIMESPAN));

    public async Task<Statistics> GetStatisticsAsync(int contactId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            var accountStats = await GetAccountStatistics(contactId);
            var contactStats = await GetContactStatistics(contactId);

            return MapAccountDatabaseToAccountModel.MapToStatistics(accountStats!, contactStats!);
        });
    }

    public async Task<double> GetAccountPercentageCustomerRelationAsync()
    {
        var totalAccounts = await _accountContext.AccountEntity.AsNoTracking()
            .Select(a => a.AccountId)
            .Distinct()
            .CountAsync();

        var accountsWithRelationClient = await _accountContext.RoleEntity.AsNoTracking()
            .Where(r => r.IsCustomerRelation == true && r.Contact.Type.Equals(ContactType.Collaborator.ToString()))
            .Select(r => r.AccountId)
            .Distinct()
            .CountAsync();

        var percentage = totalAccounts == 0 ? 0 : Math.Round((double)accountsWithRelationClient / totalAccounts * 100, 2);

        return percentage;
    }

    private async Task<Dictionary<int, int>?> GetAccountStatistics(int contactId)
    {
        var entities = from deployment in _accountContext.DeploymentEntity
                       join role in _accountContext.RoleEntity on deployment.AccountId equals role.AccountId
                       join account in _accountContext.AccountEntity on deployment.AccountId equals account.AccountId
                       where role.ContactId == contactId
                       group deployment by deployment.Status into statuses
                       select new { Status = statuses.Key, Count = statuses.Count() };

        return await entities.ToDictionaryAsync(x => x.Status, x => x.Count);
    }

    private async Task<Dictionary<string, int>?> GetContactStatistics(int contactId)
    {
        var accountIds =
            (from role in _accountContext.RoleEntity
             join account in _accountContext.AccountEntity on role.AccountId equals account.AccountId
             where role.ContactId == contactId
             select role.AccountId)
            .Distinct();

        var contactIds = accountIds
            .Join(_accountContext.RoleEntity,
                ids => ids,
                role => role.AccountId,
                (ids, role) => role)
            .Select(r => r.ContactId).Distinct();

        var contactStatusStatistics = contactIds
            .Join(_accountContext.ContactEntity,
                ids => ids,
                contact => contact.ContactId,
                (role, contact) => contact)
            .Where(contact => contact.Type == ContactType.Customer.ToString() && !string.IsNullOrEmpty(contact.Status))
            .GroupBy(x => x.Status)
            .Select(s => new { Status = s.Key, Count = s.Select(d => d.Status).Count() });

        return await contactStatusStatistics.ToDictionaryAsync(x => x.Status, x => x.Count);
    }

    public async Task<IEnumerable<(int AccountCount, int ClientCount)>> GetAccountsPerClientCountAsync()
    {
        var data = await (from r in _accountContext.RoleEntity
                          join c in _accountContext.ContactEntity on r.ContactId equals c.ContactId
                          where c.Type == ContactType.Customer.ToString()
                          select new { r.ContactId, r.AccountId })
                 .ToListAsync();

        var result = data
            .GroupBy(x => x.ContactId)
            .Select(g => new
            {
                ContactId = g.Key,
                AccountCount = g.Select(x => x.AccountId).Distinct().Count()
            })
            .GroupBy(x => x.AccountCount)
            .Select(g => (AccountCount: g.Key, ClientCount: g.Count()))
            .OrderBy(x => x.AccountCount)
            .ToList();

        return result;
    }

    public async Task<EntityCountByType> GetEntityCountByTypeAsync(int contactId)
    {
        var accountIds = _accountContext.RoleEntity
            .AsNoTracking()
            .Where(r => r.ContactId == contactId)
            .Select(r => r.AccountId)
            .Distinct();

        var regularCount = await _accountContext.ActiveAccounts
            .AsNoTracking()
            .Where(a => accountIds.Contains(a.AccountId))
            .CountAsync(a => a.AccountType == AccountType.CLIENT.ToString());

        return new EntityCountByType
        {
            RegularEntitiesCount = regularCount,
        };
    }

    public async Task<IEnumerable<(int ClientCount, int AccountCount)>> GetClientsPerAccountCountAsync()
    {
        var data = await (from r in _accountContext.RoleEntity
                          join c in _accountContext.ContactEntity on r.ContactId equals c.ContactId
                          where c.Type == ContactType.Customer.ToString()
                          select new { r.AccountId, r.ContactId })
                 .ToListAsync();

        var result = data
            .GroupBy(x => x.AccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                ClientCount = g.Select(x => x.ContactId).Distinct().Count()
            })
            .GroupBy(x => x.ClientCount)
            .Select(g => (ClientCount: g.Key, AccountCount: g.Count()))
            .OrderBy(x => x.ClientCount)
            .ToList();

        return result;
    }
}
