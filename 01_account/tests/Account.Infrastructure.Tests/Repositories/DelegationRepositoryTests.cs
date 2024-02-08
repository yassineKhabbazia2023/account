// <copyright file="DelegationRepositoryTests.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using AutoFixture;
using Kpmg.ExceptionMiddleware.AdvancedExceptions;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Dtos;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

public class DelegationRepositoryTests
{
    private readonly Fixture _fixture;

    public DelegationRepositoryTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public async Task CreateDelegationAsync_When_Request_IsValide_Should_Create_Delegation()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                          .Options;

        using (var context = new AccountContext(options))
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
                GlobalAccountId = tAccount.AccountGlobalUniqueId,
                GlobalDelegatorId = tDelegator.ContactGlobalUniqueId,
                GlobalDelegateeId = tDelegatee.ContactGlobalUniqueId,
            };

            await repository.CreateDelegationAsync(createDelegation);

            var createdDelegation = await context
                .TDelegation
                .FirstOrDefaultAsync(d =>
                    d.Account.AccountGlobalUniqueId == tAccount.AccountGlobalUniqueId
                    &&
                    d.Delegator.ContactGlobalUniqueId == tDelegator.ContactGlobalUniqueId
                    &&
                    d.Delegatee.ContactGlobalUniqueId == tDelegatee.ContactGlobalUniqueId);

            Assert.NotNull(createdDelegation);
            Assert.Equal(createDelegation.StartDate, createdDelegation.StartDate);
            Assert.Equal(createDelegation.EndDate, createdDelegation.EndDate);
            Assert.Equal((int)DelegationStatus.PENDING, createdDelegation.Status);
        }
    }

    [Fact]
    public async Task CreateDelegationAsync_When_Delegator_NotExists__Should_ThrowException()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                          .Options;

        using (var context = new AccountContext(options))
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
                GlobalAccountId = tAccount.AccountGlobalUniqueId,
                GlobalDelegatorId = Guid.Empty,
                GlobalDelegateeId = tDelegatee.ContactGlobalUniqueId,
            };

            var result = await Assert.ThrowsAsync<NotFoundException>(async () => await repository.CreateDelegationAsync(createDelegation));
            Assert.Equal($@"Le contact avec l'identifiant {Guid.Empty} est introuvable", result.Message);
        }
    }

    [Fact]
    public async Task CreateDelegationAsync_When_Delegatee_NotExists__Should_ThrowException()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                          .Options;

        using (var context = new AccountContext(options))
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
                GlobalAccountId = tAccount.AccountGlobalUniqueId,
                GlobalDelegatorId = Guid.Empty,
                GlobalDelegateeId = tDelegator.ContactGlobalUniqueId,
            };

            var result = await Assert.ThrowsAsync<NotFoundException>(async () => await repository.CreateDelegationAsync(createDelegation));
            Assert.Equal($@"Le contact avec l'identifiant {Guid.Empty} est introuvable", result.Message);
        }
    }

    [Fact]
    public async Task CreateDelegationAsync_When_Account_NotExists__Should_ThrowException()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                          .Options;

        using (var context = new AccountContext(options))
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
                GlobalAccountId = Guid.Empty,
                GlobalDelegatorId = tDelegator.ContactGlobalUniqueId,
                GlobalDelegateeId = tDelegatee.ContactGlobalUniqueId,
            };

            var result = await Assert.ThrowsAsync<NotFoundException>(async () => await repository.CreateDelegationAsync(createDelegation));
            Assert.Equal(@"L'identifiant de l'entité indiquée est incorrect", result.Message);
        }
    }

    [Fact]
    public async Task GetContactDelegationsAsync_Should_Return_ContactDelegations()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                          .Options;

        using (var context = new AccountContext(options))
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
                Status = (int)DelegationStatus.PENDING,
                Note = "Note",
            };
            await context.TDelegation.AddAsync(tDelegation);
            await context.SaveChangesAsync();

            // Try get contact delegation
            var repository = new DelegationRepository(context);
            var conactDelegations = await repository.GetContactDelegationsAsync(tDelegatee.ContactGlobalUniqueId);

            Assert.Equal(1, conactDelegations.Count);
            var contactDelegation = conactDelegations.FirstOrDefault();
            Assert.NotNull(contactDelegation);
            Assert.Equal(contactDelegation.StartDate, tDelegation.StartDate);
            Assert.Equal(contactDelegation.EndDate, tDelegation.EndDate);
            Assert.Equal(contactDelegation.Status, (DelegationStatus)tDelegation.Status);
            Assert.Equal(contactDelegation.Note, tDelegation.Note);
            Assert.Equal(contactDelegation.CreationDate, tDelegation.CreationDate);
            Assert.Equal(contactDelegation.Account!.AccountId, tDelegation.Account.AccountGlobalUniqueId);
            Assert.Equal(contactDelegation.Delegator!.ContactId, tDelegation.DelegatorId);
        }
    }

    [Fact]
    public async Task GetDelegationsAsync_Should_Return_Delegations()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                          .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                          .Options;

        using (var context = new AccountContext(options))
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
                Status = (int)DelegationStatus.PENDING,
                Note = "Note",
            };

            var anothetTDelegation = new TDelegation()
            {
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(15),
                AccountId = tAccount.AccountId,
                DelegatorId = tDelegator.ContactId,
                DelegateeId = anotherDelegatee.ContactId,
                Status = (int)DelegationStatus.PENDING,
                Note = "Note 2",
            };
            await context.TDelegation.AddRangeAsync(new List<TDelegation> { tDelegation, anothetTDelegation });
            await context.SaveChangesAsync();

            // Try get contact delegation
            var repository = new DelegationRepository(context);
            var delegationList = await repository.GetDelegationsAsync(tDelegator.ContactGlobalUniqueId, tDelegatee.ContactGlobalUniqueId);

            Assert.Equal(1, delegationList.Count);
            var delegation = delegationList.FirstOrDefault();
            Assert.NotNull(delegation);
            Assert.Equal(delegation.StartDate, tDelegation.StartDate);
            Assert.Equal(delegation.EndDate, tDelegation.EndDate);
            Assert.Equal(delegation.Status, (DelegationStatus)tDelegation.Status);
            Assert.Equal(delegation.Note, tDelegation.Note);
            Assert.Equal(delegation.CreationDate, tDelegation.CreationDate);
            Assert.Equal(delegation.Account!.AccountId, tDelegation.Account.AccountGlobalUniqueId);
            Assert.Equal(delegation.Delegator!.ContactId, tDelegation.DelegatorId);
        }
    }
}
