using FluentAssertions;
using GalacticMerchant.Core.Repository;
using GalacticMerchant.Core.Services;
using Xunit;

namespace GalacticMerchant.Tests;

/// <summary>
/// Testa o serviço de tradução: palavras intergalácticas → valor inteiro.
/// </summary>
public class GalacticTranslatorServiceTests
{
    private readonly IGalacticDictionaryRepository _dictionary;
    private readonly GalacticTranslatorService _sut;

    public GalacticTranslatorServiceTests()
    {
        _dictionary = new InMemoryGalacticDictionaryRepository();
        _dictionary.AddMapping("glob", "I");
        _dictionary.AddMapping("prok", "V");
        _dictionary.AddMapping("pish", "X");
        _dictionary.AddMapping("tegj", "L");
        _sut = new GalacticTranslatorService(_dictionary);
    }

    [Theory]
    [InlineData(new[] { "glob" },                          1)]   // I  = 1
    [InlineData(new[] { "prok" },                          5)]   // V  = 5
    [InlineData(new[] { "pish", "tegj", "glob", "glob" }, 42)]  // XLII = 42
    [InlineData(new[] { "glob", "prok" },                  4)]   // IV = 4
    [InlineData(new[] { "pish", "pish" },                 20)]   // XX = 20
    public void Translate_KnownWords_ReturnsCorrectValue(string[] words, int expected)
    {
        var result = _sut.Translate(words);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expected);
    }

    [Fact]
    public void Translate_UnknownWord_ReturnsFailure()
    {
        var result = _sut.Translate(["zorg"]);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("zorg");
    }

    [Fact]
    public void Translate_EmptyList_ReturnsFailure()
    {
        var result = _sut.Translate([]);

        result.IsSuccess.Should().BeFalse();
    }
}
