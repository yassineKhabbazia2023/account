// <copyright file="ContactCreatedEventHandler.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Providers;

public class ContactCreatedEventHandler : IEventHandler
{
    private readonly ILogger<ContactCreatedEventHandler> _logger;
    private readonly IContactEventRepository _contactEventRepository;
    private readonly IRoleEventRepository _roleEventRepository;
    private readonly IAccountEventRepository _accountEventRepository;
    private readonly IRoleEventPublisher _roleEventPublisher;

    public ContactCreatedEventHandler(
        ILogger<ContactCreatedEventHandler> logger,
        IContactEventRepository contactEventRepository,
        IRoleEventRepository roleEventRepository,
        IAccountEventRepository accountEventRepository,
        IRoleEventPublisher roleEventPublisher)
    {
        _logger = logger;
        _contactEventRepository = contactEventRepository;
        _roleEventRepository = roleEventRepository;
        _accountEventRepository = accountEventRepository;
        _roleEventPublisher = roleEventPublisher;
    }

    public async Task HandleAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var contactEvent = JsonConvert.DeserializeObject<ContactCreatedEvent>(message);
        _logger.LogInformation("Consommation de l'event type: {EventType}, contactId: {ContactId}",
            contactEvent?.EventType,
            contactEvent?.Data?.ContactId);

        if (contactEvent?.Data == null || contactEvent.Data.ContactId <= 0)
        {
            return;
        }

        var contactEntity = contactEvent.Data.ToContactEntity();

        await CreateContact(contactEntity);
        await CreateRole(contactEntity.ContactId, contactEvent.Data.AccountNumber!);
    }

    private async Task CreateContact(ContactEntity contact)
    {
        if (!await _contactEventRepository.DoesContactExistAsync(contact.ContactId))
        {
            await _contactEventRepository.CreateContactAsync(contact);
            _logger.LogInformation("Le contact avec l'identifiant: {ContactId} vient d'être créé.", contact.ContactId);
        }
        else
        {
            _logger.LogWarning("Le contact avec l'identifiant: {ContactId} existe déjà.", contact.ContactId);
        }
    }

    private async Task CreateRole(int contactId, string accountNumber)
    {
        if (!string.IsNullOrEmpty(accountNumber))
        {
            var accountEntity = await _accountEventRepository.GetAccountByNumberAsync(accountNumber!);

            if (!await _roleEventRepository.DoesRoleExistAsync(contactId, accountEntity.AccountId))
            {
                var role = await _roleEventRepository.CreateRoleForNewContactAsync(contactId, accountEntity.AccountId);
                _logger.LogInformation("Le rôle du contact {ContactId} sur l'entité {AccountId} vient d'être créé.", contactId, accountEntity.AccountId);
                await _roleEventPublisher.PublishRoleCreatedEventAsync(role!);
            }
            else
            {
                _logger.LogWarning("Le rôle du contact {ContactId} sur l'entité {AccountId} existe déjà.", contactId, accountEntity.AccountId);
            }
        }
    }
}
