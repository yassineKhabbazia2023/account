// <copyright file="AccountRepository.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using Kpmg.Account.Core.Interfaces;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.Retry;

namespace Kpmg.Account.Infrastructure.Repositories
{
    public class AccountRepository : IAccountRepository
    {
       public AccountRepository() { }
    }
}
