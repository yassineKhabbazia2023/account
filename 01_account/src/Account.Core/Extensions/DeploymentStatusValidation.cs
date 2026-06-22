// <copyright file="DeploymentStatusValidation.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>
using Pulse.Account.Core.Enum;
using Pulse.Account.Core.Exceptions;
using Pulse.ExceptionMiddleware.Exceptions;

namespace Pulse.Account.Core.Extensions
{
    public static class DeploymentStatusValidation
    {
        public static List<int>? GetValidDeploymentStatuses(List<int>? deploymentStatuses)
        {
            if (deploymentStatuses == null)
            {
                return null;
            }

            var firstInvalid = deploymentStatuses
                .Where(deploymentStatus => !System.Enum.IsDefined(typeof(DeploymentStatus), deploymentStatus))
                .Cast<int?>()
                .FirstOrDefault();

            if (firstInvalid.HasValue)
            {
                throw new BadRequestException(Errors.BadRequestDeploymentStatusCode, string.Format(Errors.BadRequestDeploymentStatusMessage, firstInvalid.Value));
            }

            return deploymentStatuses;
        }
    }
}
