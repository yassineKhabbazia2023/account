// <copyright file="ProspectRef.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models;

/// <summary>
/// Lightweight reference to an active prospect account, used when basculing a prospect to a client.
/// </summary>
public record ProspectRef(int AccountId, Guid AccountGlobalUniqueId);
