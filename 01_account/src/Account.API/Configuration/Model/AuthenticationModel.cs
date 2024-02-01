// <copyright file="AuthenticationModel.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

namespace Pulse.Account.API.Configuration.Model
{
    public class AuthenticationModel
    {
        public string AuthClientId { get; set; } = null!;

        public string AuthClientSecret { get; set; } = null!;

        public string AuthScope { get; set; } = null!;

        public string AuthTenant { get; set; } = null!;

        public string? AuthServerAdress { get; set; }
    }
}
