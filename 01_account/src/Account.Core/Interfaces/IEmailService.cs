// <copyright file="IEmailService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models.Email;

namespace Pulse.Account.Core.Interfaces;

public interface IEmailService
{
    Task SendRequestEmailAsync(RequestEmailContext context);
}
