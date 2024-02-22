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
        public void MapTAccountToAccountModel_ShouldReturnAccountModel()
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
        public void MapTAccountToAccountModel_WithNullSource_ShouldReturnNull()
        {
            var result = MapAccountDbToAccountModel.MapTAccountToAccountModel(null!);

            Assert.Null(result);
        }

        [Fact]
        public void MapTAccountToAccountDetail_ShouldReturnAccountDetail()
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
        public void MapTAccountToAccountDetail_WithNullSource_ShouldReturnNull()
        {
            var result = MapAccountDbToAccountModel.MapTAccountToAccountDetail(null!);

            Assert.Null(result);
        }

        [Fact]
        public void MapHubEntitiesToHubs_ShouldReturnHubList()
        {
            var expected = _fixture.CreateMany<THub>();

            var result = MapperReferentialDbToBusiness.MapHubEntitiesToHubs(expected);

            Assert.NotNull(result);
            Assert.Equal(expected.Count(), result.Count());
        }

        [Fact]
        public void MapHubEntitiesToHubs_WithNullSource_ShouldReturnEmptyList()
        {
            var result = MapperReferentialDbToBusiness.MapHubEntitiesToHubs(null!);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void MapHubEntityToHub_ShouldReturnHub()
        {
            var expected = _fixture.Create<THub>();

            var result = MapperReferentialDbToBusiness.MapHubEntityToHub(expected);

            Assert.NotNull(result);
            Assert.Equal(expected.HubId, result.HubId);
            Assert.Equal(expected.HubName, result.HubName);
        }

        [Fact]
        public void MapHubEntityToHub_WithNullSource_ShouldReturnNull()
        {
            var result = MapperReferentialDbToBusiness.MapHubEntityToHub(null!);

            Assert.Null(result);
        }

        [Fact]
        public void MapNafEntitiesToNafs_ShouldReturnNafList()
        {
            var expected = _fixture.CreateMany<TNaf>();

            var result = MapperReferentialDbToBusiness.MapNafEntitiesToNafs(expected);

            Assert.NotNull(result);
            Assert.Equal(expected.Count(), result.Count());
        }

        [Fact]
        public void MapNafEntitiesToNafs_WithNullSource_ShouldReturnEmptyList()
        {
            var result = MapperReferentialDbToBusiness.MapNafEntitiesToNafs(null!);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void MapNafEntityToNaf_ShouldReturnNaf()
        {
            var expected = _fixture.Create<TNaf>();

            var result = MapperReferentialDbToBusiness.MapNafEntityToNaf(expected);

            Assert.NotNull(result);
            Assert.Equal(expected.NafId, result.NafId);
            Assert.Equal(expected.NafCode, result.NafCode);
            Assert.Equal(expected.NafLabel, result.NafLabel);
        }

        [Fact]
        public void MapNafEntityToNaf_WithNullSource_ShouldReturnNull()
        {
            var result = MapperReferentialDbToBusiness.MapNafEntityToNaf(null!);

            Assert.Null(result);
        }
    }
}
