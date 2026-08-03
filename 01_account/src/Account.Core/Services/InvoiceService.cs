// <copyright file="InvoiceService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Services;

public class InvoiceService(IInvoiceRepository invoiceRepository) : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository = invoiceRepository;

    public async Task<Paging<Invoice>> GetInvoicesAsync(int accountId, SearchInvoicesCriteria criteria, Pagination? pagination)
    {
        pagination = pagination ?? new Pagination();
        pagination.PageNumber = Paginator.GetValidPageNumber(pagination.PageNumber);
        pagination.PageSize = Paginator.GetValidPageSize(pagination.PageSize);

        criteria = criteria ?? new SearchInvoicesCriteria();

        var result = await _invoiceRepository.GetInvoicesAsync(accountId, criteria, pagination);

        return result;
    }
}
