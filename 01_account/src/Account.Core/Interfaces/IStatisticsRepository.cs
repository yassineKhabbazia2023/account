// <copyright file="IStatisticsRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Interfaces
{
    public interface IStatisticsRepository
    {
        Task<Statistics> GetStatisticsAsync(int contactId);

        Task<double> GetAccountPercentageCustomerRelationAsync();

        Task<IEnumerable<(int ClientCount, int AccountCount)>> GetClientsPerAccountCountAsync();

        Task<IEnumerable<(int AccountCount, int ClientCount)>> GetAccountsPerClientCountAsync();
    }
}
