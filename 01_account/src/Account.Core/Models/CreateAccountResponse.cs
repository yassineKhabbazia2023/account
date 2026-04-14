// <copyright file="CreateAccountResponse.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models
{
    public class CreateAccountResponse
    {
        public required string Message { get; set; }

        public int AccountId { get; set; }
    }
}