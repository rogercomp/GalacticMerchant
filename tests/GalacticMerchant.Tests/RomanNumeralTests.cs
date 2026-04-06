using FluentAssertions;
using GalacticMerchant.Core.Domain;
using Xunit;

namespace GalacticMerchant.Tests;

public class RomanNumeralTests
{    

    [Theory]
    [InlineData("I",       1)]
    [InlineData("V",       5)]
    [InlineData("X",      10)]
    [InlineData("L",      50)]
    [InlineData("C",     100)]
    [InlineData("D",     500)]
    [InlineData("M",    1000)]
    [InlineData("MMVI", 2006)]
    [InlineData("MCMXLIV", 1944)]
    [InlineData("IV",      4)]
    [InlineData("IX",      9)]
    [InlineData("XL",     40)]
    [InlineData("XC",     90)]
    [InlineData("CD",    400)]
    [InlineData("CM",    900)]
    [InlineData("XLII",   42)]
    public void Parse_ValidRoman_ReturnsCorrectValue(string roman, int expected)
    {
        var result = RomanNumeral.Parse(roman);

        result.IsSuccess.Should().BeTrue(because: $"{roman} é um numeral romano válido");
        result.Value!.Value.Should().Be(expected);
    }    

    [Theory]
    [InlineData("VV",   "V não pode se repetir")]
    [InlineData("LL",   "L não pode se repetir")]
    [InlineData("DD",   "D não pode se repetir")]
    [InlineData("IIII", "I não pode aparecer mais de 3 vezes consecutivas")]
    [InlineData("XXXX", "X não pode aparecer mais de 3 vezes consecutivas")]
    [InlineData("CCCC", "C não pode aparecer mais de 3 vezes consecutivas")]
    [InlineData("MMMM", "M não pode aparecer mais de 3 vezes consecutivas")]
    public void Parse_InvalidRepetition_ReturnsFailure(string roman, string reason)
    {
        var result = RomanNumeral.Parse(roman);

        result.IsSuccess.Should().BeFalse(because: reason);
        result.Error.Should().NotBeNullOrEmpty();
    }    

    [Theory]
    [InlineData("VX",  "V nunca pode ser subtraído")]
    [InlineData("VL",  "V nunca pode ser subtraído")]
    [InlineData("LC",  "L nunca pode ser subtraído")]
    [InlineData("DM",  "D nunca pode ser subtraído")]
    [InlineData("IC",  "I só pode ser subtraído de V e X")]
    [InlineData("IL",  "I só pode ser subtraído de V e X")]
    [InlineData("XM",  "X só pode ser subtraído de L e C")]
    public void Parse_InvalidSubtraction_ReturnsFailure(string roman, string reason)
    {
        var result = RomanNumeral.Parse(roman);

        result.IsSuccess.Should().BeFalse(because: reason);
    }
   

    [Fact]
    public void Parse_InvalidSymbol_ReturnsFailure()
    {
        var result = RomanNumeral.Parse("IZX");

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Z");
    }

    [Fact]
    public void Parse_EmptyString_ReturnsFailure()
    {
        var result = RomanNumeral.Parse("");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Parse_LowercaseRoman_IsAccepted()
    {
        var result = RomanNumeral.Parse("iv");
        result.IsSuccess.Should().BeTrue();
        result.Value!.Value.Should().Be(4);
    }
}
