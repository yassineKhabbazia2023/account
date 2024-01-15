// <copyright file="Offer.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace Kpmg.Offer.Core.Models
{
    public class Offer
    {
        public int OfferId { get; set; }

        [Required]
        public string? OfferName { get; set; }

        public Guid? GlobalOfferId { get; set; }

        [Required]
        public string? OfferLabel { get; set; }

        public string? OfferDescription { get; set; }

        public decimal? OfferPrice { get; set; }

        public string? OffersBookLink { get; set; }

        public string? IntranetLink { get; set; }

        public string? Tags { get; set; }

        public bool? ApprovalRequired { get; set; }

        public string? OfferPriceType { get; set; }

        public string? LogoName { get; set; }

        [Required]
        public IEnumerable<Provider>? Providers { get; set; }
    }
}
