// <copyright file="InvoiceMapper.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Mappers;

public static class InvoiceMapper
{
    public static InvoiceEntity ToInvoiceEntity(this CreateInvoiceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new InvoiceEntity
        {
            InvoiceNumber = request.InvoiceNumber,
            DocumentPath = request.DocumentPath,
            InvoiceDate = request.InvoiceDate,
            DepositDate = request.DepositDate,
            AccountId = request.AccountId,
            Type = request.Type,
            Category = request.Category,
        };
    }

    public static Paging<Invoice> MapToPagingInvoice(this IEnumerable<InvoiceEntity> entities, int totalItems, int totalPages, int pageNumber, SearchInvoicesCriteria criteria)
    {
        var invoices = entities.Select(e => e.ToInvoice()).ToList();

        return new Paging<Invoice>
        {
            Items = invoices!,
            CurrentPage = pageNumber,
            TotalItems = totalItems,
            TotalPage = totalPages,
        };
    }

    public static Invoice? ToInvoice(this InvoiceEntity? entity)
    {
        if (entity is null)
        {
            return null;
        }

        return new Invoice
        {
            Id = entity.InvoiceId,
            Name = entity.InvoiceNumber,
            DepositDate = entity.DepositDate,
            Type = entity.Type,
            Category = entity.Category,
            InvoiceYear = entity.InvoiceDate.Year
        };
    }
}
