// <copyright file="DelegationRequestRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Linq;
using AutoFixture;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.Account.Infrastructure.Tests.Helpers;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

public class DelegationRequestRepositoryTests
{
    private static readonly int[] ExpectedRecipientIds = { 2, 3, 4 };

    private readonly Fixture _fixture;
    private readonly DbContextOptions<AccountContext> _dbContextOptions;

    public DelegationRequestRepositoryTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customizations.Add(new OmitNavigationCollectionsSpecimenBuilder());
        _dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task CreateDelegationRequestsAsync_ShouldCreateDelegationRequests()
    {
        using var context = new AccountContext(_dbContextOptions);
        var repository = new DelegationRequestRepository(context);
        var requesterId = 1;
        var accountId = 100;
        var recipientIds = new[] { 2, 3 };
        var status = "pending";

        await repository.CreateDelegationRequestsAsync(requesterId, accountId, recipientIds, status);

        var requests = context.DelegationRequestEntity.ToList();
        requests.Should().HaveCount(2);
        requests.Should().AllSatisfy(r =>
        {
            r.RequesterId.Should().Be(requesterId);
            r.AccountId.Should().Be(accountId);
            r.Status.Should().Be(status);
            r.RespondedAt.Should().BeNull();
            r.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        });
        requests.Select(r => r.RecipientId).Should().BeEquivalentTo(recipientIds);
    }

    [Fact]
    public async Task GetSentRequestsAsync_ShouldReturnPaginatedSentRequests()
    {
        using var context = new AccountContext(_dbContextOptions);
        var requester = CreateContactEntity(1);
        var recipient = CreateContactEntity(2);
        var account = CreateAccountEntity(100);
        var delegationRequest = CreateDelegationRequestEntity(1, requester.ContactId, recipient.ContactId, account.AccountId, "pending");

        context.ContactEntity.AddRange(requester, recipient);
        context.AccountEntity.Add(account);
        context.DelegationRequestEntity.Add(delegationRequest);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        var result = await repository.GetSentRequestsAsync(requester.ContactId, null, pagination);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalItems.Should().Be(1);
        result.TotalPage.Should().Be(1);
        result.CurrentPage.Should().Be(1);
        result.Items.First().RequesterId.Should().Be(requester.ContactId);
        result.Items.First().RecipientId.Should().Be(recipient.ContactId);
        result.Items.First().Recipient.Should().NotBeNull();
        result.Items.First().Recipient.ContactId.Should().Be(recipient.ContactId);
        result.Items.First().Account.Should().NotBeNull();
        result.Items.First().Account.AccountId.Should().Be(account.AccountId);
    }

    [Fact]
    public async Task GetSentRequestsAsync_WithStatusFilter_ShouldReturnFilteredResults()
    {
        using var context = new AccountContext(_dbContextOptions);
        var requester = CreateContactEntity(1);
        var recipient1 = CreateContactEntity(2);
        var recipient2 = CreateContactEntity(3);
        var account = CreateAccountEntity(100);
        var pendingRequest = CreateDelegationRequestEntity(1, requester.ContactId, recipient1.ContactId, account.AccountId, "pending");
        var acceptedRequest = CreateDelegationRequestEntity(2, requester.ContactId, recipient2.ContactId, account.AccountId, "accepted");

        context.ContactEntity.AddRange(requester, recipient1, recipient2);
        context.AccountEntity.Add(account);
        context.DelegationRequestEntity.AddRange(pendingRequest, acceptedRequest);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        var result = await repository.GetSentRequestsAsync(requester.ContactId, "accepted", pagination);

        result.Items.Should().HaveCount(1);
        result.Items.First().Status.Should().Be("accepted");
        result.TotalItems.Should().Be(1);
    }

