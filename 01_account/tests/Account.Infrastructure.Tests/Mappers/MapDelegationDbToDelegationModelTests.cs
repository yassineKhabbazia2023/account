// <copyright file="MapDelegationDbToDelegationModelTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using FluentAssertions;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Tests.Mappers
{
    public class MapDelegationDbToDelegationModelTests
    {
        private readonly Fixture _fixture;

        public MapDelegationDbToDelegationModelTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public void ToContact_ShouldMapContact()
        {
            var expected = _fixture.Create<ContactEntity>();

            var result = expected.ToContact();

            result.Should().NotBeNull();
            result!.ContactId.Should().Be(expected.ContactId);
            result!.GlobalContactId.Should().Be(expected.ContactGlobalUniqueId);
            result!.Email.Should().Be(expected.Email);
            result!.FirstName.Should().Be(expected.FirstName);
            result!.LastName.Should().Be(expected.LastName);
        }

        [Fact]
        public void ToContactWithNullSource_ShouldReturnNull()
        {
            ContactEntity? expected = null;

            var result = expected!.ToContact();

            result.Should().BeNull();
        }

        [Fact]
        public void ToAccount_ShouldMapAccount()
        {
            var expected = _fixture.Create<AccountEntity>();

            var result = expected.ToAccount();

            result.Should().NotBeNull();
            result!.AccountId.Should().Be(expected.AccountId);
            result!.AccountNumber.Should().Be(expected.AccountNumber);
            result!.LegalName.Should().Be(expected.LegalName);
        }

        [Fact]
        public void ToAccountWithNullSource_ShouldReturnNull()
        {
            AccountEntity? expected = null;

            var result = expected!.ToAccount();

            result.Should().BeNull();
        }

        [Fact]
        public void ToAccounts_ShouldMapAccounts()
        {
            var expected = _fixture.CreateMany<AccountEntity>();

            var result = expected.ToAccounts();

            result.Should().NotBeNull();
            result!.Count().Should().Be(expected.Count());
        }

        [Fact]
        public void ToAccountsWithNullSource_ShouldReturnEmptyList()
        {
            List<AccountEntity>? expected = null;

            var result = expected!.ToAccounts();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Fact]
        public void ToDelegation_ShouldMapDelegation()
        {
            var expected = _fixture.Create<DelegationEntity>();

            var result = expected!.ToDelegation();

            result.Should().NotBeNull();
            result!.DelegationId.Should().Be(expected.DelegationId);
            result!.CreationDate.Should().Be(expected.CreationDate);
            result!.StartDate.Should().Be(expected.StartDate);
            result!.EndDate.Should().Be(expected.EndDate);
            result!.Status.Should().Be(expected.Status);
            result!.Note.Should().Be(expected.Note);
            result!.Accounts.Should().NotBeEmpty();
            result!.Delegator.Should().NotBeNull();
            result!.Delegatee.Should().NotBeNull();
        }

        [Fact]
        public void ToDelegationWithNullSource_ShouldReturnNull()
        {
            DelegationEntity? expected = null;

            var result = expected!.ToDelegation();

            result.Should().BeNull();
        }

        [Fact]
        public void ToDelegations_ShouldMapDelegations()
        {
            var expected = _fixture.CreateMany<DelegationEntity>().ToList();

            var result = expected.ToDelegations();

            result.Should().NotBeNull();
            result!.Count.Should().Be(expected.Count);
        }

        [Fact]
        public void ToDelegationsWithNullSource_ShouldReturnEmptyList()
        {
            List<DelegationEntity>? expected = null;

            var result = expected!.ToDelegations();

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}
