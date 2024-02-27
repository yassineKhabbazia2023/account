// <copyright file="DelegationRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

public class DelegationRepositoryTests
{
    private readonly Fixture _fixture;
    private readonly DbContextOptions<AccountContext> _dbContextOptions;

    public DelegationRepositoryTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
          .Options;
    }

    [Fact]
    public async Task CreateDelegationAsync_When_Request_IsValide_Should_Create_Delegation()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Create Account
            var tAccount = _fixture.Create<TAccount>();
            context.TAccount.Add(tAccount);
            await context.SaveChangesAsync();

            // Create Contacts
            var tDelegator = _fixture.Create<TContact>();
            var tDelegatee = _fixture.Create<TContact>();
            context.TContact.AddRange(new List<TContact> { tDelegator, tDelegatee });
            await context.SaveChangesAsync();

            // Try create a delegation
            var repository = new DelegationRepository(context);
            var createDelegation = new CreateDelegation()
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(5),
                AccountId = tAccount.AccountId,
                Status = "pending",
                DelegatorId = tDelegator.ContactId,
                DelegateeId = tDelegatee.ContactId,
            };

            await repository.CreateDelegationAsync(createDelegation);

            var createdDelegation = await context
                .TDelegation
                .FirstOrDefaultAsync(d =>
                    d.Account.AccountId == tAccount.AccountId
                    &&
                    d.DelegatorId == tDelegator.ContactId
                    &&
                    d.DelegateeId == tDelegatee.ContactId);

            Assert.NotNull(createdDelegation);
            Assert.Equal(createDelegation.StartDate, createdDelegation.StartDate);
            Assert.Equal(createDelegation.EndDate, createdDelegation.EndDate);
            Assert.Equal(createDelegation.Status, createdDelegation.Status);
        }
    }

    [Fact]
    public async Task CreateDelegationAsync_When_Delegator_NotExists__Should_ThrowException()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Create Account
            var tAccount = _fixture.Create<TAccount>();
            context.TAccount.Add(tAccount);
            await context.SaveChangesAsync();

            // Create Contacts
            var tDelegatee = _fixture.Create<TContact>();
            context.TContact.AddRange(new List<TContact> { tDelegatee });
            await context.SaveChangesAsync();

            // Try create a delegation
            var repository = new DelegationRepository(context);
            var createDelegation = new CreateDelegation()
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(5),
                AccountId = tAccount.AccountId,
                Status = "pending",
                DelegateeId = tDelegatee.ContactId,
                DelegatorId = 0,
            };

            var result = await Assert.ThrowsAsync<NotFoundException>(async () => await repository.CreateDelegationAsync(createDelegation));
            Assert.Equal($@"Le contact avec l'identifiant {0} est introuvable", result.Message);
        }
    }

    [Fact]
    public async Task CreateDelegationAsync_When_Delegatee_NotExists__Should_ThrowException()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Create Account
            var tAccount = _fixture.Create<TAccount>();
            context.TAccount.Add(tAccount);
            await context.SaveChangesAsync();

            // Create Contacts
            var tDelegator = _fixture.Create<TContact>();
            context.TContact.AddRange(new List<TContact> { tDelegator });
            await context.SaveChangesAsync();

            // Try create a delegation
            var repository = new DelegationRepository(context);
            var createDelegation = new CreateDelegation()
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(5),
                AccountId = tAccount.AccountId,
                Status = "pending",
                DelegatorId = tDelegator.ContactId,
                DelegateeId = 0,
            };

            var result = await Assert.ThrowsAsync<NotFoundException>(async () => await repository.CreateDelegationAsync(createDelegation));
            Assert.Equal($@"Le contact avec l'identifiant {0} est introuvable", result.Message);
        }
    }

    [Fact]
    public async Task CreateDelegationAsync_When_Account_NotExists__Should_ThrowException()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Create Contacts
            var tDelegator = _fixture.Create<TContact>();
            var tDelegatee = _fixture.Create<TContact>();
            context.TContact.AddRange(new List<TContact> { tDelegator, tDelegatee });
            await context.SaveChangesAsync();

            // Try create a delegation
            var repository = new DelegationRepository(context);
            var createDelegation = new CreateDelegation()
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(5),
                AccountId = 3,
                Status = "pending",
                DelegatorId = tDelegator.ContactId,
                DelegateeId = tDelegatee.ContactId,
                AccountId = 1
            };

            var result = await Assert.ThrowsAsync<NotFoundException>(async () => await repository.CreateDelegationAsync(createDelegation));
            Assert.Equal(string.Format(Errors.NotFoundAccountMessage, createDelegation.AccountId), result.Message);
        }
    }

    [Fact]
    public async Task GetContactDelegationsAsync_Should_Return_ContactDelegations()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Create Account
            var tAccount = _fixture.Create<TAccount>();
            await context.TAccount.AddAsync(tAccount);
            await context.SaveChangesAsync();

            // Create Contacts
            var tDelegator = _fixture.Create<TContact>();
            var tDelegatee = _fixture.Create<TContact>();
            await context.TContact.AddRangeAsync(new List<TContact> { tDelegator, tDelegatee });
            await context.SaveChangesAsync();

            // Try create a delegation
            var tDelegation = new TDelegation()
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(5),
                AccountId = tAccount.AccountId,
                DelegatorId = tDelegator.ContactId,
                DelegateeId = tDelegatee.ContactId,
                Status = "pending",
                Note = "Note",
            };
            await context.TDelegation.AddAsync(tDelegation);
            await context.SaveChangesAsync();

            // Try get contact delegation
            var repository = new DelegationRepository(context);
            var conactDelegations = await repository.GetContactDelegationsAsync(tDelegatee.ContactId);

            Assert.Equal(1, conactDelegations.Count);
            var contactDelegation = conactDelegations.FirstOrDefault();
            Assert.NotNull(contactDelegation);
            Assert.Equal(contactDelegation.StartDate, tDelegation.StartDate);
            Assert.Equal(contactDelegation.EndDate, tDelegation.EndDate);
            Assert.Equal(contactDelegation.Status, tDelegation.Status);
            Assert.Equal(contactDelegation.Note, tDelegation.Note);
            Assert.Equal(contactDelegation.CreationDate, tDelegation.CreationDate);
            Assert.Equal(contactDelegation.Account!.AccountId, tDelegation.Account.AccountId);
            Assert.Equal(contactDelegation.Delegator!.ContactId, tDelegation.DelegatorId);
        }
    }

    [Fact]
    public async Task GetDelegationsAsync_Should_Return_Delegations()
    {
        using (var context = new AccountContext(_dbContextOptions))
        {
            // Create Account
            var tAccount = _fixture.Create<TAccount>();
            await context.TAccount.AddAsync(tAccount);
            await context.SaveChangesAsync();

            // Create Contacts
            var tDelegator = _fixture.Create<TContact>();
            var tDelegatee = _fixture.Create<TContact>();
            var anotherDelegatee = _fixture.Create<TContact>();
            await context.TContact.AddRangeAsync(new List<TContact> { tDelegator, tDelegatee, anotherDelegatee });
            await context.SaveChangesAsync();

            // Try create a delegation
            var tDelegation = new TDelegation()
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(5),
                AccountId = tAccount.AccountId,
                DelegatorId = tDelegator.ContactId,
                DelegateeId = tDelegatee.ContactId,
                Status = "pending",
                Note = "Note",
            };

            var anothetTDelegation = new TDelegation()
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(15),
                AccountId = tAccount.AccountId,
                DelegatorId = tDelegator.ContactId,
                DelegateeId = anotherDelegatee.ContactId,
                Status = "pending",
                Note = "Note 2",
            };
            await context.TDelegation.AddRangeAsync(new List<TDelegation> { tDelegation, anothetTDelegation });
            await context.SaveChangesAsync();

            // Try get contact delegation
            var repository = new DelegationRepository(context);
            var delegationList = await repository.GetDelegationsAsync(tDelegator.ContactId, tDelegatee.ContactId);

            Assert.Equal(1, delegationList.Count);
            var delegation = delegationList.FirstOrDefault();
            Assert.NotNull(delegation);
            Assert.Equal(delegation.StartDate, tDelegation.StartDate);
            Assert.Equal(delegation.EndDate, tDelegation.EndDate);
            Assert.Equal(delegation.Status, tDelegation.Status);
            Assert.Equal(delegation.Note, tDelegation.Note);
            Assert.Equal(delegation.CreationDate, tDelegation.CreationDate);
            Assert.Equal(delegation.Account!.AccountId, tDelegation.Account.AccountId);
            Assert.Equal(delegation.Delegator!.ContactId, tDelegation.DelegatorId);
        }
    }
}
