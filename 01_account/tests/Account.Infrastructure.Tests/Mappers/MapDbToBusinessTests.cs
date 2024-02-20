// <copyright file="MapDbToBusinessTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Tests.Mappers
{
    public class MapDbToBusinessTests
    {
        private readonly Fixture _fixture;
        private readonly DbContextOptions<AccountContext> _options;

        public MapDbToBusinessTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            _options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public void MapTAccountToAccountModel_Should_Return_AccountModel()
        {
            // Arrange
            var tAccountFixture = _fixture.Create<TAccount>();
            var expectedAccount = new Core.Models.Account();
            expectedAccount.LegalName = tAccountFixture.LegalName;
            expectedAccount.AccountId = tAccountFixture.AccountId;
            expectedAccount.AccountNumber = tAccountFixture.AccountNumber;
            expectedAccount.Address = tAccountFixture.TAddress.Select(address => new Address()
            {
                AddressId = address.AddressId,
                AddressType = address.AddressType,
                City = address.City,
                Country = address.Country,
                State = address.State,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                AddressLine3 = address.AddressLine3,
                ZipCode = address.ZipCode
            });

            // Act
            var accountModel = MapAccountDbToAccountModel.MapTAccountToAccountModel(tAccountFixture);

            // Assert
            var accountAddressExpect = JsonConvert.SerializeObject(expectedAccount.Address);
            var accountAddressReceived = JsonConvert.SerializeObject(accountModel.Address);
            Assert.Equal(accountAddressExpect, accountAddressReceived);
            Assert.Equal(expectedAccount.LegalName, accountModel.LegalName);
            Assert.Equal(expectedAccount.AccountNumber, accountModel.AccountNumber);
            Assert.Equal(expectedAccount.AccountId, accountModel.AccountId);
        }

        [Fact]
        public void MapTAccountToAccountDetail_Should_Return_AccountDetail()
        {
            // Arrange
            var tAccountFixture = _fixture.Create<TAccount>();
            var expectedAccount = new AccountDetail();
            expectedAccount.Legal = new Legal();
            expectedAccount.Legal.LegalName = tAccountFixture.LegalName;
            expectedAccount.AccountId = tAccountFixture.AccountId;
            expectedAccount.AccountNumber = tAccountFixture.AccountNumber;
            expectedAccount.Address = tAccountFixture.TAddress.Select(address => new Address()
            {
                AddressId = address.AddressId,
                AddressType = address.AddressType,
                City = address.City,
                Country = address.Country,
                State = address.State,
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                AddressLine3 = address.AddressLine3,
                ZipCode = address.ZipCode
            });

            // Act
            var accountModel = MapAccountDbToAccountModel.MapTAccountToAccountDetail(tAccountFixture);

            // Assert
            var accountAddressExpect = JsonConvert.SerializeObject(expectedAccount.Address);
            var accountAddressReceived = JsonConvert.SerializeObject(accountModel.Address);
            Assert.Equal(accountAddressExpect, accountAddressReceived);
            Assert.Equal(expectedAccount.Legal?.LegalName, accountModel.Legal?.LegalName);
            Assert.Equal(expectedAccount.AccountNumber, accountModel.AccountNumber);
            Assert.Equal(expectedAccount.AccountId, accountModel.AccountId);
        }

        [Fact]
        public void MapTHubsToHubs_Should_Return_HubList()
        {
            var expected = _fixture.CreateMany<THub>();

            var result = MapperReferentialDbToBusiness.MapTHubsToHubs(expected);

            Assert.NotNull(result);
            Assert.Equal(expected.Count(), result.Count());
        }

        [Fact]
        public void MapMapTHubsToHubs_When_SourceIsNull_Should_Return_EmptyList()
        {
            var result = MapperReferentialDbToBusiness.MapTHubsToHubs(null!);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void MapHubEntityToHub_Should_Return_Hub()
        {
            var expected = _fixture.Create<THub>();

            var result = MapperReferentialDbToBusiness.MapHubEntityToHub(expected);

            Assert.NotNull(result);
            Assert.Equal(expected.HubId, result.HubId);
            Assert.Equal(expected.HubName, result.HubName);
        }

        [Fact]
        public void MapHubEntityToHub_When_SourceIsNull_Should_ReturnNull()
        {
            var result = MapperReferentialDbToBusiness.MapHubEntityToHub(null!);

            Assert.Null(result);
        }
    }
}
