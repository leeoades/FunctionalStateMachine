# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/), and this project adheres to [Semantic Versioning](https://semver.org/).

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


