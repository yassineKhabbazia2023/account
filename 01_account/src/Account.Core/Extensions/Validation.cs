using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Extensions
{
    public static class Validation
    {
        public static bool ValidateDateDelegation(CreateDelegation delegation)
        {
            return delegation != null &&
                (delegation.EndDate == null ||
                    (delegation.EndDate != null && delegation.EndDate.Value.CompareTo(delegation.StartDate) >= 0));
        }
    }
}
