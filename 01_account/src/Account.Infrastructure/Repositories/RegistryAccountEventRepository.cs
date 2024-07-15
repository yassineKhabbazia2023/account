// <copyright file="RegistryAccountEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Extensions;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Repositories
{
    public class RegistryAccountEventRepository : IRegistryAccountEventRepository
    {
        private readonly AccountContext _context;

        public RegistryAccountEventRepository(AccountContext context)
        {
            _context = context;
            _context.HandleEFCoreFailure();
        }

        public async Task<AccountDetail> CreateAccountAsync(RegistryAccountCreatedEventData eventData)
        {
            var et = eventData.ToAccountEntity();

            _context.AccountEntity.Add(et);
            await _context.SaveChangesAsync();
            return et.MapToAccountDetail() !;
        }

        public async Task<int> RemoveAccountAsync(Guid accountGlobalUniqueIdentifier)
        {
            var accountToRemove = _context.AccountEntity.Include(a => a.DeploymentEntity).FirstOrDefault(a => a.AccountGlobalUniqueId == accountGlobalUniqueIdentifier);
            if (accountToRemove == null)
            {
                throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, accountGlobalUniqueIdentifier));
            }

            var deploymentEntity = accountToRemove.DeploymentEntity?.FirstOrDefault();

            if (deploymentEntity == null)
            {
                accountToRemove.DeploymentEntity = new List<DeploymentEntity>
                {
                    new DeploymentEntity
                    {
                        DeploymentDate = DateTime.UtcNow,
                        Status = (int)DeploymentStatus.Revoked,
                    }
                };
            }
            else
            {
                deploymentEntity.Status = (int)DeploymentStatus.Revoked;
                deploymentEntity.DeploymentDate = DateTime.UtcNow;
            }

            _context.AccountEntity.Update(accountToRemove);
            await _context.SaveChangesAsync();
            return accountToRemove.AccountId;
        }

        public async Task<AccountDetail> UpdateAccountAsync(RegistryAccountUpdatedEventData eventData)
        {
            var existingAccount = await _context.AccountEntity
                .Include(a => a.DeploymentEntity)
                .Include(a => a.AddressEntity)
                .Include(a => a.PhoneEntity)
                .FirstOrDefaultAsync(a => a.AccountGlobalUniqueId == eventData.AccountGlobalUniqueIdentifier);

            if (existingAccount == null)
            {
                throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, eventData.AccountGlobalUniqueIdentifier));
            }

            var newAccount = eventData.ToAccountEntity();
            existingAccount.ToAccountEntity(newAccount);
            await _context.SaveChangesAsync();

            return existingAccount.MapToAccountDetail() !;
        }
    }
}
