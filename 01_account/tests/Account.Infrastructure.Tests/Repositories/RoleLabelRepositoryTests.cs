// <copyright file="RoleLabelRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Data;
using System;
using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.ExceptionMiddleware.Exceptions;
using Polly;

namespace Pulse.Account.Infrastructure.Tests.Repositories
{
    public class RoleLabelRepositoryTests
    {
        private readonly Fixture _fixture;
        private readonly DbContextOptions<AccountContext> _dbContextOptions;

        public RoleLabelRepositoryTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task AddRoleLabelAsync_WhenRoleLabelIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            using var context = new AccountContext(_dbContextOptions);
            var roleLabelRepository = new RoleLabelRepository(context);
            RoleLabel? roleLabel = null;

            // Act
            Func<Task> act = async () => await roleLabelRepository.AddRoleLabelAsync(roleLabel);

            // Assert
            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage($"*{nameof(roleLabel)}*");
        }

        [Fact]
        public async Task AddRoleLabelAsync_WhenRoleLabelAlreadyExists_ShouldThrowBadRequestException()
        {
            using var context = new AccountContext(_dbContextOptions);
            var account = new AccountEntity
            {
                AccountId = 1,
                AccountGlobalUniqueId = Guid.NewGuid(),
                AccountNumber = "12345",
                CreatedBy = "System",
                LegalName = "Testing Company",
                IsActive = true
            };
            var contact = new ContactEntity
            {
                ContactId = 1,
                Email = "testing@rydge.fr",
                FirstName = "testing",
                LastName = "testing",
                PersonaName = "Testing XUNIT",
                Type = ContactType.Collaborator.ToString(),
                CreationDate = DateTime.Now,
                IsActive = true
            };
            var label = new LabelEntity
            {
                LabelId = 1,
                Code = "CODE",
                Business = "ESG",
                IsVisible = true,
                CollaboratorLabel = "COLLAB",
                CustomerLabel = "CUST"
            };
            var role = new RoleEntity { AccountId = 1, ContactId = 1 };
            var roleLabelEntity = new RoleLabelEntity { AccountId = 1, ContactId = 1, LabelId = 1, CreatedBy = 1 };
            var roleLabel = new RoleLabel { AccountId = 1, ContactId = 1, LabelId = 1, CreatedBy = 1 };
            context.AccountEntity.Add(account);
            context.ContactEntity.Add(contact);
            context.LabelEntity.Add(label);
            context.RoleEntity.Add(role);
            context.RoleLabelEntity.Add(roleLabelEntity);
            int numberOfChanges = context.SaveChanges();
            var roleLabelRepos = new RoleLabelRepository(context);

            // Act
            Func<Task> act = async () => await roleLabelRepos.AddRoleLabelAsync(roleLabel);
            var actionResult = await Assert.ThrowsAsync<ConflictException>(act);
            Assert.Equal("ACC033", actionResult.Code);
            Assert.Equal("Ce contact a déjà ce libellé.", actionResult.Message);
        }

        [Fact]
        public async Task AddRoleLabelAsync_WhenRoleDoesNotExist_ShouldThrowBadRequestException()
        {
            // Arrange
            using var context = new AccountContext(_dbContextOptions);
            var account = new AccountEntity
            {
                AccountId = 1,
                AccountGlobalUniqueId = Guid.NewGuid(),
                AccountNumber = "12345",
                CreatedBy = "System",
                LegalName = "Testing Company",
                IsActive = true
            };
            var contact = new ContactEntity
            {
                ContactId = 1,
                Email = "testing@rydge.fr",
                FirstName = "testing",
                LastName = "testing",
                PersonaName = "Testing XUNIT",
                Type = ContactType.Collaborator.ToString(),
                CreationDate = DateTime.Now,
                IsActive = true
            };
            var label = new LabelEntity
            {
                LabelId = 1,
                Code = "CODE",
                Business = "ESG",
                IsVisible = true,
                CollaboratorLabel = "COLLAB",
                CustomerLabel = "CUST"
            };
            var roleLabel = new RoleLabel { AccountId = 1, ContactId = 1, LabelId = 1, CreatedBy = 1 };
            context.AccountEntity.Add(account);
            context.ContactEntity.Add(contact);
            context.LabelEntity.Add(label);
            int numberOfChanges = context.SaveChanges();
            var roleLabelRepos = new RoleLabelRepository(context);

            // Act
            Func<Task> act = async () => await roleLabelRepos.AddRoleLabelAsync(roleLabel);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Le role accountId 1 et contactId 1 est introuvable");
        }


