// <copyright file="IDematRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Interfaces;

public interface IDematRepository
{
    Task<DateTime?> GetDematModalClosedDateAsync(int accountId, int contactId);

    Task CloseDematModalAsync(int accountId, int contactId);
}
