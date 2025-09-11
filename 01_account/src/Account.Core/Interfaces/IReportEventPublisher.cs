// <copyright file="IReportEventPublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Enum;

namespace Pulse.Account.Core.Interfaces;

public interface IReportEventPublisher
{
    Task PublishReportCreatedEventAsync(int? reportId, int accountId, int reportTypeId, string reportLabel, ReportStatus reportStatus);
}
