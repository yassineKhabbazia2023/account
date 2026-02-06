// <copyright file="RoleEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.Abstractions;
using Pulse.Back.Events.IntegrationEvents;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Providers;

public class RoleEventPublisher : IRoleEventPublisher
{
    private readonly IEventPublisher _eventPublisher;
    private readonly IContactEventRepository _contactEventRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IRoleRepository _roleRepository;

    public RoleEventPublisher(IEventPublisher eventPublisher, IContactEventRepository contactEventRepository, IAccountRepository accountRepository, IRoleRepository roleRepository)
    {
        _eventPublisher = eventPublisher;
        _contactEventRepository = contactEventRepository;
        _accountRepository = accountRepository;
        _roleRepository = roleRepository;
    }

    public async Task PublishRoleCreatedEventAsync(CreateRoleRequest roleRequest, string? subRole = null)
    {
        if (roleRequest is null)
        {
            return;
        }

        var selectedAccount = await _accountRepository.GetAccountAsync(roleRequest.AccountId);
        var selectedContact = await _contactEventRepository.GetContactAsync(roleRequest.ContactId ?? 0);

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
            SubRole = subRole,
            ContactFlagPortailFactures = roleRequest.ContactFlagPortailFactures,
        };

        await _eventPublisher.PublishAsync(new RoleCreatedEvent(data));
    }

    public async Task PublishRoleFavoriteStatusChangedEventAsync(int accountId, int contactId, bool isFavorite)
    {
        // Récupérer les informations complètes du rôle pour inclure tous les champs obligatoires
        var role = await _roleRepository.GetContactRoleAsync(accountId, contactId);

        if (role == null)
        {
            throw new ArgumentException($"Role not found for AccountId: {accountId}, ContactId: {contactId}");
        }

        var data = new RoleUpdatedEventData
        {
            AccountId = accountId,
            ContactId = contactId,
            IsFavorite = isFavorite,
            IsSignatory = role.IsSignatory, // Inclure IsSignatory pour éviter l'écrasement
            IsCustomerRelation = role.IsCustomerRelation, // Inclure pour cohérence dans Event State Carried Transfer
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
        var contact = await _contactEventRepository.GetContactAsync(contactId, true);
        var account = await _accountRepository.GetAccountAsync(accountId);

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
