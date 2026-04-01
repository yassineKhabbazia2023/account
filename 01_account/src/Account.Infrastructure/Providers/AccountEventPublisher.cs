// <copyright file="AccountEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Providers
{
    public class AccountEventPublisher : IAccountEventPublisher
    {
        private readonly IEventPublisher _eventPublisher;

        public AccountEventPublisher(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        public async Task PublishAccountCreatedEventAsync(AccountDetail account)
        {
            if (account is null)
            {
                return;
            }

            var eventData = new AccountStateEventData
            {
                AccountGlobalUniqueId = account.AccountGlobalUniqueId,
                AccountId = account.AccountId,
                AccountNumber = account.AccountNumber ?? string.Empty,
                LegalName = account.Legal?.LegalName ?? string.Empty,
                SiretNumber = account?.Legal?.Siret ?? string.Empty,
                Status = DeploymentStatus.ToDeploy.ToString(),
                CreatedBy = account.CreatedBy,
                ModifiedBy = account.ModifiedBy,
                AccountType = account.AccountType,
            };

            var @event = new AccountCreatedEvent(eventData);
            await _eventPublisher.PublishAsync(@event);
        }

        public async Task PublishAccountUpdatedEventAsync(AccountDetail account)
        {
            if (account is null)
            {
                return;
            }

            var eventData = new AccountStateEventData
            {
                AccountGlobalUniqueId = account.AccountGlobalUniqueId,
                AccountId = account.AccountId,
                AccountNumber = account.AccountNumber ?? string.Empty,
                LegalName = account.Legal?.LegalName ?? string.Empty,
                SiretNumber = account.Legal?.Siret ?? string.Empty,
                Status = ((DeploymentStatus)account.Deployment!.Status).ToString(),
                CreatedBy = account.CreatedBy,
                ModifiedBy = account.ModifiedBy,
                AccountType = account.AccountType,
            };

            var @event = new AccountUpdatedEvent(eventData);
            await _eventPublisher.PublishAsync(@event);
        }

        public async Task PublishAccountRemovedEventAsync(int accountId, string? accountType)
        {
            var eventData = new AccountRemovedEventData
            {
                AccountId = accountId,
                AccountType = accountType,
            };

            var @event = new AccountRemovedEvent(eventData);
            await _eventPublisher.PublishAsync(@event);
        }
    }
}
