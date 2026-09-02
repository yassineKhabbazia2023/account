// <copyright file="IEmailService.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace Pulse.Account.Core.Interfaces;

public interface IEmailService
{
    Task SendDelegationRequestEmailsAsync(IEnumerable<int> recipientIds, int requestorId, int accountId);
}
