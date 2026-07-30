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

        return new InvoiceEntity
        {
            InvoiceNumber = request.InvoiceNumber,
            Name = request.Name,
            InvoiceDate = request.InvoiceDate,
            DepositDate = request.DepositDate,
            AccountId = request.AccountId
        };
    }
}
