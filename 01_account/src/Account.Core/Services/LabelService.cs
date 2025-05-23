// <copyright file="LabelService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Services
{
    public class LabelService : ILabelService
    {
        private readonly ILabelRepository _labelRepository;

        public LabelService(ILabelRepository labelRepos)
        {
            _labelRepository = labelRepos;
        }

        public async Task<Paging<Label>> GetLabelsAsync(Pagination pagination)
        {
            pagination = pagination ?? new Pagination();
            pagination.PageNumber = Paginator.GetValidPageNumber(pagination.PageNumber);
            pagination.PageSize = Paginator.GetValidPageSize(pagination.PageSize);

            return await _labelRepository.GetLabelsAsync(pagination);
        }
    }
}
