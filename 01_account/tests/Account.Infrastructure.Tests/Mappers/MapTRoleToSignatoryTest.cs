using AutoFixture;
using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;
using Pulse.Account.Infrastructure.Mappers;

namespace Pulse.Account.Infrastructure.Tests.Mappers
{
    public class MapTRoleToSignatoryTest
    {
        private readonly Fixture _fixture;

        public MapTRoleToSignatoryTest()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public void MapToContact_NullSource_ReturnsNull()
        {
            // Arrange
            TRole? role = null;

            // Act
            var result = role.MapToContact();

            // Assert
            Assert.Null(result);
        }


        [Fact]
        public void MapToContacts_EmptySource_ReturnsEmptyCollection()
        {
            // Arrange
            List<TRole>? roles = new();
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
            TRole? role = _fixture.Create<TRole?>();
            var expected = new Contact
            {
                ContactId = role!.ContactId,
                ContactEmail = role.Contact.ContactEmail,
                FirstName = role.Contact.FirstName,
                LastName = role.Contact.LastName,
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
            List<TRole>? roles = _fixture.Create<List<TRole>?>();
            var expected = new List<Contact>
            {
                new Contact()
                {
                    ContactId = roles!.First().ContactId,
                    ContactEmail = roles!.First().Contact.ContactEmail,
                    FirstName = roles!.First().Contact.FirstName,
                    LastName = roles!.First().Contact.LastName,
                }
            };

            // Act
            var result = roles!.MapToContacts();

            // Assert
            Assert.Equivalent(expected, result);
        }
    }
}
