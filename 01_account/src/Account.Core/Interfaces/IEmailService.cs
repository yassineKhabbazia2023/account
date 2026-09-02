// <copyright file="IEmailService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;

namespace Pulse.Account.Core.Interfaces;

public interface IEmailService
{
    Task SendDelegationRequestEmailsAsync(IEnumerable<int> recipientIds, Contact requestor, AccountDetail account);
}
