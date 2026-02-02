# Packages

This library is split into a few packages so you can reference only what you need.

## FunctionalStateMachine.Core

The core state machine implementation. 
Most consumers should reference this package otherwise what are you even doing here? 🤣

## FunctionalStateMachine.CommandRunner

Optional command runner integration that dispatches commands through DI.
You can implement your own command dispatcher but it's very boilerplate-y.

Use it when you want:
- Command handlers as DI-resolved classes.
- A built-in provider to run commands returned by the state machine.

Note: It includes a source generator so the disptacher does not use reflection.

## FunctionalStateMachine.Diagrams

Design-time diagram generator for Mermaid diagrams.

Use it when you want:
- Build-time diagrams from your builder methods.
- No runtime dependency (analyzer-only package).

Add a package reference and annotate builder methods with `[StateMachineDiagram("path/to/diagram.md")]`.

## Quick reference

- Always: `FunctionalStateMachine.Core`
- Optional: `FunctionalStateMachine.CommandRunner` for DI-based command execution.
- Optional: `FunctionalStateMachine.Diagrams` for build-time Mermaid diagrams.
