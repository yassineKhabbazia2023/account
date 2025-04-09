// <copyright file="OfficeEntity.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Infrastructure.Entities;

public partial class OfficeEntity
{
    public int OfficeId { get; set; }

    public string Name { get; set; } = default!;

    public string PhoneNumber { get; set; } = default!;

    public int AddressId { get; set; }

    public virtual AddressEntity AddressEntity { get; set; } = default!;

    public virtual ICollection<AccountEntity> AccountEntity { get; set; } = [];
}
