// <copyright file="UnitTestUtils.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using Pulse.Account.Infrastructure.Entities;
using Kpmg.Account.Infrastructure.Repositories;
using Pulse.Account.Infrastructure.Context;
using System.Data;

namespace Pulse.Account.Infrastructure.Tests.Configuration
{
    [ExcludeFromCodeCoverage]
    public static class UnitTestUtils
    {
        public static async Task<AccountRepository> InitAccountRepository(Fixture fixture, List<TAccount> accountsModel, AccountContext accountContext)
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

            accountContext.TAccount.AddRange(accountsModel);
            accountContext.TRoles.AddRange(rolesModel);
            accountContext.TDeploymentPlanning.AddRange(deploymentsModel);
            accountContext.TAddress.AddRange(addressModel);
            accountContext.TContact.AddRange(contactModel);
            accountContext.TPhone.AddRange(phoneModel);
            await accountContext.SaveChangesAsync();

            return new AccountRepository(accountContext);
        }
    }
}
