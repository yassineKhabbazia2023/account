// <copyright file="BrokerSetting.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.API.Configuration.Model;

public class BrokerSetting
{
    public string? ServiceBusConnectionString { get; set; }

    public string? PushTopicName { get; set; }

    public List<PullTopic>? PullTopics { get; set; }
}
