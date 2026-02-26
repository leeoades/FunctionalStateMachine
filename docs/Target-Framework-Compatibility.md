# Target Framework Compatibility

`FunctionalStateMachine.Core` and `FunctionalStateMachine.CommandRunner` both multi-target so the correct build is selected automatically based on your project's target framework.

## Supported targets

| Target framework | Gets |
|---|---|
| `netstandard2.0` | Broadest compatibility — .NET Framework 4.6.1+, .NET Core 2.0+, Xamarin, Unity |
| `net10.0` | AOT-compatible build with full trim analysis |

When you reference the NuGet package from a `net10.0` project, NuGet automatically selects the `net10.0` build. When you reference it from any other target (including `net8.0`, `net9.0`, or `netstandard2.0`-compatible targets), NuGet selects the `netstandard2.0` build.

## Feature comparison

All state machine features are identical across both targets. The only difference is in the internals:

| Feature | `netstandard2.0` | `net10.0` |
|---|---|---|
| Full fluent API | ✅ | ✅ |
| Guards, conditionals | ✅ | ✅ |
| Hierarchical states | ✅ | ✅ |
| Static analysis on `.Build()` | ✅ | ✅ |
| Unused-trigger analysis | ✅ (when generator active) | ✅ (when generator active) |
| AOT / NativeAOT safe | ⚠️ (N/A for `netstandard2.0`) | ✅ |
| Trim-safe | ⚠️ (N/A for `netstandard2.0`) | ✅ |
| `[ModuleInitializer]` for trigger registry | ✅ (when PolySharp present) | ✅ |

## Source generator compatibility

The source generator (`FunctionalStateMachine.Core.Generator`) is a `netstandard2.0` Roslyn analyzer and works with all target frameworks. The generated `[ModuleInitializer]` code requires the `System.Runtime.CompilerServices.ModuleInitializerAttribute` type, which is:

- Available natively in `.NET 5` and later
- Back-ported to `netstandard2.0` by [PolySharp](https://github.com/Sergio0694/PolySharp) if you have it installed

If neither is available (e.g. plain `netstandard2.0` without PolySharp), the generator silently skips generation and unused-trigger analysis is bypassed without error.

## Using with .NET Framework

If your project targets `.NET Framework`, the `netstandard2.0` build is used. The state machine works fully, but:

- Unused-trigger analysis will not report warnings (no `[ModuleInitializer]` support without PolySharp)
- AOT and trim tooling are not applicable

## Related pages

- [AOT and Trim Compatibility](./AOT-and-Trim-Compatibility.md)
- [Packages](./Packages.md)
