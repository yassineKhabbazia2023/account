// <copyright file="MapCreateInvoiceRequestToInvoiceEntityTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Tests.Mappers;

public class MapCreateInvoiceRequestToInvoiceEntityTests
{
    [Fact]
    public void ToInvoiceEntity_WhenRequestIsNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        CreateInvoiceRequest? request = null;

        // Act
        Action action = () => request!.ToInvoiceEntity();

        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void ToInvoiceEntity_WhenRequestIsValid_ShouldMapAllProperties()
    {
        // Arrange
        var request = new CreateInvoiceRequest
        {
            InvoiceNumber = "INV-001",
            Name = "Invoice 001",
            InvoiceDate = new DateTime(2024, 01, 15, 0, 0, 0, DateTimeKind.Utc),
            DepositDate = new DateTime(2024, 01, 16, 0, 0, 0, DateTimeKind.Utc),
            AccountId = 5
        };

        // Act
        var entity = request.ToInvoiceEntity();

        // Assert
        Assert.Equal(request.InvoiceNumber, entity.InvoiceNumber);
        Assert.Equal(request.Name, entity.Name);
        Assert.Equal(request.InvoiceDate, entity.InvoiceDate);
        Assert.Equal(request.DepositDate, entity.DepositDate);
        Assert.Equal(request.AccountId, entity.AccountId);
    }
}
