// <copyright file="RegistryRoleEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Extensions;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Infrastructure.Mappers.EventsMapper;




// <copyright file="RegistryRoleEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Infrastructure.Providers.Interfaces;
using Pulse.Back.Events.IntegrationEvents.EventsData;

namespace Pulse.Account.Infrastructure.Repositories
{
    public class RegistryRoleEventRepository : IRegistryRoleEventRepository
    {
        private readonly AccountContext _context;

        public RegistryRoleEventRepository(
            AccountContext context)
        {
            _context = context;
            _context.HandleEFCoreFailure();
        }

        public async Task<CreateRoleRequest> CreateRoleAsync(RegistryRoleCreatedEventData eventData)
        {
            var role = eventData.ToRoleEntity(_context);

            _context.RoleEntity.Add(role);
            await _context.SaveChangesAsync();
            return role.ToCreateRoleRequest();
        }

        public async Task<(int, int)> RemoveRoleAsync(RegistryRoleRemovedEventData eventData)
        {
            var account = _context.AccountEntity.FirstOrDefault(x => x.AccountGlobalUniqueId == eventData.AccountId);
            var contact = _context.ContactEntity.FirstOrDefault(x => x.ContactGlobalUniqueId == eventData.ContactId);

            if (account == null || contact == null)
            {
                throw new NotFoundException(Errors.NotFoundRoleCode, string.Format(Errors.NotFoundRoleMessage, eventData.ContactId, eventData.AccountId));
            }

            var roleToRemove = _context.RoleEntity.FirstOrDefault(x => x.ContactId == contact.ContactId && x.AccountId == account.AccountId);
            if (roleToRemove == null)
            {
                throw new NotFoundException(Errors.NotFoundRoleCode, string.Format(Errors.NotFoundRoleMessage, eventData.ContactId, eventData.AccountId));
            }

            _context.RoleEntity.Remove(roleToRemove);
            await _context.SaveChangesAsync();
            return (account.AccountId, contact.ContactId);
        }
    }
}