        [Fact]
        public async Task AddRoleLabelAsync_WhenContactIsActiveIsFalse_ShouldThrowNotFoundException()
        {
            using (var context = new AccountContext(_dbContextOptions))
            {
                var account = new AccountEntity
                {
                    AccountId = 1,
                    AccountGlobalUniqueId = Guid.NewGuid(),
                    AccountNumber = "12345",
                    CreatedBy = "System",
                    LegalName = "Testing Company",
                    IsActive = true
                };
                var contact = new ContactEntity
                {
                    ContactId = 1,
                    Email = "testing@rydge.fr",
                    FirstName = "testing",
                    LastName = "testing",
                    PersonaName = "Testing XUNIT",
                    Type = ContactType.Customer.ToString(),
                    CreationDate = DateTime.Now,
                    IsActive = false
                };
                var role = new RoleEntity
                {
                    AccountId = 1,
                    ContactId = 1,
                    IsSignatory = false
                };
                var label = new LabelEntity
                {
                    LabelId = 1,
                    Code = "CODE",
                    Business = "ESG",
                    IsVisible = true,
                    CollaboratorLabel = "COLLAB",
                    CustomerLabel = "CUST"
                };
                var roleLabel = new RoleLabel { AccountId = 1, ContactId = 1, LabelId = 1, CreatedBy = 1 };
                context.AccountEntity.Add(account);
                context.ContactEntity.Add(contact);
                context.SaveChanges();
                context.RoleEntity.Add(role);
                context.LabelEntity.Add(label);
                int numberOfChanges = context.SaveChanges();
                var roleLabelRepos = new RoleLabelRepository(context);
                var action = async () => await roleLabelRepos.AddRoleLabelAsync(roleLabel);
                var exceptionResult = await Assert.ThrowsAsync<NotFoundException>(action);
                Assert.Equal("ACC002", exceptionResult.Code);
                Assert.Equal("Le contact avec l'identifiant 1 est introuvable", exceptionResult.Message);
            }
        }

        [Fact]
        public async Task AddRoleLabelAsync_WhenContactIsClient_ShouldThrowInvalidRequestException()
        {
            using (var context = new AccountContext(_dbContextOptions))
            {
                var account = new AccountEntity
                {
                    AccountId = 1,
                    AccountGlobalUniqueId = Guid.NewGuid(),
                    AccountNumber = "12345",
                    CreatedBy = "System",
                    LegalName = "Testing Company",
                    IsActive = true
                };
                var contact = new ContactEntity
                {
                    ContactId = 1,
                    Email = "testing@rydge.fr",
                    FirstName = "testing",
                    LastName = "testing",
                    PersonaName = "Testing XUNIT",
                    Type = ContactType.Customer.ToString(),
                    CreationDate = DateTime.Now,
                    IsActive = true
                };
                var role = new RoleEntity
                {
                    AccountId = 1,
                    ContactId = 1,
                    IsSignatory = false
                };
                var label = new LabelEntity
                {
                    LabelId = 1,
                    Code = "CODE",
                    Business = "ESG",
                    IsVisible = true,
                    CollaboratorLabel = "COLLAB",
                    CustomerLabel = "CUST"
                };
                var roleLabel = new RoleLabel { AccountId = 1, ContactId = 1, LabelId = 1, CreatedBy = 1 };
                context.AccountEntity.Add(account);
                context.ContactEntity.Add(contact);
                context.RoleEntity.Add(role);
                context.LabelEntity.Add(label);
                int numberOfChanges = context.SaveChanges();
                var roleLabelRepos = new RoleLabelRepository(context);
                var action = async () => await roleLabelRepos.AddRoleLabelAsync(roleLabel);
                var exceptionResult = await Assert.ThrowsAsync<Pulse.ExceptionMiddleware.Exceptions.InvalidOperationException>(action);
                Assert.Equal("ACC035", exceptionResult.Code);
                Assert.Equal("Impossible d'ajouter des libellés pour un client.", exceptionResult.Message);
            }
        }

        [Fact]
        public async Task AddRoleLabelAsync_WhenRoleLabelIsValid_ShouldAddRoleLabelToContext()
        {
            using var context = new AccountContext(_dbContextOptions);
            var account = new AccountEntity
            {
                AccountId = 1,
                AccountGlobalUniqueId = Guid.NewGuid(),
                AccountNumber = "12345",
                CreatedBy = "System",
                LegalName = "Testing Company",
                IsActive = true
            };
            var contact = new ContactEntity
            {
                ContactId = 1,
                Email = "testing@rydge.fr",
                FirstName = "testing",
                LastName = "testing",
                PersonaName = "Testing XUNIT",
                Type = ContactType.Collaborator.ToString(),
                CreationDate = DateTime.Now,
                IsActive = true
            };
            var label = new LabelEntity
            {
                LabelId = 1,
                Code = "CODE",
                Business = "ESG",
                IsVisible = true,
                CollaboratorLabel = "COLLAB",
                CustomerLabel = "CUST"
            };
            var role = new RoleEntity { AccountId = 1, ContactId = 1 };
            var roleLabel = new RoleLabel { AccountId = 1, ContactId = 1, LabelId = 1, CreatedBy = 1 };
            context.AccountEntity.Add(account);
            context.ContactEntity.Add(contact);
            context.LabelEntity.Add(label);
            context.RoleEntity.Add(role);
            int numberOfChanges = context.SaveChanges();
            var roleLabelRepos = new RoleLabelRepository(context);

            // Act
            await roleLabelRepos.AddRoleLabelAsync(roleLabel);

            var roleLabelResult = await context.RoleLabelEntity.FirstOrDefaultAsync(rl => rl.ContactId == 1 && rl.LabelId == 1 && rl.AccountId == 1);
            Assert.NotNull(roleLabelResult);
        }

