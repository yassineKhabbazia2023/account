// <copyright file="CreateAccountRequest.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using Pulse.Account.Core.Enum;

namespace Pulse.Account.Core.Requests
{
    public class CreateAccountRequest
    {
        [Required]
        public required string AccountNumber { get; set; }

        [Required]
        public required string LegalName { get; set; }

        [Required]
        public required string Siret { get; set; }

        [Required]
        public required AccountType AccountType { get; set; }
    }
}