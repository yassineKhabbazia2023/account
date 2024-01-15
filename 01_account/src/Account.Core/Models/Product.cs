// <copyright file="Product.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace Kpmg.Offer.Core.Models
{
    public class Product
    {
        [Required]
        public string? ProductName { get; set; }

        [Required]
        public string? ProductLabel { get; set; }

        public string? ProductDescription { get; set; }

        public string? PublicProductName { get; set; }

        public string? PublicProductLabel { get; set; }

        public string? PublicProductDescription { get; set; }

        public string? ProductBusiness { get; set; }

        public bool Enabled { get; set; }

        [Required]
        public bool IsOptionnal { get; set; }

        public decimal? Price { get; set; }

        public Guid? GlobalProductId { get; set; }

        public string? PriceType { get; set; }
    }
}
