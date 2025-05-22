// <copyright file="LabelRepositoryTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;

namespace Pulse.Account.Infrastructure.Tests.Repositories
{
    public class LabelRepositoryTests
    {
        private readonly Fixture _fixture;
        private readonly DbContextOptions<AccountContext> _dbContextOptions;

        public LabelRepositoryTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _dbContextOptions = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetAsync_ShouldReturnAllLabels()
        {
            // Arrange
            using var context = new AccountContext(_dbContextOptions);
            var labelEntities = _fixture.Build<LabelEntity>()
                .Without(x => x.RoleLabelEntity)
                .CreateMany(3).ToList();

            context.LabelEntity.AddRange(labelEntities);
            context.SaveChanges();

            var pagination = new Pagination { PageSize = 50, PageNumber = 1 };

            var labelRepository = new LabelRepository(context);

            // Act
            var paginLabels = await labelRepository.GetLabelsAsync(pagination);

            var result = paginLabels.Items.ToList();
            // Assert
            result.Should().HaveCount(labelEntities.Count);

            for (int i = 0; i < labelEntities.Count; i++)
            {
                result[i]!.LabelId.Should().Be(labelEntities[i].LabelId);
                result[i]!.Code.Should().Be(labelEntities[i].Code);
                result[i]!.CustomerLabel.Should().Be(labelEntities[i].CustomerLabel);
                result[i]!.CollaboratorLabel.Should().Be(labelEntities[i].CollaboratorLabel);
                result[i]!.Description.Should().Be(labelEntities[i].Description);
            }
        }

        [Fact]
        public async Task GetAsync_WhenNoLabelsExist_ShouldReturnEmptyCollection()
        {
            // Arrange
            using var context = new AccountContext(_dbContextOptions);

            var pagination = new Pagination { PageSize = 50, PageNumber = 1 };

            var labelRepository = new LabelRepository(context);

            // Act
            var paginLabels = await labelRepository.GetLabelsAsync(pagination);

            var result = paginLabels.Items.ToList();
            // Assert
            result.Should().BeEmpty();
        }
    }
}
