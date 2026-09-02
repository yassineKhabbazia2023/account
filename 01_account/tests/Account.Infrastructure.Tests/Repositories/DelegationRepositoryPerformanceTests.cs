// <copyright file="DelegationRepositoryPerformanceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Diagnostics;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.Account.Infrastructure.Tests.Helpers;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

/// <summary>
/// Tests de performance pour les requêtes du repository de délégations.
/// Ces tests mesurent le temps d'exécution avec un volume important de données (100+ délégations).
/// </summary>
public class DelegationRepositoryPerformanceTests
{
    private readonly Fixture _fixture;
    private readonly ITestOutputHelper _output;

    public DelegationRepositoryPerformanceTests(ITestOutputHelper output)
    {
        _output = output;
        _fixture = new Fixture();
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        _fixture.Customizations.Add(new OmitNavigationCollectionsSpecimenBuilder());
    }

    /// <summary>
    /// Teste la performance de GetDelegatorDelegationsAsync avec 150 délégations.
    /// Cette méthode contient une sous-requête complexe (uniquePerDelegatee).
    /// </summary>
    [Fact]
    public async Task GetDelegatorDelegationsAsync_With150Delegations_ShouldCompleteInReasonableTime()
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        var stopwatch = new Stopwatch();
        var delegatorId = 1;
        var delegationCount = 150;

        using (var context = new AccountContext(dbContextOptions))
        {
            // Créer le délégant
            var delegator = new ContactEntity
            {
                ContactId = delegatorId,
                Email = "delegator@test.fr",
                FirstName = "Jean",
                LastName = "Dupont",
                Type = "collaborator",
                Status = "Declared",
                PersonaName = "Collaborateur",
                Office = "Paris",
                CreationDate = DateTime.UtcNow,
                IsActive = true
            };
            context.ContactEntity.Add(delegator);
            await context.SaveChangesAsync();

            // Créer des comptes supplémentaires pour les relations many-to-many
            var additionalAccounts = new List<AccountEntity>();
            for (int i = 2; i <= 10; i++)
            {
                additionalAccounts.Add(new AccountEntity
                {
                    AccountId = i,
                    AccountNumber = $"ACC{i:D3}",
                    LegalName = $"Société {i}",
                    CreatedBy = "system",
                    Email = $"account{i}@test.fr",
                    IsActive = true
                });
            }

            context.AccountEntity.AddRange(additionalAccounts);
            await context.SaveChangesAsync();

            // Créer des délégués et des délégations avec plusieurs comptes par délégation
            var delegationStatus = new List<string> { DelegationStatus.Pending.ToString(), DelegationStatus.Enabled.ToString() };
            for (var i = 1; i <= delegationCount; i++)
            {
                var delegateeId = i * 10;

                var delegatee = new ContactEntity
                {
                    ContactId = delegateeId,
                    Email = $"delegatee-{delegateeId}@test.fr",
                    FirstName = $"Delegatee-FN-{delegateeId}",
                    LastName = $"Delegatee-LN-{delegateeId}",
                    Type = "collaborator",
                    Status = "Declared",
                    PersonaName = "Collaborateur",
                    Office = "Lyon",
                    CreationDate = DateTime.UtcNow,
                    IsActive = true
                };
                context.ContactEntity.Add(delegatee);
                await context.SaveChangesAsync();

                // Créer plusieurs délégations pour le même couple delegator/delegatee
                // pour stresser la sous-requête uniquePerDelegatee
                var delegationsPerDelegatee = (i % 3) + 1;
                for (int j = 0; j < delegationsPerDelegatee; j++)
                {
                    var accountsForDelegation = new List<AccountEntity> { additionalAccounts.First() };
                    var accountsToAdd = (i % 3) + 1;
                    for (int k = 0; k < accountsToAdd && k < additionalAccounts.Count; k++)
                    {
                        accountsForDelegation.Add(additionalAccounts[k]);
                    }

                    var delegation = new DelegationEntity
                    {
                        DelegationId = (i * 1000) + j,
                        StartDate = DateTime.UtcNow.AddDays(-i),
                        EndDate = DateTime.UtcNow.AddMonths(i),
                        DelegatorId = delegatorId,
                        DelegateeId = delegateeId,
                        Status = delegationStatus[j % delegationStatus.Count],
                        Note = $"Délégation de test {i}-{j}",
                        CreationDate = DateTime.UtcNow.AddDays(-i).AddMinutes(j),
                        IsAutomaticDelegation = j % 2 == 0,
                        Account = accountsForDelegation
                    };
                    await context.DelegationEntity.AddAsync(delegation);
                }
            }

            await context.SaveChangesAsync();
            _output.WriteLine($"✓ {delegationCount} délégations créées avec succès");
        }

