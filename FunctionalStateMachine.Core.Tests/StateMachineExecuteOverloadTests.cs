using FunctionalStateMachine.Core;

namespace FunctionalStateMachine.Core.Tests;

public class StateMachineExecuteOverloadTests
{
    [Fact]
    public void Execute_Overloads_ReturnExpectedCommands()
    {
        var advanceName = Trigger.Advance.GetType().Name;
        var refreshName = Trigger.Refresh.GetType().Name;

        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Advance)
                    .Execute(() => new Command("t-no-args"))
                    .Execute((Data data) => new Command($"t-data-{data.Value}"))
                    .Execute((State state, Data data) => new Command($"t-state-data-{state}-{data.Value}"))
                    .Execute((Trigger trigger) => new Command($"t-trigger-{trigger.GetType().Name}"))
                    .Execute((Data data, Trigger trigger) => new Command($"t-data-trigger-{data.Value}-{trigger.GetType().Name}"))
                    .Execute((State state, Data data, Trigger trigger) => new Command($"t-all-{state}-{data.Value}-{trigger.GetType().Name}"))
                    .Execute(() => new CommandBase[]
                    {
                        new Command("t-enum-1"),
                        new Command("t-enum-2")
                    })
                    .Execute((Data data) => new CommandBase[]
                    {
                        new Command($"t-enum-data-{data.Value}")
                    })
                    .Execute((State state, Data data) => new CommandBase[]
                    {
                        new Command($"t-enum-state-data-{state}-{data.Value}")
                    })
                    .Execute((Trigger trigger) => new CommandBase[]
                    {
                        new Command($"t-enum-trigger-{trigger.GetType().Name}")
                    })
                    .Execute((Data data, Trigger trigger) => new CommandBase[]
                    {
                        new Command($"t-enum-data-trigger-{data.Value}-{trigger.GetType().Name}")
                    })
                    .Execute((State state, Data data, Trigger trigger) => new CommandBase[]
                    {
                        new Command($"t-enum-all-{state}-{data.Value}-{trigger.GetType().Name}")
                    })
                    .TransitionTo(State.Done)
                    .Done()
                .On(Trigger.Refresh)
                    .If((Data data, Trigger trigger) => data.Value >= 0 && trigger == Trigger.Refresh)
                        .Execute(() => new Command("c-no-args"))
                        .Execute((Data data) => new Command($"c-data-{data.Value}"))
                        .Execute((State state, Data data) => new Command($"c-state-data-{state}-{data.Value}"))
                        .Execute((Trigger trigger) => new Command($"c-trigger-{trigger.GetType().Name}"))
                        .Execute((Data data, Trigger trigger) => new Command($"c-data-trigger-{data.Value}-{trigger.GetType().Name}"))
                        .Execute((State state, Data data, Trigger trigger) => new Command($"c-all-{state}-{data.Value}-{trigger.GetType().Name}"))
                        .Execute(() => new CommandBase[]
                        {
                            new Command("c-enum-1"),
                            new Command("c-enum-2")
                        })
                        .Execute((Data data) => new CommandBase[]
                        {
                            new Command($"c-enum-data-{data.Value}")
                        })
                        .Execute((State state, Data data) => new CommandBase[]
                        {
                            new Command($"c-enum-state-data-{state}-{data.Value}")
                        })
                        .Execute((Trigger trigger) => new CommandBase[]
                        {
                            new Command($"c-enum-trigger-{trigger.GetType().Name}")
                        })
                        .Execute((Data data, Trigger trigger) => new CommandBase[]
                        {
                            new Command($"c-enum-data-trigger-{data.Value}-{trigger.GetType().Name}")
                        })
                        .Execute((State state, Data data, Trigger trigger) => new CommandBase[]
                        {
                            new Command($"c-enum-all-{state}-{data.Value}-{trigger.GetType().Name}")
                        })
                        .TransitionTo(State.Done)
                        .Done()
                    .Done()
                .On<Trigger.DerivedTrigger>()
                    .Execute(() => new Command("d-no-args"))
                    .Execute((Data data) => new Command($"d-data-{data.Value}"))
                    .Execute((State state, Data data) => new Command($"d-state-data-{state}-{data.Value}"))
                    .Execute((Trigger.DerivedTrigger trigger) => new Command($"d-trigger-{trigger.Amount}"))
                    .Execute((Data data, Trigger.DerivedTrigger trigger) => new Command($"d-data-trigger-{data.Value}-{trigger.Amount}"))
                    .Execute((State state, Data data, Trigger.DerivedTrigger trigger) => new Command($"d-all-{state}-{data.Value}-{trigger.Amount}"))
                    .Execute(() => new CommandBase[]
                    {
                        new Command("d-enum-1"),
                        new Command("d-enum-2")
                    })
                    .Execute((Data data) => new CommandBase[]
                    {
                        new Command($"d-enum-data-{data.Value}")
                    })
                    .Execute((State state, Data data) => new CommandBase[]
                    {
                        new Command($"d-enum-state-data-{state}-{data.Value}")
                    })
                    .Execute((Trigger.DerivedTrigger trigger) => new CommandBase[]
                    {
                        new Command($"d-enum-trigger-{trigger.Amount}")
                    })
                    .Execute((Data data, Trigger.DerivedTrigger trigger) => new CommandBase[]
                    {
                        new Command($"d-enum-data-trigger-{data.Value}-{trigger.Amount}")
                    })
                    .Execute((State state, Data data, Trigger.DerivedTrigger trigger) => new CommandBase[]
                    {
                        new Command($"d-enum-all-{state}-{data.Value}-{trigger.Amount}")
                    })
                    .If((Data data, Trigger.DerivedTrigger trigger) => data.Value >= trigger.Amount)
                        .Execute(() => new Command("dc-no-args"))
                        .Execute((Data data) => new Command($"dc-data-{data.Value}"))
                        .Execute((State state, Data data) => new Command($"dc-state-data-{state}-{data.Value}"))
                        .Execute((Trigger.DerivedTrigger trigger) => new Command($"dc-trigger-{trigger.Amount}"))
                        .Execute((Data data, Trigger.DerivedTrigger trigger) => new Command($"dc-data-trigger-{data.Value}-{trigger.Amount}"))
                        .Execute((State state, Data data, Trigger.DerivedTrigger trigger) => new Command($"dc-all-{state}-{data.Value}-{trigger.Amount}"))
                        .Execute(() => new CommandBase[]
                        {
                            new Command("dc-enum-1"),
                            new Command("dc-enum-2")
                        })
                        .Execute((Data data) => new CommandBase[]
                        {
                            new Command($"dc-enum-data-{data.Value}")
                        })
                        .Execute((State state, Data data) => new CommandBase[]
                        {
                            new Command($"dc-enum-state-data-{state}-{data.Value}")
                        })
                        .Execute((Trigger.DerivedTrigger trigger) => new CommandBase[]
                        {
                            new Command($"dc-enum-trigger-{trigger.Amount}")
                        })
                        .Execute((Data data, Trigger.DerivedTrigger trigger) => new CommandBase[]
                        {
                            new Command($"dc-enum-data-trigger-{data.Value}-{trigger.Amount}")
                        })
                        .Execute((State state, Data data, Trigger.DerivedTrigger trigger) => new CommandBase[]
                        {
                            new Command($"dc-enum-all-{state}-{data.Value}-{trigger.Amount}")
                        })
                        .Done()
                    .TransitionTo(State.Done)
                    .Done()
                .For(State.Done)
                .Build();

