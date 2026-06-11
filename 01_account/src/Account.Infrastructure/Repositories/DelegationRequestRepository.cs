// <copyright file="DelegationRequestRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Repositories;

public class DelegationRequestRepository : IDelegationRequestRepository
{
    private readonly AccountContext _context;

    public DelegationRequestRepository(AccountContext context)
    {
        _context = context;
    }

    public async Task CreateDelegationRequestsAsync(int requesterId, int accountId, int[] recipientIds, string status)
    {
        var delegationRequests = DelegationRequestEntityMapper.ToEntities(requesterId, accountId, recipientIds, status);

        await _context.DelegationRequestEntity.AddRangeAsync(delegationRequests);
        await _context.SaveChangesAsync();
    }

    public async Task<Paging<DelegationRequest>> GetSentRequestsAsync(int contactId, string? status, Pagination pagination)
    {
        var query = _context.DelegationRequestEntity.Where(dr => dr.RequesterId == contactId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(dr => dr.Status == status);
        }

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderByDescending(dr => dr.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Include(dr => dr.Recipient)
            .Include(dr => dr.Account)
            .ToListAsync();

        var mappedItems = items.Select(dr => dr.ToDelegationRequest()).ToList();

        var totalPages = (int)Math.Ceiling(totalItems / (double)pagination.PageSize);

        return new Paging<DelegationRequest>
        {
            Items = mappedItems,
            CurrentPage = pagination.PageNumber,
            TotalPage = totalPages,
            TotalItems = totalItems
        };
    }

    public async Task<Paging<DelegationRequest>> GetReceivedRequestsAsync(int contactId, string[]? statuses, Pagination pagination)
    {
        var query = _context.DelegationRequestEntity
            .Where(dr => dr.RecipientId == contactId);

        if (statuses is { Length: > 0 })
        {
            query = query.Where(dr => statuses.Contains(dr.Status));
        }

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderBy(dr => dr.CreatedAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Include(dr => dr.Requester)
            .Include(dr => dr.Account)
            .ToListAsync();

        var mappedItems = items.Select(dr => dr.ToDelegationRequest()).ToList();

        var totalPages = (int)Math.Ceiling(totalItems / (double)pagination.PageSize);

        return new Paging<DelegationRequest>
        {
            Items = mappedItems,
            CurrentPage = pagination.PageNumber,
            TotalPage = totalPages,
            TotalItems = totalItems
        };
    }

    public async Task<bool> HasRoleOnAccountAsync(int contactId, int accountId)
    {
        return await _context.RoleEntity.AnyAsync(r => r.ContactId == contactId && r.AccountId == accountId);
    }

    public async Task<bool> HasActiveDelegationOnAccountAsync(int contactId, int accountId)
    {
        return await _context.DelegationEntity
            .AnyAsync(d => d.DelegateeId == contactId && d.Status == DelegationStatusValues.Enabled && d.Account.Any(a => a.AccountId == accountId));
    }

    public async Task<bool> HasPendingRequestAsync(int requesterId, int accountId)
    {
        return await _context.DelegationRequestEntity.AnyAsync(dr => dr.RequesterId == requesterId && dr.AccountId == accountId && dr.Status == DelegationStatusValues.Pending);
    }

    public async Task<bool> DoesAccountExistAsync(int accountId)
    {
        return await _context.AccountEntity.AnyAsync(a => a.AccountId == accountId);
    }

    public async Task<bool> DoesContactExistAsync(int contactId)
    {
        return await _context.ContactEntity.AnyAsync(c => c.ContactId == contactId);
    }

    public async Task<List<DelegationRequest>> GetPendingRequestsByIdsAndRecipientAsync(int[] delegationRequestIds, int recipientId)
    {
        var entities = await _context.DelegationRequestEntity
            .AsNoTracking()
            .Where(dr => delegationRequestIds.Contains(dr.DelegationRequestId)
                         && dr.RecipientId == recipientId
                         && dr.Status == DelegationStatusValues.Pending)
            .Include(dr => dr.Requester)
            .Include(dr => dr.Account)
            .ToListAsync();

        return entities.Select(dr => dr.ToDelegationRequest()).ToList();
    }

    public async Task AcceptRequestsAsync(int[] delegationRequestIds, DateTime respondedAt)
    {
        await UpdateRequestsAsync(delegationRequestIds, DelegationStatusValues.Accepted, respondedAt);
    }

    public async Task RefuseRequestsAsync(int[] delegationRequestIds, DateTime respondedAt)
    {
        await UpdateRequestsAsync(delegationRequestIds, DelegationStatusValues.Refused, respondedAt);
    }

    public async Task AcceptSiblingRequestsAsync(int requesterId, int accountId, DateTime respondedAt)
    {
        await UpdateSiblingRequestsAsync(requesterId, accountId, DelegationStatusValues.Accepted, respondedAt);
    }

    public async Task<bool> AreAllSiblingRequestsRefusedAsync(int requesterId, int accountId)
    {
        var siblingRequests = _context.DelegationRequestEntity
            .Where(dr => dr.RequesterId == requesterId
                         && dr.AccountId == accountId);

        return await siblingRequests.AnyAsync() && await siblingRequests.AllAsync(dr => dr.Status == DelegationStatusValues.Refused);
    }

    private async Task UpdateRequestsAsync(int[] delegationRequestIds, string status, DateTime respondedAt)
    {
        await _context.DelegationRequestEntity
            .Where(dr => delegationRequestIds.Contains(dr.DelegationRequestId))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(dr => dr.Status, status)
                .SetProperty(dr => dr.RespondedAt, respondedAt));
    }

    private async Task UpdateSiblingRequestsAsync(int requesterId, int accountId, string status, DateTime respondedAt)
    {
        await _context.DelegationRequestEntity
            .Where(dr => dr.RequesterId == requesterId
                         && dr.AccountId == accountId
                         && dr.Status == DelegationStatusValues.Pending)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(dr => dr.Status, status)
                .SetProperty(dr => dr.RespondedAt, respondedAt));
    }
}
