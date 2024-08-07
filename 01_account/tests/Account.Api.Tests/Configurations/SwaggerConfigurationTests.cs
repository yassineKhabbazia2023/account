using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Pulse.Account.API.Configuration;
using Swashbuckle.AspNetCore.Swagger;
using Pulse.Account.API.Configuration.Model;

namespace Account.Api.Tests.Configurations
{

    public class SwaggerConfigurationTests
    {
        [Fact]
        public void ConfigureSwaggerService_ShouldAddSwaggerGen()
        {
            // Arrange
            var services = new Mock<IServiceCollection>();
            var swaggerModel = new SwaggerModel
            {
                Version = "v1",
                Title = "Test API",
                Description = "Test Description",
                TermsOfService = new Uri("https://example.com/terms"),
                ContactName = "Test Contact",
                ContactEmail = "test@example.com",
                LicenseName = "Test License"
            };

            // Act
            services.Object.ConfigureSwaggerService(swaggerModel);

            // Assert
            services.Verify(s => s.Add(It.Is<ServiceDescriptor>(sd =>
                sd.ServiceType == typeof(ISwaggerProvider))), Times.Once);
        }
    }
}
