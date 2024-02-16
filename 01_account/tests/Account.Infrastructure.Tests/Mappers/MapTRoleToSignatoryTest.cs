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
        public void MapTContactToSignatory_CaseNull()
        {
            TRoles? role = null;
            var result = role.MapTContactToSignatory();
            Assert.Null(result);
        }

        [Fact]
        public void MapTContactToSignatory_CaseSuccess()
        {
            // Arrange
            TRoles? role = _fixture.Create<TRoles?>();
            var expected = new Signatory()
            {
                ContactId = role!.ContactId,
                ContactEmail = role.Contact.ContactEmail,
                FirstName = role.Contact.FirstName,
                LastName = role.Contact.LastName,
            };

            // Act
            var result = role.MapTContactToSignatory();

            // Assert
            Assert.Equivalent(expected, result);
        }

        [Fact]
        public void MapTRolesToSignatory_CaseEmpty()
        {
            List<TRoles>? roles = new List<TRoles>();
            var expected = new List<Signatory>();
            var result = roles.MapTRolesToSignatory();
            Assert.Equal(expected, result);
        }

        [Fact]
        public void MapTRolesToSignatory_CaseSuccess()
        {
            // Arrange
            List<TRoles>? roles = _fixture.Create<List<TRoles>?>();
            var expected = new List<Signatory>()
            {
                new Signatory()
                {
                    ContactId = roles!.First().ContactId,
                    ContactEmail = roles!.First().Contact.ContactEmail,
                    FirstName = roles!.First().Contact.FirstName,
                    LastName = roles!.First().Contact.LastName,
                }
            };

            // Act
            var result = roles!.MapTRolesToSignatory();

            // Assert
            Assert.Equivalent(expected, result);
        }
    }
}
