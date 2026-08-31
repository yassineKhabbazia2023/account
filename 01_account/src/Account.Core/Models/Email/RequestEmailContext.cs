// <copyright file="RequestEmailContext.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models.Email;

public class RequestEmailContext
{
    public required string RecipientEmail { get; init; }

    public required string UserFirstName { get; init; }

    public required string UserLastName { get; init; }

    public required string RequestorFirstName { get; init; }

    public required string RequestorLastName { get; init; }

    public required string RequestorEmail { get; init; }

    public required string LegalName { get; init; }

    public required string AccountNumber { get; init; }

    public required DateTime Date { get; init; }
}
