# Fluent API Formatting Conventions

## Overview

The Functional State Machine library uses a fluent API for configuring state machines. To maintain readability and clearly show the hierarchical structure of state configurations, we follow specific indentation conventions.

## Recommended Indentation Pattern

The fluent API methods should be indented to reflect their logical hierarchy:

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .StartWith(State.Initial)
    .For(State.Initial)
        .On<Trigger.Start>()
            .Guard(data => data.IsValid)
            .TransitionTo(State.Running)
            .ModifyData(data => data with { StartTime = DateTime.Now })
            .Execute(data => new Command.Log("Started"))
    .For(State.Running)
        .On<Trigger.Stop>()
            .TransitionTo(State.Stopped)
            .Execute(() => new Command.Log("Stopped"))
    .Build();
```

### Indentation Levels

1. **Base level**: The variable assignment (`var machine =`)
2. **+4 spaces**: Top-level configuration methods (`.StartWith()`, `.For()`, `.Build()`)
3. **+8 spaces**: Trigger handlers (`.On<>()`, `.OnEntry()`, `.OnExit()`)
4. **+12 spaces**: Transition configuration (`.Guard()`, `.TransitionTo()`, `.ModifyData()`, `.Execute()`)

### Conditional Branches

For conditional logic, maintain the indentation hierarchy:

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .For(State.Processing)
        .On<Trigger.Process>()
            .If(data => data.Amount > 100)
                .Execute(() => new Command.HighValue())
                .TransitionTo(State.Premium)
                .ElseIf(data => data.Amount > 50)
                .Execute(() => new Command.MediumValue())
                .TransitionTo(State.Standard)
                .Else()
                .Execute(() => new Command.LowValue())
                .TransitionTo(State.Basic)
                .Done()
    .Build();
```

## IDE Support

### JetBrains Rider / ReSharper

The `.editorconfig` file includes settings that configure Rider/ReSharper to maintain this indentation pattern automatically. When you format code (Ctrl+K, Ctrl+D on Windows or Cmd+Option+L on Mac), it should preserve the hierarchical structure.

Key settings that enable this:
- `resharper_wrap_chained_method_calls = chop_if_long` - Each method call gets its own line
- `resharper_wrap_before_first_method_call = false` - First call continues from assignment
- `resharper_align_multiline_calls_chain = false` - Don't align dots vertically

### Visual Studio

**Important**: Visual Studio's built-in formatter (and `dotnet format`) has limited support for hierarchical method chain indentation. The formatter will respect basic indentation but may not maintain the multi-level hierarchy shown above.

#### Workarounds for Visual Studio Users:

1. **Manual Formatting**: After auto-formatting, manually adjust the indentation to match the pattern
2. **Format Selection**: Use "Format Selection" (Ctrl+K, Ctrl+F) on small sections rather than the whole file
3. **Disable Auto-Format**: Consider disabling format-on-save for state machine configuration files
4. **Use Rider**: If available, JetBrains Rider provides better control over fluent API formatting

### Visual Studio Code

VS Code with the C# extension (powered by Roslyn) has the same limitations as Visual Studio. Consider using the ReSharper extension if you need better formatting control.

## Alternative: CSharpier

For teams that want consistent, automatic formatting across all IDEs, consider adopting [CSharpier](https://csharpier.com/), which is an opinionated code formatter for C#. However, note that CSharpier has its own formatting rules and may not exactly match this guide.

## CI/CD Formatting Checks

To help maintain consistent formatting across the codebase:

1. The project's CI may include format checking via `dotnet format --verify-no-changes`
2. Contributors should ensure their changes follow the indentation pattern
3. Code reviews should verify that fluent API calls maintain hierarchical indentation

## Why This Pattern?

The hierarchical indentation pattern provides several benefits:

1. **Visual Hierarchy**: The indentation mirrors the logical structure of the state machine
2. **Easier Navigation**: It's easier to see which methods belong to which state/trigger
3. **Better Diffs**: Changes to specific states/triggers are more localized in version control
4. **Reduced Cognitive Load**: The structure is immediately apparent without needing to parse method names

## Examples from the Codebase

See these files for well-formatted examples:
- `samples/Basic/FunctionalStateMachine.Samples/LightSwitchSample.cs`
- `samples/Basic/FunctionalStateMachine.Samples/ShoppingTrolleySample.cs`
- `test/FunctionalStateMachine.Core.Tests/StateMachineConditionalTests.cs`

## Summary

While IDE support varies, maintaining this indentation pattern improves code readability significantly. The `.editorconfig` settings help Rider/ReSharper users automate this, while Visual Studio users may need to manually maintain the pattern. The consistency in readability is worth the extra attention to formatting.