        (State nextState, Data _, IReadOnlyList<CommandBase> transitionCommands) =
            machine.Fire(Trigger.Advance, State.Ready, new Data(7));
        (State _, Data _, IReadOnlyList<CommandBase> conditionalCommands) =
            machine.Fire(Trigger.Refresh, State.Ready, new Data(5));
        (State _, Data _, IReadOnlyList<CommandBase> derivedCommands) =
            machine.Fire(new Trigger.DerivedTrigger(2), State.Ready, new Data(4));
        (State _, Data _, IReadOnlyList<CommandBase> derivedConditionalCommands) =
            machine.Fire(new Trigger.DerivedTrigger(2), State.Ready, new Data(3));

        Assert.Equal(State.Done, nextState);
        Assert.Contains(transitionCommands, command => ((Command)command).Message == "t-no-args");
        Assert.Contains(transitionCommands, command => ((Command)command).Message == "t-data-7");
        Assert.Contains(transitionCommands, command => ((Command)command).Message == "t-state-data-Ready-7");
        Assert.Contains(transitionCommands, command => ((Command)command).Message == $"t-trigger-{advanceName}");
        Assert.Contains(transitionCommands, command => ((Command)command).Message == $"t-data-trigger-7-{advanceName}");
        Assert.Contains(transitionCommands, command => ((Command)command).Message == $"t-all-Ready-7-{advanceName}");
        Assert.Contains(transitionCommands, command => ((Command)command).Message == "t-enum-1");
        Assert.Contains(transitionCommands, command => ((Command)command).Message == "t-enum-2");
        Assert.Contains(transitionCommands, command => ((Command)command).Message == "t-enum-data-7");
        Assert.Contains(transitionCommands, command => ((Command)command).Message == "t-enum-state-data-Ready-7");
        Assert.Contains(transitionCommands, command => ((Command)command).Message == $"t-enum-trigger-{advanceName}");
        Assert.Contains(transitionCommands, command => ((Command)command).Message == $"t-enum-data-trigger-7-{advanceName}");
        Assert.Contains(transitionCommands, command => ((Command)command).Message == $"t-enum-all-Ready-7-{advanceName}");

