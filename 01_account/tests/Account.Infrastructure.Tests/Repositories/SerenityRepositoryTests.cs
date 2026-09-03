// <copyright file="SerenityRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Constants;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.Account.Infrastructure.Tests.Context;
using Pulse.ExceptionMiddleware.Exceptions;
using Xunit;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

public class SerenityRepositoryTests
{
    private const int ContactId = 777;

    private readonly DbContextOptions<AccountContext> _dbContextOptions;

    public SerenityRepositoryTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    #region Eligibility

    [Theory]
    // Entite eligible : code de routage B2B et aucune adresse electronique.
    [InlineData(AccountRoutingCodes.B2B, null, true, true)]
    // Casse : la base est en CI_AS, l egalite doit rester insensible a la casse.
    [InlineData("0-b2b", null, true, true)]
    // Adresse electronique vide ou blanche : toujours eligible.
    [InlineData(AccountRoutingCodes.B2B, "", true, true)]
    [InlineData(AccountRoutingCodes.B2B, "   ", true, true)]
    // Adresse electronique renseignee : ecartee.
    [InlineData(AccountRoutingCodes.B2B, "PDP-12345", true, false)]
    // Autre code de routage : ecartee.
    [InlineData("0-B2G", null, true, false)]
    [InlineData(null, null, true, false)]
    // Le contact ne detient aucun role sur l entite : hors portefeuille.
    [InlineData(AccountRoutingCodes.B2B, null, false, false)]
    public async Task GetSerenityEligibilityAsync_ShouldSelectOnlyMatchingPortfolioEntities(
        string? accountRoutingCode,
        string? accountElectronicAddressId,
        bool contactHasRole,
        bool expectedCandidate)
    {
        // Arrange
        using var context = new TestAccountContext(_dbContextOptions);
        var account = BuildAccount(context, accountRoutingCode, accountElectronicAddressId, contactHasRole ? ContactId : 999);
        await context.SaveChangesAsync();

        var repository = new SerenityRepository(context);

        // Act
        var result = await repository.GetSerenityEligibilityAsync(ContactId);

        // Assert
        result.HasMadeChoice.Should().BeFalse();
        if (expectedCandidate)
        {
            result.CandidateAccountIds.Should().Contain(account.AccountId);
        }
        else
        {
            result.CandidateAccountIds.Should().NotContain(account.AccountId);
        }
    }

    [Fact]
    public async Task GetSerenityEligibilityAsync_WhenAccountIsProspect_ShouldExcludeIt()
    {
        // Arrange
        using var context = new TestAccountContext(_dbContextOptions);
        var account = BuildAccount(context, AccountRoutingCodes.B2B, null, ContactId);
        account.AccountType = GlobalConstants.ProspectAccountType;
        await context.SaveChangesAsync();

        var repository = new SerenityRepository(context);

        // Act
        var result = await repository.GetSerenityEligibilityAsync(ContactId);

        // Assert
        result.CandidateAccountIds.Should().BeEmpty();
    }

