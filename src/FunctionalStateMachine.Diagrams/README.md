# FunctionalStateMachine.Diagrams

`FunctionalStateMachine.Diagrams` is a Roslyn source generator that emits Mermaid diagrams from fluent state machine definitions at compile time. It scans methods annotated with `[StateMachineDiagram]` and writes `.md` files into your project.

## How to use

### 1) Reference as an analyzer

When using a project reference, the generator must be referenced as an analyzer:

```xml
<ItemGroup>
  <ProjectReference Include="..\FunctionalStateMachine.Diagrams\FunctionalStateMachine.Diagrams.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

### 2) Annotate a builder method

```csharp
using FunctionalStateMachine.Diagrams;

[StateMachineDiagram("diagrams/MyMachine.md")]
public static StateMachine<MyState, MyTrigger, MyData, MyCommand> Build()
{
    return StateMachine<MyState, MyTrigger, MyData, MyCommand>.Create()
        .StartWith(MyState.Initial)
        .For(MyState.Initial)
            .On<MyTrigger.StartTrigger>()
                .TransitionTo(MyState.Running)
        .Build();
}
```

### 3) Build the project

The generator writes the diagram to the path you specify (relative to the project directory).

## Notes on project references

If `FunctionalStateMachine.Diagrams` is added as a normal project reference, the source generator will not run and no diagrams will be emitted. Because Roslyn generators are only loaded via analyzer references, there is no reliable way for the generator to detect an incorrect reference.

If you want the build to complain when the analyzer is missing, you can add a guard target in your project:

```xml
<Target Name="ValidateDiagramGenerator" BeforeTargets="CoreCompile">
  <Error Condition="@(Analyzer->AnyHaveMetadataValue('Identity', 'FunctionalStateMachine.Diagrams')) == 'false'"
         Text="FunctionalStateMachine.Diagrams must be referenced as an analyzer. See README for setup." />
</Target>
```
