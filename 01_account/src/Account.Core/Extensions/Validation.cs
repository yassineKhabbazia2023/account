using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Extensions
{
    public static class Validation
    {
        public static bool ValidateStartDateDelegation(CreateDelegation delegation)
        {
            return delegation?.StartDate != null;
        }

        public static bool ValidateEndDateDelegation(CreateDelegation delegation)
        {
            if (delegation?.EndDate == null)
            {
                return true;
            }

            return delegation?.EndDate > delegation?.StartDate;
        }
    }
}
