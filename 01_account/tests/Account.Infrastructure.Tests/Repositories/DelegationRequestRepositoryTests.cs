// <copyright file="DelegationRequestRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Linq;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

public class DelegationRequestRepositoryTests
{
    private readonly Fixture _fixture;
    private readonly DbContextOptions<AccountContext> _dbContextOptions;

    public DelegationRequestRepositoryTests()
    {
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
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

        var result = await repository.GetReceivedRequestsAsync(recipient.ContactId, "refused", pagination);

        result.Items.Should().HaveCount(1);
        result.Items.First().Status.Should().Be("refused");
        result.TotalItems.Should().Be(1);
    }

    [Fact]
    public async Task HasRequesterAccessToAccountAsync_WhenRoleExists_ShouldReturnTrue()
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

        var result = await repository.HasRequesterAccessToAccountAsync(contact.ContactId, account.AccountId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasRequesterAccessToAccountAsync_WhenRoleDoesNotExist_ShouldReturnFalse()
    {
        using var context = new AccountContext(_dbContextOptions);
        var repository = new DelegationRequestRepository(context);

        var result = await repository.HasRequesterAccessToAccountAsync(1, 100);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasRecipientAccessToAccountAsync_WhenRoleExists_ShouldReturnTrue()
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

        var result = await repository.HasRecipientAccessToAccountAsync(contact.ContactId, account.AccountId);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasRecipientAccessToAccountAsync_WhenRoleDoesNotExist_ShouldReturnFalse()
    {
        using var context = new AccountContext(_dbContextOptions);
        var repository = new DelegationRequestRepository(context);

        var result = await repository.HasRecipientAccessToAccountAsync(1, 100);

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
}
