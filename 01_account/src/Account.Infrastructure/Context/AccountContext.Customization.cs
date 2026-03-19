// <copyright file="AccountContext.Customization.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Constants;
using Microsoft.EntityFrameworkCore;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Context
{
    /// <summary>
    /// Customization for AccountContext should take place here
    /// because the first account Context is auto-generated (database first approach).
    /// </summary>
    public partial class AccountContext
    {
        /// <summary>
        /// Gets active accounts by explicitly disabling all EF Core query filters on <see cref="AccountEntity"/>.
        /// Only the <c>IsActive</c> condition is reapplied afterward, so any future query filters added on this entity
        /// will also be bypassed by this query.
        /// </summary>
        internal IQueryable<AccountEntity> ActiveAccounts =>
            AccountEntity.IgnoreQueryFilters().Where(a => a.IsActive);

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            // accepted status are Invited Connected Remove Declared
            modelBuilder.Entity<ContactEntity>(builder => builder.HasQueryFilter(contact => contact.IsActive));
            modelBuilder.Entity<AccountEntity>(builder => builder.HasQueryFilter(account =>
                account.IsActive
                && account.AccountType != null
                && account.AccountType.ToLower() != GlobalConstants.ProspectAccountType.ToLower()));
        }
    }
}
