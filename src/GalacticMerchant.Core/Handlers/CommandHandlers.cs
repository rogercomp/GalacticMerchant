using GalacticMerchant.Core.Repository;
using GalacticMerchant.Core.Services;
using System.Text.RegularExpressions;

namespace GalacticMerchant.Core.Handlers;

public interface ICommandHandler{
    
    ICommandHandler SetNext(ICommandHandler next);
    
    string? Handle(string line);
}

public abstract class BaseCommandHandler : ICommandHandler
{
    private ICommandHandler? _next;

    public ICommandHandler SetNext(ICommandHandler next)
    {
        _next = next;
        return next; 
    }

    public string? Handle(string line)
    {
        var result = TryHandle(line.Trim());
        return result ?? _next?.Handle(line);
    }
    
    protected abstract string? TryHandle(string line);
}

// ═══════════════════════════════════════════════════════════════════════════════
//  HANDLER 1 — Mapeamento de palavra intergaláctica → símbolo romano
// ═══════════════════════════════════════════════════════════════════════════════
public sealed class GalacticMappingHandler : BaseCommandHandler
{    
    private static readonly Regex _pattern =
        new(@"^(?<word>[a-záàâãéêíóôõúüçA-Z]+)\s+é\s+(?<symbol>[IVXLCDMivxlcdm])$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly IGalacticDictionaryRepository _dictionary;

    public GalacticMappingHandler(IGalacticDictionaryRepository dictionary)
        => _dictionary = dictionary;

    protected override string? TryHandle(string line)
    {
        var match = _pattern.Match(line);
        if (!match.Success) return null;

        _dictionary.AddMapping(
            match.Groups["word"].Value,
            match.Groups["symbol"].Value.ToUpper());

        return null; 
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
//  HANDLER 2 — Definição de preço de metal
// ═══════════════════════════════════════════════════════════════════════════════
public sealed class MetalPriceDefinitionHandler : BaseCommandHandler
{
    // Padrão: <palavras galácticas> <Metal> é <valor> Créditos
    private static readonly Regex _pattern =
        new(@"^(?<galactic>(?:[a-záàâãéêíóôõúüç]+\s+)+)(?<metal>[A-ZÁÀÂÃÉÊÍÓÔÕÚÜÇ][a-záàâãéêíóôõúüç]+)\s+é\s+(?<credits>\d+(?:[.,]\d+)?)\s+Créditos$",
            RegexOptions.Compiled);

    private readonly IGalacticDictionaryRepository _dictionary;
    private readonly IMetalPriceRepository _metalRepo;
    private readonly GalacticTranslatorService _translator;

    public MetalPriceDefinitionHandler(
        IGalacticDictionaryRepository dictionary,
        IMetalPriceRepository metalRepo,
        GalacticTranslatorService translator)
    {
        _dictionary  = dictionary;
        _metalRepo   = metalRepo;
        _translator  = translator;
    }

    protected override string? TryHandle(string line)
    {
        var match = _pattern.Match(line);
        if (!match.Success) return null;

        var galacticPart = match.Groups["galactic"].Value.Trim();
        var metal        = match.Groups["metal"].Value.Trim();
        var totalCredits = decimal.Parse(
            match.Groups["credits"].Value.Replace(',', '.'),
            System.Globalization.CultureInfo.InvariantCulture);

        var galacticWords = galacticPart.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        var knownGalactic = galacticWords
            .Where(w => !string.Equals(w, metal, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var quantityResult = _translator.Translate(knownGalactic);
        if (!quantityResult.IsSuccess) return null;

        var quantity = quantityResult.Value;
        if (quantity <= 0) return null;

        var pricePerUnit = totalCredits / quantity;
        _metalRepo.SetPrice(metal, pricePerUnit);

        return null; 
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
//  HANDLER 3 — Consulta de valor galáctico
// ═══════════════════════════════════════════════════════════════════════════════
public sealed class GalacticValueQueryHandler : BaseCommandHandler
{
    private static readonly Regex _pattern =
        new(@"^quanto\s+é\s+(?<galactic>.+?)\s*\?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly GalacticTranslatorService _translator;

    public GalacticValueQueryHandler(GalacticTranslatorService translator)
        => _translator = translator;

    protected override string? TryHandle(string line)
    {
        var match = _pattern.Match(line);
        if (!match.Success) return null;

        var galacticPart  = match.Groups["galactic"].Value.Trim();
        var galacticWords = galacticPart.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var result = _translator.Translate(galacticWords);
        return result.IsSuccess
            ? $"{galacticPart} é {result.Value}"
            : "Não tenho a menor ideia do que você está falando";
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
//  HANDLER 4 — Consulta de créditos de metal
// ═══════════════════════════════════════════════════════════════════════════════
public sealed class MetalCreditsQueryHandler : BaseCommandHandler
{
    private static readonly Regex _pattern =
        new(@"^quantos\s+Créditos\s+é\s+(?<galactic>(?:[a-záàâãéêíóôõúüç]+\s+)+)(?<metal>[A-ZÁÀÂÃÉÊÍÓÔÕÚÜÇ][a-záàâãéêíóôõúüç]+)\s*\?$",
            RegexOptions.Compiled);

    private readonly GalacticTranslatorService _translator;
    private readonly IMetalPriceRepository _metalRepo;

    public MetalCreditsQueryHandler(
        GalacticTranslatorService translator,
        IMetalPriceRepository metalRepo)
    {
        _translator = translator;
        _metalRepo  = metalRepo;
    }

    protected override string? TryHandle(string line)
    {
        var match = _pattern.Match(line);
        if (!match.Success) return null;

        var galacticPart  = match.Groups["galactic"].Value.Trim();
        var metal         = match.Groups["metal"].Value.Trim();
        var galacticWords = galacticPart.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var quantityResult = _translator.Translate(galacticWords);
        if (!quantityResult.IsSuccess)
            return "Não tenho a menor ideia do que você está falando";

        if (!_metalRepo.TryGetPrice(metal, out var pricePerUnit))
            return "Não tenho a menor ideia do que você está falando";

        var total = quantityResult.Value * pricePerUnit;
        
        var totalDisplay = total == Math.Floor(total)
            ? ((long)total).ToString()
            : total.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

        return $"{galacticPart} {metal} é {totalDisplay} Créditos";
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
//  HANDLER 5 — responde com mensagem de erro genérica
// ═══════════════════════════════════════════════════════════════════════════════
public sealed class UnknownQueryHandler : BaseCommandHandler
{
    protected override string? TryHandle(string line)
    {
        if (line.TrimEnd().EndsWith('?'))
            return "Não tenho a menor ideia do que você está falando";
        return null;
    }
}
