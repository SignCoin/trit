# ADR 0001 — Trit library architecture

- **Status:** Accepted, partially amended
- **Date:** 2026-05-24
- **Decision-makers:** Addam Boord
- **Amendments:** [ADR 0002](0002-root-namespace-ternary.md) supersedes §D5 —
  root C# namespace renamed from `Trit` to `Ternary`. Inline namespace
  references in this document have been updated to match the current state;
  the architectural reasoning is unchanged.

## Context

Trit is a new .NET 10 library providing balanced ternary primitives. The
explicit driver is a future downstream consumer (a ternary blockchain) where
trit-level operations sit on hot paths: hashing, signing, ledger encoding.
The library itself must remain general-purpose — no blockchain coupling.

A reasonable trit library design has many forks. This ADR records the choices
made during the initial design pass so future work has a durable "why".

## Decisions

### D1. Target framework: `net10.0` only

We use the full modern surface — generic math interfaces (`INumber<T>`,
`IBinaryInteger<T>`-style operations), `ref struct` interfaces,
`System.Numerics.Tensors`-friendly memory layouts, AVX-512 intrinsics where
available. Multi-targeting `netstandard2.0` or `net8.0` would strip the
features that justify writing this library in the first place.

**Cost:** Consumers locked to legacy TFMs cannot use Trit. Acceptable —
perf-sensitive .NET work has migrated.

### D2. v1 scope: primitive + fixed-width family

v1 ships:

1. `Trit` — atomic value type representing a single balanced ternary digit.
2. `TritWord16`, `TritWord32`, `TritWord64` — fixed-width packed integers
   generic over the storage strategy (see D4).
3. Arithmetic (+ - * / %, shifts), bitwise-equivalent logic (AND/OR/XOR/NOT,
   plus the trit-specific Cycle/AntiCycle), parsing/formatting,
   `INumber<T>` conformance on the word types.

Out of v1: arbitrary-precision `BigTrit`, SIMD-accelerated bulk operations.
Both are tracked as follow-up tasks and can be added without breaking the
v1 public surface.

### D3. License: MIT

Maximizes adoption. Apache 2.0's patent grant is unnecessary at this scale.

### D4. Storage strategy: pluggable via generic struct type parameter

This is the most consequential decision in the design.

**Problem.** Two viable encodings for packed trit words:

- **2-bit-per-trit** — encode each trit in 2 bits (e.g. `00=-1, 01=0, 10=+1`).
  33% bit waste, but cache-friendly, SIMD-friendly, every trit op decomposes
  to a small mask/shift sequence.
- **Base-243 (5 trits per byte)** — pack 5 trits into a byte (3⁵ = 243 < 256).
  ~5% bit waste, denser on disk/wire, but arithmetic requires modular decode.

Both are useful for different workloads (compute vs. serialization). We
do not want to pick one and lose the other.

**Naive strategy pattern.** A virtual `ITritStorage` interface field on each
word type. Clean SOLID-D, but every op pays a virtual dispatch cost, the JIT
can't inline read/write, and SIMD is dead. Non-starter for a perf library.

**Decision.** Encode the strategy as a generic struct type parameter:

```csharp
public interface ITritStorage
{
    int  ReadTrit(ReadOnlySpan<byte> buffer, int tritIndex);
    void WriteTrit(Span<byte> buffer, int tritIndex, int value);
    int  BytesFor(int tritCount);
    int  BitsPerTrit { get; }
}

public readonly struct TwoBitStorage  : ITritStorage { /* ... */ }
public readonly struct Base243Storage : ITritStorage { /* ... */ }

public readonly struct TritWord64<TStorage>
    where TStorage : struct, ITritStorage
{
    public TritWord64<TStorage> Add(TritWord64<TStorage> other)
    {
        var storage = default(TStorage);  // zero-cost
        // storage.ReadTrit(...) inlines as a direct call
        // each TStorage gets its own JIT specialization
    }
}
```

The `struct` constraint on `TStorage` makes the JIT emit a separate
specialization for each concrete storage type — no virtual dispatch, full
inlining, SIMD-friendly. The pattern is well-precedented in .NET's BCL
(comparers, equality comparers, sorters in `Collections.Generic` /
`System.Numerics.Tensors`).

**Ergonomics.** Bare `TritWord64<TwoBitStorage>` is verbose. We provide
type aliases via `global using` for the common cases:

```csharp
global using Trit64       = Ternary.Words.TritWord64<Ternary.Encoding.TwoBitStorage>;
global using DenseTrit64  = Ternary.Words.TritWord64<Ternary.Encoding.Base243Storage>;
```

