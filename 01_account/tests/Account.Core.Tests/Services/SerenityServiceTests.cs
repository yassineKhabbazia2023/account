// <copyright file="SerenityServiceTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Moq;
using Pulse.Account.Core.Interfaces;
using Pulse.Account.Core.Services;

namespace Pulse.Account.Core.Tests.Services;

/// <summary>
/// Tests the Serenity application service.
/// </summary>
public class SerenityServiceTests
{
    private const int ContactId = 777;

    private readonly Mock<ISerenityRepository> _serenityRepository = new(MockBehavior.Strict);

    #region Reset choice

    /// <summary>
    /// Verifies that reset delegates the target contact to the repository.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ResetSerenityChoiceAsync_ShouldForwardContactId()
    {
        // Arrange
        _serenityRepository
            .Setup(repository => repository.ResetSerenityChoiceAsync(ContactId))
            .Returns(Task.CompletedTask);
        var service = new SerenityService(_serenityRepository.Object);

        // Act
        await service.ResetSerenityChoiceAsync(ContactId);

        // Assert
        _serenityRepository.Verify(repository => repository.ResetSerenityChoiceAsync(ContactId), Times.Once);
    }

    /// <summary>
    /// Verifies that repository failures propagate through the service.
    /// </summary>
    /// <returns>A task representing the asynchronous test.</returns>
    [Fact]
    public async Task ResetSerenityChoiceAsync_WhenRepositoryFails_ShouldPropagateFailure()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Repository failure");
        _serenityRepository
            .Setup(repository => repository.ResetSerenityChoiceAsync(ContactId))
            .ThrowsAsync(expectedException);
        var service = new SerenityService(_serenityRepository.Object);

        // Act
        var action = () => service.ResetSerenityChoiceAsync(ContactId);

        // Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(action);
        Assert.Same(expectedException, exception);
    }

    #endregion
}
