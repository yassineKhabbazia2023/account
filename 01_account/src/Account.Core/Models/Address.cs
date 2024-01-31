// <copyright file="Address.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

namespace Kpmg.Account.Core.Models
{
    public class Address
    {
        public int AddressId { get; set; }

        public string? Street { get; set; }

        public string? State { get; set; }

        public string? ZipCode { get; set; }

        public string? Country { get; set; }

        public string? City { get; set; }
    }
}
