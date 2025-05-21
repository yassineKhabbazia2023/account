// <copyright file="ILabelService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Interfaces
{
    public interface ILabelService
    {
        Task<Paging<Label>> GetLabelsAsync(Pagination pagination);
    }
}
