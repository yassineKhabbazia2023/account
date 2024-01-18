// <copyright file="Accounting.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

namespace Kpmg.Account.Core.Models
{
    public class Accounting
    {
        public DateOnly FiscalExerciceStartDate { get; set; }

        public int FiscalExerciceDuration { get; set; }

        public string? AccountingMethod { get; set; }

        public string? ActivityType { get; set; }

        public string? FiscalSystem { get; set; }

        public string? TaxationSystem { get; set; }
    }
}
