// <copyright file="NafCodeFormatterTests.cs" company="Pulse">
// Copyright (c) Pulse. All rights reserved.
// </copyright>

using Pulse.Account.Core.Extensions;

namespace Pulse.Account.Core.Tests.Extensions
{
    public class NafCodeFormatterTests
    {
        [Theory]
        [InlineData("6234Z", "62.34Z")]
        [InlineData("62.34Z", "62.34Z")]
        [InlineData("6201z", "62.01z")]
        [InlineData(" 6234Z ", "62.34Z")]
        public void Format_WithFiveCharacterCode_ShouldAddDotAtThirdPosition(string input, string expected)
        {
            // Act
            var result = NafCodeFormatter.Format(input);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("623Z")]
        [InlineData("62345Z")]
        [InlineData("Z")]
        public void Format_WithInvalidLength_ShouldReturnOriginal(string input)
        {
            // Act
            var result = NafCodeFormatter.Format(input);

            // Assert
            Assert.Equal(input, result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Format_WithNullOrWhiteSpace_ShouldReturnInput(string? input)
        {
            // Act
            var result = NafCodeFormatter.Format(input);

            // Assert
            Assert.Equal(input, result);
        }
    }
}
