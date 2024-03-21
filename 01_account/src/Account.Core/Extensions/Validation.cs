// <copyright file="Validation.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Enum;

namespace Pulse.Account.Core.Extensions
{
    public static class Validation
    {
        public static void SetStartDateAndStatus(this IEnumerable<DelegationDetails> details)
        {
            if (details?.Any() != true)
            {
                return;
            }

            foreach (var detail in details)
            {
                if (detail.StartDate == null || detail.StartDate <= DateTime.UtcNow)
                {
                    detail.StartDate = DateTime.UtcNow;
                    detail.Status = DelegationStatus.Enabled.ToString().ToLower();
                }
                else
                {
                    detail.Status = DelegationStatus.Pending.ToString().ToLower();
                }
            }
        }

        public static bool ValidateEndDateDelegation(IEnumerable<DelegationDetails> details)
        {
            if (details?.Any() != true)
            {
                return false;
            }

            return details.All(detail => detail.EndDate == null || detail.EndDate > detail.StartDate);
        }
    }
}
