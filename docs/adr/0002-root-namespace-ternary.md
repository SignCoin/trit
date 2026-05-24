# ADR 0002 — Root namespace renamed from `Trit` to `Ternary`

- **Status:** Accepted
- **Date:** 2026-05-24
- **Supersedes:** §D5 of [ADR 0001](0001-trit-architecture.md)
- **Decision-makers:** Addam Boord

## Context

ADR 0001 §D5 set the root namespace and the primitive type to both be `Trit`,
acknowledging a known type-vs-namespace collision risk. The risk was
characterized as "latent" and mitigated by a rule against adding static
members that collide with sub-namespace names.

In practice, the collision bit immediately and in a way ADR 0001 did not
anticipate: any source file inside a `Trit.*` sub-namespace (e.g.
`Trit.Tests.Numerics`, `Trit.Benchmarks.Numerics`) cannot refer to the
`Trit` primitive type by its simple name. C#'s name lookup binds the
identifier `Trit` to the enclosing parent namespace before consulting
`using` directives, so even `using Trit.Numerics;` does not bring the type
into scope. The compiler emits

```
error CS0118: 'Trit' is a namespace but is used like a type
```

This affected our own tests and benchmarks (which structurally live in
`Trit.*` namespaces) and would affect any consumer who organized their own
code under `Trit.*`.

## Decision

Rename the root namespace from `Trit` to `Ternary`. Keep everything else:

- NuGet **package ID** stays `Trit`.
- .NET **assembly name** stays `Trit`.
- **Primitive type name** stays `Trit`.
- Folder names and `.csproj` file names stay `Trit`, `Trit.Tests`,
  `Trit.Benchmarks` (project identity, not C# namespaces).

After the change, the public API looks like:

```csharp
using Ternary.Numerics;          // brings in the Trit type
using Ternary.Encoding;          // TwoBitStorage, Base243Storage

Trit zero = Trit.Zero;
Trit negated = -Trit.Positive;
```

Sub-namespaces become `Ternary.Numerics`, `Ternary.Encoding`,
`Ternary.Words`, `Ternary.Arithmetic`, `Ternary.Formatting`.

## Alternatives considered

### A1. Rename the primitive type (e.g. `Tribit`)

Rejected. The `Trit` name carries strong industry recognition; renaming
the type leaks awkwardness into every consumer's call sites
(`Tribit.Zero`, `Tribit.And(...)`). The namespace is comparatively
boilerplate — consumers write the `using` once and move on.

### A2. Use type aliases inside library code

Rejected. The pattern `using Trit = Trit.Numerics.Trit;` works but is
required in every internal file, and obscures references to other
sub-namespaces (`Trit.Encoding.X` becomes ambiguous because `Trit` now
resolves to the type alias, not the namespace). Permanently more friction
than a single namespace rename.

### A3. Keep both names and live with the collision

Rejected. This is the path ADR 0001 chose. It demonstrably does not work
in practice — see Context.

## Consequences

**Good:**

- Internal code (tests, benchmarks, future sub-libraries) and external
  consumers all refer to the primitive as `Trit` without any aliasing or
  fully qualified names.
- No latent landmines from adding static members or new sub-namespaces.

**Bad:**

- Package ID and namespace no longer match exactly (`Trit` vs `Ternary`).
  Minor discoverability tax: a developer who knows the package name `Trit`
  must learn the namespace `Ternary` on first use. The README and NuGet
  description spell this out.

## Migration

Mechanical — `sed` replacement of `namespace Trit.` → `namespace Ternary.`
and `using Trit.` → `using Ternary.` across all `.cs` files, plus
`<RootNamespace>` updates in the three `.csproj` files. No public API
changes (type names, signatures, semantics all preserved).
