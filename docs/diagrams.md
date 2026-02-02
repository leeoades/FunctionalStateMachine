# Mermaid Diagram Generation

Use the diagrams source generator to emit a Mermaid flowchart from your builder method.

Add the `[StateMachineDiagram]` attribute to your builder method and supply the output path.
The generator writes to that location relative to the project directory and creates folders as needed.

## Why it is useful

- Keeps documentation in sync with code.
- Produces diagrams without runtime reflection.
- Easy to review in pull requests.

## Simple example

```csharp
using FunctionalStateMachine.Diagrams;

[StateMachineDiagram("diagrams/LightSwitch.md")]
public static StateMachine<LightState, LightTrigger, LightCommand> Build()
{
    return StateMachine<LightState, LightTrigger, LightCommand>.Create()
        .StartWith(LightState.Off)
        .For(LightState.Off)
            .On<LightTrigger.Toggle>()
                .TransitionTo(LightState.On)
        .Build();
}
```

## Notes

- Immediate transitions are labeled as `immediate` in the generated flowchart.
