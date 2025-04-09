// <copyright file="Address.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models;

public class Address
{
    public int AddressId { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? AddressLine3 { get; set; }

    public string? State { get; set; }

    public string? ZipCode { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }

    public string? AddressType { get; set; }

    public decimal Latitude { get; set; } = default!;

    public decimal Longitude { get; set; } = default!;
}
