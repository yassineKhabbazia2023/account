// <copyright file="Contact.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Pulse.Account.Core.Models;

public class Contact
{
    [JsonIgnore]
    public int ContactId { get; set; }

    public Guid GlobalContactId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string ContactEmail { get; set; } = null!;

    public string? Type { get; set; }
}
