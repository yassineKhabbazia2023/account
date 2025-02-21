// <copyright file="AccountRepositoryOptions.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Exceptions;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Infrastructure
{
    public class AccountRepositoryOptions
    {
        public string? ConnectionString { get; set; }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.ConnectionString!))
            {
                throw new NullArgumentException(Errors.NotFoundDatabaseConnectionStringCode, Errors.NotFoundDataBaseConnectionStringMessage);
            }
        }
    }
}
