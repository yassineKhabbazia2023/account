// <copyright file="AccountRolePair.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Utils;

/// <summary>
/// Paire (compte, rôle du contact courant).
/// </summary>
public sealed class AccountRolePair
{
    public required AccountEntity Account { get; init; }

    public required RoleEntity Role { get; init; }
}
