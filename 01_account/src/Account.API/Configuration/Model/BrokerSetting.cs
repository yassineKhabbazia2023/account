// <copyright file="BrokerSetting.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.API.Configuration.Model;

public class BrokerSetting
{
    public string? ServiceBusNamespace { get; set; }

    public string? ManagedIdentityClientId { get; set; }

    public List<string>? PushTopicName { get; set; }

    public required List<PullTopic> PullTopics { get; set; }
}
