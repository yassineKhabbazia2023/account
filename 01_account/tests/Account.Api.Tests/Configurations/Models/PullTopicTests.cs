// <copyright file="PullTopicTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Account.Api.Tests.Configurations.Models;
using FluentAssertions;
using Pulse.Account.API.Configuration.Model;

public class PullTopicTests
{
    [Fact]
    public void PullTopic_ShouldHaveNullableTopicName()
    {
        // Arrange
        var pullTopic = new PullTopic();

        // Assert
        pullTopic.TopicName.Should().BeNull();
    }

    [Fact]
    public void PullTopic_ShouldAllowSettingTopicName()
    {
        // Arrange
        var pullTopic = new PullTopic();

        // Act
        pullTopic.TopicName = "TestTopic";

        // Assert
        pullTopic.TopicName.Should().Be("TestTopic");
    }

    [Fact]
    public void PullTopic_ShouldHaveNullableSubscriptions()
    {
        // Arrange
        var pullTopic = new PullTopic();

        // Assert
        pullTopic.Subscriptions.Should().BeNull();
    }

    [Fact]
    public void PullTopic_ShouldAllowSettingSubscriptions()
    {
        // Arrange
        var pullTopic = new PullTopic();
        var subscriptions = new List<string> { "Sub1", "Sub2" };

        // Act
        pullTopic.Subscriptions = subscriptions;

        // Assert
        pullTopic.Subscriptions.Should().NotBeNull();
        pullTopic.Subscriptions.Should().BeEquivalentTo(subscriptions);
    }

    [Fact]
    public void PullTopic_ShouldAllowAddingSubscriptions()
    {
        // Arrange
        var pullTopic = new PullTopic
        {
            Subscriptions = new List<string>()
        };

        // Act
        pullTopic.Subscriptions.Add("Sub1");
        pullTopic.Subscriptions.Add("Sub2");

        // Assert
        pullTopic.Subscriptions.Should().HaveCount(2);
        pullTopic.Subscriptions.Should().ContainInOrder("Sub1", "Sub2");
    }

    [Fact]
    public void PullTopic_ShouldAllowClearingSubscriptions()
    {
        // Arrange
        var pullTopic = new PullTopic
        {
            Subscriptions = new List<string> { "Sub1", "Sub2" }
        };

        // Act
        pullTopic.Subscriptions.Clear();

        // Assert
        pullTopic.Subscriptions.Should().BeEmpty();
    }

    [Fact]
    public void PullTopic_Constructor_ShouldCreateEmptyObject()
    {
        // Act
        var pullTopic = new PullTopic();

        // Assert
        pullTopic.TopicName.Should().BeNull();
        pullTopic.Subscriptions.Should().BeNull();
    }
}
