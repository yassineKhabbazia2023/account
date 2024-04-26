// <copyright file="MapDelegationBusinessToDelegationDbTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Requests;
using Pulse.Account.Infrastructure.Mappers;
using Pulse.Account.Infrastructure.Entities;
using AutoFixture;
using FluentAssertions;

namespace Pulse.Account.Infrastructure.Tests.Mappers
{
    public class MapDelegationBusinessToDelegationDbTests
    {
        private readonly Fixture _fixture;

        public MapDelegationBusinessToDelegationDbTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public void MapDelegationRequestToDelegationsDb_ShouldMapDelegation()
        {
            var accounts = _fixture.CreateMany<AccountEntity>();
            var expected = _fixture.Build<CreateDelegationRequest>()
                .With(x => x.AccountIds, accounts.Select(a => a.AccountId))
                .Create();

            var result = expected.MapDelegationRequestToDelegationsDb(accounts);

            result.Should().NotBeNull();
            result.Should().HaveCount(expected.DelegationDetails.Count());

            for (int i = 0; i < expected.DelegationDetails.Count(); i++)
            {
                var resultItem = result.ElementAt(i);
                var expectedItem = expected.DelegationDetails.ElementAt(i);
                resultItem.DelegatorId.Should().Be(expected.DelegatorId);
                resultItem.DelegateeId.Should().Be(expectedItem.DelegateeId);
                resultItem.StartDate.Should().Be(expectedItem.StartDate);
                resultItem.EndDate.Should().Be(expectedItem.EndDate);
                resultItem.Status.Should().Be(expectedItem.Status);
                resultItem.Note.Should().Be(expectedItem.Note);
                resultItem.IsFullDelegation.Should().Be(expected.IsFullDelegation);
                resultItem.IsAutomaticDelegation.Should().Be(expectedItem.IsAutomaticDelegation);

                for (int j = 0; j < expected.AccountIds!.Count(); j++)
                {
                    resultItem.Account.Should().HaveCount(expected.AccountIds!.Count());
                    resultItem.Account.Select(a => a.AccountId).Should().BeEquivalentTo(expected.AccountIds);
                }
            }
        }

        [Theory]
        [MemberData(nameof(DelegationRequestData))]
        public void MapDelegationRequestToDelegationsDbWithNullOrEmptyDelegationDetails_ShouldReturnEmptyList(CreateDelegationRequest delegation)
        {
            var result = MapDelegationBusinessToDelegationDb.MapDelegationRequestToDelegationsDb(delegation, null!);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        public static IEnumerable<object[]> DelegationRequestData => new List<object[]>
        {
            new object[] { null! },
            new object[]
            {
                new CreateDelegationRequest
                {
                    DelegatorId = 0,
                    DelegationDetails = null!,
                    AccountIds = null!,
                }
            },
            new object[]
            {
                new CreateDelegationRequest
                {
                    DelegatorId = 0,
                    DelegationDetails = Enumerable.Empty<DelegationDetails>(),
                    AccountIds = null!,
                }
            },
        };
    }
}
