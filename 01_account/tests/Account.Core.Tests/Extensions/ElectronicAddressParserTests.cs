// <copyright file="ElectronicAddressParserTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Extensions;

namespace Pulse.Account.Core.Tests.Extensions
{
    public class ElectronicAddressParserTests
    {
        [Fact]
        public void ToElectronicAddresses_WithSingleSite_ShouldReturnOneAddress()
        {
            // Arrange
            var raw = "SiteParis-COD01-12345678900010-ADR001";

            // Act
            var result = raw.ToElectronicAddresses();

            // Assert
            Assert.Single(result);
            Assert.Equal("SiteParis", result[0].SiteName);
            Assert.Equal("COD01", result[0].SiteCode);
            Assert.Equal("12345678900010", result[0].Siret);
            Assert.Equal("ADR001", result[0].AddressingId);
        }

        [Fact]
        public void ToElectronicAddresses_WithMultipleSites_ShouldSplitLinesThenFields()
        {
            // Arrange
            var raw = "SiteParis-COD01-12345678900010-ADR001/SiteLyon-COD02-98765432100010-ADR002";

            // Act
            var result = raw.ToElectronicAddresses();

            // Assert
            Assert.Equal(2, result.Count);

            Assert.Equal("SiteParis", result[0].SiteName);
            Assert.Equal("COD01", result[0].SiteCode);
            Assert.Equal("12345678900010", result[0].Siret);
            Assert.Equal("ADR001", result[0].AddressingId);

            Assert.Equal("SiteLyon", result[1].SiteName);
            Assert.Equal("COD02", result[1].SiteCode);
            Assert.Equal("98765432100010", result[1].Siret);
            Assert.Equal("ADR002", result[1].AddressingId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ToElectronicAddresses_WithNullOrWhiteSpace_ShouldReturnEmptyList(string? raw)
        {
            // Act
            var result = raw.ToElectronicAddresses();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void ToElectronicAddresses_WithIncompleteLine_ShouldReturnNullForMissingFields()
        {
            // Arrange
            var raw = "SiteParis-COD01/SiteLyon-COD02-98765432100010-ADR002";

            // Act
            var result = raw.ToElectronicAddresses();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("SiteParis", result[0].SiteName);
            Assert.Equal("COD01", result[0].SiteCode);
            Assert.Null(result[0].Siret);
            Assert.Null(result[0].AddressingId);
        }

        [Fact]
        public void ToElectronicAddresses_WithEmptyFieldValue_ShouldReturnNullForThatField()
        {
            // Arrange
            var raw = "SiteParis--12345678900010-ADR001";

            // Act
            var result = raw.ToElectronicAddresses();

            // Assert
            Assert.Single(result);
            Assert.Equal("SiteParis", result[0].SiteName);
            Assert.Null(result[0].SiteCode);
            Assert.Equal("12345678900010", result[0].Siret);
            Assert.Equal("ADR001", result[0].AddressingId);
        }

        [Fact]
        public void ToElectronicAddresses_WithExtraDashInLastField_ShouldNotBreakEarlierFieldsAlignment()
        {
            // Arrange
            var raw = "SiteParis-COD01-12345678900010-ADR001-SUFFIX";

            // Act
            var result = raw.ToElectronicAddresses();

            // Assert
            Assert.Single(result);
            Assert.Equal("SiteParis", result[0].SiteName);
            Assert.Equal("COD01", result[0].SiteCode);
            Assert.Equal("12345678900010", result[0].Siret);
            Assert.Equal("ADR001-SUFFIX", result[0].AddressingId);
        }

        [Fact]
        public void ToElectronicAddresses_WithBlankLineBetweenSeparators_ShouldSkipIt()
        {
            // Arrange
            var raw = "SiteParis-COD01-12345678900010-ADR001//SiteLyon-COD02-98765432100010-ADR002";

            // Act
            var result = raw.ToElectronicAddresses();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("SiteParis", result[0].SiteName);
            Assert.Equal("SiteLyon", result[1].SiteName);
        }
    }
}
