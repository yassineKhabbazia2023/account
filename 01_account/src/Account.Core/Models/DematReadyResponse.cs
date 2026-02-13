// <copyright file="DematReadyResponse.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models;

public class DematReadyResponse
{
    public bool IsReady { get; set; }

    public string? ContactWithAccess { get; set; }

    public string? ExternalDematMail { get; set; }
}
