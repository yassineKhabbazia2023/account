// <copyright file="InvoiceService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Core.Services;

public class InvoiceService(IInvoiceRepository invoiceRepository, IDelegationRequestRepository delegationRequestRepository) : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository = invoiceRepository;
    private readonly IDelegationRequestRepository _delegationRequestRepository = delegationRequestRepository;

    public async Task<Paging<Invoice>> GetInvoicesAsync(int accountId, SearchInvoicesCriteria criteria, Pagination? pagination)
    {
        var doesAccountExist = await _delegationRequestRepository.DoesAccountExistAsync(accountId);

        if (!doesAccountExist)
        {
            throw new NotFoundException(Errors.NotFoundAccountCode, string.Format(Errors.NotFoundAccountMessage, accountId));
        }

        pagination = pagination ?? new Pagination();
        pagination.PageNumber = Paginator.GetValidPageNumber(pagination.PageNumber);
        pagination.PageSize = Paginator.GetValidPageSize(pagination.PageSize);

        criteria = criteria ?? new SearchInvoicesCriteria();

        var result = await _invoiceRepository.GetInvoicesAsync(accountId, criteria, pagination);

        return result;
    }
}
