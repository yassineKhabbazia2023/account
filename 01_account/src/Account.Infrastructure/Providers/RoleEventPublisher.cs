// <copyright file="RoleEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents.EventsData;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Interfaces;
using Pulse.Account.Infrastructure.Context;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Pulse.Account.Infrastructure.Providers
{
    public class RoleEventPublisher : IRoleEventPublisher
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IContactRepository _contactRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IServiceScopeFactory _serviceScope;

        public RoleEventPublisher(IEventPublisher eventPublisher, IContactRepository contactRepository, IAccountRepository accountRepository, IServiceScopeFactory serviceScope)
        {
            _eventPublisher = eventPublisher;
            _contactRepository = contactRepository;
            _accountRepository = accountRepository;
            _serviceScope = serviceScope;
        }

        public async Task PublishRoleCreatedEventAsync(CreateRoleRequest roleRequest)
        {
                if (roleRequest is null)
                {
                    return;
                }

                var scope = _serviceScope.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AccountContext>();

                var selectedAccount = await context.AccountEntity.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.AccountId == roleRequest.AccountId);

                var selectedContact = await context.ContactEntity.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ContactId == roleRequest.ContactId);

                ArgumentNullException.ThrowIfNull(selectedAccount, nameof(selectedAccount));
                ArgumentNullException.ThrowIfNull(selectedContact,nameof(selectedContact));

                roleRequest.AccountGlobalUniqueId = roleRequest.AccountGlobalUniqueId ??
                   selectedAccount!.AccountGlobalUniqueId;

                roleRequest.ContactGlobalUniqueId = roleRequest.ContactGlobalUniqueId ??
                    selectedContact!.ContactGlobalUniqueId;

                var data = new RoleCreatedEventData
                {
                    AccountId = roleRequest.AccountId,
                    ContactId = roleRequest.ContactId,
                    IsDelegation = roleRequest.IsDelegation,
                    IsFavorite = roleRequest.IsFavorite,
                    IsSignatory = roleRequest.IsSignatory,
                    AccountGlobalUniqueId = (Guid)roleRequest.AccountGlobalUniqueId!,
                    ContactGlobalUniqueId = (Guid)roleRequest.ContactGlobalUniqueId!,
                    DelegatorContactId = roleRequest.DelegatorId,
                    AccountNumber = selectedAccount!.AccountNumber,
                    ContactEmail = selectedContact!.Email
                };

                await _eventPublisher.PublishAsync(new RoleCreatedEvent(data));
        }

        public async Task PublishRoleUpdatedEventAsync(int accountId, int contactId, bool isSignatory)
        {
            var data = new RoleUpdatedEventData
            {
                AccountId = accountId,
                ContactId = contactId,
                IsSignatory = isSignatory,
            };
            await _eventPublisher.PublishAsync(new RoleUpdatedEvent(data));
        }

        public async Task PublishRoleDeletedEventAsync(int accountId, int contactId)
        {
            var contact = await _contactRepository.GetContactAsync(contactId);

            var account = await _accountRepository.GetAccountAsync(accountId);

            var roleDataReg = await GetEmailAndAccountNumber(accountId, contactId);

            var data = new RoleDeletedEventData
            {
                AccountId = accountId,
                ContactId = contactId,
                AccountGlobalUniqueId = account!.AccountGlobalUniqueId,
                ContactGlobalUniqueId = contact!.ContactGlobalUniqueId,
                AccountNumber = roleDataReg.AccountNumber,
                ContactEmail = roleDataReg.ContactEmail
            };

            await _eventPublisher.PublishAsync(new RoleDeletedEvent(data));
        }

        private async Task<(string AccountNumber, string ContactEmail)> GetEmailAndAccountNumber(int accountId, int contactId)
        {
            var accountNumber = (await _accountRepository.GetAccountAsync(accountId))?.AccountNumber ?? string.Empty;
            var contactEmail = (await _contactRepository.GetContactAsync(contactId))?.Email ?? string.Empty;

            return (accountNumber, contactEmail);
        }
    }
}
