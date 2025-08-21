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
using Pulse.Account.Infrastructure.Entities;
using Pulse.ExceptionMiddleware.Exceptions;
using Pulse.Account.Core.Exceptions;

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

            if (selectedAccount == default(AccountEntity))
            {
                throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, roleRequest.AccountId));
            }

            if (selectedContact == default(ContactEntity))
            {
                throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, roleRequest.ContactId));
            }

            roleRequest.AccountGlobalUniqueId = roleRequest.AccountGlobalUniqueId ??
               selectedAccount!.AccountGlobalUniqueId;

            roleRequest.ContactGlobalUniqueId = roleRequest.ContactGlobalUniqueId ??
                selectedContact!.ContactGlobalUniqueId;

            var data = new RoleCreatedEventData
            {
                AccountId = roleRequest.AccountId,
                ContactId = (int)roleRequest.ContactId!,
                IsDelegation = roleRequest.IsDelegation,
                IsFavorite = roleRequest.IsFavorite,
                IsSignatory = roleRequest.IsSignatory,
                AccountGlobalUniqueId = (Guid)roleRequest.AccountGlobalUniqueId!,
                ContactGlobalUniqueId = (Guid)roleRequest.ContactGlobalUniqueId!,
                DelegatorContactId = roleRequest.DelegatorId,
                AccountNumber = selectedAccount!.AccountNumber,
                ContactEmail = selectedContact!.Email,
                IsCustomerRelation = roleRequest.IsCustomerRelation,
                IncludePennylaneAccess = roleRequest.IncludePennylaneAccess,
            };

            await _eventPublisher.PublishAsync(new RoleCreatedEvent(data));
        }

        public async Task PublishRoleFavoriteStatusChangedEventAsync(int accountId, int contactId, bool isFavorite)
        {
            var data = new RoleUpdatedEventData
            {
                AccountId = accountId,
                ContactId = contactId,
                IsFavorite = isFavorite,
            };
            await _eventPublisher.PublishAsync(new RoleUpdatedEvent(data));
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
            using var scope = _serviceScope.CreateScope();
            var contactRepository = scope.ServiceProvider.GetRequiredService<IContactRepository>();
            var accountRepository = scope.ServiceProvider.GetRequiredService<IAccountRepository>();

            var contact = await contactRepository.GetContactAsync(contactId, true);
            var account = await accountRepository.GetAccountAsync(accountId);

            var data = new RoleDeletedEventData
            {
                AccountId = accountId,
                ContactId = contactId,
                AccountGlobalUniqueId = account!.AccountGlobalUniqueId,
                ContactGlobalUniqueId = contact!.ContactGlobalUniqueId,
                AccountNumber = account.AccountNumber ?? string.Empty,
                ContactEmail = contact.Email ?? string.Empty,
            };

            await _eventPublisher.PublishAsync(new RoleDeletedEvent(data));
        }
    }
}
