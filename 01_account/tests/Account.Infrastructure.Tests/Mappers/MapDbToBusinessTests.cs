// <copyright file="MapDbToBusinessTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Enum;
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
        public void MapToAccount_ShouldReturnAccountModel()
        {
            // Arrange
            var addressEntity = _fixture.Build<AddressEntity>().With(a => a.AddressType, AddressType.delivery.ToString()).Create();
            var addressList = new List<AddressEntity>();
            addressList!.Add(addressEntity);
            var tAccountFixture = _fixture.Build<AccountEntity>().With(a => a.AddressEntity, addressList).Create();
            var expectedAccount = new Core.Models.Account();
            expectedAccount.LegalName = tAccountFixture.LegalName;
            expectedAccount.AccountId = tAccountFixture.AccountId;
            expectedAccount.AccountNumber = tAccountFixture.AccountNumber;
            expectedAccount.Address = tAccountFixture.AddressEntity.Select(address => new Address()
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
            }).First();

            // Act
            var accountModel = MapAccountDbToAccountModel.MapToAccount(tAccountFixture, 0);

            // Assert
            var accountAddressExpect = JsonConvert.SerializeObject(expectedAccount.Address);
            var accountAddressReceived = JsonConvert.SerializeObject(accountModel!.Address);
            Assert.Equal(accountAddressExpect, accountAddressReceived);
            Assert.Equal(expectedAccount.LegalName, accountModel.LegalName);
            Assert.Equal(expectedAccount.AccountNumber, accountModel.AccountNumber);
            Assert.Equal(expectedAccount.AccountId, accountModel.AccountId);
        }

        [Fact]
        public void MapToAccount_WithNullSource_ShouldReturnNull()
        {
            var result = MapAccountDbToAccountModel.MapToAccount(null!, 0);

            Assert.Null(result);
        }

        [Fact]
        public void MapToAccountDetail_ShouldReturnAccountDetail()
        {
            // Arrange
            var tAccountFixture = _fixture.Create<AccountEntity>();
            var expectedAccount = new AccountDetail();
            expectedAccount.Legal = new Legal();
            expectedAccount.Legal.LegalName = tAccountFixture.LegalName;
            expectedAccount.AccountId = tAccountFixture.AccountId;
            expectedAccount.AccountNumber = tAccountFixture.AccountNumber;
            expectedAccount.Address = tAccountFixture.AddressEntity.Select(address => new Address()
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
            var accountModel = MapAccountDbToAccountModel.MapToAccountDetail(tAccountFixture);

            // Assert
            var accountAddressExpect = JsonConvert.SerializeObject(expectedAccount.Address);
            var accountAddressReceived = JsonConvert.SerializeObject(accountModel.Address);
            Assert.Equal(accountAddressExpect, accountAddressReceived);
            Assert.Equal(expectedAccount.Legal?.LegalName, accountModel.Legal?.LegalName);
            Assert.Equal(expectedAccount.AccountNumber, accountModel.AccountNumber);
            Assert.Equal(expectedAccount.AccountId, accountModel.AccountId);
        }

        [Fact]
        public void MapToAccountDetail_WithNullSource_ShouldReturnNull()
        {
            var result = MapAccountDbToAccountModel.MapToAccountDetail(null!);

            Assert.Null(result);
        }

        [Fact]
        public void MapHubEntitiesToHubs_ShouldReturnHubs()
        {
            var expected = _fixture.CreateMany<HubEntity>();

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
            var expected = _fixture.Create<HubEntity>();

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
        public void MapToPagingNaf_ShouldReturnPagingNaf()
        {
            var expectedSource = _fixture.CreateMany<NafEntity>();
            var expectedpageNumber = 1;
            var expectedTotalRows = 1;
            var expectedTotalPageCalcul = 1f;

            var result = MapperReferentialDbToBusiness.MapToPagingNaf(expectedSource, expectedpageNumber, expectedTotalRows, expectedTotalPageCalcul);

            Assert.NotNull(result);
            Assert.Equal(expectedSource.Count(), result.Items.Count());
            Assert.Equal(expectedpageNumber, result.CurrentPage);
            Assert.Equal(expectedTotalRows, result.TotalItems);
            Assert.Equal(expectedTotalPageCalcul, result.TotalPage);
        }

        [Fact]
        public void MapToPagingNaf_WithNullSource_ShouldReturnEmptyItemList()
        {
            var result = MapperReferentialDbToBusiness.MapToPagingNaf(null!, 1, 1, 1f);

            Assert.NotNull(result);
            Assert.Empty(result.Items);
        }

        [Fact]
        public void MapNafEntitiesToNafs_ShouldReturnNafs()
        {
            var expected = _fixture.CreateMany<NafEntity>();

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
            var expected = _fixture.Create<NafEntity>();

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