        // Act - Mesurer la performance
        using (var context = new AccountContext(dbContextOptions))
        {
            var repository = new DelegationRepository(context);
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 10
            };
            var filter = new DelegationFilter();

            stopwatch.Start();
            var result = await repository.GetDelegatorDelegationsAsync(delegatorId, filter, pagination);
            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeNull().And.NotBeEmpty();
            result.TotalItems.Should().BeGreaterThan(0);

            var executionTime = stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"");
            _output.WriteLine($"📊 RÉSULTATS DE PERFORMANCE - GetDelegatorDelegationsAsync");
            _output.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            _output.WriteLine($"  Nombre de délégations : {delegationCount}");
            _output.WriteLine($"  Temps d'exécution      : {executionTime} ms");
            _output.WriteLine($"  Items retournés        : {result.Items!.Count()}");
            _output.WriteLine($"  Total items            : {result.TotalItems}");
            _output.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            // Seuil de performance : doit être inférieur à 5 secondes (5000ms)
            executionTime.Should().BeLessThan(5000,
                $"La requête a pris {executionTime}ms, ce qui est trop lent pour {delegationCount} délégations");
        }
    }

    /// <summary>
    /// Teste la performance de GetDelegatorDelegationsAsync avec un filtre.
    /// Le filtre IsAutomatic peut impacter les performances.
    /// </summary>
    [Fact]
    public async Task GetDelegatorDelegationsAsync_With150Delegations_AndFilter_ShouldCompleteInReasonableTime()
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        var stopwatch = new Stopwatch();
        var delegatorId = 1;
        var delegationCount = 150;

        using (var context = new AccountContext(dbContextOptions))
        {
            var delegator = new ContactEntity
            {
                ContactId = delegatorId,
                Email = "delegator@test.fr",
                FirstName = "Jean",
                LastName = "Dupont",
                Type = "collaborator",
                Status = "Declared",
                PersonaName = "Collaborateur",
                Office = "Paris",
                CreationDate = DateTime.UtcNow,
                IsActive = true
            };
            context.ContactEntity.Add(delegator);
            await context.SaveChangesAsync();

            var delegationStatus = new List<string> { DelegationStatus.Pending.ToString(), DelegationStatus.Enabled.ToString() };
            for (var i = 1; i <= delegationCount; i++)
            {
                var delegateeId = i * 10;

                var delegatee = new ContactEntity
                {
                    ContactId = delegateeId,
                    Email = $"delegatee-{delegateeId}@test.fr",
                    FirstName = $"Delegatee-FN-{delegateeId}",
                    LastName = $"Delegatee-LN-{delegateeId}",
                    Type = "collaborator",
                    Status = "Declared",
                    PersonaName = "Collaborateur",
                    Office = "Lyon",
                    CreationDate = DateTime.UtcNow,
                    IsActive = true
                };
                context.ContactEntity.Add(delegatee);
                await context.SaveChangesAsync();

                var delegation = new DelegationEntity
                {
                    DelegationId = i,
                    StartDate = DateTime.UtcNow.AddDays(-i),
                    EndDate = DateTime.UtcNow.AddMonths(i),
                    DelegatorId = delegatorId,
                    DelegateeId = delegateeId,
                    Status = delegationStatus[i % delegationStatus.Count],
                    Note = $"Délégation {i}",
                    CreationDate = DateTime.UtcNow.AddDays(-i),
                    IsAutomaticDelegation = i % 2 == 0,
                    Account = new List<AccountEntity>()
                };
                await context.DelegationEntity.AddAsync(delegation);
            }

            await context.SaveChangesAsync();
            _output.WriteLine($"✓ {delegationCount} délégations créées avec succès");
        }

        // Act
        using (var context = new AccountContext(dbContextOptions))
        {
            var repository = new DelegationRepository(context);
            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };
            var filter = new DelegationFilter { IsAutomatic = true };

            stopwatch.Start();
            var result = await repository.GetDelegatorDelegationsAsync(delegatorId, filter, pagination);
            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeNull().And.NotBeEmpty();

            var executionTime = stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"");
            _output.WriteLine($"📊 RÉSULTATS DE PERFORMANCE - GetDelegatorDelegationsAsync (avec filtre)");
            _output.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            _output.WriteLine($"  Nombre de délégations : {delegationCount}");
            _output.WriteLine($"  Filtre appliqué        : IsAutomatic = true");
            _output.WriteLine($"  Temps d'exécution      : {executionTime} ms");
            _output.WriteLine($"  Items retournés        : {result.Items!.Count()}");
            _output.WriteLine($"  Total items trouvés    : {result.TotalItems}");
            _output.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            executionTime.Should().BeLessThan(5000);
        }
    }

    /// <summary>
    /// Teste la performance de GetContactDelegationsAsync avec 150 délégations.
    /// Cette méthode utilise plusieurs Include (Account, Delegator, Delegatee).
    /// </summary>
    [Fact]
    public async Task GetContactDelegationsAsync_With150Delegations_ShouldCompleteInReasonableTime()
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        var stopwatch = new Stopwatch();
        var delegateeId = 1;
        var delegationCount = 150;

        using (var context = new AccountContext(dbContextOptions))
        {
            // Créer le délégué principal
            var mainDelegatee = new ContactEntity
            {
                ContactId = delegateeId,
                Email = "delegatee@test.fr",
                FirstName = "Paul",
                LastName = "Martin",
                Type = "collaborator",
                Status = "Declared",
                PersonaName = "Collaborateur",
                Office = "Paris",
                CreationDate = DateTime.UtcNow,
                IsActive = true
            };
            context.ContactEntity.Add(mainDelegatee);
            await context.SaveChangesAsync();

            // Créer des comptes
            var accounts = new List<AccountEntity>();
            for (int i = 1; i <= 20; i++)
            {
                accounts.Add(new AccountEntity
                {
                    AccountId = i,
                    AccountNumber = $"ACC{i:D3}",
                    LegalName = $"Société {(char)('A' + (i % 26))}{i}",
                    CreatedBy = "system",
                    Email = $"account{i}@test.fr",
                    IsActive = true
                });
            }

            context.AccountEntity.AddRange(accounts);
            await context.SaveChangesAsync();

            // Créer les délégations
            var delegationStatus = new List<string> { DelegationStatus.Pending.ToString(), DelegationStatus.Enabled.ToString() };
            for (var i = 1; i <= delegationCount; i++)
            {
                var delegatorId = (i * 100) + 10;

                var delegator = new ContactEntity
                {
                    ContactId = delegatorId,
                    Email = $"delegator-{delegatorId}@test.fr",
                    FirstName = $"Delegator-FN-{delegatorId}",
                    LastName = $"Delegator-LN-{delegatorId}",
                    Type = "collaborator",
                    Status = "Declared",
                    PersonaName = "Collaborateur",
                    Office = "Lyon",
                    CreationDate = DateTime.UtcNow,
                    IsActive = true
                };
                context.ContactEntity.Add(delegator);
                await context.SaveChangesAsync();

                // Ajouter 3-5 comptes par délégation pour tester les Include
                var accountsForDelegation = accounts.Take((i % 5) + 3).ToList();

                var delegation = new DelegationEntity
                {
                    DelegationId = i,
                    StartDate = DateTime.UtcNow.AddDays(-i),
                    EndDate = DateTime.UtcNow.AddMonths(i),
                    DelegatorId = delegatorId,
                    DelegateeId = delegateeId,
                    Status = delegationStatus[i % delegationStatus.Count],
                    Note = $"Délégation {i}",
                    CreationDate = DateTime.UtcNow.AddDays(-i),
                    Account = accountsForDelegation
                };

                await context.DelegationEntity.AddAsync(delegation);
            }

            await context.SaveChangesAsync();
            _output.WriteLine($"✓ {delegationCount} délégations créées avec succès");
        }

        // Act
        using (var context = new AccountContext(dbContextOptions))
        {
            var repository = new DelegationRepository(context);

            stopwatch.Start();
            var result = await repository.GetContactDelegationsAsync(delegateeId);
            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();

            var executionTime = stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"");
            _output.WriteLine($"📊 RÉSULTATS DE PERFORMANCE - GetContactDelegationsAsync");
            _output.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            _output.WriteLine($"  Nombre de délégations : {delegationCount}");
            _output.WriteLine($"  Temps d'exécution      : {executionTime} ms");
            _output.WriteLine($"  Items retournés        : {result.Count}");
            _output.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            executionTime.Should().BeLessThan(5000,
                $"La requête a pris {executionTime}ms, ce qui est trop lent pour {delegationCount} délégations");
        }
    }

    /// <summary>
    /// Teste la comparaison de performance entre les deux méthodes de lecture principales.
    /// Utile pour identifier laquelle est la plus problématique.
    /// </summary>
    [Fact]
    public async Task ComparePerformance_BetweenDelegatorAndContactMethods()
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        var delegatorId = 1;
        var delegateeId = 2;
        var delegationCount = 150;

        using (var context = new AccountContext(dbContextOptions))
        {
            // Setup data
            var mainDelegator = new ContactEntity
            {
                ContactId = delegatorId,
                Email = "delegator@test.fr",
                FirstName = "Jean",
                LastName = "Dupont",
                Type = "collaborator",
                Status = "Declared",
                PersonaName = "Collaborateur",
                Office = "Paris",
                CreationDate = DateTime.UtcNow,
                IsActive = true
            };
            var mainDelegatee = new ContactEntity
            {
                ContactId = delegateeId,
                Email = "delegatee@test.fr",
                FirstName = "Paul",
                LastName = "Martin",
                Type = "collaborator",
                Status = "Declared",
                PersonaName = "Collaborateur",
                Office = "Lyon",
                CreationDate = DateTime.UtcNow,
                IsActive = true
            };
            context.ContactEntity.AddRange(mainDelegator, mainDelegatee);
            await context.SaveChangesAsync();

            var mainAccount = new AccountEntity
            {
                AccountId = 1,
                AccountNumber = "MAIN001",
                LegalName = "Société Test",
                CreatedBy = "system",
                Email = "main@test.fr",
                IsActive = true
            };
            context.AccountEntity.Add(mainAccount);
            await context.SaveChangesAsync();

            // Créer des délégations
            for (var i = 1; i <= delegationCount; i++)
            {
                var otherContactId = i * 1000;

                var otherContact = new ContactEntity
                {
                    ContactId = otherContactId,
                    Email = $"contact-{otherContactId}@test.fr",
                    FirstName = $"FN-{otherContactId}",
                    LastName = $"LN-{otherContactId}",
                    Type = "collaborator",
                    Status = "Declared",
                    PersonaName = "Collaborateur",
                    Office = "Marseille",
                    CreationDate = DateTime.UtcNow,
                    IsActive = true
                };
                context.ContactEntity.Add(otherContact);
                await context.SaveChangesAsync();

                var delegation = new DelegationEntity
                {
                    DelegationId = i,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(i),
                    DelegatorId = delegatorId,
                    DelegateeId = otherContactId,
                    Status = DelegationStatus.Enabled.ToString(),
                    Note = $"Délégation {i}",
                    CreationDate = DateTime.UtcNow,
                    Account = new List<AccountEntity> { mainAccount }
                };

                await context.DelegationEntity.AddAsync(delegation);
            }

            await context.SaveChangesAsync();
        }

        // Act & Assert
        using (var context = new AccountContext(dbContextOptions))
        {
            var repository = new DelegationRepository(context);
            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };
            var filter = new DelegationFilter();

            // Test GetDelegatorDelegationsAsync
            var sw1 = Stopwatch.StartNew();
            var result1 = await repository.GetDelegatorDelegationsAsync(delegatorId, filter, pagination);
            sw1.Stop();

            // Test GetContactDelegationsAsync (sur un des delegatees)
            var sw2 = Stopwatch.StartNew();
            var result2 = await repository.GetContactDelegationsAsync(delegatorId);
            sw2.Stop();

            _output.WriteLine($"");
            result1.Should().NotBeNull();
            result1.Items.Should().NotBeNull();
            result2.Should().NotBeNull();
            sw1.ElapsedMilliseconds.Should().BeLessThan(5000, "GetDelegatorDelegationsAsync est trop lent");
            sw2.ElapsedMilliseconds.Should().BeLessThan(5000, "GetContactDelegationsAsync est trop lent");

            _output.WriteLine($"📊 COMPARAISON DE PERFORMANCE");
            _output.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            _output.WriteLine($"  Nombre de délégations : {delegationCount}");
            _output.WriteLine($"");
            _output.WriteLine($"  GetDelegatorDelegationsAsync :");
            _output.WriteLine($"    • Temps : {sw1.ElapsedMilliseconds} ms");
            _output.WriteLine($"    • Items : {result1.Items!.Count()}");
            _output.WriteLine($"");
            _output.WriteLine($"  GetContactDelegationsAsync :");
            _output.WriteLine($"    • Temps : {sw2.ElapsedMilliseconds} ms");
            _output.WriteLine($"    • Items : {result2.Count}");
            _output.WriteLine($"");
            _output.WriteLine($"  Différence : {Math.Abs(sw1.ElapsedMilliseconds - sw2.ElapsedMilliseconds)} ms");
            _output.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        }
    }
}
