// <copyright file="AuthenticationModel.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

namespace Pulse.Account.API.Configuration.Model
{
    public class AuthenticationModel
    {
        required public string AuthClientId { get; set; }

        required public string AuthClientSecret { get; set; }

        required public string AuthScope { get; set; }

        required public string AuthTenant { get; set; }

        public string? AuthServerAdress { get; set; }
    }
}
