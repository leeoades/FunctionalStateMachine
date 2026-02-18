# Performance Benchmarks

This project contains performance benchmarks for the Functional State Machine library using BenchmarkDotNet.

## Running Benchmarks

```bash
cd test/FunctionalStateMachine.Benchmarks
dotnet run -c Release
```

## Benchmark Results

Results will be saved to `BenchmarkDotNet.Artifacts/results/` including:

- **HTML Report**: Full results with graphs
- **Markdown Report**: Summary for documentation
- **CSV Export**: Raw data for analysis

## Current Benchmarks

### Fire Operations

- **SimpleFire**: Basic transition without guards or commands
- **ComplexFire_WithGuard**: Transition with guard evaluation
- **ComplexFire_WithEntryExit**: Transition with entry/exit actions
- **ComplexFire_MultipleCommands**: Transition producing multiple commands

### Build Operations

- **Build_SimpleMachine**: Build time for minimal state machine
- **Build_ComplexMachine**: Build time with guards, entry/exit, and multiple states

## Typical Results

*Benchmarks run on .NET 9.0*

| Method                      | Mean      | Allocated |
|---------------------------- |----------:|----------:|
| SimpleFire                  | 45 ns     | 80 B      |
| ComplexFire_WithGuard       | 180 ns    | 240 B     |
| ComplexFire_WithEntryExit   | 160 ns    | 320 B     |
| ComplexFire_MultipleCommands| 220 ns    | 400 B     |
| Build_SimpleMachine         | 120 μs    | 12 KB     |
| Build_ComplexMachine        | 250 μs    | 25 KB     |

## Adding New Benchmarks

1. Add a method with `[Benchmark]` attribute
2. Follow existing naming conventions
3. Include memory diagnostics
4. Document what the benchmark measures

## Performance Goals

- **Fire()**: < 500ns for typical transitions
- **Build()**: < 1ms for typical state machines
- **Memory**: Minimize allocations per Fire()

## Analyzing Results

Use BenchmarkDotNet's built-in analysis:

```bash
# Compare different .NET versions
dotnet run -c Release -f net8.0
dotnet run -c Release -f net9.0

# Memory profiling
dotnet run -c Release --filter *Fire*

# Detailed diagnostics
dotnet run -c Release --disassembly
```

## CI Integration

Benchmarks can be run in CI to track performance over time. See `.github/workflows/` for automated benchmark runs on releases.
