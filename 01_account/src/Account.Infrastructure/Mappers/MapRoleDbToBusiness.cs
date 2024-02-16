using Pulse.Account.Core.Models;
using Pulse.Account.Infrastructure.Entities;

namespace Pulse.Account.Infrastructure.Mappers
{
    public static class MapRoleDbToBusiness
    {
        public static Signatory? MapTContactToSignatory(this TRole? role)
        {
            if (role == null)
            {
                return null;
            }

            return new Signatory()
            {
                ContactId = role.ContactId,
                FirstName = role.Contact.FirstName,
                LastName = role.Contact.LastName,
                ContactEmail = role.Contact.ContactEmail,
            };
        }

        public static IEnumerable<Signatory> MapTRolesToSignatory(this IReadOnlyCollection<TRole> roles)
        {
            var res = new List<Signatory>();

            if (roles == null || roles.Count == 0)
            {
                return res;
            }

            foreach (var role in roles)
            {
                res.Add(role.MapTContactToSignatory() !);
            }

            return res;
        }
    }
}
