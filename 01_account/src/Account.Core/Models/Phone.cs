// <copyright file="Phone.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Pulse.Account.Core.Extensions;

namespace Pulse.Account.Core.Models
{
    public class Phone
    {
        public int PhoneId { get; set; }

        [NotEmptyOrWhiteSpace]
        public required string PhoneNumber { get; set; }

        public string? Type { get; set; }
    }
}
