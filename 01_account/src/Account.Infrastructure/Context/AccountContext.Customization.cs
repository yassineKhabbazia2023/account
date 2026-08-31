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

        /// <summary>
        /// Gets the Serenity modal choices, one row per contact. Declared here rather than in the generated
        /// context so that a regeneration run against a database that predates the table cannot silently drop it.
        /// Move it to the generated file at the next official EF Core Power Tools regeneration.
        /// </summary>
        public virtual DbSet<SerenityChoiceEntity> SerenityChoiceEntity { get; set; }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            // accepted status are Invited Connected Remove Declared
            modelBuilder.Entity<ContactEntity>(builder => builder.HasQueryFilter(contact => contact.IsActive));
            modelBuilder.Entity<AccountEntity>(builder => builder.HasQueryFilter(account =>
                account.IsActive
                && account.AccountType != null
                && account.AccountType.ToLower() != GlobalConstants.ProspectAccountType.ToLower()));

            // No navigation to ContactEntity on purpose: the table is only ever read by its primary key,
            // and a required navigation towards a query-filtered principal would be flagged by EF Core.
            modelBuilder.Entity<SerenityChoiceEntity>(builder =>
            {
                builder.HasKey(choice => choice.ContactId).HasName("C_SerenityChoice_PK");
                builder.ToTable("SerenityChoice", "account");
                builder.Property(choice => choice.ContactId)
                    .ValueGeneratedNever()
                    .HasComment("Identifiant technique du contact ayant fait le choix (clé primaire et étrangère vers actor.Contact)");
                builder.Property(choice => choice.IsAccepted)
                    .HasComment("Choix exprimé par le contact sur la modal Sérénité (1 = accepté, 0 = refusé)");
                builder.Property(choice => choice.ChoiceDate)
                    .HasComment("Date UTC à laquelle le choix a été enregistré");
            });
        }
    }
}
