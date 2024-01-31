// <copyright file="Accounting.cs" company="KPMG">
// Copyright (c) KPMG. All rights reserved.
// </copyright>

namespace Kpmg.Account.Core.Models
{
    public class Accounting
    {
        public string? FiscalExerciseStartDate { get; set; }

        public int FiscalExerciseDuration { get; set; }

        public string? AccountingType { get; set; }

        public string? FiscalSystem { get; set; }
    }
}
