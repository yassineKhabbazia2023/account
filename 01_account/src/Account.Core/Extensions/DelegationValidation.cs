// <copyright file="DelegationValidation.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Enum;

namespace Pulse.Account.Core.Extensions
{
    public static class DelegationValidation
    {
        public static void SetDelegationInformation(this IEnumerable<DelegationDetails> details)
        {
            if (details?.Any() != true)
            {
                return;
            }

            foreach (var detail in details)
            {
                if (detail.StartDate == null || detail.StartDate.Value.Date <= DateTime.UtcNow.Date)
                {
                    detail.StartDate = DateTime.UtcNow;
                    detail.Status = DelegationStatus.Enabled.ToString().ToLower();
                    detail.IsRoleToCreate = true;
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

            return details.All(detail => detail.EndDate == null || detail.EndDate.Value.Date > detail.StartDate!.Value.Date);
        }
    }
}
