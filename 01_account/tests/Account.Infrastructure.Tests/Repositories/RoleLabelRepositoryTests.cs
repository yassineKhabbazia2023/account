// <copyright file="RoleLabelRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Exceptions;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;
using Pulse.ExceptionMiddleware.Exceptions;

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
            // Arrange
            using var context = new AccountContext(_dbContextOptions);
            var existingRoleLabelEntity = new RoleLabelEntity() { AccountId = 1, ContactId = 1, LabelId = 1, CreatedDate = DateTime.UtcNow, CreatedBy = 1 };
            context.RoleLabelEntity.Add(existingRoleLabelEntity);
            await context.SaveChangesAsync();

            var roleLabelRepository = new RoleLabelRepository(context);
            var duplicateRoleLabel = _fixture.Build<RoleLabel>()
                .With(x => x.AccountId, 1)
                .With(x => x.ContactId, 1)
                .With(x => x.LabelId, 1)
                .Create();

            // Act
            Func<Task> act = async () => await roleLabelRepository.AddRoleLabelAsync(duplicateRoleLabel);

            // Assert
            await act.Should().ThrowAsync<ConflictException>()
                .WithMessage("Ce contact a déjà ce label.");
        }

        [Fact]
        public async Task AddRoleLabelAsync_WhenRoleDoesNotExist_ShouldThrowBadRequestException()
        {
            // Arrange
            using var context = new AccountContext(_dbContextOptions);
            var roleLabelRepository = new RoleLabelRepository(context);
            var roleLabelWithoutRole = _fixture.Build<RoleLabel>()
                .With(x => x.AccountId, 999)
                .With(x => x.ContactId, 999)
                .With(x => x.LabelId, 1)
                .Create();

            // Act
            Func<Task> act = async () => await roleLabelRepository.AddRoleLabelAsync(roleLabelWithoutRole);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("Le role accountId 999 et contactId 999 est introuvable");
        }

        [Fact]
        public async Task AddRoleLabelAsync_WhenRoleLabelIsValid_ShouldAddRoleLabelToContext()
        {
            // Arrange
            using var context = new AccountContext(_dbContextOptions);

            // Create and add a role entity first
            var roleEntity = _fixture.Build<RoleEntity>()
                .With(x => x.AccountId, 1)
                .With(x => x.ContactId, 1)
                .Without(x => x.Account)
                .Without(x => x.Contact)
                .Create();

            // Create and add a label entity
            var labelEntity = _fixture.Build<LabelEntity>()
                .With(x => x.LabelId, 1)
                .Create();

            context.RoleEntity.Add(roleEntity);
            context.LabelEntity.Add(labelEntity);
            await context.SaveChangesAsync();

            var roleLabelRepository = new RoleLabelRepository(context);
            var roleLabel = _fixture.Build<RoleLabel>()
                .With(x => x.AccountId, 1)
                .With(x => x.ContactId, 1)
                .With(x => x.LabelId, 1)
                .Create();

            // Act
            await roleLabelRepository.AddRoleLabelAsync(roleLabel);

            // Assert
            var savedRoleLabel = await context.RoleLabelEntity.FirstOrDefaultAsync(rl =>
                rl.AccountId == roleLabel.AccountId &&
                rl.ContactId == roleLabel.ContactId &&
                rl.LabelId == roleLabel.LabelId);

            savedRoleLabel.Should().NotBeNull();
            savedRoleLabel!.AccountId.Should().Be(roleLabel.AccountId);
            savedRoleLabel.ContactId.Should().Be(roleLabel.ContactId);
            savedRoleLabel.LabelId.Should().Be(roleLabel.LabelId);
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
