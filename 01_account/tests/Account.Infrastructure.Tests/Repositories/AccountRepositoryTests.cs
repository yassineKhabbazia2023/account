// <copyright file="AccountRepositoryTests.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Dtos;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;

namespace Pulse.Account.Infrastructure.Tests.Repositories
{
    public class AccountRepositoryTests
    {
        private readonly Fixture _fixture;

        public AccountRepositoryTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }
    }
}
