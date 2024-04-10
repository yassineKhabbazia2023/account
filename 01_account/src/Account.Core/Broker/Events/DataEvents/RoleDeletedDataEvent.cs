// <copyright file="RoleDeletedDataEvent.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Broker.Events.DataEvents
{
    public class RoleDeletedDataEvent
    {
        public int AccountId { get; set; }

        public int ContactId { get; set; }
    }
}
