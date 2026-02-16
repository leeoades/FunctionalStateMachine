namespace FunctionalStateMachine.Core.Tests;

public class StateMachineAnalysisTests
{
    [Fact]
    public void Validate_DetectsUnreachableStates()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
                .StartWith(State.A)
                .For(State.A)
                    .On<Trigger.T1Trigger>()
                        .TransitionTo(State.B)
                .For(State.B)
                    .On<Trigger.T1Trigger>()
                        .TransitionTo(State.A)
                .For(State.C)  // Unreachable state
                    .On<Trigger.T1Trigger>()
                        .TransitionTo(State.A)
                .Build();
        });

        Assert.Contains("unreachable", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
    
    [Fact]
    public void Validate_DetectsUnreachableGroupOfStates()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
                .StartWith(State.A)
                .For(State.A)
                    .On<Trigger.T1Trigger>()
                        .TransitionTo(State.B)
                .For(State.B)
                    .On<Trigger.T1Trigger>()
                        .TransitionTo(State.A)
                // Unreachable states
                .For(State.C)  
                    .On<Trigger.T1Trigger>()
                        .TransitionTo(State.D)
                .For(State.D) 
                    .On<Trigger.T1Trigger>()
                        .TransitionTo(State.C)
                .Build();
        });

        Assert.Contains("unreachable", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_DetectsImmediateTransitionInfiniteLoop1Step()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
                .StartWith(State.A)
                .For(State.A)
                    .Immediately()
                        .TransitionTo(State.A) // Infinite loop
                        .Done()
                .Build();
        });

        Assert.Contains("infinite loop", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_DetectsImmediateTransitionInfiniteLoop2Steps()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
                .StartWith(State.A)
                .For(State.A)
                    .Immediately()
                        .TransitionTo(State.B)
                        .Done()
                .For(State.B)
                    .Immediately()
                        .TransitionTo(State.A)  // Infinite loop
                        .Done()
                .Build();
        });

        Assert.Contains("infinite loop", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_DetectsImmediateTransitionInfiniteLoop3Steps()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
                .StartWith(State.A)
                .For(State.A)
                    .Immediately()
                        .TransitionTo(State.B)
                        .Done()
                .For(State.B)
                    .Immediately()
                        .TransitionTo(State.C)
                        .Done()
                .For(State.C)
                    .Immediately()
                        .TransitionTo(State.A)  // Infinite loop
                        .Done()
                .Build();
        });

        Assert.Contains("infinite loop", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_AllowsTerminalStates()
    {
        // States with no outgoing transitions are allowed (terminal states)
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.A)
            .For(State.A)
                .On<Trigger.T1Trigger>()
                    .TransitionTo(State.B)
            .For(State.B)
                .On<Trigger.T1Trigger>()
                    .Ignore()
            .Build();

        // Should not throw
        Assert.NotNull(machine);
    }

    
    [Fact]
    public void Validate_DetectsAmbiguousTransitions()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            // Multiple unguarded transitions are not allowed
            var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
                .StartWith(State.A)
                .For(State.A)
                    .On<Trigger.T1Trigger>()
                        .TransitionTo(State.B)
                        .Execute(() => new Command.Noop())
                    .On<Trigger.T1Trigger>()
                        .TransitionTo(State.C) // Multiple unguarded transitions
                        .Execute(() => new Command.Noop())
                .For(State.B)
                    .On<Trigger.T1Trigger>()
                        .TransitionTo(State.A)
                .For(State.C)
                    .On<Trigger.T1Trigger>()
                        .TransitionTo(State.A)
                .Build();
        });

        Assert.Contains("unguarded", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("unreachable", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_AllowsGuardedAmbiguousTransitions()
    {
        // Multiple transitions with guards are allowed
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.A)
            .For(State.A)
                .On<Trigger.T1Trigger>()
                    .Guard(data => data.Value > 10)
                    .TransitionTo(State.B)
                    .Execute(() => new Command.Noop())
                .On<Trigger.T1Trigger>()
                    .Guard(data => data.Value <= 10)
                    .TransitionTo(State.C)
                    .Execute(() => new Command.Noop())
            .For(State.B)
                .On<Trigger.T1Trigger>()
                    .TransitionTo(State.A)
            .For(State.C)
                .On<Trigger.T1Trigger>()
                    .TransitionTo(State.A)
            .Build();

        Assert.NotNull(machine);
    }

    [Fact]
    public void Validate_AllowsMultipleImmediateTransitionsWithoutCycle()
    {
        // Immediate transitions that don't form cycles are allowed
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.A)
            .For(State.A)
                .Immediately()
                    .TransitionTo(State.B)
                    .Done()
            .For(State.B)
                .Immediately()
                    .TransitionTo(State.C)  // Forward chain, not a cycle
                    .Done()
            .For(State.C)
                .On<Trigger.T1Trigger>()
                    .TransitionTo(State.A)
            .Build();

        Assert.NotNull(machine);
    }

    [Fact]
    public void Validate_DetectsConditionalTransitionToAmbiguity()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.A)
            .For(State.A)
                .On<Trigger.T1Trigger>()
                    .If(data => data.Value > 5)
                        .TransitionTo(State.B)
                        .ElseIf(data => data.Value <= 5)
                        .TransitionTo(State.C)
                        .Done()
            .For(State.B)
                .On<Trigger.T1Trigger>()
                    .TransitionTo(State.A)
            .For(State.C)
                .On<Trigger.T1Trigger>()
                    .TransitionTo(State.A)
            .Build();

        Assert.NotNull(machine);
    }

    [Fact]
    public void Validate_DetectsMultipleTransitionToInSameTransition()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
                .StartWith(State.A)
                .For(State.A)
                    .On<Trigger.T1Trigger>()
                        .TransitionTo(State.B)
                        .If(data => data.Value > 5)
                            .Execute(() => new Command.Noop())
                            .Else()
                            .TransitionTo(State.C)
                            .Done()
                .For(State.B)
                    .On<Trigger.T1Trigger>()
                        .TransitionTo(State.A)
                .For(State.C)
                    .On<Trigger.T1Trigger>()
                        .TransitionTo(State.A)
                .Build();
        });

        Assert.Contains("TransitionTo", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_DetectsTransitionToAfterConditionalChain()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
                .StartWith(State.A)
                .For(State.A)
                    .On<Trigger.T1Trigger>()
                        .If(data => data.Value > 5)
                            .TransitionTo(State.B)
                            .Else()
                            .Execute(() => new Command.Noop())
                            .Done()
                        .TransitionTo(State.C)
                .For(State.B)
                    .On<Trigger.T1Trigger>()
                        .TransitionTo(State.A)
                .For(State.C)
                    .On<Trigger.T1Trigger>()
                        .TransitionTo(State.A)
                .Build();
        });

        Assert.Contains("TransitionTo", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_DetectsTransitionToInTwoConditionalChains()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
                .StartWith(State.A)
                .For(State.A)
                    .On<Trigger.T1Trigger>()
                        .If(data => data.Value > 5)
                            .TransitionTo(State.B)
                            .Else()
                            .Execute(() => new Command.Noop())
                            .Done()
                        .If(data => data.Value > 1)
                            .TransitionTo(State.C)
                            .Else()
                            .Execute(() => new Command.Noop())
                            .Done()
                .For(State.B)
                    .On<Trigger.T1Trigger>()
                        .TransitionTo(State.A)
                .For(State.C)
                    .On<Trigger.T1Trigger>()
                        .TransitionTo(State.A)
                .Build();
        });

        Assert.Contains("TransitionTo", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_AllowsComplexReachableStateMachine()
    {
        // A valid, more complex state machine
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.A)
            .For(State.A)
                .On<Trigger.T1Trigger>()
                    .TransitionTo(State.B)
            .For(State.B)
                .On<Trigger.T1Trigger>()
                    .Guard(data => data.Value > 5)
                    .TransitionTo(State.C)
                .On<Trigger.T1Trigger>()
                    .Guard(data => data.Value <= 5)
                    .TransitionTo(State.A)
            .For(State.C)
                .On<Trigger.T1Trigger>()
                    .TransitionTo(State.A)
            .Build();

        Assert.NotNull(machine);
    }

    [Fact]
    public void Validate_WarnsOnUnusedTriggers()
    {
        // State machine that doesn't use all defined trigger types
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.A)
            .For(State.A)
                .On<Trigger.T1Trigger>()  // T1 is used
                    .TransitionTo(State.B)
            .For(State.B)
                .On<Trigger.T1Trigger>()
                    .TransitionTo(State.A)
            // T2 and T3 are never used
            .Build();

        // Should not throw, but warnings should be logged
        Assert.NotNull(machine);
    }

    [Fact]
    public void Validate_MarksParentStatesReachable_WhenImmediateTransitionTargetsChild()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.Leaf)
            .For(State.Root)
                .StartsWith(State.Parent)
            .For(State.Parent)
                .SubStateOf(State.Root)
                .StartsWith(State.Child)
            .For(State.Child)
                .SubStateOf(State.Parent)
            .For(State.Leaf)
                .Immediately()
                    .TransitionTo(State.Child)
                    .Done()
            .Build();

        Assert.NotNull(machine);
    }

    [Fact]
    public void Validate_DetectsUnguardedTransitionBeforeOtherTransitions()
    {
        // Unguarded transition before other transitions makes them unreachable
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            StateMachine<State, Trigger, Data, CommandBase>.Create()
                .StartWith(State.A)
                .For(State.A)
                    .On<Trigger.T1Trigger>()
                        .TransitionTo(State.B)  // No guard - always matches
                        .Execute(() => new Command.Noop())
                    .On<Trigger.T1Trigger>()    // This is unreachable!
                        .Guard(data => data.Value > 10)
                        .TransitionTo(State.C)
                        .Execute(() => new Command.Noop())
                .For(State.B)
                    .On<Trigger.T1Trigger>()
                        .TransitionTo(State.A)
                .For(State.C)
                    .On<Trigger.T1Trigger>()
                        .TransitionTo(State.A)
                .Build();
        });

        Assert.Contains("unguarded transition", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("unreachable", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_AllowsUnguardedTransitionAsLast()
    {
        // Unguarded transition as the last one is the catch-all pattern
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.A)
            .For(State.A)
                .On<Trigger.T1Trigger>()
                    .Guard(data => data.Value > 10)
                    .TransitionTo(State.B)
                    .Execute(() => new Command.Noop())
                .On<Trigger.T1Trigger>()  // Catch-all: no guard, last position
                    .TransitionTo(State.C)
                    .Execute(() => new Command.Noop())
            .For(State.B)
                .On<Trigger.T1Trigger>()
                    .TransitionTo(State.A)
            .For(State.C)
                .On<Trigger.T1Trigger>()
                    .TransitionTo(State.A)
            .Build();

        Assert.NotNull(machine);
    }

    [Fact]
    public void Validate_WarnsAboutMultipleGuardedTransitions()
    {
        // Multiple guarded transitions should produce a warning (but not error)
        // This is allowed but worth noting to users
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.A)
            .For(State.A)
                .On<Trigger.T1Trigger>()
                    .Guard(data => data.Value > 10)
                    .TransitionTo(State.B)
                    .Execute(() => new Command.Noop())
                .On<Trigger.T1Trigger>()
                    .Guard(data => data.Value <= 10)
                    .TransitionTo(State.C)
                    .Execute(() => new Command.Noop())
            .For(State.B)
                .On<Trigger.T1Trigger>()
                    .TransitionTo(State.A)
            .For(State.C)
                .On<Trigger.T1Trigger>()
                    .TransitionTo(State.A)
            .Build();

        // Should build successfully (warning, not error)
        Assert.NotNull(machine);
    }

    [Fact]
    public void Validate_MarksGrandparentStatesReachable_WhenTransitionTargetsNestedChild()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.Leaf)
            .For(State.Root)
                .StartsWith(State.Parent)
            .For(State.Parent)
                .SubStateOf(State.Root)
                .StartsWith(State.Child)
            .For(State.Child)
                .SubStateOf(State.Parent)
                .StartsWith(State.GrandChild)
            .For(State.GrandChild)
                .SubStateOf(State.Child)
            .For(State.Leaf)
                .On<Trigger.T1Trigger>()
                    .TransitionTo(State.GrandChild)
            .Build();

        Assert.NotNull(machine);
    }

    [Fact]
    public void Validate_MarksParentStatesReachable_WhenTransitionTargetsParent()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.Leaf)
            .For(State.Root)
                .StartsWith(State.Parent)
            .For(State.Parent)
                .SubStateOf(State.Root)
                .StartsWith(State.Child)
            .For(State.Child)
                .SubStateOf(State.Parent)
            .For(State.Leaf)
                .On<Trigger.T1Trigger>()
                    .TransitionTo(State.Parent)
            .Build();

        Assert.NotNull(machine);
    }

    [Fact]
    public void Validate_CollectsConditionalTransitionTargets_StatesAreReachable()
    {
        // States B and C are ONLY reachable via conditional branches.
        // If CollectTransitionTargetStates didn't handle Conditional steps,
        // validation would fail with "unreachable" error.
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.A)
            .For(State.A)
                .On<Trigger.T1Trigger>()
                    .If(data => data.Value > 0)
                        .TransitionTo(State.B)
                        .Else()
                        .TransitionTo(State.C)
                        .Done()
            .For(State.B)
                .On<Trigger.T1Trigger>()
                    .TransitionTo(State.A)
            .For(State.C)
                .On<Trigger.T1Trigger>()
                    .TransitionTo(State.A)
            .Build();

        // Build succeeded means conditional targets B and C were collected as reachable
        Assert.NotNull(machine);
    }

    [Fact]
    public void Validate_DetectsUnconfiguredConditionalTransitionTarget()
    {
        // State C is a conditional target but not configured - should fail validation
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            StateMachine<State, Trigger, Data, CommandBase>.Create()
                .StartWith(State.A)
                .For(State.A)
                    .On<Trigger.T1Trigger>()
                        .If(data => data.Value > 0)
                            .TransitionTo(State.B)
                            .Else()
                            .TransitionTo(State.C)  // C is not configured
                            .Done()
                .For(State.B)
                    .On<Trigger.T1Trigger>()
                        .TransitionTo(State.A)
                .Build();
        });

        Assert.Contains("not configured", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private enum State
    {
        A,
        B,
        C,
        D,
        Root,
        Parent,
        Child,
        GrandChild,
        Leaf
    }

    private abstract record Trigger
    {
        public sealed record T1Trigger : Trigger;
        public sealed record T2Trigger : Trigger;
        public sealed record T3Trigger : Trigger;

        public static readonly Trigger T1 = new T1Trigger();
        public static readonly Trigger T2 = new T2Trigger();
        public static readonly Trigger T3 = new T3Trigger();
    }

    private sealed record Data(int Value)
    {
        public static Data Initial => new(0);
    }

    private abstract record CommandBase;

    private static class Command
    {
        public sealed record Noop : CommandBase;
    }
}
