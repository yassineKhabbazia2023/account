// <copyright file="DelegationStatusValues.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Constants;

/// <summary>
/// Lowercase string values for delegation statuses stored in database.
/// </summary>
public static class DelegationStatusValues
{
    public const string Pending = "pending";
    public const string Disabled = "disabled";
    public const string Enabled = "enabled";
    public const string Accepted = "accepted";
    public const string Refused = "refused";
}