        [Fact]
        public async Task DeleteRoleLabel_WhenRoleLabelExists_ShouldDeleteRoleLabel()
        {
            // Arrange
            using var context = new AccountContext(_dbContextOptions);
            var roleLabelEntity = new RoleLabelEntity() { AccountId = 1, ContactId = 1, LabelId = 1, CreatedDate = DateTime.UtcNow, CreatedBy = 1 };
            context.RoleLabelEntity.Add(roleLabelEntity);
            await context.SaveChangesAsync();

            context.ChangeTracker.Clear();
            var roleLabelRepository = new RoleLabelRepository(context);

            // Act
            await roleLabelRepository.DeleteRoleLabelAsync(1, 1, 1);

            // Assert
            var deletedRoleLabel = await context.RoleLabelEntity.FirstOrDefaultAsync(rl =>
                rl.AccountId == roleLabelEntity.AccountId &&
                rl.ContactId == roleLabelEntity.ContactId &&
                rl.LabelId == roleLabelEntity.LabelId);

            deletedRoleLabel.Should().BeNull();
        }

        [Fact]
        public async Task DeleteRoleLabel_WhenRoleLabelDoesNotExist_ShouldNotThrowException()
        {
            // Arrange
            using var context = new AccountContext(_dbContextOptions);
            var roleLabelRepository = new RoleLabelRepository(context);

            // Act
            Func<Task> act = async () => await roleLabelRepository.DeleteRoleLabelAsync(999, 999, 999);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task DeleteRoleLabel_WhenMultipleRoleLabelsExist_ShouldOnlyDeleteSpecifiedRoleLabel()
        {
            // Arrange
            using var context = new AccountContext(_dbContextOptions);

            // Create multiple role label entities
            var roleLabelEntity1 = new RoleLabelEntity() { AccountId = 1, ContactId = 1, LabelId = 1, CreatedDate = DateTime.UtcNow, CreatedBy = 1 };

            var roleLabelEntity2 = new RoleLabelEntity() { AccountId = 1, ContactId = 1, LabelId = 2, CreatedDate = DateTime.UtcNow, CreatedBy = 1 };

            var roleLabelEntity3 = new RoleLabelEntity() { AccountId = 1, ContactId = 1, LabelId = 3, CreatedDate = DateTime.UtcNow, CreatedBy = 1 };

            context.RoleLabelEntity.AddRange(roleLabelEntity1, roleLabelEntity2, roleLabelEntity3);
            await context.SaveChangesAsync();

            var initialCount = await context.RoleLabelEntity.CountAsync();
            initialCount.Should().Be(3);

            context.ChangeTracker.Clear();
            var roleLabelRepository = new RoleLabelRepository(context);

            // Act
            await roleLabelRepository.DeleteRoleLabelAsync(1, 1, 1);

            // Assert
            var finalCount = await context.RoleLabelEntity.CountAsync();
            finalCount.Should().Be(2);

            var remainingRoleLabel1 = await context.RoleLabelEntity.FirstOrDefaultAsync(rl =>
                rl.AccountId == roleLabelEntity1.AccountId &&
                rl.ContactId == roleLabelEntity1.ContactId &&
                rl.LabelId == roleLabelEntity1.LabelId);

            var remainingRoleLabel2 = await context.RoleLabelEntity.FirstOrDefaultAsync(rl =>
                rl.AccountId == roleLabelEntity2.AccountId &&
                rl.ContactId == roleLabelEntity2.ContactId &&
                rl.LabelId == roleLabelEntity2.LabelId);

            var remainingRoleLabel3 = await context.RoleLabelEntity.FirstOrDefaultAsync(rl =>
                rl.AccountId == roleLabelEntity3.AccountId &&
                rl.ContactId == roleLabelEntity3.ContactId &&
                rl.LabelId == roleLabelEntity3.LabelId);

            remainingRoleLabel1.Should().BeNull();
            remainingRoleLabel2.Should().NotBeNull();
            remainingRoleLabel3.Should().NotBeNull();
        }
    }
}
