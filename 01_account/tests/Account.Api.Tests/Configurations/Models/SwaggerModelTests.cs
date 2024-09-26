// <copyright file="SwaggerModelTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Account.Api.Tests.Configurations.Models;
using System;
using Xunit;
using FluentAssertions;
using Pulse.Account.API.Configuration.Model;

public class SwaggerModelTests
{
    [Fact]
    public void SwaggerModel_ShouldHaveNullableProperties()
    {
        // Arrange & Act
        var model = new SwaggerModel();

        // Assert
        model.UiEndpoint.Should().BeNull();
        model.JsonEndpoint.Should().BeNull();
        model.Title.Should().BeNull();
        model.Version.Should().BeNull();
        model.ContactName.Should().BeNull();
        model.ContactEmail.Should().BeNull();
        model.LicenseName.Should().BeNull();
        model.Description.Should().BeNull();
        model.TermsOfService.Should().BeNull();
        model.RouteTemplate.Should().BeNull();
    }

    [Fact]
    public void SwaggerModel_ShouldAllowSettingAndGettingProperties()
    {
        // Arrange
        var model = new SwaggerModel
        {
            UiEndpoint = "/swagger",
            JsonEndpoint = "/swagger/v1/swagger.json",
            Title = "Test API",
            Version = "v1",
            ContactName = "John Doe",
            ContactEmail = "john@example.com",
            LicenseName = "MIT",
            Description = "This is a test API",
            TermsOfService = new Uri("https://example.com/terms"),
            RouteTemplate = "api-docs/{documentName}/swagger.json"
        };

        // Act & Assert
        model.UiEndpoint.Should().Be("/swagger");
        model.JsonEndpoint.Should().Be("/swagger/v1/swagger.json");
        model.Title.Should().Be("Test API");
        model.Version.Should().Be("v1");
        model.ContactName.Should().Be("John Doe");
        model.ContactEmail.Should().Be("john@example.com");
        model.LicenseName.Should().Be("MIT");
        model.Description.Should().Be("This is a test API");
        model.TermsOfService.Should().Be(new Uri("https://example.com/terms"));
        model.RouteTemplate.Should().Be("api-docs/{documentName}/swagger.json");
    }

    [Fact]
    public void SwaggerModel_ShouldAllowNullValuesForProperties()
    {
        // Arrange
        var model = new SwaggerModel();

        // Act
        model.UiEndpoint = null!;
        model.JsonEndpoint = null!;
        model.Title = null!;
        model.Version = null!;
        model.ContactName = null!;
        model.ContactEmail = null!;
        model.LicenseName = null!;
        model.Description = null!;
        model.TermsOfService = null!;
        model.RouteTemplate = null!;

        // Assert
        model.UiEndpoint.Should().BeNull();
        model.JsonEndpoint.Should().BeNull();
        model.Title.Should().BeNull();
        model.Version.Should().BeNull();
        model.ContactName.Should().BeNull();
        model.ContactEmail.Should().BeNull();
        model.LicenseName.Should().BeNull();
        model.Description.Should().BeNull();
        model.TermsOfService.Should().BeNull();
        model.RouteTemplate.Should().BeNull();
    }

    [Fact]
    public void SwaggerModel_ShouldAllowEmptyStringsForStringProperties()
    {
        // Arrange
        var model = new SwaggerModel();

        // Act
        model.UiEndpoint = string.Empty;
        model.JsonEndpoint = string.Empty;
        model.Title = string.Empty;
        model.Version = string.Empty;
        model.ContactName = string.Empty;
        model.ContactEmail = string.Empty;
        model.LicenseName = string.Empty;
        model.Description = string.Empty;
        model.RouteTemplate = string.Empty;

        // Assert
        model.UiEndpoint.Should().BeEmpty();
        model.JsonEndpoint.Should().BeEmpty();
        model.Title.Should().BeEmpty();
        model.Version.Should().BeEmpty();
        model.ContactName.Should().BeEmpty();
        model.ContactEmail.Should().BeEmpty();
        model.LicenseName.Should().BeEmpty();
        model.Description.Should().BeEmpty();
        model.RouteTemplate.Should().BeEmpty();
    }
}
