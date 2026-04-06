# Guia do Mercador para a Galáxia

Solução do teste técnico em **C# (.NET 8)** para o problema de conversão de numerais intergalácticos inspirados em algarismos romanos.

##  Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Como Rodar

### 1. Clonar e entrar no projeto

```bash
git clone <url-do-repositorio>
cd GalacticMerchant
```

### 2. Restaurar dependências

```bash
dotnet restore
```

### 3. Executar com o arquivo de exemplo

```bash
dotnet run --project src/GalacticMerchant.Console -- src/GalacticMerchant.Console/input.txt
```

**Saída esperada:**
```
pish tegj glob glob é 42
glob prok Prata é 68 Créditos
glob prok Ouro é 57800 Créditos
glob prok Ferro é 782 Créditos
Não tenho a menor ideia do que você está falando
```

### 4. Executar via stdin (pipe)

```bash
cat src/GalacticMerchant.Console/input.txt | dotnet run --project src/GalacticMerchant.Console
```

### 5. Build de produção

```bash
dotnet publish src/GalacticMerchant.Console -c Release -o ./publish
./publish/galactic-merchant input.txt
```

---

## Como Rodar os Testes

```bash
dotnet test
```