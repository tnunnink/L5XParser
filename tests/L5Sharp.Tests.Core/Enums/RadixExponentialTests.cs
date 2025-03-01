﻿using System.Globalization;
using AutoFixture;
using FluentAssertions;

namespace L5Sharp.Tests.Core.Enums
{
    [TestFixture]
    public class RadixExponentialTests
    {
        [Test]
        public void Exponential_WhenCalled_ShouldBeExpected()
        {
            var radix = Radix.Exponential;

            radix.Should().NotBeNull();
            radix.Should().Be(Radix.Exponential);
        }
        
        [Test]
        public void Format_Null_ShouldThrowArgumentNullException()
        {
            FluentActions.Invoking(() => Radix.Exponential.FormatValue(null!)).Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void Format_NonSupportedAtomic_ShouldThrowNotSupportedException()
        {
            FluentActions.Invoking(() => Radix.Exponential.FormatValue(new DINT())).Should().Throw<NotSupportedException>();
        }

        [Test]
        public void Format_Zero_ShouldBeExpectedFormat()
        {
            var result = Radix.Exponential.FormatValue(new REAL());

            result.Should().Be("0.00000000e+000");
        }
        
        [Test]
        public void Format_ValidReal_ShouldBeExpectedFormat()
        {
            var fixture = new Fixture();
            var value = fixture.Create<float>();
            var result = Radix.Exponential.FormatValue(new REAL(value));

            result.Should().Be(value.ToString("e8", CultureInfo.InvariantCulture));
        }

        [Test]
        public void Format_CustomRealSevenDecimal_ShouldBeExpectedFormat()
        {
            var result = Radix.Exponential.FormatValue(new REAL(1.123e3f));

            result.Should().Be("1.12300000e+003");
        }

        [Test]
        public void Parse_Null_ShouldThrowArgumentNullException()
        {
            FluentActions.Invoking(() => Radix.Exponential.ParseValue(null!)).Should().Throw<ArgumentException>();
        }
        
        [Test]
        public void Parse_Empty_ShouldThrowArgumentNullException()
        {
            FluentActions.Invoking(() => Radix.Exponential.ParseValue(string.Empty)).Should().Throw<ArgumentException>();
        }

        [Test]
        public void Parse_Exponential_ShouldBeExpected()
        {
            var result = Radix.Exponential.ParseValue("1.12300000e+002");

            result.Should().Be(112.3f);
        }

    }
}