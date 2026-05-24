using BenchmarkDotNet.Attributes;
using Ternary.Numerics;

namespace Ternary.Benchmarks.Numerics;

[MemoryDiagnoser]
public class TritLogicBenchmarks
{
    private readonly Trit _a = Trit.Negative;
    private readonly Trit _b = Trit.Positive;

    [Benchmark(Baseline = true)]
    public Trit And_via_method() => Trit.And(_a, _b);

    [Benchmark]
    public Trit And_via_operator() => _a & _b;

    [Benchmark]
    public Trit Or_via_method() => Trit.Or(_a, _b);

    [Benchmark]
    public Trit Or_via_operator() => _a | _b;

    [Benchmark]
    public Trit Not_via_method() => Trit.Not(_a);

    [Benchmark]
    public Trit Not_via_unary_minus() => -_a;
}
