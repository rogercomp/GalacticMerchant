using GalacticMerchant.Core;
using GalacticMerchant.Core.Repository;

// ═══════════════════════════════════════════════════════════════════════════════
//  Guia do Mercador para a Galáxia
//   (sem argumento lê do stdin — permite pipe)
// ═══════════════════════════════════════════════════════════════════════════════

IEnumerable<string> ReadLines(string[] args)
{
    if (args.Length > 0)
    {
        var path = args[0];
        if (!File.Exists(path))
        {
            Console.Error.WriteLine($"Arquivo não encontrado: {path}");
            Environment.Exit(1);
        }
        return File.ReadLines(path);
    }

    return ReadFromStdin();
}

IEnumerable<string> ReadFromStdin()
{
    string? line;
    while ((line = Console.ReadLine()) is not null)
        yield return line;
}

var dictionary = new InMemoryGalacticDictionaryRepository();
var metalRepo  = new InMemoryMetalPriceRepository();
var processor  = new MerchantProcessor(dictionary, metalRepo);

var lines   = ReadLines(args);
var outputs = processor.ProcessAll(lines);

foreach (var output in outputs)
    Console.WriteLine(output);
