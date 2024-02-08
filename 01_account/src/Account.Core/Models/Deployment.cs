// <copyright file="Deployment.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

namespace Kpmg.Account.Core.Models
{
    public class Deployment
    {
        public int DeploymentId { get; set; }

        public DateTime? DeploymentDate { get; set; }

        public int Status { get; set; }
    }
}
