// <copyright file="Accounting.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

namespace Pulse.Account.Core.Models
{
    public class Accounting
    {
        public DateTime? FiscalExerciseStartDate { get; set; }

        public int? FiscalExerciseDuration { get; set; }

        public string? AccountingType { get; set; }

        public string? FiscalSystem { get; set; }

        public string? TaxationSystem { get; set; }

        public string? ActivityType { get; set; }

        public string? ActivityDescription { get; set; }
    }
}
