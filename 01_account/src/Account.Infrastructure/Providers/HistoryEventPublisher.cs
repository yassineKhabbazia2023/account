// <copyright file="HistoryEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Back.Events.Abstractions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Infrastructure.Interfaces;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Back.Events.IntegrationEvents.EventsData;
using Pulse.Account.Core.Exceptions;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Infrastructure.Providers
{
    public class HistoryEventPublisher : IHistoryEventPublisher
    {
        private readonly IContactRepository _contactRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly IEventPublisher _eventPublisher;
        private const string AddCustomerRegistryCode = "ADDCVREG";
        private const string DeleteContactActionCode = "DELCMANU";

        public HistoryEventPublisher(IContactRepository contactRepository, IAccountRepository accountRepository, IEventPublisher eventPublisher)
        {
            _contactRepository = contactRepository;
            _accountRepository = accountRepository;
            _eventPublisher = eventPublisher;
        }

        public async Task PublishHistoryCreatedEventAsync(string registryApproverEmail, int contactId, int accountId)
        {
            var currentUser = await _contactRepository.GetContactByEmailAsync(registryApproverEmail);
            var currentUserName = currentUser.FirstName + " " + currentUser.LastName;

            var targetUser = await _contactRepository.GetContactAsync(contactId);
            var targetUserName = targetUser.FirstName + " " + targetUser.LastName;

            var account = await _accountRepository.GetAccountAsync(accountId);

            var data = new HistoryCreatedEventData
            {
                CreationDate = DateTime.UtcNow,
                Account = new AccountHistoryEventData
                {
                    AccountId = accountId,
                    AccountNumber = account.AccountNumber!,
                    LegalName = account.Legal.LegalName!,
                },
                Action = new ActionHistoryEventData
                {
                    Code = AddCustomerRegistryCode
                },
                User = new UserHistoryEventData
                {
                    Email = currentUser.Email,
                    Name = currentUserName,
                    UserType = currentUser.Type,
                },
                TargetUser = new UserHistoryEventData
                {
                    Email = targetUser.Email,
                    Name = targetUserName,
                    UserType = targetUser.Type,
                }
            };

            await _eventPublisher.PublishAsync(new HistoryCreatedEvent(data));
        }

        public async Task PublishHistoryCreatedEventAsync(int currentUserId, int contactId, int accountId)
        {
            var currentUser = await _contactRepository.GetContactAsync(currentUserId)
                ?? throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, currentUserId));
            var currentUserName = currentUser.FirstName + " " + currentUser.LastName;
            var contact = await _contactRepository.GetContactAsync(contactId)
                    ?? throw new NotFoundException(Errors.NotFoundContactCode, string.Format(Errors.NotFoundContactMessage, contactId));
            var contactName = contact.FirstName + " " + contact.LastName;
            var account = await _accountRepository.GetAccountAsync(accountId)
                    ?? throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, accountId));
            var historyActivity = new HistoryCreatedEventData
            {
                CreationDate = DateTime.UtcNow,
                Action = new ActionHistoryEventData
                {
                    Code = DeleteContactActionCode,
                },
                Account = new AccountHistoryEventData
                {
                    AccountId = accountId,
                    AccountNumber = account.AccountNumber,
                    LegalName = account.Legal.LegalName,
                },
                User = new UserHistoryEventData
                {
                    Email = currentUser.Email,
                    Name = currentUserName,
                    UserType = currentUser.Type,
                },
                TargetUser = new UserHistoryEventData
                {
                    Email = contact.Email,
                    Name = contactName,
                    UserType = contact.Type,
                },
            };
            await _eventPublisher.PublishAsync(new HistoryCreatedEvent(historyActivity));
        }
    }
}
