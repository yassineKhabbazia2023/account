// <copyright file="BrokerSettingTest.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Account.Api.Tests.Configurations.Models;
using FluentAssertions;
using Pulse.Account.API.Configuration.Model;

public class BrokerSettingTests
{
    [Fact]
    public void BrokerSetting_ShouldHaveCorrectProperties()
    {
        // Arrange
        var brokerSetting = new BrokerSetting
        {
            ServiceBusNamespace = "testNamespace",
            ManagedIdentityClientId = "testClientId",
            PushTopicName = new List<string> { "topic1", "topic2" },
            PullTopics = new List<PullTopic> { new PullTopic(), new PullTopic() }
        };

        // Assert
        brokerSetting.ServiceBusNamespace.Should().Be("testNamespace");
        brokerSetting.ManagedIdentityClientId.Should().Be("testClientId");
        brokerSetting.PushTopicName.Should().ContainInOrder("topic1", "topic2");
        brokerSetting.PullTopics.Should().HaveCount(2);
    }

    [Fact]
    public void BrokerSetting_ShouldAllowNullableProperties()
    {
        // Arrange
        var brokerSetting = new BrokerSetting
        {
            PullTopics = new List<PullTopic>()
        };

        // Assert
        brokerSetting.ServiceBusNamespace.Should().BeNull();
        brokerSetting.ManagedIdentityClientId.Should().BeNull();
        brokerSetting.PushTopicName.Should().BeNull();
    }

    [Fact]
    public void BrokerSetting_ShouldAllowEmptyPullTopics()
    {
        // Arrange & Act
        var brokerSetting = new BrokerSetting
        {
            PullTopics = new List<PullTopic>()
        };

        // Assert
        brokerSetting.PullTopics.Should().NotBeNull();
        brokerSetting.PullTopics.Should().BeEmpty();
    }
}
