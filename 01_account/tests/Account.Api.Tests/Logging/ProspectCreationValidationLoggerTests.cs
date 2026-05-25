// <copyright file="ProspectCreationValidationLoggerTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.Account.API.Logging;

namespace Account.Api.Tests.Logging;

/// <summary>
/// Tests for <see cref="ProspectCreationValidationLogger"/>.
/// </summary>
public class ProspectCreationValidationLoggerTests
{
    #region Prospect creation validation logging

    /// <summary>
    /// Ensures the logger writes a structured warning with identifiers resolved from route, query, model state, and headers.
    /// </summary>
    [Fact]
    public void Log_WithValidationErrors_ShouldWriteStructuredWarningWithResolvedIdentifiers()
    {
        var loggerMock = new Mock<ILogger>();
        var context = CreateActionContext(loggerMock, descriptor =>
        {
            descriptor.ActionName = "CreateAccountAsync";
            descriptor.DisplayName = "AccountController.CreateAccountAsync";
        });

        context.HttpContext.Request.Path = "/api/accounts";
        context.HttpContext.Request.QueryString = QueryString.Create(new Dictionary<string, string?>
        {
            ["Email"] = "prospect@test.fr",
            ["AccountNumber"] = "ACC-001",
        });
        context.HttpContext.Request.Headers["CurrentUser"] = "42";
        context.RouteData.Values["AccountId"] = "10";
        context.RouteData.Values["ProspectId"] = "prospect-001";

        context.ModelState.SetModelValue("Siret", "12345678901234", "12345678901234");
        context.ModelState.AddModelError("Siret", "The Siret field is required.");
        context.ModelState.SetModelValue("Contacts[0].ContactId", "88", "88");
        context.ModelState.AddModelError("Contacts[0].ContactId", "The ContactId field is required.");

        ProspectCreationValidationLogger.Log(context, "Pulse.Back.Account");

        var invocation = Assert.Single(loggerMock.Invocations.Where(i => i.Method.Name == nameof(ILogger.Log)));
        Assert.Equal(LogLevel.Warning, invocation.Arguments[0]);

        var state = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object?>>>(invocation.Arguments[2]);
        Assert.Contains(state, item => item.Key == "ServiceName" && item.Value?.ToString() == "Pulse.Back.Account");
        Assert.Contains(state, item => item.Key == "OperationName" && item.Value?.ToString() == "CreateAccountAsync");
        Assert.Contains(state, item => item.Key == "Route" && item.Value?.ToString() == "/api/accounts");
        Assert.Contains(state, item => item.Key == "Siret" && item.Value?.ToString() == "12345678901234");
        Assert.Contains(state, item => item.Key == "ProspectId" && item.Value?.ToString() == "prospect-001");
        Assert.Contains(state, item => item.Key == "AccountNumber" && item.Value?.ToString() == "ACC-001");
        Assert.Contains(state, item => item.Key == "AccountId" && item.Value?.ToString() == "10");
        Assert.Contains(state, item => item.Key == "ContactId" && item.Value?.ToString() == "88");
        Assert.Contains(state, item => item.Key == "Email" && item.Value?.ToString() == "prospect@test.fr");
        Assert.Contains(state, item => item.Key == "CurrentUserId" && item.Value?.ToString() == "42");

        var validationErrorsEntry = state.Single(item => item.Key is "ValidationErrors" or "@ValidationErrors");
        var validationErrors = Assert.IsAssignableFrom<IReadOnlyDictionary<string, string[]>>(validationErrorsEntry.Value);
        Assert.Equal(["The Siret field is required."], validationErrors["Siret"]);
        Assert.Equal(["The ContactId field is required."], validationErrors["Contacts[0].ContactId"]);
    }

    /// <summary>
    /// Ensures the logger falls back to the action display name when the controller action name is unavailable.
    /// </summary>
    [Fact]
    public void Log_WhenActionNameIsMissing_ShouldFallbackToDisplayName()
    {
        var loggerMock = new Mock<ILogger>();
        var context = CreateActionContext(loggerMock, descriptor =>
        {
            descriptor.ActionName = null!;
            descriptor.DisplayName = "AccountController.DisplayNameFallback";
        });

        context.HttpContext.Request.Path = "/api/accounts";
        context.ModelState.AddModelError("LegalName", "The LegalName field is required.");

        ProspectCreationValidationLogger.Log(context, "Pulse.Back.Account");

        var invocation = Assert.Single(loggerMock.Invocations.Where(i => i.Method.Name == nameof(ILogger.Log)));
        var state = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object?>>>(invocation.Arguments[2]);

        Assert.Contains(state, item => item.Key == "OperationName" && item.Value?.ToString() == "AccountController.DisplayNameFallback");
        Assert.Contains(state, item => item.Key == "{OriginalFormat}" && item.Value?.ToString() == "Validation failed for prospect-creation endpoint. ServiceName: {ServiceName}, OperationName: {OperationName}, Route: {Route}, Siret: {Siret}, ProspectId: {ProspectId}, AccountNumber: {AccountNumber}, AccountId: {AccountId}, ContactId: {ContactId}, Email: {Email}, CurrentUserId: {CurrentUserId}, ValidationErrors: {@ValidationErrors}");
    }

    /// <summary>
    /// Ensures the logger validates its required arguments.
    /// </summary>
    [Fact]
    public void Log_WithInvalidArguments_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(() => ProspectCreationValidationLogger.Log(null!, "Pulse.Back.Account"));

        var loggerMock = new Mock<ILogger>();
        var context = CreateActionContext(loggerMock, descriptor => descriptor.ActionName = "CreateAccountAsync");

        Assert.Throws<ArgumentException>(() => ProspectCreationValidationLogger.Log(context, string.Empty));
    }

    #endregion Prospect creation validation logging

    /// <summary>
    /// Creates an MVC action context configured with a mocked logger factory.
    /// </summary>
    /// <param name="loggerMock">The logger mock used by the logger factory.</param>
    /// <param name="configureDescriptor">The action descriptor configuration.</param>
    /// <returns>A configured action context.</returns>
    private static ActionContext CreateActionContext(Mock<ILogger> loggerMock, Action<ControllerActionDescriptor> configureDescriptor)
    {
        var loggerFactoryMock = new Mock<ILoggerFactory>();
        loggerFactoryMock
            .Setup(factory => factory.CreateLogger("ProspectCreationValidation"))
            .Returns(loggerMock.Object);

        var services = new ServiceCollection()
            .AddSingleton(loggerFactoryMock.Object)
            .BuildServiceProvider();

        var httpContext = new DefaultHttpContext
        {
            RequestServices = services
        };

        var descriptor = new ControllerActionDescriptor();
        configureDescriptor(descriptor);

        return new ActionContext(
            httpContext,
            new Microsoft.AspNetCore.Routing.RouteData(),
            descriptor,
            new ModelStateDictionary());
    }
}
