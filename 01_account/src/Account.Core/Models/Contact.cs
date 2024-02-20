// <copyright file="Contact.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text.Json.Serialization;

namespace Pulse.Account.Core.Models;

public class Contact
{
    [JsonIgnore]
    public int ContactId { get; set; }

    public Guid GlobalContactId { get; set; }

    public required string FirstName { get; set; }

    public required string LastName { get; set; }

    public required string ContactEmail { get; set; }

    public string? Type { get; set; }
}
