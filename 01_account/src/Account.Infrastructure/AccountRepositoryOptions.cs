// <copyright file="AccountRepositoryOptions.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pulse.Account.Infrastructure
{
    public class AccountRepositoryOptions
    {
        public string? ConnectionString { get; set; }

        internal void Validate()
        {
            if (string.IsNullOrWhiteSpace(this.ConnectionString!))
            {
                throw new InvalidOperationException($"Instance of {nameof(AccountRepositoryOptions)} is invalid, {nameof(AccountRepositoryOptions.ConnectionString)} is null or empty.");
            }
        }
    }

}
