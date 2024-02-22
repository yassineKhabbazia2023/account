// <copyright file="CreateRole.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Requests
{
    public class CreateRole
    {
        public int AccountId { get; set; }

        public int ContactId { get; set; }

        public bool? IsSignatory { get; set; }

        public bool? IsFavorite { get; set; }
    }
}