    [Fact]
    public async Task GetSerenityEligibilityAsync_WhenAccountIsInactive_ShouldExcludeIt()
    {
        // Arrange
        using var context = new TestAccountContext(_dbContextOptions);
        var account = BuildAccount(context, AccountRoutingCodes.B2B, null, ContactId);
        account.IsActive = false;
        await context.SaveChangesAsync();

        var repository = new SerenityRepository(context);

        // Act
        var result = await repository.GetSerenityEligibilityAsync(ContactId);

        // Assert
        result.CandidateAccountIds.Should().BeEmpty();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetSerenityEligibilityAsync_WhenChoiceExists_ShouldShortCircuit(bool isAccepted)
    {
        // Arrange : une entite parfaitement eligible, mais un choix a deja ete exprime.
        // Un choix FALSE doit eteindre la modal autant qu un choix TRUE.
        using var context = new TestAccountContext(_dbContextOptions);
        BuildAccount(context, AccountRoutingCodes.B2B, null, ContactId);
        context.SerenityChoiceEntity.Add(new SerenityChoiceEntity
        {
            ContactId = ContactId,
            IsAccepted = isAccepted,
            ChoiceDate = DateTime.UtcNow,
        });
        await context.SaveChangesAsync();

        var repository = new SerenityRepository(context);

        // Act
        var result = await repository.GetSerenityEligibilityAsync(ContactId);

        // Assert
        result.HasMadeChoice.Should().BeTrue();
        result.CandidateAccountIds.Should().BeEmpty();
    }

    #endregion

    #region Create choice

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateSerenityChoiceAsync_ShouldPersistTheChoice(bool isAccepted)
    {
        // Arrange
        using var context = new TestAccountContext(_dbContextOptions);
        var repository = new SerenityRepository(context);

        // Act
        await repository.CreateSerenityChoiceAsync(ContactId, isAccepted);

        // Assert
        var stored = await context.SerenityChoiceEntity.SingleAsync(choice => choice.ContactId == ContactId);
        stored.IsAccepted.Should().Be(isAccepted);
        stored.ChoiceDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task CreateSerenityChoiceAsync_WhenChoiceAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        using var context = new TestAccountContext(_dbContextOptions);
        var repository = new SerenityRepository(context);
        await repository.CreateSerenityChoiceAsync(ContactId, true);

        // Act
        var act = async () => await repository.CreateSerenityChoiceAsync(ContactId, false);

        // Assert
        var exception = await act.Should().ThrowAsync<ConflictException>();
        exception.And.Code.Should().Be(Errors.SerenityChoiceAlreadyExistsCode);
    }

    #endregion

    #region Reset choice

    /// <summary>
    /// Verifies that reset deletes exactly the persisted Serenity row for the target contact.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ResetSerenityChoiceAsync_WhenChoiceExists_ShouldDeleteOnlyTargetChoice()
    {
        // Arrange
        const int otherContactId = 778;
        using var context = new TestAccountContext(_dbContextOptions);
        context.SerenityChoiceEntity.AddRange(
            new SerenityChoiceEntity { ContactId = ContactId, IsAccepted = true, ChoiceDate = DateTime.UtcNow },
            new SerenityChoiceEntity { ContactId = otherContactId, IsAccepted = false, ChoiceDate = DateTime.UtcNow });
        await context.SaveChangesAsync();
        var repository = new SerenityRepository(context);

        // Act
        await repository.ResetSerenityChoiceAsync(ContactId);

        // Assert
        context.SerenityChoiceEntity.Should().NotContain(choice => choice.ContactId == ContactId);
        var otherChoice = await context.SerenityChoiceEntity.SingleAsync(choice => choice.ContactId == otherContactId);
        otherChoice.IsAccepted.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that resetting an absent or already-reset choice succeeds without creating state.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ResetSerenityChoiceAsync_WhenChoiceDoesNotExist_ShouldBeIdempotent()
    {
        // Arrange
        using var context = new TestAccountContext(_dbContextOptions);
        var repository = new SerenityRepository(context);

        // Act
        await repository.ResetSerenityChoiceAsync(ContactId);
        await repository.ResetSerenityChoiceAsync(ContactId);

        // Assert
        context.SerenityChoiceEntity.Should().BeEmpty();
        context.ChangeTracker.HasChanges().Should().BeFalse();
    }

    /// <summary>
    /// Verifies that repository failures are not mistaken for a successful idempotent reset.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ResetSerenityChoiceAsync_WhenRepositoryContextFails_ShouldPropagateFailure()
    {
        // Arrange
        var context = new TestAccountContext(_dbContextOptions);
        var repository = new SerenityRepository(context);
        await context.DisposeAsync();

        // Act
        var action = () => repository.ResetSerenityChoiceAsync(ContactId);

        // Assert
        await action.Should().ThrowAsync<ObjectDisposedException>();
    }

    #endregion

    private static AccountEntity BuildAccount(
        TestAccountContext context,
        string? accountRoutingCode,
        string? accountElectronicAddressId,
        int contactId)
    {
        var contact = new ContactEntity
        {
            ContactId = contactId,
            FirstName = "first",
            LastName = "last",
            Email = $"contact{contactId}@test.fr",
            CreationDate = DateTime.UtcNow,
            PersonaName = "persona",
            Type = ContactType.Customer.ToString(),
            IsActive = true,
        };

        var account = new AccountEntity
        {
            AccountNumber = "199900046522",
            LegalName = "test sca",
            CreatedBy = "tests",
            IsActive = true,
            AccountType = AccountType.CLIENT.ToString(),
            AccountRoutingCode = accountRoutingCode,
            AccountElectronicAddressId = accountElectronicAddressId,
            RoleEntity = [new RoleEntity { Contact = contact }],
        };

        context.AccountEntity.Add(account);

        return account;
    }
}
