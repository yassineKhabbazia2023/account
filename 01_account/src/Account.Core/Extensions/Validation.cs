using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Extensions
{
    public static class Validation
    {
        public static bool ValidateStartDateDelegation(CreateDelegation delegation)
        {
            return delegation is not null &&
                delegation.StartDate is not null;
        }

        public static bool ValidateEndDateDelegation(CreateDelegation delegation)
        {
            return delegation is not null &&
                (delegation.EndDate is null ||
                    (delegation.EndDate is not null && delegation.EndDate.Value.CompareTo(delegation.StartDate) >= 0));
        }
    }
}
