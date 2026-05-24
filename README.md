# Trit

A high-performance balanced ternary library for .NET 10.

> **NuGet package:** `Trit`
> **C# namespace:** `Ternary` (see [ADR 0002](docs/adr/0002-root-namespace-ternary.md))

Trit provides primitive value types and packed fixed-width words for arithmetic
in balanced ternary (digits `{-1, 0, +1}`). It is designed as a foundational
building block — clean enough to drop into any .NET 10 project, fast enough
to use on a hot path.

## Quickstart

```csharp
using Ternary.Numerics;     // Trit primitive
using Ternary.Encoding;     // TwoBitStorage, Base243Storage

Trit a = Trit.Negative;
Trit b = Trit.Positive;

Trit and = Trit.And(a, b);  // = Trit.Negative (minimum)
Trit or  = a | b;           // = Trit.Positive (maximum)
Trit neg = -a;              // = Trit.Positive (sign flip / NOT)
```

## Status

Pre-alpha. Public API is unstable.

## Design goals

- **Zero allocations on the hot path.** All core types are `readonly struct`
  values; bulk APIs accept `Span<T>` and `ReadOnlySpan<T>`.
- **Pluggable encoding without virtual dispatch.** Storage strategies
  (`TwoBitStorage`, `Base243Storage`) are generic struct type parameters,
  so the JIT specializes each call site and inlines every encoding read/write.
  See [ADR 0001](docs/adr/0001-trit-architecture.md).
- **Generic math participation.** Fixed-width word types implement the
  `INumber<T>` family so consumers can write generic algorithms over
  trit-word types and `int`/`long` alike.
- **Determinism and reproducibility.** No statics, no globals, no I/O —
  every operation is a pure function of its inputs.

## Layout

```
src/Trit/                Assembly + package = Trit; root namespace = Ternary
  Numerics/              Trit primitive value type
  Encoding/              ITritStorage + TwoBitStorage / Base243Storage strategies
  Words/                 TritWord16/32/64<TStorage>  (planned)
  Arithmetic/            Add, Sub, Mul, Div, Shift   (planned)
  Formatting/            ISpanFormattable / ISpanParsable<T>  (planned)
tests/Trit.Tests/        xUnit + FsCheck.Xunit
benchmarks/Trit.Benchmarks/   BenchmarkDotNet
docs/adr/                Architecture Decision Records
```

## Build

```
dotnet restore
dotnet build -c Release
dotnet test
dotnet run --project benchmarks/Trit.Benchmarks -c Release -- --list flat
dotnet run --project benchmarks/Trit.Benchmarks -c Release -- --filter '*'
```

## License

MIT — see [LICENSE](LICENSE).