    [Fact]
    public async Task GetSentRequestsAsync_WithPagination_ShouldReturnCorrectPage()
    {
        using var context = new AccountContext(_dbContextOptions);
        var requester = CreateContactEntity(1);
        var account = CreateAccountEntity(100);
        var recipients = Enumerable.Range(2, 5).Select(i => CreateContactEntity(i)).ToList();
        var requests = recipients.Select((recipient, index) =>
            CreateDelegationRequestEntity(index + 1, requester.ContactId, recipient.ContactId, account.AccountId, "pending")).ToList();

        context.ContactEntity.AddRange(requester);
        context.ContactEntity.AddRange(recipients);
        context.AccountEntity.Add(account);
        context.DelegationRequestEntity.AddRange(requests);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);
        var pagination = new Pagination { PageNumber = 1, PageSize = 3 };

        var result = await repository.GetSentRequestsAsync(requester.ContactId, null, pagination);

        result.Items.Should().HaveCount(3);
        result.TotalItems.Should().Be(5);
        result.TotalPage.Should().Be(2);
    }

    [Fact]
    public async Task GetReceivedRequestsAsync_ShouldReturnPaginatedReceivedRequests()
    {
        using var context = new AccountContext(_dbContextOptions);
        var requester = CreateContactEntity(1);
        var recipient = CreateContactEntity(2);
        var account = CreateAccountEntity(100);
        var delegationRequest = CreateDelegationRequestEntity(1, requester.ContactId, recipient.ContactId, account.AccountId, "pending");

        context.ContactEntity.AddRange(requester, recipient);
        context.AccountEntity.Add(account);
        context.DelegationRequestEntity.Add(delegationRequest);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        var result = await repository.GetReceivedRequestsAsync(recipient.ContactId, null, pagination);

        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.Items.First().RecipientId.Should().Be(recipient.ContactId);
        result.Items.First().Requester.Should().NotBeNull();
        result.Items.First().Requester.ContactId.Should().Be(requester.ContactId);
        result.Items.First().Account.Should().NotBeNull();
        result.Items.First().Account.AccountId.Should().Be(account.AccountId);
    }

    [Fact]
    public async Task GetReceivedRequestsAsync_WithStatusFilter_ShouldReturnFilteredResults()
    {
        using var context = new AccountContext(_dbContextOptions);
        var requester = CreateContactEntity(1);
        var recipient = CreateContactEntity(2);
        var account = CreateAccountEntity(100);
        var pendingRequest = CreateDelegationRequestEntity(1, requester.ContactId, recipient.ContactId, account.AccountId, "pending");
        var refusedRequest = CreateDelegationRequestEntity(2, requester.ContactId, recipient.ContactId, account.AccountId, "refused");

        context.ContactEntity.AddRange(requester, recipient);
        context.AccountEntity.Add(account);
        context.DelegationRequestEntity.AddRange(pendingRequest, refusedRequest);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);
        var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

        var result = await repository.GetReceivedRequestsAsync(recipient.ContactId, new[] { "refused" }, pagination);

        result.Items.Should().HaveCount(1);
        result.Items.First().Status.Should().Be("refused");
        result.TotalItems.Should().Be(1);
    }

    [Fact]
    public async Task HasRoleOnAccountAsync_WhenRoleExists_ShouldReturnTrue()
    {
        using var context = new AccountContext(_dbContextOptions);
        var contact = CreateContactEntity(1);
        var account = CreateAccountEntity(100);
        var role = new RoleEntity
        {
            ContactId = contact.ContactId,
            AccountId = account.AccountId,
            IsFavorite = false,
            IsSignatory = false,
            ActionLevel = 0
        };

        context.ContactEntity.Add(contact);
        context.AccountEntity.Add(account);
        context.RoleEntity.Add(role);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);

        var result = await repository.HasRoleOnAccountAsync(contact.ContactId, account.AccountId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasRoleOnAccountAsync_WhenRoleDoesNotExist_ShouldReturnFalse()
    {
        using var context = new AccountContext(_dbContextOptions);
        var repository = new DelegationRequestRepository(context);

        var result = await repository.HasRoleOnAccountAsync(1, 100);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasActiveDelegationOnAccountAsync_WhenNoDelegationExists_ShouldReturnFalse()
    {
        using var context = new AccountContext(_dbContextOptions);
        var repository = new DelegationRequestRepository(context);

        var result = await repository.HasActiveDelegationOnAccountAsync(1, 100);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPendingRequestAsync_WhenPendingRequestExists_ShouldReturnTrue()
    {
        using var context = new AccountContext(_dbContextOptions);
        var requester = CreateContactEntity(1);
        var recipient = CreateContactEntity(2);
        var account = CreateAccountEntity(100);
        var delegationRequest = CreateDelegationRequestEntity(1, requester.ContactId, recipient.ContactId, account.AccountId, "pending");

        context.ContactEntity.AddRange(requester, recipient);
        context.AccountEntity.Add(account);
        context.DelegationRequestEntity.Add(delegationRequest);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);

        var result = await repository.HasPendingRequestAsync(requester.ContactId, account.AccountId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPendingRequestAsync_WhenNoPendingRequestExists_ShouldReturnFalse()
    {
        using var context = new AccountContext(_dbContextOptions);
        var requester = CreateContactEntity(1);
        var recipient = CreateContactEntity(2);
        var account = CreateAccountEntity(100);
        var delegationRequest = CreateDelegationRequestEntity(1, requester.ContactId, recipient.ContactId, account.AccountId, "accepted");

        context.ContactEntity.AddRange(requester, recipient);
        context.AccountEntity.Add(account);
        context.DelegationRequestEntity.Add(delegationRequest);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);

        var result = await repository.HasPendingRequestAsync(requester.ContactId, account.AccountId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DoesAccountExistAsync_WhenAccountExists_ShouldReturnTrue()
    {
        using var context = new AccountContext(_dbContextOptions);
        var account = CreateAccountEntity(100);
        context.AccountEntity.Add(account);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);

        var result = await repository.DoesAccountExistAsync(account.AccountId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DoesAccountExistAsync_WhenAccountDoesNotExist_ShouldReturnFalse()
    {
        using var context = new AccountContext(_dbContextOptions);
        var repository = new DelegationRequestRepository(context);

        var result = await repository.DoesAccountExistAsync(999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task DoesContactExistAsync_WhenContactExists_ShouldReturnTrue()
    {
        using var context = new AccountContext(_dbContextOptions);
        var contact = CreateContactEntity(1);
        context.ContactEntity.Add(contact);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);

        var result = await repository.DoesContactExistAsync(contact.ContactId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task DoesContactExistAsync_WhenContactDoesNotExist_ShouldReturnFalse()
    {
        using var context = new AccountContext(_dbContextOptions);
        var repository = new DelegationRequestRepository(context);

        var result = await repository.DoesContactExistAsync(999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetPendingRequestsByIdsAndRecipientAsync_ShouldReturnOnlyMatchingRequests()
    {
        using var context = new AccountContext(_dbContextOptions);
        var requester = CreateContactEntity(1);
        var recipient = CreateContactEntity(2);
        var otherRecipient = CreateContactEntity(3);
        var account = CreateAccountEntity(100);
        var pendingForRecipient = CreateDelegationRequestEntity(1, requester.ContactId, recipient.ContactId, account.AccountId, "pending");
        var pendingForOther = CreateDelegationRequestEntity(2, requester.ContactId, otherRecipient.ContactId, account.AccountId, "pending");
        var acceptedForRecipient = CreateDelegationRequestEntity(3, requester.ContactId, recipient.ContactId, account.AccountId, "accepted");

        context.ContactEntity.AddRange(requester, recipient, otherRecipient);
        context.AccountEntity.Add(account);
        context.DelegationRequestEntity.AddRange(pendingForRecipient, pendingForOther, acceptedForRecipient);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);

        var result = await repository.GetPendingRequestsByIdsAndRecipientAsync(new[] { 1, 2, 3 }, recipient.ContactId);

        result.Should().HaveCount(1);
        result.First().DelegationRequestId.Should().Be(1);
        result.First().RecipientId.Should().Be(recipient.ContactId);
        result.First().Status.Should().Be("pending");
    }

    [Fact]
    public async Task GetPendingRequestsByIdsAndRecipientAsync_WhenNoMatchingRequests_ShouldReturnEmptyList()
    {
        using var context = new AccountContext(_dbContextOptions);
        var repository = new DelegationRequestRepository(context);

        var result = await repository.GetPendingRequestsByIdsAndRecipientAsync(new[] { 999 }, 1);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task AreAllSiblingRequestsRefusedAsync_ShouldReturnFalse_WhenPendingRequestExists()
    {
        using var context = new AccountContext(_dbContextOptions);
        var requester = CreateContactEntity(1);
        var recipient1 = CreateContactEntity(2);
        var recipient2 = CreateContactEntity(3);
        var account = CreateAccountEntity(100);
        var refusedRequest = CreateDelegationRequestEntity(1, requester.ContactId, recipient1.ContactId, account.AccountId, "refused");
        var pendingRequest = CreateDelegationRequestEntity(2, requester.ContactId, recipient2.ContactId, account.AccountId, "pending");

        context.ContactEntity.AddRange(requester, recipient1, recipient2);
        context.AccountEntity.Add(account);
        context.DelegationRequestEntity.AddRange(refusedRequest, pendingRequest);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);

        var result = await repository.AreAllSiblingRequestsRefusedAsync(requester.ContactId, account.AccountId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AreAllSiblingRequestsRefusedAsync_ShouldReturnFalse_WhenAcceptedRequestExists()
    {
        using var context = new AccountContext(_dbContextOptions);
        var requester = CreateContactEntity(1);
        var recipient = CreateContactEntity(2);
        var account = CreateAccountEntity(100);
        var acceptedRequest = CreateDelegationRequestEntity(1, requester.ContactId, recipient.ContactId, account.AccountId, "accepted");

        context.ContactEntity.AddRange(requester, recipient);
        context.AccountEntity.Add(account);
        context.DelegationRequestEntity.Add(acceptedRequest);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);

        var result = await repository.AreAllSiblingRequestsRefusedAsync(requester.ContactId, account.AccountId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AreAllSiblingRequestsRefusedAsync_ShouldReturnTrue_WhenAllSiblingRequestsAreRefused()
    {
        using var context = new AccountContext(_dbContextOptions);
        var requester = CreateContactEntity(1);
        var recipient1 = CreateContactEntity(2);
        var recipient2 = CreateContactEntity(3);
        var account = CreateAccountEntity(100);
        var refusedRequest1 = CreateDelegationRequestEntity(1, requester.ContactId, recipient1.ContactId, account.AccountId, "refused");
        var refusedRequest2 = CreateDelegationRequestEntity(2, requester.ContactId, recipient2.ContactId, account.AccountId, "refused");

        context.ContactEntity.AddRange(requester, recipient1, recipient2);
        context.AccountEntity.Add(account);
        context.DelegationRequestEntity.AddRange(refusedRequest1, refusedRequest2);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);

        var result = await repository.AreAllSiblingRequestsRefusedAsync(requester.ContactId, account.AccountId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task AreAllSiblingRequestsRefusedAsync_ShouldReturnFalse_WhenNoSiblingRequestsExist()
    {
        using var context = new AccountContext(_dbContextOptions);
        var repository = new DelegationRequestRepository(context);

        var result = await repository.AreAllSiblingRequestsRefusedAsync(999, 999);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task AcceptRequestsAsync_ShouldAcceptMultipleRequestsByIds()
    {
        using var context = CreateSqliteContext();
        var requester = CreateContactEntity(1);
        var recipient1 = CreateContactEntity(2);
        var recipient2 = CreateContactEntity(3);
        var account = CreateAccountEntity(100);
        var request1 = CreateDelegationRequestEntity(1, requester.ContactId, recipient1.ContactId, account.AccountId, "pending");
        var request2 = CreateDelegationRequestEntity(2, requester.ContactId, recipient2.ContactId, account.AccountId, "pending");

        context.ContactEntity.AddRange(requester, recipient1, recipient2);
        context.AccountEntity.Add(account);
        context.DelegationRequestEntity.AddRange(request1, request2);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);
        var respondedAt = DateTime.UtcNow;
        var idsToAccept = new[] { 1, 2 };

        await repository.AcceptRequestsAsync(idsToAccept, respondedAt);
        context.ChangeTracker.Clear();

        var results = context.DelegationRequestEntity
            .Where(dr => idsToAccept.Contains(dr.DelegationRequestId))
            .OrderBy(dr => dr.DelegationRequestId)
            .ToList();

        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r =>
        {
            r.Status.Should().Be(DelegationStatusValues.Accepted);
            r.RespondedAt.Should().BeCloseTo(respondedAt, TimeSpan.FromSeconds(1));
        });
    }

    [Fact]
    public async Task AcceptRequestsAsync_ShouldNotAffectOtherRequests()
    {
        using var context = CreateSqliteContext();
        var requester = CreateContactEntity(1);
        var recipient1 = CreateContactEntity(2);
        var recipient2 = CreateContactEntity(3);
        var account = CreateAccountEntity(100);
        var request1 = CreateDelegationRequestEntity(1, requester.ContactId, recipient1.ContactId, account.AccountId, "pending");
        var request2 = CreateDelegationRequestEntity(2, requester.ContactId, recipient2.ContactId, account.AccountId, "pending");

        context.ContactEntity.AddRange(requester, recipient1, recipient2);
        context.AccountEntity.Add(account);
        context.DelegationRequestEntity.AddRange(request1, request2);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);
        var respondedAt = DateTime.UtcNow;

        await repository.AcceptRequestsAsync(new[] { 1 }, respondedAt);
        context.ChangeTracker.Clear();

        var untouched = context.DelegationRequestEntity.First(dr => dr.DelegationRequestId == 2);
        untouched.Status.Should().Be("pending");
        untouched.RespondedAt.Should().BeNull();
    }

    [Fact]
    public async Task RefuseRequestsAsync_ShouldRefuseMultipleRequestsByIds()
    {
        using var context = CreateSqliteContext();
        var requester = CreateContactEntity(1);
        var recipient1 = CreateContactEntity(2);
        var recipient2 = CreateContactEntity(3);
        var account = CreateAccountEntity(100);
        var request1 = CreateDelegationRequestEntity(1, requester.ContactId, recipient1.ContactId, account.AccountId, "pending");
        var request2 = CreateDelegationRequestEntity(2, requester.ContactId, recipient2.ContactId, account.AccountId, "pending");

        context.ContactEntity.AddRange(requester, recipient1, recipient2);
        context.AccountEntity.Add(account);
        context.DelegationRequestEntity.AddRange(request1, request2);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);
        var respondedAt = DateTime.UtcNow;
        var idsToRefuse = new[] { 1, 2 };

        await repository.RefuseRequestsAsync(idsToRefuse, respondedAt);
        context.ChangeTracker.Clear();

        var results = context.DelegationRequestEntity
            .Where(dr => idsToRefuse.Contains(dr.DelegationRequestId))
            .OrderBy(dr => dr.DelegationRequestId)
            .ToList();

        results.Should().HaveCount(2);
        results.Should().AllSatisfy(r =>
        {
            r.Status.Should().Be(DelegationStatusValues.Refused);
            r.RespondedAt.Should().BeCloseTo(respondedAt, TimeSpan.FromSeconds(1));
        });
    }

    [Fact]
    public async Task RefuseRequestsAsync_WithPartialIds_ShouldRefuseOnlySelectedRequests()
    {
        using var context = CreateSqliteContext();
        var requester = CreateContactEntity(1);
        var recipients = Enumerable.Range(2, 3).Select(i => CreateContactEntity(i)).ToList();
        var account = CreateAccountEntity(100);
        var requests = recipients.Select((r, i) =>
            CreateDelegationRequestEntity(i + 1, requester.ContactId, r.ContactId, account.AccountId, "pending")).ToList();

        context.ContactEntity.AddRange(requester);
        context.ContactEntity.AddRange(recipients);
        context.AccountEntity.Add(account);
        context.DelegationRequestEntity.AddRange(requests);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);
        var respondedAt = DateTime.UtcNow;
        var idsToRefuse = new[] { 1, 2 };

        await repository.RefuseRequestsAsync(idsToRefuse, respondedAt);
        context.ChangeTracker.Clear();

        var refused = context.DelegationRequestEntity.Where(dr => dr.Status == DelegationStatusValues.Refused).ToList();
        var pending = context.DelegationRequestEntity.Where(dr => dr.Status == "pending").ToList();

        refused.Should().HaveCount(2);
        refused.Select(r => r.DelegationRequestId).Should().BeEquivalentTo(idsToRefuse);
        pending.Should().HaveCount(1);
        pending.First().DelegationRequestId.Should().Be(3);
    }

    [Fact]
    public async Task RefuseRequestsAsync_WithEmptyIdArray_ShouldNotRefuseAnyRequests()
    {
        using var context = CreateSqliteContext();
        var requester = CreateContactEntity(1);
        var recipient = CreateContactEntity(2);
        var account = CreateAccountEntity(100);
        var request = CreateDelegationRequestEntity(1, requester.ContactId, recipient.ContactId, account.AccountId, "pending");

        context.ContactEntity.AddRange(requester, recipient);
        context.AccountEntity.Add(account);
        context.DelegationRequestEntity.Add(request);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);
        var respondedAt = DateTime.UtcNow;

        await repository.RefuseRequestsAsync(Array.Empty<int>(), respondedAt);
        context.ChangeTracker.Clear();

        var allRequests = context.DelegationRequestEntity.ToList();
        allRequests.Should().HaveCount(1);
        allRequests.First().Status.Should().Be("pending");
        allRequests.First().RespondedAt.Should().BeNull();
    }

    [Fact]
    public async Task AcceptSiblingRequestsAsync_ShouldAcceptOnlyPendingSiblingRequests()
    {
        using var context = CreateSqliteContext();
        var requester = CreateContactEntity(1);
        var recipient1 = CreateContactEntity(2);
        var recipient2 = CreateContactEntity(3);
        var account = CreateAccountEntity(100);
        var pendingRequest1 = CreateDelegationRequestEntity(1, requester.ContactId, recipient1.ContactId, account.AccountId, "pending");
        var pendingRequest2 = CreateDelegationRequestEntity(2, requester.ContactId, recipient2.ContactId, account.AccountId, "pending");
        var refusedRequest = CreateDelegationRequestEntity(3, requester.ContactId, recipient2.ContactId, account.AccountId, "refused");

        context.ContactEntity.AddRange(requester, recipient1, recipient2);
        context.AccountEntity.Add(account);
        context.DelegationRequestEntity.AddRange(pendingRequest1, pendingRequest2, refusedRequest);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);
        var respondedAt = DateTime.UtcNow;

        await repository.AcceptSiblingRequestsAsync(requester.ContactId, account.AccountId, respondedAt);
        context.ChangeTracker.Clear();

        var accepted = context.DelegationRequestEntity.Where(dr => dr.Status == DelegationStatusValues.Accepted).ToList();
        var refused = context.DelegationRequestEntity.Where(dr => dr.Status == DelegationStatusValues.Refused).ToList();

        accepted.Should().HaveCount(2);
        accepted.Select(dr => dr.DelegationRequestId).Should().BeEquivalentTo(new[] { 1, 2 });
        refused.Should().HaveCount(1);
        refused.First().DelegationRequestId.Should().Be(3);
    }

    [Fact]
    public async Task AcceptSiblingRequestsAsync_ShouldNotAffectOtherAccountRequests()
    {
        using var context = CreateSqliteContext();
        var requester = CreateContactEntity(1);
        var recipient = CreateContactEntity(2);
        var account1 = CreateAccountEntity(100);
        var account2 = CreateAccountEntity(200);
        var request1 = CreateDelegationRequestEntity(1, requester.ContactId, recipient.ContactId, account1.AccountId, "pending");
        var request2 = CreateDelegationRequestEntity(2, requester.ContactId, recipient.ContactId, account2.AccountId, "pending");

        context.ContactEntity.AddRange(requester, recipient);
        context.AccountEntity.AddRange(account1, account2);
        context.DelegationRequestEntity.AddRange(request1, request2);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);
        var respondedAt = DateTime.UtcNow;

        await repository.AcceptSiblingRequestsAsync(requester.ContactId, account1.AccountId, respondedAt);
        context.ChangeTracker.Clear();

        var accepted = context.DelegationRequestEntity.Where(dr => dr.Status == DelegationStatusValues.Accepted).ToList();
        var pending = context.DelegationRequestEntity.Where(dr => dr.Status == "pending").ToList();

        accepted.Should().HaveCount(1);
        accepted.First().DelegationRequestId.Should().Be(1);
        pending.Should().HaveCount(1);
        pending.First().DelegationRequestId.Should().Be(2);
    }

    [Fact]
    public async Task AcceptSiblingRequestsAsync_WithMultipleSiblings_ShouldAcceptAllPending()
    {
        using var context = CreateSqliteContext();
        var requester = CreateContactEntity(1);
        var recipients = Enumerable.Range(2, 4).Select(i => CreateContactEntity(i)).ToList();
        var account = CreateAccountEntity(100);
        var requests = recipients.Select((r, i) =>
            CreateDelegationRequestEntity(i + 1, requester.ContactId, r.ContactId, account.AccountId, "pending")).ToList();

        context.ContactEntity.AddRange(requester);
        context.ContactEntity.AddRange(recipients);
        context.AccountEntity.Add(account);
        context.DelegationRequestEntity.AddRange(requests);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);
        var respondedAt = DateTime.UtcNow;

        await repository.AcceptSiblingRequestsAsync(requester.ContactId, account.AccountId, respondedAt);
        context.ChangeTracker.Clear();

        var allAccepted = context.DelegationRequestEntity.Where(dr => dr.Status == DelegationStatusValues.Accepted).ToList();

        allAccepted.Should().HaveCount(4);
        allAccepted.Should().AllSatisfy(r => r.RespondedAt.Should().BeCloseTo(respondedAt, TimeSpan.FromSeconds(1)));
    }

    [Fact]
    public async Task GetAcceptedSiblingRequestsAsync_ShouldReturnOnlyRequestsAcceptedAtGivenDate()
    {
        using var context = new AccountContext(_dbContextOptions);
        var requester = CreateContactEntity(1);
        var account = CreateAccountEntity(100);
        var otherAccount = CreateAccountEntity(200);
        var respondedAt = DateTime.UtcNow;

        var accepted1 = CreateDelegationRequestEntity(1, requester.ContactId, 2, account.AccountId, "accepted");
        accepted1.RespondedAt = respondedAt;
        var accepted2 = CreateDelegationRequestEntity(2, requester.ContactId, 3, account.AccountId, "accepted");
        accepted2.RespondedAt = respondedAt;
        var accepted3 = CreateDelegationRequestEntity(3, requester.ContactId, 4, account.AccountId, "accepted");
        accepted3.RespondedAt = respondedAt;
        var previousRoundAccepted = CreateDelegationRequestEntity(4, requester.ContactId, 5, account.AccountId, "accepted");
        previousRoundAccepted.RespondedAt = respondedAt.AddDays(-30);
        var refused = CreateDelegationRequestEntity(5, requester.ContactId, 6, account.AccountId, "refused");
        refused.RespondedAt = respondedAt;
        var otherAccountAccepted = CreateDelegationRequestEntity(6, requester.ContactId, 7, otherAccount.AccountId, "accepted");
        otherAccountAccepted.RespondedAt = respondedAt;

        context.ContactEntity.Add(requester);
        context.AccountEntity.AddRange(account, otherAccount);
        context.DelegationRequestEntity.AddRange(accepted1, accepted2, accepted3, previousRoundAccepted, refused, otherAccountAccepted);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);

        var result = await repository.GetAcceptedSiblingRequestsAsync(requester.ContactId, account.AccountId, respondedAt);

        result.Should().HaveCount(3);
        result.Select(r => r.RecipientId).Should().BeEquivalentTo(ExpectedRecipientIds);
    }

    [Fact]
    public async Task GetAcceptedSiblingRequestsAsync_ShouldTolerateSmallPrecisionShift()
    {
        using var context = new AccountContext(_dbContextOptions);
        var requester = CreateContactEntity(1);
        var account = CreateAccountEntity(100);
        var respondedAt = DateTime.UtcNow;

        var acceptedWithinTolerance = CreateDelegationRequestEntity(1, requester.ContactId, 2, account.AccountId, "accepted");
        acceptedWithinTolerance.RespondedAt = respondedAt.AddMilliseconds(-50);
        var acceptedOutsideTolerance = CreateDelegationRequestEntity(2, requester.ContactId, 3, account.AccountId, "accepted");
        acceptedOutsideTolerance.RespondedAt = respondedAt.AddMilliseconds(200);

        context.ContactEntity.Add(requester);
        context.AccountEntity.Add(account);
        context.DelegationRequestEntity.AddRange(acceptedWithinTolerance, acceptedOutsideTolerance);
        context.SaveChanges();

        var repository = new DelegationRequestRepository(context);

        var result = await repository.GetAcceptedSiblingRequestsAsync(requester.ContactId, account.AccountId, respondedAt);

        result.Should().ContainSingle();
        result.First().RecipientId.Should().Be(2);
    }

    [Fact]
    public async Task GetAcceptedSiblingRequestsAsync_WhenNoMatchingRequests_ShouldReturnEmptyList()
    {
        using var context = new AccountContext(_dbContextOptions);
        var repository = new DelegationRequestRepository(context);

        var result = await repository.GetAcceptedSiblingRequestsAsync(1, 100, DateTime.UtcNow);

        result.Should().BeEmpty();
    }

    private static ContactEntity CreateContactEntity(int contactId)
    {
        return new ContactEntity
        {
            ContactId = contactId,
            ContactGlobalUniqueId = Guid.NewGuid(),
            FirstName = $"FirstName{contactId}",
            LastName = $"LastName{contactId}",
            Email = $"contact{contactId}@test.com",
            LandPhone = string.Empty,
            MobilePhone = string.Empty,
            Type = "1",
            Status = "active",
            PersonaName = string.Empty,
            Office = string.Empty,
            CreationDate = DateTime.UtcNow,
            IsActive = true
        };
    }

    private static AccountEntity CreateAccountEntity(int accountId)
    {
        return new AccountEntity
        {
            AccountId = accountId,
            AccountGlobalUniqueId = Guid.NewGuid(),
            AccountNumber = $"ACC{accountId}",
            LegalName = $"LegalName{accountId}",
            CommercialName = string.Empty,
            AccountType = "REGULAR",
            Email = string.Empty,
            DeliveryEmail = string.Empty,
            BillingEmail = string.Empty,
            DeliveryFax = string.Empty,
            BillingFax = string.Empty,
            IsActive = true,
            SectorCode = string.Empty,
            Sector = string.Empty,
            StaffSizeRange = string.Empty,
            AccountingMethod = string.Empty,
            LegalFormCode = string.Empty,
            LegalForm = string.Empty,
            FiscalSystem = string.Empty,
            Isin = string.Empty,
            Siret = string.Empty,
            TaxationSystem = string.Empty,
            ActivityDescription = string.Empty,
            ActivityType = string.Empty,
            Vat = string.Empty,
            Vatintra = string.Empty,
            Vattype = string.Empty,
            SourceName = string.Empty,
            CreatedBy = string.Empty,
            ModifiedBy = string.Empty,
            CreationDate = DateTime.UtcNow,
            IconName = string.Empty,
            MissionType = string.Empty
        };
    }

    private static DelegationRequestEntity CreateDelegationRequestEntity(int id, int requesterId, int recipientId, int accountId, string status)
    {
        return new DelegationRequestEntity
        {
            DelegationRequestId = id,
            RequesterId = requesterId,
            RecipientId = recipientId,
            AccountId = accountId,
            CreatedAt = DateTime.UtcNow.AddMinutes(-id),
            Status = status,
            RespondedAt = status == "pending" ? null : DateTime.UtcNow
        };
    }

    private static AccountContext CreateSqliteContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        connection.CreateFunction("newid", () => Guid.NewGuid().ToString());
        var options = new DbContextOptionsBuilder<AccountContext>()
            .UseSqlite(connection)
            .Options;
        var context = new AccountContext(options);
        context.Database.EnsureCreated();
        context.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;");
        return context;
    }
}
