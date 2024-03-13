// <copyright file="Statistics.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models
{
    public class Statistics
    {
        public int AccountToDeploy { get; set; }

        public int AccountInProgress { get; set; }

        public int AccountConnected { get; set; }

        public int ContactDeclared { get; set; }

        public int ContactInvited { get; set; }

        public int ContactConnected { get; set; }
    }
}
