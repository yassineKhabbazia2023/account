// <copyright file="IStatisticsService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Interfaces
{
    public interface IStatisticsService
    {
        Task<Statistics> GetStatisticsAsync(int contactId);
    }
}
