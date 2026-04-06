namespace GalacticMerchant.Core.Domain;

public enum RomanSymbol
{
    I = 1,
    V = 5,
    X = 10,
    L = 50,
    C = 100,
    D = 500,
    M = 1000
}

public sealed class RomanNumeral
{
    private readonly string _roman;
    public int Value { get; }

    private RomanNumeral(string roman, int value)
    {
        _roman = roman;
        Value = value;
    }
    
    public static Result<RomanNumeral> Parse(string roman)
    {
        if (string.IsNullOrWhiteSpace(roman))
            return Result<RomanNumeral>.Failure("Numeral romano não pode ser vazio.");

        var validationError = Validate(roman.ToUpper());
        if (validationError is not null)
            return Result<RomanNumeral>.Failure(validationError);

        var value = ComputeValue(roman.ToUpper());
        return Result<RomanNumeral>.Success(new RomanNumeral(roman.ToUpper(), value));
    }

    private static string? Validate(string roman)
    {
        // Símbolos válidos
        var validSymbols = new HashSet<char> { 'I', 'V', 'X', 'L', 'C', 'D', 'M' };
        foreach (var c in roman)
            if (!validSymbols.Contains(c))
                return $"Símbolo romano inválido: '{c}'.";

        // D, L, V não podem se repetir
        foreach (var sym in new[] { 'D', 'L', 'V' })
            if (roman.Contains(new string(sym, 2)))
                return $"Símbolo '{sym}' não pode se repetir.";

        // I, X, C, M máximo 3 consecutivos
        foreach (var sym in new[] { 'I', 'X', 'C', 'M' })
            if (roman.Contains(new string(sym, 4)))
                return $"Símbolo '{sym}' não pode aparecer mais de 3 vezes consecutivas.";

        // Regras de subtração
        var subtractRules = new Dictionary<char, HashSet<char>>
        {
            ['I'] = ['V', 'X'],
            ['X'] = ['L', 'C'],
            ['C'] = ['D', 'M'],
        };
        var noSubtract = new HashSet<char> { 'V', 'L', 'D' };

        for (int i = 0; i < roman.Length - 1; i++)
        {
            var cur  = roman[i];
            var next = roman[i + 1];
            var curVal  = (int)Enum.Parse<RomanSymbol>(cur.ToString());
            var nextVal = (int)Enum.Parse<RomanSymbol>(next.ToString());

            if (curVal < nextVal)
            {
                if (noSubtract.Contains(cur))
                    return $"Símbolo '{cur}' nunca pode ser subtraído.";

                if (!subtractRules.TryGetValue(cur, out var allowed) || !allowed.Contains(next))
                    return $"'{cur}' só pode ser subtraído de {string.Join(" ou ", subtractRules[cur])}.";
            }
        }

        return null;
    }

    private static int ComputeValue(string roman)
    {
        int total = 0;
        for (int i = 0; i < roman.Length; i++)
        {
            var cur  = (int)Enum.Parse<RomanSymbol>(roman[i].ToString());
            var next = i + 1 < roman.Length
                ? (int)Enum.Parse<RomanSymbol>(roman[i + 1].ToString())
                : 0;

            total += cur < next ? -cur : cur;
        }
        return total;
    }

    public override string ToString() => _roman;
}