### D5. Naming: package `Trit`, primitive type `Ternary.Numerics.Trit`

> **Superseded by [ADR 0002](0002-root-namespace-ternary.md).** The original
> decision was to use `Trit` as both the package ID *and* the C# root
> namespace, with the primitive type also called `Trit` inside the
> `Trit.Numerics` sub-namespace. In practice this created an unworkable
> type-vs-namespace collision — any source file inside a `Trit.*`
> sub-namespace could not refer to the `Trit` primitive type by its simple
> name. ADR 0002 keeps the package ID, assembly name, and type name as
> `Trit`, and renames the C# root namespace to `Ternary` to eliminate the
> collision. The text below is the *current* state, post-amendment.

The NuGet package ID, assembly name, and primitive type name are all `Trit`.
The C# root namespace is `Ternary`. The primitive type lives in the
`Ternary.Numerics` sub-namespace. Consumer code typically writes
`using Ternary.Numerics;` and refers to the primitive as just `Trit` — the
namespace import brings the type into scope unambiguously because the
enclosing namespace (`Ternary`) no longer shadows it.

Sub-namespaces are `Ternary.Numerics`, `Ternary.Encoding`, `Ternary.Words`,
`Ternary.Arithmetic`, `Ternary.Formatting`.

### D6. Project layout: src / tests / benchmarks

Standard three-project shape. Source under `src/Trit/`, organized by domain
concern (`Numerics`, `Encoding`, `Words`, `Arithmetic`, `Formatting`) to
keep SOLID-S (single responsibility) visible at the folder level. Tests
under `tests/Trit.Tests/`, mirrored structure. Benchmarks under
`benchmarks/Trit.Benchmarks/`, BenchmarkDotNet console host.

### D7. Test strategy: xUnit + FsCheck.Xunit

xUnit for example-based tests. FsCheck.Xunit for property tests of
arithmetic identities (`a + (-a) = 0`, associativity, commutativity,
distributivity, encoding round-trip). An arithmetic library that ships
without property tests is a bug factory — the failure modes hide in
input distributions that example-based tests don't sample.

### D8. Central Package Management

NuGet versions live in `Directory.Packages.props`. Project `.csproj` files
contain version-less `PackageReference` entries. Simpler upgrades, no
version drift across projects.

### D9. Determinism, analyzers, warnings-as-errors

- `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` — never let a warning
  rot.
- `<AnalysisMode>AllEnabledByDefault</AnalysisMode>` — strictest analyzer
  level. We can selectively suppress in `.editorconfig` if a rule is wrong
  for our domain.
- `<Deterministic>true</Deterministic>` + `<EmbedUntrackedSources>true</>`
  on CI for reproducible builds and SourceLink-able symbols.

## Alternatives considered

### A1. Single hardcoded encoding (2-bit only)

Rejected. The base-243 encoding pays off for serialization/storage-heavy
workloads. Locking in 2-bit closes that door.

### A2. Class-based polymorphism over storage

Rejected — see D4. The perf cost of virtual dispatch in hot loops kills
the library's reason for existing.

### A3. Multi-target `net10.0` + `net8.0` LTS

Rejected — see D1. The perf primitives we want (AVX-512, generic math)
are .NET 10-only or much easier in .NET 10. Multi-target adds 2x compilation
matrix overhead for ~10% extra consumer reach.

### A4. Different primitive type name (`Trit3`, `BalancedTrit`, `T`)

Rejected. The `Trit` name has the best industry recognition and lowest
cognitive overhead. The namespace collision risk in D5 is manageable.

## Consequences

**Good:**

- Consumers can swap encoding strategy at the type level without rewriting
  arithmetic code.
- Hot-path performance ceiling is set by the JIT's ability to inline a
  concrete struct — not by virtual dispatch.
- Generic math integration lets Trit slot into existing LINQ/numeric
  generic code unmodified.

**Bad:**

- The generic-struct pattern is unfamiliar to many .NET developers. Code
  reading `TritWord64<TwoBitStorage>` looks scary; the type aliases mitigate
  but don't eliminate this.
- Type-name / namespace collision risk (D5) means we lose a degree of
  freedom in adding static members to the primitive type.
- Multi-targeting decision is forward-looking — if a major consumer
  surfaces who's stuck on net8.0, we'll have a re-litigation.

## Follow-ups

Tracked separately:

- BigTrit arbitrary-precision type
- SIMD-accelerated bulk operations (`Vector256`, `Vector512`, NEON)
- Cross-validation reference impl (Python or Rust)
- BenchmarkDotNet baselines vs `BigInteger` and raw `int` arithmetic
