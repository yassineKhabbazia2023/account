// <copyright file="AccountRepository.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Kpmg.Account.Core.Interfaces;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;
using Pulse.Account.Infrastructure.Entities;

namespace Kpmg.Account.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly AccountContext _accountContext;
    private readonly AsyncRetryPolicy _retryPolicy;

    public AccountRepository(AccountContext context)
    {
        _accountContext = context;

        _retryPolicy = Policy.Handle<SqlException>()
            .WaitAndRetryAsync(
                retryCount: 1,
                sleepDurationProvider: attempt => TimeSpan.FromMilliseconds(3000));
    }
}
