# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres to [Semantic Versioning](https://semver.org/).

## [1.0.0]

### Changed
- **⚡ Maximum compatibility** - Migrated to .NET Standard 2.0 for vastly broader platform support
  - Core library: net9.0 → netstandard2.0
  - CommandRunner: net9.0 → netstandard2.0
  - Now supports: .NET Framework 4.6.1+, .NET Core 2.0+, .NET 5-9+, Xamarin, Unity 2021.2+, Mono 5.4+, UWP
  - Added PolySharp for C# 9+ feature polyfills (records, init properties)
  - Downgraded Microsoft.Extensions.DependencyInjection to v2.0 for compatibility
  - Fixed modern API usage: `ArgumentNullException.ThrowIfNull` → manual null checks
  - Fixed KeyValuePair deconstruction for netstandard2.0 compatibility
- **📁 Project organization** - Restructured solution for better maintainability
  - Moved library projects to `/src` folder
  - Moved test projects to `/test` folder
  - Organized samples into `/samples/Basic`, `/samples/StockPurchaser`, `/samples/VendingMachine`
  - Updated all project references, solution paths, and documentation links
- **📚 Documentation overhaul** - Completely rewritten and expanded documentation
  - Main README reorganized with inline examples and progressive feature introduction
  - Each feature page rebuilt from basics to advanced with step-by-step examples
  - Added conceptual pages section for non-feature topics
  - Improved consistency across all documentation pages
  - Added comprehensive index page with links to all documentation
  - **Clarified first-match semantics** - Made it explicit that transitions evaluate in order and first match wins
  - Replaced `Guard(() => true)` catch-all pattern with clearer "no guard" approach
- **🚀 NuGet publishing** - Set up automated Trusted Publishing with GitHub Actions
  - Configured OIDC-based authentication (no long-lived API keys)
  - Added comprehensive PUBLISHING.md guide
  - Fixed symbol package generation for analyzer projects
  - Added proper package descriptions for all projects
  - Separate handling for analyzer vs library packages

### Added
- **🔍 Enhanced static analysis** - New build-time validations for guard patterns
  - **Error**: Unguarded transition appearing before other transitions for same trigger (makes subsequent transitions unreachable)
  - **Warning**: Multiple guarded transitions on same trigger (reminder about first-match semantics)
  - Helps prevent common guard ordering mistakes
  - Clear error messages explain first-match behavior

### Fixed
- **📦 Analyzer packaging** - Fixed NuGet pack errors for Roslyn analyzer project
  - Disabled symbol package generation for FunctionalStateMachine.Diagrams
  - Suppressed NU5128 warnings about missing lib/ref assemblies
  - Correctly configured analyzer DLL inclusion in package

## [0.10.0] - 2026-02-07

### Added
- **NoData builder API parity** - The NoData state machine builder now supports all features available in the Data builder
  - `Start()` method for triggering initial state entry and immediate transitions
  - `Ignore()` for non-generic `TransitionConfiguration`
  - `TransitionTo()` in conditional branches (`If`/`ElseIf`/`Else`)
  - `SkipAnalysis()` to disable build-time validation
  - `OnEntry()`, `OnExit()`, `Immediately()` for state lifecycle
  - `SubStateOf()`, `StartsWith()` for hierarchical states
- **Comprehensive test coverage** - 141 tests covering all core functionality
  - Extension method overload tests (OnEntry, OnExit, Guard, If, ElseIf, ModifyData variants)
  - NoData feature tests for new API methods
  - Undefined state validation tests
  - Conditional transition configuration tests

### Removed
- **Dead code cleanup** - Removed unreachable `TransitionStepKind.Conditional` code path
  - Removed enum value and associated properties (`ConditionalTrueSteps`, `ConditionalFalseSteps`)
  - Removed switch cases in Core and Analysis (37 lines total)
  - `ConditionalChain` already handles all If/ElseIf/Else scenarios
  - `TryFireInternal` removed

## [0.9.0] - 2026-02-06

### Added
- **Stock purchaser actor-style sample** - New console app with in-memory persistence, timed price ticks, tests, and README
- **Diagrams README** - Usage guide for the diagram generator project
- **Command runner README** - Usage guide for command runner DI integration
- **Copilot instructions** - Added/updated `.github/copilot-instructions.md`

### Changed
- **OnUnhandled behavior** - Unhandled handlers now return commands rather than mutating data
- **Vending machine sample** - Reshaped sample flow and responsibilities
- **Solution organization** - Projects arranged more neatly in the solution

## [0.8.0] - 2026-02-05

### Added
- **Conditional TransitionTo support** - Transition steps now work inside `If`/`ElseIf`/`Else`/`Done` blocks with ambiguity detection for multiple transitions in a single execution path
- **Additional conditional transition coverage** - Expanded tests and documentation for conditional transitions and ambiguity scenarios
- **Vending machine sample app** - New interactive sample with command runners, diagrams, README, and xUnit tests

## [0.7.0] - 2026-02-04

