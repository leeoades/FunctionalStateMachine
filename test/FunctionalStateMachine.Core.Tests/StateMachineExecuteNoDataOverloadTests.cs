namespace FunctionalStateMachine.Core.Tests;

public class StateMachineExecuteNoDataOverloadTests
{
    [Fact]
    public void Execute_NoDataOverloads_ReturnExpectedCommands()
    {
        var advanceName = Trigger.Advance.GetType().Name;
        var refreshName = Trigger.Refresh.GetType().Name;

        var machine = StateMachine<State, Trigger, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Advance)
                    .Execute(() => new Command("t-no-args"))
                    .Execute((State state) => new Command($"t-state-{state}"))
                    .Execute((Trigger trigger) => new Command($"t-trigger-{trigger.GetType().Name}"))
                    .Execute(() => new CommandBase[]
                    {
                        new Command("t-enum-1"),
                        new Command("t-enum-2")
                    })
                    .Execute((State state) => new CommandBase[]
                    {
                        new Command($"t-enum-state-{state}")
                    })
                    .Execute((Trigger trigger) => new CommandBase[]
                    {
                        new Command($"t-enum-trigger-{trigger.GetType().Name}")
                    })
                    .TransitionTo(State.Done)
                .On(Trigger.Refresh)
                    .If((State state, Trigger trigger) => state == State.Ready && trigger == Trigger.Refresh)
                        .Execute((State state, Trigger trigger) => new CommandBase[]
                        {
                            new Command($"c-all-{state}-{trigger.GetType().Name}")
                        })
                        .Execute((State state, Trigger trigger) => new CommandBase[]
                        {
                            new Command($"c-enum-all-{state}-{trigger.GetType().Name}")
                        })
                        .Done()
                    .TransitionTo(State.Done)
                .On<Trigger.DerivedTrigger>()
                    .Execute(() => new Command("d-no-args"))
                    .Execute((State state) => new Command($"d-state-{state}"))
                    .Execute((Trigger.DerivedTrigger trigger) => new Command($"d-trigger-{trigger.Amount}"))
                    .Execute(() => new CommandBase[]
                    {
                        new Command("d-enum-1"),
                        new Command("d-enum-2")
                    })
                    .Execute((State state) => new CommandBase[]
                    {
                        new Command($"d-enum-state-{state}")
                    })
                    .Execute((Trigger.DerivedTrigger trigger) => new CommandBase[]
                    {
                        new Command($"d-enum-trigger-{trigger.Amount}")
                    })
                    .If((State state, Trigger.DerivedTrigger trigger) => trigger.Amount >= 0 && state == State.Ready)
                        .Execute((State state, Trigger.DerivedTrigger trigger) => new CommandBase[]
                        {
                            new Command($"dc-all-{state}-{trigger.Amount}")
                        })
                        .Execute((State state, Trigger.DerivedTrigger trigger) => new CommandBase[]
                        {
                            new Command($"dc-enum-all-{state}-{trigger.Amount}")
                        })
                        .Done()
                    .TransitionTo(State.Done)
                .For(State.Done)
                .Build();

        (State nextState, IReadOnlyList<CommandBase> transitionCommands) = machine.Fire(Trigger.Advance, State.Ready);
        (State conditionalState, IReadOnlyList<CommandBase> conditionalCommands) = machine.Fire(Trigger.Refresh, State.Ready);
        (State derivedState, IReadOnlyList<CommandBase> derivedCommands) = machine.Fire(new Trigger.DerivedTrigger(2), State.Ready);
        (State derivedConditionalState, IReadOnlyList<CommandBase> derivedConditionalCommands) = machine.Fire(new Trigger.DerivedTrigger(1), State.Ready);

        Assert.Equal(State.Done, nextState);
        Assert.Contains(transitionCommands, command => ((Command)command).Message == "t-no-args");
        Assert.Contains(transitionCommands, command => ((Command)command).Message == "t-state-Ready");
        Assert.Contains(transitionCommands, command => ((Command)command).Message == $"t-trigger-{advanceName}");
        Assert.Contains(transitionCommands, command => ((Command)command).Message == "t-enum-1");
        Assert.Contains(transitionCommands, command => ((Command)command).Message == "t-enum-2");
        Assert.Contains(transitionCommands, command => ((Command)command).Message == "t-enum-state-Ready");
        Assert.Contains(transitionCommands, command => ((Command)command).Message == $"t-enum-trigger-{advanceName}");

        Assert.Equal(State.Done, conditionalState);
        Assert.Contains(conditionalCommands, command => ((Command)command).Message == $"c-all-Ready-{refreshName}");
        Assert.Contains(conditionalCommands, command => ((Command)command).Message == $"c-enum-all-Ready-{refreshName}");

        Assert.Equal(State.Done, derivedState);
        Assert.Contains(derivedCommands, command => ((Command)command).Message == "d-no-args");
        Assert.Contains(derivedCommands, command => ((Command)command).Message == "d-state-Ready");
        Assert.Contains(derivedCommands, command => ((Command)command).Message == "d-trigger-2");
        Assert.Contains(derivedCommands, command => ((Command)command).Message == "d-enum-1");
        Assert.Contains(derivedCommands, command => ((Command)command).Message == "d-enum-2");
        Assert.Contains(derivedCommands, command => ((Command)command).Message == "d-enum-state-Ready");
        Assert.Contains(derivedCommands, command => ((Command)command).Message == "d-enum-trigger-2");

        Assert.Equal(State.Done, derivedConditionalState);
        Assert.Contains(derivedConditionalCommands, command => ((Command)command).Message == "dc-all-Ready-1");
        Assert.Contains(derivedConditionalCommands, command => ((Command)command).Message == "dc-enum-all-Ready-1");
    }

    private enum State
    {
        Ready,
        Done
    }

    private abstract record Trigger
    {
        public sealed record AdvanceTrigger : Trigger;
        public sealed record RefreshTrigger : Trigger;
        public sealed record DerivedTrigger(int Amount) : Trigger;

        public static readonly Trigger Advance = new AdvanceTrigger();
        public static readonly Trigger Refresh = new RefreshTrigger();
    }

    private abstract record CommandBase;

    private sealed record Command(string Message) : CommandBase;
}
