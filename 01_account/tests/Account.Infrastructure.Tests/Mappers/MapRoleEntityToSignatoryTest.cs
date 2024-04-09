// <copyright file="MapRoleEntityToSignatoryTest.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using AutoFixture;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Tests.Mappers
{
    public class MapRoleEntityToSignatoryTest
    {
        private readonly Fixture _fixture;

        public MapRoleEntityToSignatoryTest()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public void MapToContact_NullSource_ReturnsNull()
        {
            // Arrange
            RoleEntity? role = null;

            // Act
            var result = role.MapToContact();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void MapToContacts_EmptySource_ReturnsEmptyCollection()
        {
            // Arrange
            List<RoleEntity>? roles = new();
            List<Contact> expected = new();

            // Act
            var result = roles.MapToContacts();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void MapToContacts_NotNulllSource_ReturnsMappedContact()
        {
            // Arrange
            RoleEntity? role = _fixture.Create<RoleEntity?>();
            var expected = new Contact
            {
                ContactId = role!.ContactId,
                Email = role.Contact.Email,
                FirstName = role.Contact.FirstName,
                LastName = role.Contact.LastName,
                Status = role.Contact.Status,
                PersonaName = role.Contact.PersonaName,
                Office = role.Contact.Office,
                CreationDate = role.Contact.CreationDate,
            };

            // Act
            var result = role.MapToContact();

            // Assert
            Assert.Equivalent(expected, result);
        }

        [Fact]
        public void MapToContacts_NotEmptySource_ReturnsMappedContact()
        {
            // Arrange
            List<RoleEntity>? roles = _fixture.Create<List<RoleEntity>?>();
            var expected = new List<Contact>
            {
                new Contact()
                {
                    ContactId = roles!.First().ContactId,
                    Email = roles!.First().Contact.Email,
                    FirstName = roles!.First().Contact.FirstName,
                    LastName = roles!.First().Contact.LastName,
                    Status = roles!.First().Contact.Status,
                    PersonaName = roles!.First().Contact.PersonaName,
                    Office = roles!.First().Contact.Office,
                    CreationDate = roles!.First().Contact.CreationDate,
                }
            };

            // Act
            var result = roles!.MapToContacts();

            // Assert
            Assert.Equivalent(expected, result);
        }
    }
}
