using System.Text;

namespace FunctionalStateMachine.Diagrams.Tests;

public sealed class DiagramBuilderTests
{
    [Fact]
    public void Builds_simple_transition_diagram()
    {
        var source = """
            using FunctionalStateMachine.Core;

            public static class Sample
            {
                public static object Build() =>
                    StateMachine<State, Trigger, Command>.Create()
                        .StartWith(State.Off)
                        .For(State.Off)
                            .On<Trigger.Toggle>()
                                .TransitionTo(State.On)
                        .Build();
            }

            public enum State
            {
                Off,
                On
            }

            public abstract record Trigger
            {
                public sealed record Toggle : Trigger;
            }

            public abstract record Command;
            """;

        var diagram = DiagramBuilder.GenerateDiagram(source, "Build", "Sample");

        var expected = """
            # Sample

            ```mermaid
            flowchart LR
              START((start)) --> S_State_Off
              S_State_Off[State.Off]
              S_State_On[State.On]
              S_State_Off -->|Trigger.Toggle| S_State_On
            ```
            """;

        Assert.Equal(Normalize(expected), Normalize(diagram));
    }

    [Fact]
    public void Builds_internal_transition_when_no_transition_to_is_declared()
    {
        var source = """
            using FunctionalStateMachine.Core;

            public static class Sample
            {
                public static object Build() =>
                    StateMachine<State, Trigger, Command>.Create()
                        .StartWith(State.Running)
                        .For(State.Running)
                            .On<Trigger.Tick>()
                                .Execute(() => new Command.Log())
                        .Build();
            }

            public enum State
            {
                Running
            }

            public abstract record Trigger
            {
                public sealed record Tick : Trigger;
            }

            public abstract record Command
            {
                public sealed record Log : Command;
            }
            """;

        var diagram = DiagramBuilder.GenerateDiagram(source, "Build", "Internal");

        Assert.NotNull(diagram);
        Assert.Contains("S_State_Running -->|Trigger.Tick| S_State_Running", diagram);
    }

    [Fact]
    public void Deduplicates_repeated_transitions()
    {
        var source = """
            using FunctionalStateMachine.Core;

            public static class Sample
            {
                public static object Build() =>
                    StateMachine<State, Trigger, Data, Command>.Create()
                        .StartWith(State.Running)
                        .For(State.Running)
                            .On<Trigger.Tick>()
                                .ModifyData(state => state.Data)
                                .Execute(state => new Command.Log())
                        .For(State.Paused)
                            .On<Trigger.Resume>()
                                .TransitionTo(State.Running)
                        .Build();
            }

            public enum State
            {
                Running,
                Paused
            }

            public abstract record Trigger
            {
                public sealed record Tick : Trigger;
                public sealed record Resume : Trigger;
            }

            public sealed record Data;

            public abstract record Command
            {
                public sealed record Log : Command;
            }
            """;

        var diagram = DiagramBuilder.GenerateDiagram(source, "Build", "TimerLike");

        Assert.NotNull(diagram);
        Assert.Equal(1, CountOccurrences(diagram!, "Trigger.Tick"));
        Assert.Equal(1, CountOccurrences(diagram!, "Trigger.Resume"));
    }

    [Fact]
    public void Renders_substates_inside_superstate_container()
    {
        var source = """
            using FunctionalStateMachine.Core;

            public static class Sample
            {
                public static object Build() =>
                    StateMachine<State, Trigger, Command>.Create()
                        .StartWith(State.Outside)
                        .For(State.InStore)
                            .StartsWith(State.Shopping)
                            .On<Trigger.Cancel>()
                                .TransitionTo(State.Outside)
                        .For(State.Shopping)
                            .SubStateOf(State.InStore)
                            .On<Trigger.GoToCheckout>()
                                .TransitionTo(State.CheckingOut)
                        .For(State.CheckingOut)
                            .SubStateOf(State.InStore)
                            .On<Trigger.Pay>()
                                .TransitionTo(State.Outside)
                        .Build();
            }

            public enum State
            {
                Outside,
                InStore,
                Shopping,
                CheckingOut
            }

            public abstract record Trigger
            {
                public sealed record Cancel : Trigger;
                public sealed record GoToCheckout : Trigger;
                public sealed record Pay : Trigger;
            }

            public abstract record Command;
            """;

        var diagram = DiagramBuilder.GenerateDiagram(source, "Build", "Hierarchy");

        Assert.NotNull(diagram);
        Assert.Contains("subgraph SG_State_InStore[State.InStore]", diagram);
        Assert.Contains("S_State_Shopping[State.Shopping]", diagram);
        Assert.Contains("S_State_CheckingOut[State.CheckingOut]", diagram);
    }

    private static string Normalize(string? value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        return value.Replace("\r\n", "\n").Trim();
    }

    private static int CountOccurrences(string value, string fragment)
    {
        int count = 0;
        int index = 0;

        while ((index = value.IndexOf(fragment, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += fragment.Length;
        }

        return count;
    }
}
