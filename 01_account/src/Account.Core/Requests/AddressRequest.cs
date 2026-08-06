// <copyright file="AddressRequest.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Requests
{
    public class AddressRequest
    {
        public string? Street { get; set; }

        public string City { get; set; } = default!;

        public string Department { get; set; } = default!;

        public string? ZipCode { get; set; }

        public string Region { get; set; } = default!;

        public string Country { get; set; } = default!;
    }
}
