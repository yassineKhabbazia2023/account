// <copyright file="SerenityChoiceRequest.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Requests;

/// <summary>
/// Choix exprimé par le contact sur la modal Sérénité.
/// </summary>
public class SerenityChoiceRequest
{
    /// <summary>
    /// Gets or sets a value indicating whether le contact a accepté la proposition.
    /// </summary>
    public bool IsAccepted { get; set; }
}
