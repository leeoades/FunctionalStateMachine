# Changelog

All notable changes to this project will be documented in this file.

The format is based on Keep a Changelog, and this project adheres to Semantic Versioning.

## [0.1.0] - 2026-01-31
### Added
- Functional, data-carrying state machine core API with fluent builder.
- Hierarchical states with parent/child relationships and initial sub-state support.
- Conditional transition steps via `If`/`Else`/`Done`.
- Typed trigger records and typed `On<TTrigger>()` transitions.
- Commands returned from transitions, entry, and exit actions.
- Samples and tests covering core behavior, hierarchy, and shopping trolley flows.

