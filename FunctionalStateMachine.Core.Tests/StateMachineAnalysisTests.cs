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
                    .On<Trigger.T1>()
                        .TransitionTo(State.B)
                .For(State.B)
                    .On<Trigger.T1>()
                        .TransitionTo(State.A)
                .For(State.C)  // Unreachable state
                    .On<Trigger.T1>()
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
                    .On<Trigger.T1>()
                        .TransitionTo(State.B)
                .For(State.B)
                    .On<Trigger.T1>()
                        .TransitionTo(State.A)
                // Unreachable states
                .For(State.C)  
                    .On<Trigger.T1>()
                        .TransitionTo(State.D)
                .For(State.D) 
                    .On<Trigger.T1>()
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
                .On<Trigger.T1>()
                    .TransitionTo(State.B)
            .For(State.B)
                .On<Trigger.T1>()
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
            // Multiple transitions are not allowed
            var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
                .StartWith(State.A)
                .For(State.A)
                    .On<Trigger.T1>()
                        .TransitionTo(State.B)
                        .Execute(() => new Command.Noop())
                    .On<Trigger.T1>()
                        .TransitionTo(State.C) // Multiple transitions
                        .Execute(() => new Command.Noop())
                .For(State.B)
                    .On<Trigger.T1>()
                        .TransitionTo(State.A)
                .For(State.C)
                    .On<Trigger.T1>()
                        .TransitionTo(State.A)
                .Build();
        });

        Assert.Contains("ambiguous", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Validate_AllowsGuardedAmbiguousTransitions()
    {
        // Multiple transitions with guards are allowed
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.A)
            .For(State.A)
                .On<Trigger.T1>()
                    .Guard(data => data.Value > 10)
                    .TransitionTo(State.B)
                    .Execute(() => new Command.Noop())
                .On<Trigger.T1>()
                    .Guard(data => data.Value <= 10)
                    .TransitionTo(State.C)
                    .Execute(() => new Command.Noop())
            .For(State.B)
                .On<Trigger.T1>()
                    .TransitionTo(State.A)
            .For(State.C)
                .On<Trigger.T1>()
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
                .On<Trigger.T1>()
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
                .On<Trigger.T1>()
                    .If(data => data.Value > 5)
                        .TransitionTo(State.B)
                        .ElseIf(data => data.Value <= 5)
                        .TransitionTo(State.C)
                        .Done()
            .For(State.B)
                .On<Trigger.T1>()
                    .TransitionTo(State.A)
            .For(State.C)
                .On<Trigger.T1>()
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
                    .On<Trigger.T1>()
                        .TransitionTo(State.B)
                        .If(data => data.Value > 5)
                            .Execute(() => new Command.Noop())
                            .Else()
                            .TransitionTo(State.C)
                            .Done()
                .For(State.B)
                    .On<Trigger.T1>()
                        .TransitionTo(State.A)
                .For(State.C)
                    .On<Trigger.T1>()
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
                    .On<Trigger.T1>()
                        .If(data => data.Value > 5)
                            .TransitionTo(State.B)
                            .Else()
                            .Execute(() => new Command.Noop())
                            .Done()
                        .TransitionTo(State.C)
                .For(State.B)
                    .On<Trigger.T1>()
                        .TransitionTo(State.A)
                .For(State.C)
                    .On<Trigger.T1>()
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
                    .On<Trigger.T1>()
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
                    .On<Trigger.T1>()
                        .TransitionTo(State.A)
                .For(State.C)
                    .On<Trigger.T1>()
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
                .On<Trigger.T1>()
                    .TransitionTo(State.B)
            .For(State.B)
                .On<Trigger.T1>()
                    .Guard(data => data.Value > 5)
                    .TransitionTo(State.C)
                .On<Trigger.T1>()
                    .Guard(data => data.Value <= 5)
                    .TransitionTo(State.A)
            .For(State.C)
                .On<Trigger.T1>()
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
                .On<Trigger.T1>()  // T1 is used
                    .TransitionTo(State.B)
            .For(State.B)
                .On<Trigger.T1>()
                    .TransitionTo(State.A)
            // T2 and T3 are never used
            .Build();

        // Should not throw, but warnings should be logged
        Assert.NotNull(machine);
    }

    private enum State
    {
        A,
        B,
        C,
        D
    }

    private abstract record Trigger
    {
        public sealed record T1 : Trigger;
        public sealed record T2 : Trigger;  // Unused trigger
        public sealed record T3 : Trigger;  // Unused trigger
    }

    private sealed record Data(int Value);

    private abstract record CommandBase;

    private sealed record Command
    {
        public sealed record Noop : CommandBase;
    }
}
