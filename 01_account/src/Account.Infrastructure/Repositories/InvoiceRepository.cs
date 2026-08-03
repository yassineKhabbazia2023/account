// <copyright file="InvoiceRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Extensions;
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Repositories;

public class InvoiceRepository(AccountContext context) : IInvoiceRepository
{
    private readonly AccountContext _context = context;
    private static readonly SortMap<InvoiceEntity> InvoiceSorts = new SortMap<InvoiceEntity>()
    .Add("DepositDate", i => i.DepositDate, isDefault: true)
    .Add("Name", i => i.InvoiceNumber)
    .Add("InvoiceYear", i => i.InvoiceDate.Year);

    public async Task<bool> ExistsByInvoiceNumberAsync(string invoiceNumber)
    {
        ArgumentNullException.ThrowIfNull(invoiceNumber);
        return await _context.InvoiceEntity
            .AsNoTracking()
            .AnyAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public async Task AddAsync(CreateInvoiceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var invoice = request.ToInvoiceEntity();

        _context.InvoiceEntity.Add(invoice);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveByInvoiceNumberAsync(string invoiceNumber)
    {
        ArgumentNullException.ThrowIfNull(invoiceNumber);

        var invoice = await _context.InvoiceEntity
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);

        if (invoice != null)
        {
            _context.InvoiceEntity.Remove(invoice);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<InvoiceEntity?> GetByInvoiceNumberAsync(string invoiceNumber)
    {
        ArgumentNullException.ThrowIfNull(invoiceNumber);

        return await _context.InvoiceEntity
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public async Task<int?> GetAccountIdByAccountNumberAsync(string accountNumber)
    {
        ArgumentNullException.ThrowIfNull(accountNumber);

        var account = await _context.AccountEntity
            .AsNoTracking()
            .Where(a => a.AccountNumber == accountNumber)
            .Select(a => new { a.AccountId })
            .FirstOrDefaultAsync();

        return account?.AccountId;
    }

    public async Task<Paging<Invoice>> GetInvoicesAsync(int accountId, SearchInvoicesCriteria criteria, Pagination pagination)
    {
        var query = _context.InvoiceEntity.AsNoTracking()
            .Where(i => i.AccountId == accountId)
            .ApplyFilters(criteria.Search);

        query = InvoiceSorts.Apply(query, criteria.SortBy, criteria.SortOrder);

        var totalItems = await query.CountAsync();
        var totalPages = Paginator.GetTotalPages(totalItems, pagination.PageSize);

        var invoices = await query
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return invoices.MapToPagingInvoice(totalItems, totalPages, pagination.PageNumber, criteria);
    }
}
