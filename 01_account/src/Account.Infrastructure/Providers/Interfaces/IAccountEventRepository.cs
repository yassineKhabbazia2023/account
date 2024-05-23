// <copyright file="IAccountEventRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Providers.Interfaces;

public interface IAccountEventRepository
{
    Task<IEnumerable<int>> UpdateAccountStatusByContactAsync(IEnumerable<int> accountIds, int deploymentStatus);

    List<AccountEntity> GetAccountBySignatory(int contactId);
}
