// <copyright file="HistoryEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Infrastructure.Interfaces;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Back.Events.IntegrationEvents.EventsData;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Infrastructure.Providers;

public class HistoryEventPublisher : IHistoryEventPublisher
{
  private readonly IContactRepository _contactRepository;
  private readonly IAccountRepository _accountRepository;
  private readonly IEventPublisher _eventPublisher;
  private const string DeleteContactActionCode = "DELCMANU";

  public HistoryEventPublisher(IContactRepository contactRepository, IAccountRepository accountRepository, IEventPublisher eventPublisher)
  {
    _contactRepository = contactRepository;
    _accountRepository = accountRepository;
    _eventPublisher = eventPublisher;
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
