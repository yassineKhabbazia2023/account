using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Pulse.Account.Infrastructure.Context;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Providers;

namespace Pulse.Account.Infrastructure.Tests.Providers;

public class ContactEventRepositoryTests
{
    [Fact]
    public async Task CreateContactAsync_WithContactData_ShouldCreateContact()
    {
        var options = new DbContextOptionsBuilder<AccountContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        using var context = new AccountContext(options);
        var repository = new ContactEventRepository(context);
        var contactEntity = new ContactEntity
        {
            ContactId = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@test.com",
            PersonaName = "Collab GS",
            Status = "Declared",
            Type = "collaborator",
            CreationDate = DateTime.Parse("2024-04-16T09:19:16Z"),
            ContactGlobalUniqueId = Guid.Parse("6F9619FF-8B86-D011-B42D-00C04FC964FF"),
        };

        // Act
        await repository.CreateContactAsync(contactEntity);

        // Assert
        var addedContact = await context.ContactEntity.FirstOrDefaultAsync();

        Assert.NotNull(addedContact);
        Assert.Equal(1, addedContact.ContactId);
        Assert.Equal(Guid.Parse("6F9619FF-8B86-D011-B42D-00C04FC964FF"), addedContact.ContactGlobalUniqueId);
        Assert.Equal("John", addedContact.FirstName);
        Assert.Equal("Doe", addedContact.LastName);
        Assert.Equal("john.doe@test.com", addedContact.Email);
        Assert.Equal("Collab GS", addedContact.PersonaName);
        Assert.Equal("Declared", addedContact.Status);
        Assert.Equal("collaborator", addedContact.Type);
        Assert.Equal(DateTime.Parse("2024-04-16T09:19:16Z"), addedContact.CreationDate);
    }
}
