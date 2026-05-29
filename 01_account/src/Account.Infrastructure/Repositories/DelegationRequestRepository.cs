// <copyright file="DelegationRequestRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
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

    public async Task<Paging<DelegationRequest>> GetReceivedRequestsAsync(int contactId, string? status, Pagination pagination)
    {
        var query = _context.DelegationRequestEntity
            .Where(dr => dr.RecipientId == contactId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(dr => dr.Status == status);
        }

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderByDescending(dr => dr.CreatedAt)
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

    public async Task<bool> HasRequesterAccessToAccountAsync(int contactId, int accountId)
    {
        return await _context.RoleEntity.AnyAsync(r => r.ContactId == contactId && r.AccountId == accountId);
    }

    public async Task<bool> HasRecipientAccessToAccountAsync(int recipientId, int accountId)
    {
        return await _context.RoleEntity.AnyAsync(r => r.ContactId == recipientId && r.AccountId == accountId);
    }

    public async Task<bool> HasPendingRequestAsync(int requesterId, int accountId)
    {
        var pendingStatus = DelegationRequestStatus.Pending.ToString().ToLower();
        return await _context.DelegationRequestEntity.AnyAsync(dr => dr.RequesterId == requesterId && dr.AccountId == accountId && dr.Status == pendingStatus);
    }

    public async Task<bool> DoesAccountExistAsync(int accountId)
    {
        return await _context.AccountEntity.AnyAsync(a => a.AccountId == accountId);
    }

    public async Task<bool> DoesContactExistAsync(int contactId)
    {
        return await _context.ContactEntity.AnyAsync(c => c.ContactId == contactId);
    }
}