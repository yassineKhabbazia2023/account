// <copyright file="MapCreateInvoiceRequestToInvoiceEntity.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Mappers;

public static class MapCreateInvoiceRequestToInvoiceEntity
{
    public static InvoiceEntity ToInvoiceEntity(this CreateInvoiceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var entity = new InvoiceEntity
        {
            InvoiceNumber = request.InvoiceNumber,
            DocumentPath = request.DocumentPath,
            Type = request.Type,
            Category = request.Category,
            InvoiceDate = request.InvoiceDate,
            AccountId = request.AccountId,
            DepositDate = request.DepositDate
        };

        return entity;
    }
}
