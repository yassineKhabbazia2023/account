// <copyright file="SearchContactsAccountCriteria.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Enum;

namespace Pulse.Account.Core.Requests;

public class SearchContactsAccountCriteria
{
    public string? Search { get; set; }

    public Sorting? Sorting { get; set; }

    public ContactType? Type { get; set; }

    public bool? IsCustomerRelation { get; set; }
}
