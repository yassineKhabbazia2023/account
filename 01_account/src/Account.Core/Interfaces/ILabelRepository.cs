// <copyright file="ILabelRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Interfaces;
public interface ILabelRepository
{
    Task<Paging<Label>> GetLabelsAsync(Pagination pagination);
}
