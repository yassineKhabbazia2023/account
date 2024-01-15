// <copyright file="Provider.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Kpmg.Offer.Core.Models
{
    public class Provider
    {
        [Required]
        public string? ProviderName { get; set; }

        public string? ProviderEditor { get; set; }

        public string? ProviderCategory { get; set; }

        public string? LogoName { get; set; }

        public IList<Product>? Products { get; set; }
    }
}
