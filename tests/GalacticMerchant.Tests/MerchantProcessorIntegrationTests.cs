using FluentAssertions;
using GalacticMerchant.Core;
using GalacticMerchant.Core.Repository;
using Xunit;

namespace GalacticMerchant.Tests;

/// <summary>
/// Testa o MerchantProcessor de ponta a ponta, cobrindo o exemplo completo do enunciado
/// e cenários extras de erro e edge cases.
/// </summary>
public class MerchantProcessorIntegrationTests
{
    private MerchantProcessor CreateProcessor()
    {
        var dictionary = new InMemoryGalacticDictionaryRepository();
        var metalRepo  = new InMemoryMetalPriceRepository();
        return new MerchantProcessor(dictionary, metalRepo);
    }    

    [Fact]
    public void FullExample_FromSpec_ReturnsExpectedOutput()
    {
        var processor = CreateProcessor();
        var input = new[]
        {
            "glob é I",
            "prok é V",
            "pish é X",
            "tegj é L",
            "glob glob Prata é 34 Créditos",
            "glob prok Ouro é 57800 Créditos",
            "pish pish Ferro é 3910 Créditos",
            "quanto é pish tegj glob glob ?",
            "quantos Créditos é glob prok Prata ?",
            "quantos Créditos é glob prok Ouro ?",
            "quantos Créditos é glob prok Ferro ?",
            "quanto de madeira uma marmota poderia roer se uma marmota pudesse roer madeira ?",
        };

        var outputs = processor.ProcessAll(input).ToList();

        outputs.Should().HaveCount(5);
        outputs[0].Should().Be("pish tegj glob glob é 42");
        outputs[1].Should().Be("glob prok Prata é 68 Créditos");
        outputs[2].Should().Be("glob prok Ouro é 57800 Créditos");
        outputs[3].Should().Be("glob prok Ferro é 782 Créditos");
        outputs[4].Should().Be("Não tenho a menor ideia do que você está falando");
    }
    

    [Fact]
    public void MappingLine_DoesNotProduceOutput()
    {
        var processor = CreateProcessor();
        var result = processor.ProcessLine("glob é I");
        result.Should().BeNull();
    }

    [Fact]
    public void GalacticQuery_KnownWords_ReturnsValue()
    {
        var processor = CreateProcessor();
        processor.ProcessLine("glob é I");
        processor.ProcessLine("prok é V");

        var result = processor.ProcessLine("quanto é glob prok ?");

        result.Should().Be("glob prok é 4");
    }    

    [Fact]
    public void CreditsQuery_AfterDefinition_ReturnsCredits()
    {
        var processor = CreateProcessor();
        processor.ProcessLine("glob é I");
        processor.ProcessLine("prok é V");
        processor.ProcessLine("glob prok Ouro é 57800 Créditos");

        var result = processor.ProcessLine("quantos Créditos é glob prok Ouro ?");

        result.Should().Be("glob prok Ouro é 57800 Créditos");
    }

    [Fact]
    public void CreditsQuery_UnknownMetal_ReturnsUnknownMessage()
    {
        var processor = CreateProcessor();
        processor.ProcessLine("glob é I");

        var result = processor.ProcessLine("quantos Créditos é glob Platina ?");

        result.Should().Be("Não tenho a menor ideia do que você está falando");
    }
   

    [Fact]
    public void UnknownQuery_WithQuestionMark_ReturnsFallbackMessage()
    {
        var processor = CreateProcessor();

        var result = processor.ProcessLine("quanto de madeira uma marmota poderia roer ?");

        result.Should().Be("Não tenho a menor ideia do que você está falando");
    }

    [Fact]
    public void RandomLine_WithoutQuestionMark_ReturnsNull()
    {
        var processor = CreateProcessor();

        var result = processor.ProcessLine("linha aleatória sem ponto de interrogação");

        result.Should().BeNull();
    }    

    [Fact]
    public void EmptyLine_ReturnsNull()
    {
        var processor = CreateProcessor();
        processor.ProcessLine("").Should().BeNull();
        processor.ProcessLine("   ").Should().BeNull();
    }    

    [Fact]
    public void MetalPrice_WithMultipleUnits_ComputesPricePerUnitCorrectly()
    {
        var processor = CreateProcessor();
        processor.ProcessLine("glob é I");
        processor.ProcessLine("prok é V");
        processor.ProcessLine("glob glob Prata é 34 Créditos");

        var result = processor.ProcessLine("quantos Créditos é glob prok Prata ?");

        result.Should().Be("glob prok Prata é 68 Créditos");
    }
}
