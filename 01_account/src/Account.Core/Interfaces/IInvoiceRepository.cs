// <copyright file="IInvoiceRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Interfaces;

public interface IInvoiceRepository
{
    Task<bool> ExistsByInvoiceNumberAsync(string invoiceNumber);

    Task AddAsync(CreateInvoiceRequest request);

    Task RemoveByInvoiceNumberAsync(string invoiceNumber);

    Task<int?> GetAccountIdByAccountNumberAsync(string accountNumber);
}
