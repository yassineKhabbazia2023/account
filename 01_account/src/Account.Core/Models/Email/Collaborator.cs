// <copyright file="Collaborator.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models.Email;

public class Collaborator
{
    public required string FirstName { get; init; }

    public required string LastName { get; init; }

    public required string Email { get; init; }
}
