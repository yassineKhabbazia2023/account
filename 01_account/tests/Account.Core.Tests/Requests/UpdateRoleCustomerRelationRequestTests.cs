// <copyright file="UpdateRoleCustomerRelationRequestTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Tests.Requests;

public class UpdateRoleCustomerRelationRequestTests
{
    [Fact]
    public void Validate_WhenIsCustomerRelationIsMissing_ShouldFail()
    {
        // Arrange
        var request = new UpdateRoleCustomerRelationRequest
        {
            AccountIds = new List<int> { 1 }
        };
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        // Act
        var isValid = AnnotationValidator.TryValidateObjectRecursive(request, results);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateRoleCustomerRelationRequest.IsCustomerRelation)));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Validate_WhenIsCustomerRelationIsProvided_ShouldSucceed(bool isCustomerRelation)
    {
        // Arrange
        var request = new UpdateRoleCustomerRelationRequest
        {
            IsCustomerRelation = isCustomerRelation,
            AccountIds = new List<int> { 1 }
        };
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        // Act
        var isValid = AnnotationValidator.TryValidateObjectRecursive(request, results);

        // Assert
        Assert.True(isValid);
        Assert.Empty(results);
    }

    [Fact]
    public void Validate_WhenAccountIdsIsEmpty_ShouldFail()
    {
        // Arrange
        var request = new UpdateRoleCustomerRelationRequest
        {
            IsCustomerRelation = true,
            AccountIds = new List<int>()
        };
        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        // Act
        var isValid = AnnotationValidator.TryValidateObjectRecursive(request, results);

        // Assert
        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(UpdateRoleCustomerRelationRequest.AccountIds)));
    }
}
