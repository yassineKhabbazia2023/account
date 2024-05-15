// <copyright file="Sorting.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Requests;

public class Sorting
{
    public string Field { get; set; } = string.Empty;

    public bool Descending { get; set; }
}
