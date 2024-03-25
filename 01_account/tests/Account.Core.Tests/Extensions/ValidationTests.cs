// <copyright file="ValidationTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Extensions;
using Pulse.Account.Core.Models;
using Pulse.Account.Core.Models.Enum;

namespace Pulse.Account.Core.Tests.Extensions
{
    public class ValidationTests
    {
        [Fact]
        public void SetStartDateAndStatusWithStartDateInFuture_ShouldSetStatusToPending()
        {
            var details = new List<DelegationDetails>
            {
                new()
                {
                    DelegateeId = 1,
                    StartDate = DateTime.UtcNow.AddDays(1)
                }
            };

            details.SetDelegationInformation();

            Assert.Equal(DelegationStatus.Pending.ToString().ToLower(), details.FirstOrDefault() !.Status);
            Assert.False(details.FirstOrDefault() !.IsRoleToCreate);
        }

        [Theory]
        [MemberData(nameof(StartDateData))]
        public void SetStartDateAndStatusWithStartDateNullOrBeforeNow_ShouldSetStatusToEnabledAndStartDateToNowAndIsRoleToCreateToTrue(DateTime? startDate)
        {
            var details = new List<DelegationDetails>
            {
                new()
                {
                    DelegateeId = 1,
                    StartDate = startDate
                }
            };

            details.SetDelegationInformation();

            Assert.Equal(DelegationStatus.Enabled.ToString().ToLower(), details.FirstOrDefault() !.Status);
            Assert.Equal(DateTime.Now.Date, details.FirstOrDefault() !.StartDate!.Value.Date);
            Assert.True(details.FirstOrDefault() !.IsRoleToCreate);
        }

        [Theory]
        [MemberData(nameof(EndDateData))]
        public void ValidateEndDateDelegationWithNullEndDateOrEndDateAfterStartDate_ShouldReturnTrue(DateTime? endDate)
        {
            var details = new List<DelegationDetails>
            {
                new()
                {
                    DelegateeId = 1,
                    StartDate = DateTime.UtcNow,
                    EndDate = endDate,
                }
            };

            var result = DelegationValidation.ValidateEndDateDelegation(details);

            Assert.True(result);
        }

        [Fact]
        public void ValidateEndDateDelegationWithEndDateBeforeStartDate_ShouldReturnFalse()
        {
            var details = new List<DelegationDetails>
            {
                new()
                {
                    DelegateeId = 1,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(-1),
                }
            };

            var result = DelegationValidation.ValidateEndDateDelegation(details);

            Assert.False(result);
        }

        [Theory]
        [MemberData(nameof(DelegDetailsData))]
        public void ValidateEndDateDelegationWithNullOrEmptySource_ShouldReturnFalse(IEnumerable<DelegationDetails> details)
        {
            var result = DelegationValidation.ValidateEndDateDelegation(details);

            Assert.False(result);
        }

        public static IEnumerable<object[]> StartDateData => new List<object[]>
        {
            new object[] { null! },
            new object[] { DateTime.MinValue }
        };

        public static IEnumerable<object[]> EndDateData => new List<object[]>
        {
            new object[] { null! },
            new object[] { DateTime.MaxValue }
        };

        public static IEnumerable<object[]> DelegDetailsData => new List<object[]>
        {
            new object[] { null! },
            new object[] { Enumerable.Empty<DelegationDetails>() }
        };
    }
}
