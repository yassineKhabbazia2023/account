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
    private readonly AccountContext context;

    public InvoiceRepository(AccountContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        this.context = context;
    }

    public async Task<bool> ExistsByInvoiceNumberAsync(string invoiceNumber)
    {
        ArgumentNullException.ThrowIfNull(invoiceNumber);
        return await this.context.InvoiceEntity
            .AsNoTracking()
            .AnyAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public async Task AddAsync(CreateInvoiceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var invoice = request.ToInvoiceEntity();

        this.context.InvoiceEntity.Add(invoice);
        await this.context.SaveChangesAsync();
    }

    public async Task RemoveByInvoiceNumberAsync(string invoiceNumber)
    {
        ArgumentNullException.ThrowIfNull(invoiceNumber);

        var invoice = await this.context.InvoiceEntity
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);

        if (invoice != null)
        {
            this.context.InvoiceEntity.Remove(invoice);
            await this.context.SaveChangesAsync();
        }
    }

    public async Task<InvoiceEntity?> GetByInvoiceNumberAsync(string invoiceNumber)
    {
        ArgumentNullException.ThrowIfNull(invoiceNumber);

        return await this.context.InvoiceEntity
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public async Task<int?> GetAccountIdByAccountNumberAsync(string accountNumber)
    {
        ArgumentNullException.ThrowIfNull(accountNumber);

        var account = await this.context.AccountEntity
            .AsNoTracking()
            .Where(a => a.AccountNumber == accountNumber)
            .Select(a => new { a.AccountId })
            .FirstOrDefaultAsync();

        return account?.AccountId;
    }
}
