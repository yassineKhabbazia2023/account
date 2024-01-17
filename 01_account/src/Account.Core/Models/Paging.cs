// <copyright file="Paging.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Kpmg.Account.Core.Models
{
    public class Paging<T>
    {
        public IEnumerable<T>? Items { get; set; }

        public int CurrentPage { get; set; }

        public int TotalPage { get; set; }

        public int TotalItems { get; set; }
    }
}
