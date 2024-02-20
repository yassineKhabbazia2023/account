// <copyright file="Signatory.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models
{
    public class Signatory
    {
        public int ContactId { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? ContactEmail { get; set; }
    }
}
