namespace GalacticMerchant.Core.Repository;


public interface IGalacticDictionaryRepository
{
    void AddMapping(string galacticWord, string romanSymbol);
    bool TryGetRomanSymbol(string galacticWord, out string romanSymbol);
    IReadOnlyDictionary<string, string> GetAll();
}


public interface IMetalPriceRepository
{
    void SetPrice(string metalName, decimal pricePerUnit);
    bool TryGetPrice(string metalName, out decimal price);
}

public sealed class InMemoryGalacticDictionaryRepository : IGalacticDictionaryRepository
{
    private readonly Dictionary<string, string> _mappings =
        new(StringComparer.OrdinalIgnoreCase);

    public void AddMapping(string galacticWord, string romanSymbol) =>
        _mappings[galacticWord.Trim()] = romanSymbol.Trim().ToUpper();

    public bool TryGetRomanSymbol(string galacticWord, out string romanSymbol) =>
        _mappings.TryGetValue(galacticWord.Trim(), out romanSymbol!);

    public IReadOnlyDictionary<string, string> GetAll() => _mappings;
}

public sealed class InMemoryMetalPriceRepository : IMetalPriceRepository
{
    private readonly Dictionary<string, decimal> _prices =
        new(StringComparer.OrdinalIgnoreCase);

    public void SetPrice(string metalName, decimal pricePerUnit) =>
        _prices[metalName.Trim()] = pricePerUnit;

    public bool TryGetPrice(string metalName, out decimal price) =>
        _prices.TryGetValue(metalName.Trim(), out price);
}
