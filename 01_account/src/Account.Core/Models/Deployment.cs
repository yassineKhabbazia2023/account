// <copyright file="Deployment.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models
{
    public class Deployment
    {
        public int DeploymentId { get; set; }

        public DateTime? DeploymentDate { get; set; }

        public int Status { get; set; }
    }
}
