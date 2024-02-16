// <copyright file="MapDbToBusinessTests.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
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
    }
}
