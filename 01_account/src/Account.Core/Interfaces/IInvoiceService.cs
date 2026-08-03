// <copyright file="IInvoiceService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Interfaces;

public interface IInvoiceService
{
    Task<Paging<Invoice>> GetInvoicesAsync(int accountId, SearchInvoicesCriteria criteria, Pagination? pagination);
}
