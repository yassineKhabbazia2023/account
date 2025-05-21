// <copyright file="LabelControllerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.Account.API.Controllers;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Utils;
using Pulse.Account.Core.Requests;

namespace Account.Api.Tests.Controllers
{
    public class LabelControllerTests
    {
        private readonly Mock<ILabelService> _mockLabelService;
        private readonly LabelController _controller;

        public LabelControllerTests()
        {
            _mockLabelService = new Mock<ILabelService>();
            _controller = new LabelController(_mockLabelService.Object);
        }

        [Fact]
        public async Task GetAsync_ShouldReturnOkWithLabels_WhenLabelsAreRetrieved()
        {
            // Arrange
            var pagination = new Pagination { PageNumber = 1, PageSize = 40 };
            var fixture = new Fixture();
            var expectedLabels = fixture.Build<Label>().CreateMany(10).ToList();
            var paging = new Paging<Label>() { CurrentPage = 1, Items = expectedLabels, TotalItems = 10, TotalPage = 1 };
            _mockLabelService.Setup(s => s.GetLabelsAsync(It.IsAny<Pagination>())).ReturnsAsync(paging);

            // Act
            var result = await _controller.GetLabelsAsync(pagination);

            // Assert
            var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
            var returnedLabels = okResult.Value.Should().BeAssignableTo<Paging<Label>>().Subject;
            returnedLabels.Items.Should().BeEquivalentTo(expectedLabels);
            _mockLabelService.Verify(s => s.GetLabelsAsync(pagination), Times.Once);
        }
    }
}