        Assert.Contains(conditionalCommands, command => ((Command)command).Message == "c-no-args");
        Assert.Contains(conditionalCommands, command => ((Command)command).Message == "c-data-5");
        Assert.Contains(conditionalCommands, command => ((Command)command).Message == "c-state-data-Ready-5");
        Assert.Contains(conditionalCommands, command => ((Command)command).Message == $"c-trigger-{refreshName}");
        Assert.Contains(conditionalCommands, command => ((Command)command).Message == $"c-data-trigger-5-{refreshName}");
        Assert.Contains(conditionalCommands, command => ((Command)command).Message == $"c-all-Ready-5-{refreshName}");
        Assert.Contains(conditionalCommands, command => ((Command)command).Message == "c-enum-1");
        Assert.Contains(conditionalCommands, command => ((Command)command).Message == "c-enum-2");
        Assert.Contains(conditionalCommands, command => ((Command)command).Message == "c-enum-data-5");
        Assert.Contains(conditionalCommands, command => ((Command)command).Message == "c-enum-state-data-Ready-5");
        Assert.Contains(conditionalCommands, command => ((Command)command).Message == $"c-enum-trigger-{refreshName}");
        Assert.Contains(conditionalCommands, command => ((Command)command).Message == $"c-enum-data-trigger-5-{refreshName}");
        Assert.Contains(conditionalCommands, command => ((Command)command).Message == $"c-enum-all-Ready-5-{refreshName}");

        Assert.Contains(derivedCommands, command => ((Command)command).Message == "d-no-args");
        Assert.Contains(derivedCommands, command => ((Command)command).Message == "d-data-4");
        Assert.Contains(derivedCommands, command => ((Command)command).Message == "d-state-data-Ready-4");
        Assert.Contains(derivedCommands, command => ((Command)command).Message == "d-trigger-2");
        Assert.Contains(derivedCommands, command => ((Command)command).Message == "d-data-trigger-4-2");
        Assert.Contains(derivedCommands, command => ((Command)command).Message == "d-all-Ready-4-2");
        Assert.Contains(derivedCommands, command => ((Command)command).Message == "d-enum-1");
        Assert.Contains(derivedCommands, command => ((Command)command).Message == "d-enum-2");
        Assert.Contains(derivedCommands, command => ((Command)command).Message == "d-enum-data-4");
        Assert.Contains(derivedCommands, command => ((Command)command).Message == "d-enum-state-data-Ready-4");
        Assert.Contains(derivedCommands, command => ((Command)command).Message == "d-enum-trigger-2");
        Assert.Contains(derivedCommands, command => ((Command)command).Message == "d-enum-data-trigger-4-2");
        Assert.Contains(derivedCommands, command => ((Command)command).Message == "d-enum-all-Ready-4-2");

        Assert.Contains(derivedConditionalCommands, command => ((Command)command).Message == "dc-no-args");
        Assert.Contains(derivedConditionalCommands, command => ((Command)command).Message == "dc-data-3");
        Assert.Contains(derivedConditionalCommands, command => ((Command)command).Message == "dc-state-data-Ready-3");
        Assert.Contains(derivedConditionalCommands, command => ((Command)command).Message == "dc-trigger-2");
        Assert.Contains(derivedConditionalCommands, command => ((Command)command).Message == "dc-data-trigger-3-2");
        Assert.Contains(derivedConditionalCommands, command => ((Command)command).Message == "dc-all-Ready-3-2");
        Assert.Contains(derivedConditionalCommands, command => ((Command)command).Message == "dc-enum-1");
        Assert.Contains(derivedConditionalCommands, command => ((Command)command).Message == "dc-enum-2");
        Assert.Contains(derivedConditionalCommands, command => ((Command)command).Message == "dc-enum-data-3");
        Assert.Contains(derivedConditionalCommands, command => ((Command)command).Message == "dc-enum-state-data-Ready-3");
        Assert.Contains(derivedConditionalCommands, command => ((Command)command).Message == "dc-enum-trigger-2");
        Assert.Contains(derivedConditionalCommands, command => ((Command)command).Message == "dc-enum-data-trigger-3-2");
        Assert.Contains(derivedConditionalCommands, command => ((Command)command).Message == "dc-enum-all-Ready-3-2");
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

    private sealed record Data(int Value);

    private abstract record CommandBase;

    private sealed record Command(string Message) : CommandBase;
}
