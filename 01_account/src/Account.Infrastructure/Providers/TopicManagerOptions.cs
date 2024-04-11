// <copyright file="TopicManagerOptions.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Infrastructure.Providers
{
    public class TopicManagerOptions
    {
        public string TopicName { get; set; } = string.Empty;

        public TopicManagerOptions Register(string? topicName)
        {
            this.TopicName = topicName ?? string.Empty;
            return this;
        }
    }
}
