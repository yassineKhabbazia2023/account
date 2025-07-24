// <copyright file="LabelRepository.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Infrastructure.Repositories;

public class LabelRepository : ILabelRepository
{
    private readonly AccountContext _accountContext;

    public LabelRepository(AccountContext accountContext)
    {
        _accountContext = accountContext;
    }

    public async Task<Paging<Label>> GetLabelsAsync(Pagination pagination)
    {
        var query = _accountContext.LabelEntity.AsNoTracking();

        int totalItems = await query.CountAsync();

        var labelEntitites = await query
            .Take(pagination.PageSize)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Select(x => x.Map()).ToListAsync();

        if (labelEntitites?.Any() != true)
        {
            throw new NoContentException(Errors.LabelNotFoundCode, Errors.LabelNotFoundMessage);
        }

        return new Paging<Label>
        {
            CurrentPage = pagination.PageNumber,
            Items = labelEntitites,
            TotalItems = totalItems,
            TotalPage = Paginator.GetTotalPages(totalItems, pagination.PageSize)
        };
    }
}
