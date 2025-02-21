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
        [InlineData(null)]
        public void GetValidDeploymentStatus_Should_Return0K(int? deploymentStatus)
        {
            // Arrange
            // Act
            var result = DeploymentStatusValidation.GetValidDeploymentStatus(deploymentStatus);

            // Assert
            Assert.Equal(result, deploymentStatus);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(5)]
        public void GetValidDeploymentStatus_Should_ReturnNotOK(int? deploymentStatus)
        {
            var result = Assert.Throws<NotFoundException>(() => DeploymentStatusValidation.GetValidDeploymentStatus(deploymentStatus));
        }
    }
}