### Fixed
- **Duplicate superstate labels in diagrams** - Superstates no longer render redundant labels alongside their subgraph containers
  - Improved visual clarity of hierarchical state diagrams
  - Cleaner Mermaid diagram output for complex state machines

### Added (Documentation)
- **Complete changelog** - Documented all versions 0.1.0 through 0.6.0 with human-readable release notes
  - Comprehensive feature descriptions for each release
  - Clear tracking of project evolution and capabilities

## [0.6.0] - 2026-02-04

### Added
- **Complete static analysis system** - Build-time validation that detects configuration errors
  - Reachability analysis: detects unreachable states (BFS-based)
  - Cycle detection: catches immediate self-transitions and circular transition chains
  - Ambiguous transition detection: identifies conflicting unguarded transitions for same trigger
  - Dead-end state warnings: highlights states with no outgoing transitions
  - Unused trigger type detection: warns about trigger types defined but never used
- **Unused trigger detection** - Reflection-based analysis finds trigger record types that aren't used in any transition
- **Configurable analysis** - `.SkipAnalysis()` fluent method allows disabling validation for edge cases
- **Comprehensive static analysis documentation** - New `docs/static-analysis.md` (399 lines)
  - Detailed examples of each detection scenario with code patterns
  - Anti-patterns and how to fix them
  - Performance analysis (< 1ms per state machine)
  - Instructions for opt-out when needed

### Fixed
- Static analysis test expectations to properly validate error and warning conditions

### Changed
- `Validate()` method now accepts `skipAnalysis` parameter for conditional validation
- Analysis execution integrated into state machine build pipeline

## [0.5.0] - 2026-02-04

### Added
- **ElseIf conditional branching** - Multiple conditions for complex transition routing
  - 6 overload variants for different predicate signatures
  - Short-circuit evaluation (first matching condition wins)
  - Full backward compatibility with `If`/`Else` code
- **Diagram labels for guard clauses** - Conditional transitions now show their guard conditions on diagram edges
  - Clearer visual representation of complex routing logic
- **Diagram improvements for superstates** - Better handling of transitions into hierarchical states
  - Transitions into superstate now properly point to initial substate
  - Cleaner superstate cleanup and organization

### Added (Documentation)
- **Conditional steps guide** - New `docs/conditional-steps.md` (123 lines)
  - If/ElseIf/Else patterns with complete examples
  - Complex routing scenarios and best practices
  - Multiple predicate signature options explained

## [0.4.0] - 2026-02-02

### Added
- **Immediate transitions** - States can immediately transition on entry without requiring triggers
  - `.Immediately()` fluent method to chain immediate transitions
  - `.Start()` method to trigger initial state entry and any immediate transitions
  - Support for guarded immediate transitions with conditions
  - 110+ lines of comprehensive test coverage
- **Start() method** - Replace cumbersome `InitialStateOrDefault` initialization
  - More fluent API: `machine.Start()` vs `machine.ProcessTrigger(null)`
  - Allows initial state entry actions and commands to execute
  - Supports immediate transitions from initial state

### Fixed
- Removed temporary test console app reference from solution file

### Changed
- Renamed `InitialStateOrDefault` to `InitialState` (no longer optional)
- Immediate transition API and state machine lifecycle improved for clarity

### Added (Documentation)
- New `docs/immediate-transitions.md` with examples and patterns

## [0.3.0] - 2026-02-02

### Fixed
- **Command runner generator package** - Added missing code generation components to project file
  - Generator package was declared but missing actual analyzer/code gen files
  - Now properly included in build and deployment
- **File naming after rename** - Updated interface names to match CommandDispatcher pattern
  - `ICommandRunnerProvider` → `ICommandDispatcher`
  - `IAsyncCommandRunnerProvider` → `IAsyncCommandDispatcher`
  - Consistent naming across all packages

## [0.2.0] - 2026-02-02

### Added
- **Multi-package release structure** - Separated concerns across NuGet packages
  - `FunctionalStateMachine.Core` - State machine engine
  - `FunctionalStateMachine.Diagrams` - Mermaid diagram generation
  - `FunctionalStateMachine.CommandRunner` - Command execution framework
  - `FunctionalStateMachine.CommandRunner.Generator` - Source generators for commands
- **Package documentation** - New `docs/packages.md` explaining each package's purpose and usage

### Changed
- Release workflow updated to publish all packages to NuGet
- Directory.Build.props configured for multi-package versioning

## [0.1.0] - 2026-01-31

### Added
- Functional, data-carrying state machine core API with fluent builder
- Hierarchical states with parent/child relationships and initial sub-state support
- Conditional transition steps via `If`/`Else`/`Done`
- Typed trigger records and typed `On<TTrigger>()` transitions
- Commands returned from transitions, entry, and exit actions
- Entry and exit actions with automatic command collection
- Mermaid diagram generation for visual state machine representation
- Shopping trolley sample demonstrating domain-driven design patterns
- Comprehensive test coverage including:
  - State machine core functionality
  - Hierarchical state handling
  - Conditional transitions
  - Entry/exit actions
  - Command collection
  - Diagram generation


