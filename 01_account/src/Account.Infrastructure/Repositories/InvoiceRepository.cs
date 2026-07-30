// <copyright file="InvoiceRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Repositories;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly AccountContext _context;

    public InvoiceRepository(AccountContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        this._context = context;
    }

    public async Task<bool> ExistsByInvoiceNumberAsync(string invoiceNumber)
    {
        ArgumentNullException.ThrowIfNull(invoiceNumber);
        return await this._context.InvoiceEntity
            .AsNoTracking()
            .AnyAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public async Task AddAsync(CreateInvoiceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.InvoiceNumber);
        ArgumentNullException.ThrowIfNull(request.Name);

        var invoice = request.ToInvoiceEntity();

        this._context.InvoiceEntity.Add(invoice);
        await this._context.SaveChangesAsync();
    }

    public async Task RemoveByInvoiceNumberAsync(string invoiceNumber)
    {
        ArgumentNullException.ThrowIfNull(invoiceNumber);

        var invoice = await this._context.InvoiceEntity
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);

        if (invoice != null)
        {
            this._context.InvoiceEntity.Remove(invoice);
            await this._context.SaveChangesAsync();
        }
    }

    public async Task<InvoiceEntity?> GetByInvoiceNumberAsync(string invoiceNumber)
    {
        ArgumentNullException.ThrowIfNull(invoiceNumber);

        return await this._context.InvoiceEntity
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public async Task<int?> GetAccountIdByAccountNumberAsync(string accountNumber)
    {
        ArgumentNullException.ThrowIfNull(accountNumber);

        var account = await this._context.AccountEntity
            .AsNoTracking()
            .Where(a => a.AccountNumber == accountNumber)
            .Select(a => new { a.AccountId })
            .FirstOrDefaultAsync();

        return account?.AccountId;
    }
}
