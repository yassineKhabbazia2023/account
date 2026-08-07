// <copyright file="LegalFormResolverTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Extensions;

namespace Pulse.Account.Core.Tests.Extensions
{
    public class LegalFormResolverTests
    {
        [Theory]
        [InlineData("ENT IND", "ENT IND", "ENTREPRISE INDIVIDUELLE")]
        [InlineData("SAS", "SAS", "SAS - SOCIÉTÉ PAR ACTIONS SIMPLIFIÉE")]
        [InlineData("SARL", "SARL", "SARL - SOCIÉTÉ À RESPONSABILITÉ LIMITÉ")]
        public void Resolve_WithKnownCode_ShouldReturnCodeAndLabel(string input, string expectedCode, string expectedLabel)
        {
            // Act
            var (code, label) = LegalFormResolver.Resolve(input);

            // Assert
            Assert.Equal(expectedCode, code);
            Assert.Equal(expectedLabel, label);
        }

        [Theory]
        [InlineData("ent ind")]
        [InlineData("Ent Ind")]
        [InlineData(" ENT IND ")]
        public void Resolve_WithKnownCodeIgnoringCaseAndSpaces_ShouldReturnReferentialCodeAndLabel(string input)
        {
            // Act
            var (code, label) = LegalFormResolver.Resolve(input);

            // Assert
            Assert.Equal("ENT IND", code);
            Assert.Equal("ENTREPRISE INDIVIDUELLE", label);
        }

        [Theory]
        [InlineData("ENTREPRISE INDIVIDUELLE")]
        [InlineData("FORME INCONNUE")]
        public void Resolve_WithUnknownValue_ShouldReturnOriginalLabelWithoutCode(string input)
        {
            // Act
            var (code, label) = LegalFormResolver.Resolve(input);

            // Assert
            Assert.Null(code);
            Assert.Equal(input, label);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Resolve_WithNullOrWhiteSpace_ShouldReturnInputWithoutCode(string? input)
        {
            // Act
            var (code, label) = LegalFormResolver.Resolve(input);

            // Assert
            Assert.Null(code);
            Assert.Equal(input, label);
        }
    }
}
