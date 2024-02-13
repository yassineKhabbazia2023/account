// <copyright file="UnitTestUtils.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using Pulse.Account.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Pulse.Account.Infrastructure.Entities;
using Kpmg.Account.Infrastructure.Repositories;

namespace Pulse.Account.Infrastructure.Tests.Configuration
{
    [ExcludeFromCodeCoverage]
    public static class UnitTestUtils
    {
        public static AccountRepository InitAccountRepository(Fixture fixture, List<TAccount> accountsModel)
        {
            var rolesModel = accountsModel.SelectMany(item => item.TRoles).ToList();
            var deploymentsModel = fixture.Create<List<TDeploymentPlanning>>().AsQueryable();
            var addressModel = fixture.Create<List<TAddress>>().AsQueryable();
            var phoneModel = fixture.Create<List<TPhone>>().AsQueryable();
            var contactModel = fixture.Create<List<TContact>>();

            accountsModel.ForEach(itemAccount =>
            {
                foreach (var itemRole in itemAccount.TRoles)
                {
                    itemRole.AccountId = itemAccount.AccountId;
                }
            });
            rolesModel.ForEach(item => item.ContactId = 123);
            contactModel.ForEach(item => item.ContactId = 123);
            var contactModelIQueryable = contactModel.AsQueryable();
            var rolesModelIQueryable = rolesModel.AsQueryable();
            var accountModelIQueryable = accountsModel.AsQueryable().BuildMock();

            var accountMockSet = new Mock<DbSet<TAccount>>();
            accountMockSet.As<IQueryable<TAccount>>().Setup(m => m.Provider).Returns(accountModelIQueryable.Provider);
            accountMockSet.As<IQueryable<TAccount>>().Setup(m => m.Expression).Returns(accountModelIQueryable.Expression);

            var roleMockSet = new Mock<DbSet<TRoles>>();
            roleMockSet.As<IQueryable<TRoles>>().Setup(m => m.Provider).Returns(rolesModelIQueryable.Provider);
            roleMockSet.As<IQueryable<TRoles>>().Setup(m => m.Expression).Returns(rolesModelIQueryable.Expression);

            var deploymentMockSet = new Mock<DbSet<TDeploymentPlanning>>();
            deploymentMockSet.As<IQueryable<TDeploymentPlanning>>().Setup(m => m.Provider).Returns(deploymentsModel.Provider);
            deploymentMockSet.As<IQueryable<TDeploymentPlanning>>().Setup(m => m.Expression).Returns(deploymentsModel.Expression);

            var addressMockSet = new Mock<DbSet<TAddress>>();
            addressMockSet.As<IQueryable<TAddress>>().Setup(m => m.Provider).Returns(addressModel.Provider);
            addressMockSet.As<IQueryable<TAddress>>().Setup(m => m.Expression).Returns(addressModel.Expression);

            var contactMockSet = new Mock<DbSet<TContact>>();
            contactMockSet.As<IQueryable<TAddress>>().Setup(m => m.Provider).Returns(contactModelIQueryable.Provider);
            contactMockSet.As<IQueryable<TAddress>>().Setup(m => m.Expression).Returns(contactModelIQueryable.Expression);

            var phoneMockSet = new Mock<DbSet<TPhone>>();
            phoneMockSet.As<IQueryable<TPhone>>().Setup(m => m.Provider).Returns(phoneModel.Provider);
            phoneMockSet.As<IQueryable<TPhone>>().Setup(m => m.Expression).Returns(phoneModel.Expression);

            var mockContext = new Mock<AccountContext>();
            mockContext.Setup(m => m.TAccount).Returns(accountMockSet.Object);
            mockContext.Setup(m => m.TRoles).Returns(roleMockSet.Object);
            mockContext.Setup(m => m.TDeploymentPlanning).Returns(deploymentMockSet.Object);
            mockContext.Setup(m => m.TAddress).Returns(addressMockSet.Object);
            mockContext.Setup(m => m.TContact).Returns(contactMockSet.Object);
            mockContext.Setup(m => m.TPhone).Returns(phoneMockSet.Object);

            return new AccountRepository(mockContext.Object);
        }
    }
}
