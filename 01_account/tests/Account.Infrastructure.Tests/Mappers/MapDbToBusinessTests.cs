// <copyright file="MapDbToBusinessTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Newtonsoft.Json;
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Tests.Mappers
{
    public class MapDbToBusinessTests
    {
        private readonly Fixture _fixture;

        public MapDbToBusinessTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public void MapToAccount_ShouldReturnAccountModel()
        {
            // Arrange
            var addressEntity = _fixture.Build<AddressEntity>()
                .With(a => a.AddressType, AddressType.Delivery.ToString())
                .Create();
            var contactEntity = _fixture.Build<ContactEntity>()
                .With(c => c.Type, "1")
                .Create();
            var roleEntity = _fixture.Build<RoleEntity>()
                .With(c => c.Contact, contactEntity)
                .Create();
            var addressList = new List<AddressEntity>();
            addressList!.Add(addressEntity);
            var tAccountFixture = _fixture.Build<AccountEntity>()
                .With(a => a.AddressEntity, addressList)
                .With(a => a.RoleEntity, new List<RoleEntity> { roleEntity })
                .Create();
            var expectedAccount = new Core.Models.Account();
            expectedAccount.LegalName = tAccountFixture.LegalName;
            expectedAccount.AccountId = tAccountFixture.AccountId;
            expectedAccount.AccountNumber = tAccountFixture.AccountNumber;
            expectedAccount.AccountGlobalUniqueId = tAccountFixture.AccountGlobalUniqueId;
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
                ZipCode = address.ZipCode,
                Latitude = address.Latitude ?? default,
                Longitude = address.Longitude ?? default
            }).First();

            // Act
            var accountModel = MapAccountDatabaseToAccountModel.MapToAccount(tAccountFixture, 0);

            // Assert
            var accountAddressExpect = JsonConvert.SerializeObject(expectedAccount.Address);
            var accountAddressReceived = JsonConvert.SerializeObject(accountModel!.Address);
            Assert.Equal(accountAddressExpect, accountAddressReceived);
            Assert.Equal(expectedAccount.LegalName, accountModel.LegalName);
            Assert.Equal(expectedAccount.AccountNumber, accountModel.AccountNumber);
            Assert.Equal(expectedAccount.AccountId, accountModel.AccountId);
            Assert.Equal(expectedAccount.AccountGlobalUniqueId, accountModel.AccountGlobalUniqueId);
            Assert.NotNull(accountModel.Address);
            Assert.Equal(expectedAccount.Address.Latitude, accountModel.Address.Latitude);
            Assert.Equal(expectedAccount.Address.Longitude, accountModel.Address.Longitude);
        }

        [Fact]
        public void MapToAccount_WithNullSource_ShouldReturnNull()
        {
            var result = MapAccountDatabaseToAccountModel.MapToAccount(null!, 0);

            Assert.Null(result);
        }

        [Fact]
        public void MapToAccountDetail_ShouldReturnAccountDetail()
        {
            // Arrange
            var tAccountFixture = _fixture.Create<AccountEntity>();
            var expectedAccount = new AccountDetail
            {
                AccountNumber = "T12345",
                Legal = new Legal { LegalName = "Test", Siren = "123456789" },
                Phone = new List<Phone> { new Phone { PhoneNumber = "0600000000" } }
            };
            expectedAccount.Legal.LegalName = tAccountFixture.LegalName;
            expectedAccount.AccountId = tAccountFixture.AccountId;
            expectedAccount.AccountNumber = tAccountFixture.AccountNumber;
            expectedAccount.AccountGlobalUniqueId = tAccountFixture.AccountGlobalUniqueId;
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
                ZipCode = address.ZipCode,
                Latitude = address.Latitude ?? default,
                Longitude = address.Longitude ?? default
            });
            expectedAccount.CreatedBy = tAccountFixture.CreatedBy;
            expectedAccount.ModifiedBy = tAccountFixture.ModifiedBy;

            // Act
            var accountModel = MapAccountDatabaseToAccountModel.MapToAccountDetail(tAccountFixture);

            // Assert
            var accountAddressExpect = JsonConvert.SerializeObject(expectedAccount.Address);
            var accountAddressReceived = JsonConvert.SerializeObject(accountModel!.Address);
            Assert.Equal(accountAddressExpect, accountAddressReceived);
            Assert.Equal(expectedAccount.Legal?.LegalName, accountModel.Legal?.LegalName);
            Assert.Equal(expectedAccount.AccountNumber, accountModel.AccountNumber);
            Assert.Equal(expectedAccount.AccountId, accountModel.AccountId);
            Assert.Equal(expectedAccount.AccountGlobalUniqueId, accountModel.AccountGlobalUniqueId);
            Assert.Equal(expectedAccount.CreatedBy, accountModel.CreatedBy);
            Assert.Equal(expectedAccount.ModifiedBy, accountModel.ModifiedBy);
            Assert.NotNull(accountModel.Address);
            Assert.All(accountModel.Address, address =>
            {
                Assert.NotNull(address);
                var expectedAddress = expectedAccount.Address.First(a => a.AddressId == address.AddressId);
                Assert.Equal(expectedAddress.AddressId, address.AddressId);
                Assert.Equal(expectedAddress.AddressType, address.AddressType);
                Assert.Equal(expectedAddress.City, address.City);
                Assert.Equal(expectedAddress.Country, address.Country);
                Assert.Equal(expectedAddress.State, address.State);
                Assert.Equal(expectedAddress.AddressLine1, address.AddressLine1);
                Assert.Equal(expectedAddress.AddressLine2, address.AddressLine2);
                Assert.Equal(expectedAddress.AddressLine3, address.AddressLine3);
                Assert.Equal(expectedAddress.Latitude, address.Latitude);
                Assert.Equal(expectedAddress.Longitude, address.Longitude);
                Assert.Equal(expectedAddress.ZipCode, address.ZipCode);
            });
        }

        [Fact]
        public void MapToAccountDetail_WithNullSource_ShouldReturnNull()
        {
            var result = MapAccountDatabaseToAccountModel.MapToAccountDetail(null!);

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
            var expectedTotalPageCalcul = 1;

            var result = MapperReferentialDbToBusiness.MapToPagingNaf(expectedSource, expectedpageNumber, expectedTotalRows, expectedTotalPageCalcul);

            Assert.NotNull(result);
            Assert.Equal(expectedSource.Count(), result.Items!.Count());
            Assert.Equal(expectedpageNumber, result.CurrentPage);
            Assert.Equal(expectedTotalRows, result.TotalItems);
            Assert.Equal(expectedTotalPageCalcul, result.TotalPage);
        }

        [Fact]
        public void MapToPagingNaf_WithNullSource_ShouldReturnEmptyItemList()
        {
            var result = MapperReferentialDbToBusiness.MapToPagingNaf(null!, 1, 1, 1);

            Assert.NotNull(result);
            Assert.Empty(result.Items!);
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