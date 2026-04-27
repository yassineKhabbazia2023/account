using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pulse.Account.API.Configuration;

namespace Account.Api.Tests.Configurations;

public class OpenTelemetryRegistrationTests
{
    [Fact]
    public void RegisterOpenTelemetry_WithValidConnectionString_ShouldRegisterServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["APPLICATIONINSIGHTS_CONNECTION_STRING"] = "InstrumentationKey=test-key;IngestionEndpoint=https://test.in.applicationinsights.azure.com/"
            })
            .Build();

        var initialCount = services.Count;

        // Act
        services.RegisterOpenTelemetry(configuration);

        // Assert
        services.Count.Should().BeGreaterThan(initialCount);
    }

    [Fact]
    public void RegisterOpenTelemetry_WithMissingConnectionString_ShouldThrowNullArgumentException()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act
        var act = () => services.RegisterOpenTelemetry(configuration);

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void RegisterOpenTelemetry_WithNullConfiguration_ShouldThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var act = () => services.RegisterOpenTelemetry(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
