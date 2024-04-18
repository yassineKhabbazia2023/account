// <copyright file="Paging.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models.Utils
{
    public class Paging<T>
    {
        public IEnumerable<T>? Items { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPage { get; set; }

        public int TotalItems { get; set; }
    }
}
