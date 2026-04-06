using GalacticMerchant.Core.Domain;
using GalacticMerchant.Core.Repository;

namespace GalacticMerchant.Core.Services;

public sealed class GalacticTranslatorService
{
    private readonly IGalacticDictionaryRepository _dictionary;

    public GalacticTranslatorService(IGalacticDictionaryRepository dictionary)
        => _dictionary = dictionary;

    public Result<int> Translate(IEnumerable<string> galacticWords)
    {
        var romanBuilder = new System.Text.StringBuilder();

        foreach (var word in galacticWords)
        {
            if (!_dictionary.TryGetRomanSymbol(word, out var symbol))
                return Result<int>.Failure($"Palavra intergaláctica desconhecida: '{word}'.");
            romanBuilder.Append(symbol);
        }

        var romanString = romanBuilder.ToString();
        if (string.IsNullOrEmpty(romanString))
            return Result<int>.Failure("Nenhuma palavra intergaláctica fornecida.");

        return RomanNumeral.Parse(romanString).Map(r => r.Value);
    }
}
