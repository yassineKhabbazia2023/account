// <copyright file="ServicePublisher.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pulse.Account.Core.Broker.Events;
using Pulse.Account.Core.Interfaces;

namespace Pulse.Account.Infrastructure.Providers
{
    public class ServicePublisher : IServicePublisher
    {
        private readonly TopicManagerOptions _options;
        private readonly ILogger<ServicePublisher> _logger;
        private readonly IAzureClientFactory<ServiceBusSender> _clientFactory;

        public ServicePublisher(
            IAzureClientFactory<ServiceBusSender> clientFactory,
            IOptions<TopicManagerOptions> options,
            ILogger<ServicePublisher> logger)
        {
            _clientFactory = clientFactory;
            _options = options == null ? throw new ArgumentNullException(nameof(options)) : options.Value;
            _logger = logger;
        }

        public async Task PublishAsync(BaseEvent @event)
        {
            var topicName = _options.TopicName;
            var sender = _clientFactory.CreateClient(topicName);

            try
            {
                var serializedContent = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));
                var message = new ServiceBusMessage()
                {
                    ContentType = "application/json",
                    Body = new BinaryData(serializedContent),
                };

                await sender.SendMessageAsync(message);
            }
            catch (Exception exception)
            {
                _logger.LogError("Publish event error: {eventIdentifier}; EventType: {eventType}; Exception: {message}", @event!.EventIdentifier, @event.EventType, exception.Message);
            }
        }
    }
}
