# Roadmap

This document outlines the planned features and improvements for Functional State Machine. This roadmap is subject to change based on community feedback and priorities.

## Version 1.x (Current)

### Completed ✅
- [x] Core functional state machine with command-based architecture
- [x] Hierarchical states with parent/child relationships
- [x] Conditional transitions with guards
- [x] Entry/exit actions
- [x] Immediate transitions
- [x] Internal transitions (self-transitions)
- [x] Static analysis and validation
- [x] Mermaid diagram generation
- [x] Command runner framework with DI integration
- [x] Source generators for command dispatching
- [x] .NET Standard 2.0 compatibility
- [x] Comprehensive documentation
- [x] Multiple sample applications

### In Progress 🚧
- [ ] Enhanced code coverage reporting in CI
- [ ] Community building (templates, guides)
- [ ] Performance benchmarking suite

## Version 2.0 (Future)

### Enhanced State Machine Features

#### Advanced Transitions
- [ ] **Parallel States**: Support for orthogonal regions where multiple states can be active simultaneously
- [ ] **History States**: Deep and shallow history to return to previous substates
- [ ] **Junction Points**: Pseudo-states for complex conditional routing
- [ ] **Choice Points**: Dynamic transition selection based on runtime conditions

#### Improved Guard System
- [ ] **Named Guards**: Reusable guard definitions with names
- [ ] **Guard Composition**: Combine guards with AND/OR/NOT operators
- [ ] **Async Guards**: Support for guards that need to perform async operations

#### Time-Based Features
- [ ] **Timeout Transitions**: Automatic transitions after a duration
- [ ] **Scheduled Commands**: Commands that execute at specific times
- [ ] **Time-based Guards**: Guards that consider time/duration

### Performance & Scalability

- [ ] **Compiled State Machines**: AOT-compiled state machines for zero-allocation execution
- [ ] **State Machine Pooling**: Object pooling for high-throughput scenarios
- [ ] **Snapshot/Restore**: Efficient state checkpointing for rollback scenarios
- [ ] **Incremental Diagram Updates**: Update diagrams without regenerating from scratch

### Developer Experience

#### Tooling
- [ ] **Visual Designer**: Web-based visual state machine designer
- [ ] **VS Code Extension**: Syntax highlighting and IntelliSense for state machines
- [ ] **Live Debugging**: Runtime visualization of state transitions
- [ ] **State Machine Testing DSL**: Fluent API for testing state machines

#### Documentation
- [ ] **Interactive Tutorials**: Step-by-step interactive guides
- [ ] **Video Tutorials**: Video series covering common patterns
- [ ] **Pattern Library**: Catalog of common state machine patterns
- [ ] **Migration Guides**: Guides for migrating from other state machine libraries

### Integration & Ecosystem

#### Persistence
- [ ] **Entity Framework Integration**: First-class EF Core support
- [ ] **Event Sourcing Integration**: Native event sourcing patterns
- [ ] **Redis Persistence**: Redis-based state storage
- [ ] **Cosmos DB Integration**: Azure Cosmos DB persistence adapter

#### Platforms
- [ ] **Blazor Integration**: Client-side state machine support in Blazor
- [ ] **Orleans Integration**: First-class support for Microsoft Orleans
- [ ] **Akka.NET Integration**: Adapter for Akka.NET actors
- [ ] **MAUI Support**: Mobile application state management

#### Observability
- [ ] **OpenTelemetry Integration**: Distributed tracing for transitions
- [ ] **Metrics Export**: Prometheus/Grafana metrics
- [ ] **Structured Logging**: Rich structured logs for debugging
- [ ] **Health Checks**: State machine health endpoints

### Advanced Features

- [ ] **State Machine Composition**: Combine multiple state machines
- [ ] **Dynamic State Machine**: Runtime state/transition modifications
- [ ] **State Machine Versioning**: Handle schema evolution
- [ ] **Undo/Redo**: Built-in command reversal
- [ ] **Transaction Support**: Rollback on command failure

## Version 3.0 (Vision)

### AI-Assisted Features
- [ ] **AI-Generated Diagrams**: Natural language to state machine
- [ ] **Smart Suggestions**: AI suggestions for missing transitions
- [ ] **Pattern Recognition**: Detect common patterns and suggest optimizations

### Cross-Platform
- [ ] **WebAssembly Support**: Browser-side state machines
- [ ] **Unity Integration**: Game state management
- [ ] **Unreal Engine Plugin**: State machines for Unreal

### Enterprise Features
- [ ] **Multi-Tenant**: Tenant-aware state machines
- [ ] **Audit Trail**: Complete transition history
- [ ] **Policy Engine**: Rule-based state machine configuration
- [ ] **Workflow Engine**: Long-running workflow support

## Community Requests

Have a feature request? We'd love to hear it!

- Open an issue with the `enhancement` label
- Vote on existing feature requests with 👍
- Join discussions in the Issues section

Popular requests will be prioritized for upcoming releases.

## Principles

Our roadmap follows these principles:

1. **Backward Compatibility**: We avoid breaking changes when possible
2. **Performance First**: New features shouldn't sacrifice performance
3. **Developer Experience**: APIs should be intuitive and discoverable
4. **Minimal Dependencies**: Core library stays lean
5. **Extensibility**: Advanced features are opt-in via separate packages

## How to Contribute

Interested in helping with roadmap items?

1. Comment on related issues to express interest
2. Submit a proposal for how you'd implement the feature
3. Open a draft PR for early feedback
4. See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines

## Timeline

We follow semantic versioning and aim for:

- **Patch releases**: Monthly (bug fixes, docs)
- **Minor releases**: Quarterly (new features, non-breaking)
- **Major releases**: Yearly (breaking changes)

Actual timing depends on feature complexity and community contributions.

---

*Last Updated: February 2026*
