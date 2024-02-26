using Pulse.Account.Core.Requests;

namespace Pulse.Account.Core.Extensions
{
    public static class Validation
    {
        public static bool ValidateDateDelegation(CreateDelegation delegation)
        {
            if (delegation.EndDate.CompareTo(delegation.StartDate) < 0)
            {
                return false;
            }

            return true;
        }
    }
}
