// <copyright file="LabelServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;
using Pulse.Account.Core.Services;

namespace Pulse.Account.Core.Tests.Services
{
    public class LabelServiceTests
    {
        private readonly Mock<ILabelRepository> _mockLabelRepository;
        private readonly LabelService _labelService;

        public LabelServiceTests()
        {
            _mockLabelRepository = new Mock<ILabelRepository>();
            _labelService = new LabelService(_mockLabelRepository.Object);
        }

        [Fact]
        public async Task GetLabelsAsync_ReturnsLabelsFromRepository()
        {
            // Arrange
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 10
            };

            var expectedLabels = new List<Label>
            {
                new Label
                {
                    LabelId = 1,
                    Code = "TEST1",
                    CustomerLabel = "Customer Test 1",
                    CollaboratorLabel = "Collaborator Test 1",
                    Business = "Business Test 1",
                    Description = "Description Test 1",
                    IsVisible = true
                },
                new Label
                {
                    LabelId = 2,
                    Code = "TEST2",
                    CustomerLabel = "Customer Test 2",
                    CollaboratorLabel = "Collaborator Test 2",
                    Business = "Business Test 2",
                    Description = "Description Test 2",
                    IsVisible = true
                }
            };

            var expectedResult = new Paging<Label>
            {
                Items = expectedLabels,
                CurrentPage = 1,
                TotalPage = 1,
                TotalItems = 2
            };

            _mockLabelRepository.Setup(repo => repo.GetLabelsAsync(pagination))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _labelService.GetLabelsAsync(pagination);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedResult.CurrentPage, result.CurrentPage);
            Assert.Equal(expectedResult.TotalPage, result.TotalPage);
            Assert.Equal(expectedResult.TotalItems, result.TotalItems);
            Assert.Equal(expectedResult.Items.Count(), result.Items.Count());

            // Verify the repository method was called with the correct pagination
            _mockLabelRepository.Verify(repo => repo.GetLabelsAsync(pagination), Times.Once);
        }

        [Fact]
        public async Task GetLabelsAsync_WithNullPagination_UsesDefaultPagination()
        {
            // Arrange
            Pagination pagination = null;
            var defaultPagination = new Pagination { PageNumber = 1, PageSize = 10 };

            var expectedLabels = new List<Label>
            {
                new Label
                {
                    LabelId = 1,
                    Code = "TEST1",
                    CustomerLabel = "Customer Test 1",
                    CollaboratorLabel = "Collaborator Test 1",
                    Business = "Business Test 1",
                    Description = "Description Test 1",
                    IsVisible = true
                }
            };

            var expectedResult = new Paging<Label>
            {
                Items = expectedLabels,
                CurrentPage = 1,
                TotalPage = 1,
                TotalItems = 1
            };

            _mockLabelRepository.Setup(repo => repo.GetLabelsAsync(It.IsAny<Pagination>()))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _labelService.GetLabelsAsync(pagination);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Items);

            // Verify the repository method was called with default pagination
            _mockLabelRepository.Verify(repo => repo.GetLabelsAsync(It.IsAny<Pagination>()),
                Times.Once);
        }

        [Fact]
        public async Task GetLabelsAsync_WithCustomPagination_UsesProvidedValues()
        {
            // Arrange
            var pagination = new Pagination
            {
                PageNumber = 2,
                PageSize = 5
            };

            var expectedLabels = new List<Label>
            {
                new Label
                {
                    LabelId = 6,
                    Code = "TEST6",
                    CustomerLabel = "Customer Test 6",
                    CollaboratorLabel = "Collaborator Test 6",
                    Business = "Business Test 6",
                    Description = "Description Test 6",
                    IsVisible = true
                }
            };

            var expectedResult = new Paging<Label>
            {
                Items = expectedLabels,
                CurrentPage = 2,
                TotalPage = 2,
                TotalItems = 6
            };

            _mockLabelRepository.Setup(repo => repo.GetLabelsAsync(pagination))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _labelService.GetLabelsAsync(pagination);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.CurrentPage);
            Assert.Equal(2, result.TotalPage);
            Assert.Equal(6, result.TotalItems);
            Assert.Single(result.Items);

            // Verify the repository method was called with the correct pagination
            _mockLabelRepository.Verify(repo => repo.GetLabelsAsync(pagination), Times.Once);
        }

        [Fact]
        public async Task GetLabelsAsync_WhenRepositoryThrowsException_PropagatesException()
        {
            // Arrange
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 10
            };

            var expectedException = new Exception("Repository error");

            _mockLabelRepository.Setup(repo => repo.GetLabelsAsync(pagination))
                .ThrowsAsync(expectedException);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _labelService.GetLabelsAsync(pagination));

            Assert.Equal(expectedException.Message, exception.Message);
            _mockLabelRepository.Verify(repo => repo.GetLabelsAsync(pagination), Times.Once);
        }

        [Fact]
        public async Task GetLabelsAsync_ReturnsEmptyResult_WhenNoLabelsExist()
        {
            // Arrange
            var pagination = new Pagination
            {
                PageNumber = 1,
                PageSize = 10
            };

            var expectedResult = new Paging<Label>
            {
                Items = new List<Label>(),
                CurrentPage = 1,
                TotalPage = 0,
                TotalItems = 0
            };

            _mockLabelRepository.Setup(repo => repo.GetLabelsAsync(pagination))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _labelService.GetLabelsAsync(pagination);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalItems);
            Assert.Equal(0, result.TotalPage);
            Assert.Equal(1, result.CurrentPage);

            // Verify the repository method was called with the correct pagination
            _mockLabelRepository.Verify(repo => repo.GetLabelsAsync(pagination), Times.Once);
        }
    }
}
