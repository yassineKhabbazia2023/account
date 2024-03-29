// <copyright file="TopicManagerOptions.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Infrastructure.Providers
{
    public class TopicManagerOptions
    {
        public IDictionary<string, string> TopicName { get; } = new Dictionary<string, string>();

        public TopicManagerOptions Register(string key, string topicName)
        {
            this.TopicName.Add(key, topicName);
            return this;
        }
    }
}
