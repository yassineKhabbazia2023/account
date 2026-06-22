// <copyright file="DeploymentStatusValidationTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Extensions;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Core.Tests.Extensions
{
    public class DeploymentStatusValidationTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public void GetValidDeploymentStatuses_Should_ReturnOK(int deploymentStatus)
        {
            // Arrange
            var input = new List<int> { deploymentStatus };

            // Act
            var result = DeploymentStatusValidation.GetValidDeploymentStatuses(input);

            // Assert
            Assert.Equal(input, result);
        }

        [Fact]
        public void GetValidDeploymentStatuses_WhenNull_ShouldReturnNull()
        {
            // Act
            var result = DeploymentStatusValidation.GetValidDeploymentStatuses(null);

            // Assert
            Assert.Null(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        public void GetValidDeploymentStatuses_Should_ThrowBadRequest(int deploymentStatus)
        {
            Assert.Throws<BadRequestException>(() => DeploymentStatusValidation.GetValidDeploymentStatuses(new List<int> { deploymentStatus }));
        }
    }
}
