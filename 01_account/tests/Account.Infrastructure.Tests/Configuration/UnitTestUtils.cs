// <copyright file="UnitTestUtils.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Repositories;

namespace Pulse.Account.Infrastructure.Tests.Configuration
{
    [ExcludeFromCodeCoverage]
    public static class UnitTestUtils
    {
        public static async Task<AccountRepository> InitAccountRepository(Fixture fixture, List<TAccount> accountsModel, AccountContext accountContext)
        {
            var rolesModel = accountsModel.SelectMany(item => item.TRole).ToList();
            var deploymentsModel = fixture.Create<List<TDeploymentPlanning>>().AsQueryable();
            var addressModel = fixture.Create<List<TAddress>>().AsQueryable();
            var phoneModel = fixture.Create<List<TPhone>>().AsQueryable();
            var contactModel = fixture.Create<List<TContact>>();

            accountsModel.ForEach(itemAccount =>
            {
                foreach (var itemRole in itemAccount.TRole)
                {
                    itemRole.AccountId = itemAccount.AccountId;
                }
            });

            accountContext.TAccount.AddRange(accountsModel);
            accountContext.TRole.AddRange(rolesModel);
            accountContext.TDeploymentPlanning.AddRange(deploymentsModel);
            accountContext.TAddress.AddRange(addressModel);
            accountContext.TContact.AddRange(contactModel);
            accountContext.TPhone.AddRange(phoneModel);
            await accountContext.SaveChangesAsync();

            return new AccountRepository(accountContext);
        }
    }
}
