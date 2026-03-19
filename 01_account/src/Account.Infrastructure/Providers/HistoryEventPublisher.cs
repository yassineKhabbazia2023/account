// <copyright file="HistoryEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Providers;

public class HistoryEventPublisher : IHistoryEventPublisher
{
    private readonly IContactEventRepository _contactEventRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IEventPublisher _eventPublisher;

    public HistoryEventPublisher(IContactEventRepository contactEventRepository, IAccountRepository accountRepository, IEventPublisher eventPublisher)
    {
        _contactEventRepository = contactEventRepository;
        _accountRepository = accountRepository;
        _eventPublisher = eventPublisher;
    }

    public async Task PublishHistoryCreatedEventAsync(string registryApproverEmail, int contactId, int accountId)
    {
        var currentUser = await _contactEventRepository.GetContactByEmailAsync(registryApproverEmail);
        var currentUserName = currentUser.FirstName + " " + currentUser.LastName;

        var targetUser = await _contactEventRepository.GetContactAsync(contactId);
        var targetUserName = targetUser.FirstName + " " + targetUser.LastName;

        var account = await _accountRepository.GetAccountAsync(accountId);

        var data = new HistoryCreatedEventData
        {
            CreationDate = DateTime.UtcNow,
            Account = new AccountHistoryEventData
            {
                AccountId = accountId,
                AccountNumber = account.AccountNumber!,
                LegalName = account.Legal!.LegalName!,
            },
            Action = new ActionHistoryEventData
            {
                Code = ActionCode.ADDCVREG.ToString(),
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

        await _eventPublisher.PublishAsync(new HistoryCreatedEvent(data) { AccountType = account.AccountType });
    }

    public async Task PublishHistoryCreatedEventAsync(int currentUserId, int contactId, int accountId, string actionCode)
    {
        var currentUser = await _contactEventRepository.GetContactAsync(currentUserId);
        var currentUserName = currentUser.FirstName + " " + currentUser.LastName;

        var contact = await _contactEventRepository.GetContactAsync(contactId);
        var contactName = contact.FirstName + " " + contact.LastName;

        var account = await _accountRepository.GetAccountAsync(accountId);

        var historyActivity = new HistoryCreatedEventData
        {
            CreationDate = DateTime.UtcNow,
            Action = new ActionHistoryEventData
            {
                Code = actionCode,
            },
            Account = new AccountHistoryEventData
            {
                AccountId = accountId,
                AccountNumber = account!.AccountNumber!,
                LegalName = account.Legal!.LegalName!,
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

        await _eventPublisher.PublishAsync(new HistoryCreatedEvent(historyActivity) { AccountType = account!.AccountType });
    }
}
