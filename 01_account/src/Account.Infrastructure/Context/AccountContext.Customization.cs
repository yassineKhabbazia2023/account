using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            // accepted status are Invited Connected Remove Declared
            modelBuilder.Entity<ContactEntity>(builder => builder.HasQueryFilter(contact => contact.IsActive));
        }
    }
}
