// <copyright file="Office.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models;

public class Office
{
    public int OfficeId { get; set; }

    public string Name { get; set; } = default!;

    public string PhoneNumber { get; set; } = default!;

    public int AddressId { get; set; }

    public Address? Address { get; set; }
}
