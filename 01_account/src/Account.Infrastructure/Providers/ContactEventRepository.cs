// <copyright file="ContactEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Data.SqlClient;
using Polly.Retry;
using Polly;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Account.Core.Constants;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;
using Pulse.Account.Core.Enum;

namespace Pulse.Account.Infrastructure.Providers;

public class ContactEventRepository : IContactEventRepository
{
    private readonly AccountContext _accountContext;
    private readonly AsyncRetryPolicy _retryPolicy;

    public ContactEventRepository(AccountContext accountContext)
    {
        _accountContext = accountContext;

        _retryPolicy = Policy
                .Handle<SqlException>()
                .WaitAndRetryAsync(
                    retryCount: 1,
                    sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(GlobalConstants.RETRYTIMESPAN));
    }

    public async Task CreateContactAsync(ContactEntity contactEntity)
    {
        await _accountContext.ContactEntity.AddAsync(contactEntity);
        await _retryPolicy.ExecuteAsync(async () =>
        {
            await _accountContext.SaveChangesAsync();
        });
    }

    public async Task RemoveContactAsync(int contactId)
    {
        var existingContact = await _accountContext.ContactEntity.SingleAsync(x => x.ContactId == contactId);
        existingContact.Status = ContactStatus.Removed.ToString();

        await _retryPolicy.ExecuteAsync(async () =>
        {
            await _accountContext.SaveChangesAsync();
        });
    }

    public async Task UpdateContactAsync(ContactEntity contactEntity)
    {
        var existingContact = await _accountContext.ContactEntity.SingleAsync(x => x.ContactId == contactEntity.ContactId);
        contactEntity.ToContactEntity(existingContact);

        await _retryPolicy.ExecuteAsync(async () =>
        {
            await _accountContext.SaveChangesAsync();
        });
    }

    public async Task<IEnumerable<int>> UpdateAccountStatusByContactAsync(IEnumerable<int> accountIds, int deploymentStatus)
    {
        var accountDeployment = _accountContext.DeploymentEntity.Where(x => accountIds.Contains(x.AccountId));
        foreach (var item in accountDeployment)
        {
            item.Status = deploymentStatus;
        }

        await _retryPolicy.ExecuteAsync(async () =>
        {
            await _accountContext.SaveChangesAsync();
        });

        return accountDeployment.Select(x => x.AccountId);
    }

    public List<AccountEntity> GetAccountBySignatory(int contactId)
    {
        return _accountContext.AccountEntity
                    .AsNoTracking()
                    .Include(x => x.RoleEntity)
                    .ThenInclude(r => r.Contact)
                    .Where(a => a.RoleEntity.Any(r => r.ContactId == contactId && r.IsSignatory == true))
                    .ToList();
    }

    public ContactEntity? GetContactById(int contactId)
    {
        return _accountContext.ContactEntity
                    .AsNoTracking()
                    .FirstOrDefault(c => c.ContactId == contactId);
    }
}
