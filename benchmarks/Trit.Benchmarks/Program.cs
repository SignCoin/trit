using BenchmarkDotNet.Running;

// Top-level entry. `dotnet run -c Release -- --list flat` enumerates all benchmark methods;
// `dotnet run -c Release -- --filter *` runs everything. See the BenchmarkDotNet docs.
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

// Marker type so BenchmarkSwitcher can find the entry assembly with top-level statements.
internal sealed partial class Program;
