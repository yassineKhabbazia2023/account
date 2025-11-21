// <copyright file="DelegationRepositoryPerformanceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Diagnostics;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;
using Xunit.Abstractions;

namespace Pulse.Account.Infrastructure.Tests.Repositories;

/// <summary>
/// Tests de performance pour les requêtes d'historique de délégations.
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
    }

    /// <summary>
    /// Teste la performance de GetAccountDelegationsHistoryAsync avec 150 délégations.
    /// Ce test mesure le temps d'exécution pour identifier les problèmes de performance.
    /// </summary>
    [Fact]
    public async Task GetAccountDelegationsHistoryAsync_With150Delegations_ShouldCompleteInReasonableTime()
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        var stopwatch = new Stopwatch();
        var accountId = 1;
        var delegationCount = 150;

        using (var context = new AccountContext(dbContextOptions))
        {
            // Créer un compte principal
            var mainAccount = new AccountEntity
            {
                AccountId = accountId,
                AccountNumber = "MAIN001",
                LegalName = "Société Principale",
                CreatedBy = "system",
                Email = "main@test.fr",
                IsActive = true
            };
            context.AccountEntity.Add(mainAccount);
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

            // Créer les contacts et délégations
            var delegationStatus = new List<string> { "Pending", "Enabled" };
            for (var i = 1; i <= delegationCount; i++)
            {
                var delegatorId = i * 100;
                var delegateeId = i * 100 + 1;

                // Créer les contacts
                await context.ContactEntity.AddAsync(new ContactEntity
                {
                    ContactId = delegatorId,
                    Email = $"delegator-{delegatorId}@test.fr",
                    FirstName = $"Delegator-FN-{delegatorId}",
                    LastName = $"Delegator-LN-{delegatorId}",
                    Type = "collaborator",
                    Status = "Declared",
                    PersonaName = "Collaborateur",
                    Office = "Paris",
                    CreationDate = DateTime.UtcNow.AddDays(-i),
                    IsActive = true
                });

                await context.ContactEntity.AddAsync(new ContactEntity
                {
                    ContactId = delegateeId,
                    Email = $"delegatee-{delegateeId}@test.fr",
                    FirstName = $"Delegatee-FN-{delegateeId}",
                    LastName = $"Delegatee-LN-{delegateeId}",
                    Type = "collaborator",
                    Status = "Declared",
                    PersonaName = "Collaborateur",
                    Office = "Lyon",
                    CreationDate = DateTime.UtcNow.AddDays(-i),
                    IsActive = true
                });

                await context.SaveChangesAsync();

                // Créer une délégation avec plusieurs comptes (relation many-to-many)
                var accountsForDelegation = new List<AccountEntity> { mainAccount };

                // Ajouter 2-3 comptes supplémentaires pour simuler des relations complexes
                var accountsToAdd = (i % 3) + 1;
                for (int j = 0; j < accountsToAdd && j < additionalAccounts.Count; j++)
                {
                    accountsForDelegation.Add(additionalAccounts[j]);
                }

                var delegation = new DelegationEntity
                {
                    StartDate = DateTime.UtcNow.AddDays(-i),
                    EndDate = DateTime.UtcNow.AddMonths(i),
                    DelegatorId = delegatorId,
                    DelegateeId = delegateeId,
                    Status = delegationStatus[i % delegationStatus.Count],
                    Note = $"Délégation de test {i}",
                    CreationDate = DateTime.UtcNow.AddDays(-i),
                    Account = accountsForDelegation
                };

                await context.DelegationEntity.AddAsync(delegation);
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

            stopwatch.Start();
            var result = await repository.GetAccountDelegationsHistoryAsync(accountId, null, pagination);
            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();
            result.TotalItems.Should().Be(delegationCount);

            var executionTime = stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"");
            _output.WriteLine($"📊 RÉSULTATS DE PERFORMANCE - GetAccountDelegationsHistoryAsync");
            _output.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            _output.WriteLine($"  Nombre de délégations : {delegationCount}");
            _output.WriteLine($"  Temps d'exécution      : {executionTime} ms");
            _output.WriteLine($"  Items retournés        : {result.Items.Count()}");
            _output.WriteLine($"  Total items            : {result.TotalItems}");
            _output.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            // Seuil de performance : doit être inférieur à 5 secondes (5000ms)
            // Note: En production avec SQL Server réel, le timeout actuel est probablement 30s
            executionTime.Should().BeLessThan(5000,
                $"La requête a pris {executionTime}ms, ce qui est trop lent pour {delegationCount} délégations");
        }
    }

    /// <summary>
    /// Teste la performance de GetAccountDelegationsHistoryAsync avec recherche.
    /// La recherche sur les noms peut aggraver les problèmes de performance.
    /// </summary>
    [Fact]
    public async Task GetAccountDelegationsHistoryAsync_With150Delegations_AndSearch_ShouldCompleteInReasonableTime()
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        var stopwatch = new Stopwatch();
        var accountId = 1;
        var delegationCount = 150;
        var searchTerm = "Delegator-FN-100";

        using (var context = new AccountContext(dbContextOptions))
        {
            var mainAccount = new AccountEntity
            {
                AccountId = accountId,
                AccountNumber = "MAIN001",
                LegalName = "Société Principale",
                CreatedBy = "system",
                Email = "main@test.fr",
                IsActive = true
            };
            context.AccountEntity.Add(mainAccount);
            await context.SaveChangesAsync();

            var delegationStatus = new List<string> { "Pending", "Enabled" };
            for (var i = 1; i <= delegationCount; i++)
            {
                var delegatorId = i * 100;
                var delegateeId = i * 100 + 1;

                await context.ContactEntity.AddAsync(new ContactEntity
                {
                    ContactId = delegatorId,
                    Email = $"delegator-{delegatorId}@test.fr",
                    FirstName = $"Delegator-FN-{delegatorId}",
                    LastName = $"Delegator-LN-{delegatorId}",
                    Type = "collaborator",
                    Status = "Declared",
                    PersonaName = "Collaborateur",
                    CreationDate = DateTime.UtcNow,
                    IsActive = true
                });

                await context.ContactEntity.AddAsync(new ContactEntity
                {
                    ContactId = delegateeId,
                    Email = $"delegatee-{delegateeId}@test.fr",
                    FirstName = $"Delegatee-FN-{delegateeId}",
                    LastName = $"Delegatee-LN-{delegateeId}",
                    Type = "collaborator",
                    Status = "Declared",
                    PersonaName = "Collaborateur",
                    CreationDate = DateTime.UtcNow,
                    IsActive = true
                });

                await context.SaveChangesAsync();

                var delegation = new DelegationEntity
                {
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(i),
                    DelegatorId = delegatorId,
                    DelegateeId = delegateeId,
                    Status = delegationStatus[i % delegationStatus.Count],
                    Note = $"Délégation {i}",
                    CreationDate = DateTime.UtcNow.AddDays(-i),
                    Account = new List<AccountEntity> { mainAccount }
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

            stopwatch.Start();
            var result = await repository.GetAccountDelegationsHistoryAsync(accountId, searchTerm, pagination);
            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();

            var executionTime = stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"");
            _output.WriteLine($"📊 RÉSULTATS DE PERFORMANCE - GetAccountDelegationsHistoryAsync (avec recherche)");
            _output.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            _output.WriteLine($"  Nombre de délégations  : {delegationCount}");
            _output.WriteLine($"  Terme de recherche     : {searchTerm}");
            _output.WriteLine($"  Temps d'exécution      : {executionTime} ms");
            _output.WriteLine($"  Items retournés        : {result.Items.Count()}");
            _output.WriteLine($"  Total items trouvés    : {result.TotalItems}");
            _output.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            executionTime.Should().BeLessThan(5000);
        }
    }

    /// <summary>
    /// Teste la performance de GetContactDelegationsHistoryAsync avec 150 délégations.
    /// Cette méthode est particulièrement problématique à cause du tri dans l'Include.
    /// </summary>
    [Fact]
    public async Task GetContactDelegationsHistoryAsync_With150Delegations_ShouldCompleteInReasonableTime()
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        var stopwatch = new Stopwatch();
        var contactId = 1;
        var delegationCount = 150;

        using (var context = new AccountContext(dbContextOptions))
        {
            // Créer le contact principal
            var mainContact = new ContactEntity
            {
                ContactId = contactId,
                Email = "main.contact@test.fr",
                FirstName = "Main",
                LastName = "Contact",
                Type = "collaborator",
                Status = "Declared",
                PersonaName = "Collaborateur",
                Office = "Paris",
                CreationDate = DateTime.UtcNow,
                IsActive = true
            };
            context.ContactEntity.Add(mainContact);
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
            var delegationStatus = new List<string> { "Pending", "Enabled" };
            for (var i = 1; i <= delegationCount; i++)
            {
                var otherContactId = i * 100 + 10;

                await context.ContactEntity.AddAsync(new ContactEntity
                {
                    ContactId = otherContactId,
                    Email = $"contact-{otherContactId}@test.fr",
                    FirstName = $"Contact-FN-{otherContactId}",
                    LastName = $"Contact-LN-{otherContactId}",
                    Type = "collaborator",
                    Status = "Declared",
                    PersonaName = "Collaborateur",
                    Office = "Lyon",
                    CreationDate = DateTime.UtcNow,
                    IsActive = true
                });

                await context.SaveChangesAsync();

                // Alterner entre delegator et delegatee pour le contact principal
                var isDelegator = i % 2 == 0;

                // Ajouter 3-5 comptes par délégation pour tester le tri
                var accountsForDelegation = accounts.Take((i % 5) + 3).ToList();

                var delegation = new DelegationEntity
                {
                    StartDate = DateTime.UtcNow.AddDays(-i),
                    EndDate = DateTime.UtcNow.AddMonths(i),
                    DelegatorId = isDelegator ? contactId : otherContactId,
                    DelegateeId = isDelegator ? otherContactId : contactId,
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

        // Act - Test avec tri ascendant
        using (var context = new AccountContext(dbContextOptions))
        {
            var repository = new DelegationRepository(context);
            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

            stopwatch.Start();
            var result = await repository.GetContactDelegationsHistoryAsync(contactId, pagination, sortAscending: true);
            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();

            var executionTime = stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"");
            _output.WriteLine($"📊 RÉSULTATS DE PERFORMANCE - GetContactDelegationsHistoryAsync (tri ASC)");
            _output.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            _output.WriteLine($"  Nombre de délégations  : {delegationCount}");
            _output.WriteLine($"  Temps d'exécution      : {executionTime} ms");
            _output.WriteLine($"  Items retournés        : {result.Items.Count()}");
            _output.WriteLine($"  Total items            : {result.TotalItems}");
            _output.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            executionTime.Should().BeLessThan(5000,
                $"La requête a pris {executionTime}ms avec tri ascendant");
        }

        // Act - Test avec tri descendant
        using (var context = new AccountContext(dbContextOptions))
        {
            var repository = new DelegationRepository(context);
            var pagination = new Pagination { PageNumber = 1, PageSize = 10 };

            stopwatch.Restart();
            var result = await repository.GetContactDelegationsHistoryAsync(contactId, pagination, sortAscending: false);
            stopwatch.Stop();

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().NotBeEmpty();

            var executionTime = stopwatch.ElapsedMilliseconds;
            _output.WriteLine($"");
            _output.WriteLine($"📊 RÉSULTATS DE PERFORMANCE - GetContactDelegationsHistoryAsync (tri DESC)");
            _output.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            _output.WriteLine($"  Nombre de délégations  : {delegationCount}");
            _output.WriteLine($"  Temps d'exécution      : {executionTime} ms");
            _output.WriteLine($"  Items retournés        : {result.Items.Count()}");
            _output.WriteLine($"  Total items            : {result.TotalItems}");
            _output.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

            executionTime.Should().BeLessThan(5000,
                $"La requête a pris {executionTime}ms avec tri descendant");
        }
    }

    /// <summary>
    /// Teste la comparaison de performance entre les deux méthodes.
    /// Utile pour voir laquelle est la plus problématique.
    /// </summary>
    [Fact]
    public async Task ComparePerformance_BetweenAccountAndContactHistoryMethods()
    {
        // Arrange
        var dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        var accountId = 1;
        var contactId = 100;
        var delegationCount = 150;

        using (var context = new AccountContext(dbContextOptions))
        {
            // Setup data
            var mainAccount = new AccountEntity
            {
                AccountId = accountId,
                AccountNumber = "MAIN001",
                LegalName = "Société Test",
                CreatedBy = "system",
                Email = "main@test.fr",
                IsActive = true
            };
            context.AccountEntity.Add(mainAccount);

            var mainContact = new ContactEntity
            {
                ContactId = contactId,
                Email = "contact@test.fr",
                FirstName = "Test",
                LastName = "Contact",
                Type = "collaborator",
                Status = "Declared",
                PersonaName = "Collaborateur",
                CreationDate = DateTime.UtcNow,
                IsActive = true
            };
            context.ContactEntity.Add(mainContact);
            await context.SaveChangesAsync();

            // Créer des délégations
            for (var i = 1; i <= delegationCount; i++)
            {
                var delegatorId = i * 1000;
                var delegateeId = contactId;

                await context.ContactEntity.AddAsync(new ContactEntity
                {
                    ContactId = delegatorId,
                    Email = $"delegator-{delegatorId}@test.fr",
                    FirstName = $"FN-{delegatorId}",
                    LastName = $"LN-{delegatorId}",
                    Type = "collaborator",
                    Status = "Declared",
                    PersonaName = "Collaborateur",
                    CreationDate = DateTime.UtcNow,
                    IsActive = true
                });
                await context.SaveChangesAsync();

                var delegation = new DelegationEntity
                {
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(i),
                    DelegatorId = delegatorId,
                    DelegateeId = delegateeId,
                    Status = "Enabled",
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

            // Test GetAccountDelegationsHistoryAsync
            var sw1 = Stopwatch.StartNew();
            var result1 = await repository.GetAccountDelegationsHistoryAsync(accountId, null, pagination);
            sw1.Stop();

            // Test GetContactDelegationsHistoryAsync
            var sw2 = Stopwatch.StartNew();
            var result2 = await repository.GetContactDelegationsHistoryAsync(contactId, pagination, true);
            sw2.Stop();

            _output.WriteLine($"");
            _output.WriteLine($"📊 COMPARAISON DE PERFORMANCE");
            _output.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            _output.WriteLine($"  Nombre de délégations : {delegationCount}");
            _output.WriteLine($"");
            _output.WriteLine($"  GetAccountDelegationsHistoryAsync :");
            _output.WriteLine($"    • Temps : {sw1.ElapsedMilliseconds} ms");
            _output.WriteLine($"    • Items : {result1.Items.Count()}");
            _output.WriteLine($"");
            _output.WriteLine($"  GetContactDelegationsHistoryAsync :");
            _output.WriteLine($"    • Temps : {sw2.ElapsedMilliseconds} ms");
            _output.WriteLine($"    • Items : {result2.Items.Count()}");
            _output.WriteLine($"");
            _output.WriteLine($"  Différence : {Math.Abs(sw1.ElapsedMilliseconds - sw2.ElapsedMilliseconds)} ms");
            _output.WriteLine($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        }
    }
}
