using FunctionalStateMachine.Core;

namespace FunctionalStateMachine.Core.Tests;

public class StateMachineInvalidOperationTests
{
    [Fact]
    public void InitialState_ThrowsWhenNoInitialStateConfigured()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
            .Build();

        var exception = Assert.Throws<InvalidOperationException>(() => _ = machine.InitialState);

        Assert.Contains("initial state", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Fire_ThrowsWhenStateIsNotConfigured()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On(Trigger.Go)
                    .TransitionTo(State.Ready)
            .Build();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            machine.Fire(Trigger.Go, State.Missing, Data.Initial));

        Assert.Contains("not configured", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Fire_ThrowsWhenCurrentStateIsParent()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.Parent)
            .For(State.Parent)
                .StartsWith(State.Child)
                .On(Trigger.Go)
                    .TransitionTo(State.Child)
            .For(State.Child)
                .SubStateOf(State.Parent)
            .Build();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            machine.Fire(Trigger.Go, State.Parent, Data.Initial));

        Assert.Contains("parent state", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_ThrowsWhenParentStateNotConfigured()
    {
        var builder = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Child)
                .SubStateOf(State.Parent);

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Contains("parent", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_ThrowsWhenStartsWithStateNotConfigured()
    {
        var builder = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Parent)
                .StartsWith(State.Child)
            .For(State.Other)
                .SubStateOf(State.Parent);

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Contains("starts with", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_ThrowsWhenStartsWithIsNotDirectChild()
    {
        var builder = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Parent)
                .StartsWith(State.Child)
            .For(State.Other)
                .SubStateOf(State.Parent)
                .StartsWith(State.Child)
            .For(State.Child)
                .SubStateOf(State.Other);

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Contains("direct child", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_ThrowsWhenStartsWithHasNoChildren()
    {
        var builder = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Parent)
                .StartsWith(State.Child)
            .For(State.Child);

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Contains("StartsWith", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_ThrowsWhenConditionalTransitionTargetsUnconfiguredState()
    {
        var builder = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On(Trigger.Go)
                    .If((state, data, trigger) => true)
                        .TransitionTo(State.Missing)
                        .Done();

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Contains("not configured", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_ThrowsWhenImmediateTransitionMissingTargetState()
    {
        var builder = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .Immediately()
                    .Execute((state, data) => new CommandBase[] { new Command("noop") })
                    .Done();

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Contains("immediate transition", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_ThrowsWhenImmediateTransitionTargetsUnconfiguredState()
    {
        var builder = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .Immediately()
                    .TransitionTo(State.Missing)
                    .Done();

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Contains("immediately", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_ThrowsOnHierarchyCycle()
    {
        var builder = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.A)
                .SubStateOf(State.B)
                .StartsWith(State.B)
            .For(State.B)
                .SubStateOf(State.A)
                .StartsWith(State.A);

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Contains("cycle", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Start_ThrowsOnImmediateTransitionLoopWhenAnalysisSkipped()
    {
        var builder = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.A);

        builder.For(State.A)
            .Immediately()
                .TransitionTo(State.B)
                .Done()
            .For(State.B)
                .Immediately()
                    .TransitionTo(State.A)
                    .Done();

        var machine = builder.SkipAnalysis().Build();

        var exception = Assert.Throws<InvalidOperationException>(() => machine.Start(Data.Initial));

        Assert.Contains("Immediate transition loop", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private enum State
    {
        Ready,
        Parent,
        Child,
        Other,
        Missing,
        A,
        B
    }

    private enum Trigger
    {
        Go
    }

    private sealed record Data(int Value)
    {
        public static Data Initial => new(0);
    }

    private abstract record CommandBase;

    private sealed record Command(string Message) : CommandBase;
}
