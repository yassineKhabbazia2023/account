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
        public static int? GetValidDeploymentStatus(int? deploymentStatus)
        {
            return deploymentStatus == null || System.Enum.IsDefined(typeof(DeploymentStatus), deploymentStatus)
                ? deploymentStatus
                : throw new NotFoundException(Errors.BadRequestDeploymentStatusCode, string.Format(Errors.BadRequestDeploymentStatusMessage, deploymentStatus));
        }
    }
}
