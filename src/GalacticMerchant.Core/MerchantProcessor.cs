using GalacticMerchant.Core.Handlers;
using GalacticMerchant.Core.Repository;
using GalacticMerchant.Core.Services;

namespace GalacticMerchant.Core;

public sealed class MerchantProcessor
{
    private readonly ICommandHandler _chain;

    public MerchantProcessor(
        IGalacticDictionaryRepository dictionary,
        IMetalPriceRepository metalRepo)
    {
        var translator = new GalacticTranslatorService(dictionary);
        
        var mappingHandler   = new GalacticMappingHandler(dictionary);
        var metalHandler     = new MetalPriceDefinitionHandler(dictionary, metalRepo, translator);
        var galacticQuery    = new GalacticValueQueryHandler(translator);
        var creditsQuery     = new MetalCreditsQueryHandler(translator, metalRepo);
        var unknownQuery     = new UnknownQueryHandler();

        mappingHandler
            .SetNext(metalHandler)
            .SetNext(galacticQuery)
            .SetNext(creditsQuery)
            .SetNext(unknownQuery);

        _chain = mappingHandler;
    }
    
    public string? ProcessLine(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return null;
        return _chain.Handle(line);
    }
    
    public IEnumerable<string> ProcessAll(IEnumerable<string> lines) =>
        lines
            .Select(ProcessLine)
            .Where(r => r is not null)
            .Select(r => r!);
}
