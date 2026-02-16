namespace FunctionalStateMachine.Core.Tests;

/// <summary>
/// Tests for extension method overloads in ConfigurationExtensions.*.cs files.
/// These provide convenient shorthand signatures for the core configuration methods.
/// </summary>
public class ConfigurationExtensionOverloadTests
{
    #region OnEntry Overloads

    [Fact]
    public void OnEntry_FuncCommand_ReturnsCommand()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Go)
                    .TransitionTo(State.Done)
            .For(State.Done)
                .OnEntry(() => new Command("entered"))
            .Build();

        var (_, _, commands) = machine.Fire(Trigger.Go, State.Ready, Data.Initial);

        Assert.Single(commands);
        Assert.Equal("entered", commands[0].Message);
    }

    [Fact]
    public void OnEntry_FuncDataCommand_ReturnsCommandWithData()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Go)
                    .TransitionTo(State.Done)
            .For(State.Done)
                .OnEntry((Data data) => new Command($"entered:{data.Value}"))
            .Build();

        var (_, _, commands) = machine.Fire(Trigger.Go, State.Ready, new Data(42));

        Assert.Single(commands);
        Assert.Equal("entered:42", commands[0].Message);
    }

    [Fact]
    public void OnEntry_FuncStateDataCommand_ReturnsCommandWithStateAndData()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Go)
                    .TransitionTo(State.Done)
            .For(State.Done)
                .OnEntry((State state, Data data) => new Command($"{state}:{data.Value}"))
            .Build();

        var (_, _, commands) = machine.Fire(Trigger.Go, State.Ready, new Data(42));

        Assert.Single(commands);
        Assert.Equal("Done:42", commands[0].Message);
    }

    [Fact]
    public void OnEntry_FuncIEnumerableCommand_ReturnsMultipleCommands()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Go)
                    .TransitionTo(State.Done)
            .For(State.Done)
                .OnEntry(() => new[] { new Command("a"), new Command("b") })
            .Build();

        var (_, _, commands) = machine.Fire(Trigger.Go, State.Ready, Data.Initial);

        Assert.Equal(2, commands.Count);
    }

    [Fact]
    public void OnEntry_FuncDataIEnumerableCommand_ReturnsMultipleCommandsWithData()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Go)
                    .TransitionTo(State.Done)
            .For(State.Done)
                .OnEntry((Data data) => new[] { new Command($"a:{data.Value}"), new Command("b") })
            .Build();

        var (_, _, commands) = machine.Fire(Trigger.Go, State.Ready, new Data(5));

        Assert.Equal(2, commands.Count);
        Assert.Equal("a:5", commands[0].Message);
    }

    #endregion

    #region OnExit Overloads

    [Fact]
    public void OnExit_FuncCommand_ReturnsCommand()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .OnExit(() => new Command("exited"))
                .On(Trigger.Go)
                    .TransitionTo(State.Done)
            .For(State.Done)
            .Build();

        var (_, _, commands) = machine.Fire(Trigger.Go, State.Ready, Data.Initial);

        Assert.Single(commands);
        Assert.Equal("exited", commands[0].Message);
    }

    [Fact]
    public void OnExit_FuncDataCommand_ReturnsCommandWithData()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .OnExit((Data data) => new Command($"exited:{data.Value}"))
                .On(Trigger.Go)
                    .TransitionTo(State.Done)
            .For(State.Done)
            .Build();

        var (_, _, commands) = machine.Fire(Trigger.Go, State.Ready, new Data(99));

        Assert.Single(commands);
        Assert.Equal("exited:99", commands[0].Message);
    }

    [Fact]
    public void OnExit_FuncStateDataCommand_ReturnsCommandWithStateAndData()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .OnExit((State state, Data data) => new Command($"{state}:{data.Value}"))
                .On(Trigger.Go)
                    .TransitionTo(State.Done)
            .For(State.Done)
            .Build();

        var (_, _, commands) = machine.Fire(Trigger.Go, State.Ready, new Data(77));

        Assert.Single(commands);
        Assert.Equal("Ready:77", commands[0].Message);
    }

    #endregion

    #region Guard Overloads - ImmediateTransitionConfiguration

    [Fact]
    public void Guard_Immediate_FuncDataBool_EvaluatesGuard()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .Immediately()
                    .Guard((Data data) => data.Value > 10)
                    .TransitionTo(State.Done)
                    .Done()
            .For(State.Done)
            .Build();

        var (state1, _, _) = machine.Start(new Data(5));
        var (state2, _, _) = machine.Start(new Data(15));

        Assert.Equal(State.Ready, state1);
        Assert.Equal(State.Done, state2);
    }

    #endregion

    #region Guard Overloads - TransitionConfiguration (non-generic)

    [Fact]
    public void Guard_Transition_FuncDataTriggerBool_EvaluatesGuard()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .OnUnhandled().Ignore()
            .For(State.Ready)
                .On(Trigger.Go)
                    .Guard((Data data, Trigger trigger) => data.Value > 10)
                    .TransitionTo(State.Done)
            .For(State.Done)
            .Build();

        var (state1, _, _) = machine.Fire(Trigger.Go, State.Ready, new Data(5));
        var (state2, _, _) = machine.Fire(Trigger.Go, State.Ready, new Data(15));

        Assert.Equal(State.Ready, state1);
        Assert.Equal(State.Done, state2);
    }

    [Fact]
    public void Guard_Transition_FuncDataBool_EvaluatesGuard()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Go)
                    .Guard((Data data) => data.Value > 10)
                    .TransitionTo(State.Done)
            .For(State.Done)
            .Build();

        var (state, _, _) = machine.Fire(Trigger.Go, State.Ready, new Data(15));

        Assert.Equal(State.Done, state);
    }

    [Fact]
    public void Guard_Transition_LabelFuncDataBool_EvaluatesGuard()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Go)
                    .Guard("CheckValue", (Data data) => data.Value > 10)
                    .TransitionTo(State.Done)
            .For(State.Done)
            .Build();

        var (state, _, _) = machine.Fire(Trigger.Go, State.Ready, new Data(15));

        Assert.Equal(State.Done, state);
    }

    [Fact]
    public void Guard_Transition_FuncStateDataBool_EvaluatesGuard()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Go)
                    .Guard((State state, Data data) => state == State.Ready && data.Value > 10)
                    .TransitionTo(State.Done)
            .For(State.Done)
            .Build();

        var (state, _, _) = machine.Fire(Trigger.Go, State.Ready, new Data(15));

        Assert.Equal(State.Done, state);
    }

    [Fact]
    public void Guard_Transition_LabelFuncStateDataBool_EvaluatesGuard()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Go)
                    .Guard("StateCheck", (State state, Data data) => state == State.Ready)
                    .TransitionTo(State.Done)
            .For(State.Done)
            .Build();

        var (state, _, _) = machine.Fire(Trigger.Go, State.Ready, Data.Initial);

        Assert.Equal(State.Done, state);
    }

    [Fact]
    public void Guard_Transition_LabelFuncStateDataTriggerBool_EvaluatesGuard()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Go)
                    .Guard("FullCheck", (State state, Data data, Trigger trigger) => state == State.Ready)
                    .TransitionTo(State.Done)
            .For(State.Done)
            .Build();

        var (state, _, _) = machine.Fire(Trigger.Go, State.Ready, Data.Initial);

        Assert.Equal(State.Done, state);
    }

    #endregion

    #region Guard Overloads - TransitionConfiguration<TDerivedTrigger>

    [Fact]
    public void Guard_GenericTransition_FuncDataDerivedTriggerBool_EvaluatesGuard()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .OnUnhandled().Ignore()
            .For(State.Ready)
                .On<Trigger.AmountTrigger>()
                    .Guard((Data data, Trigger.AmountTrigger trigger) => trigger.Amount > 10)
                    .TransitionTo(State.Done)
            .For(State.Done)
            .Build();

        var (state1, _, _) = machine.Fire(new Trigger.AmountTrigger(5), State.Ready, Data.Initial);
        var (state2, _, _) = machine.Fire(new Trigger.AmountTrigger(15), State.Ready, Data.Initial);

        Assert.Equal(State.Ready, state1);
        Assert.Equal(State.Done, state2);
    }

    [Fact]
    public void Guard_GenericTransition_LabelFuncStateDataDerivedTriggerBool_EvaluatesGuard()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On<Trigger.AmountTrigger>()
                    .Guard("AmountCheck", (State state, Data data, Trigger.AmountTrigger trigger) => trigger.Amount > 10)
                    .TransitionTo(State.Done)
            .For(State.Done)
            .Build();

        var (state, _, _) = machine.Fire(new Trigger.AmountTrigger(15), State.Ready, Data.Initial);

        Assert.Equal(State.Done, state);
    }

    [Fact]
    public void Guard_GenericTransition_FuncDataBool_EvaluatesGuard()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On<Trigger.AmountTrigger>()
                    .Guard((Data data) => data.Value > 10)
                    .TransitionTo(State.Done)
            .For(State.Done)
            .Build();

        var (state, _, _) = machine.Fire(new Trigger.AmountTrigger(1), State.Ready, new Data(15));

        Assert.Equal(State.Done, state);
    }

    #endregion

    #region Guard Overloads - NoData TransitionConfiguration

    [Fact]
    public void Guard_NoDataTransition_FuncStateBool_EvaluatesGuard()
    {
        var machine = StateMachine<State, Trigger, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Go)
                    .Guard((State state) => state == State.Ready)
                    .TransitionTo(State.Done)
                    .Done()
            .For(State.Done)
            .Build();

        var (state, _) = machine.Fire(Trigger.Go, State.Ready);

        Assert.Equal(State.Done, state);
    }

    [Fact]
    public void Guard_NoDataTransition_LabelFuncStateBool_EvaluatesGuard()
    {
        var machine = StateMachine<State, Trigger, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Go)
                    .Guard("StateCheck", (State state) => state == State.Ready)
                    .TransitionTo(State.Done)
                    .Done()
            .For(State.Done)
            .Build();

        var (state, _) = machine.Fire(Trigger.Go, State.Ready);

        Assert.Equal(State.Done, state);
    }

    [Fact]
    public void Guard_NoDataTransition_LabelFuncStateTriggerBool_EvaluatesGuard()
    {
        var machine = StateMachine<State, Trigger, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Go)
                    .Guard("FullCheck", (State state, Trigger trigger) => state == State.Ready && trigger == Trigger.Go)
                    .TransitionTo(State.Done)
                    .Done()
            .For(State.Done)
            .Build();

        var (state, _) = machine.Fire(Trigger.Go, State.Ready);

        Assert.Equal(State.Done, state);
    }

    [Fact]
    public void Guard_NoDataGenericTransition_LabelFuncStateDerivedTriggerBool_EvaluatesGuard()
    {
        var machine = StateMachine<State, Trigger, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On<Trigger.AmountTrigger>()
                    .Guard("AmountCheck", (State state, Trigger.AmountTrigger trigger) => trigger.Amount > 10)
                    .TransitionTo(State.Done)
                    .Done()
            .For(State.Done)
            .Build();

        var (state, _) = machine.Fire(new Trigger.AmountTrigger(15), State.Ready);

        Assert.Equal(State.Done, state);
    }

    #endregion

    #region If Overloads

    [Fact]
    public void If_Transition_FuncDataTriggerBool_EvaluatesPredicate()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Go)
                    .If((Data data, Trigger trigger) => data.Value > 10)
                        .TransitionTo(State.Done)
                        .Done()
            .For(State.Done)
            .Build();

        var (state, _, _) = machine.Fire(Trigger.Go, State.Ready, new Data(15));

        Assert.Equal(State.Done, state);
    }

    [Fact]
    public void If_GenericTransition_FuncDataDerivedTriggerBool_EvaluatesPredicate()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On<Trigger.AmountTrigger>()
                    .If((Data data, Trigger.AmountTrigger trigger) => trigger.Amount > 10)
                        .TransitionTo(State.Done)
                        .Done()
            .For(State.Done)
            .Build();

        var (state, _, _) = machine.Fire(new Trigger.AmountTrigger(15), State.Ready, Data.Initial);

        Assert.Equal(State.Done, state);
    }

    [Fact]
    public void If_GenericTransition_FuncDataBool_EvaluatesPredicate()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On<Trigger.AmountTrigger>()
                    .If((Data data) => data.Value > 10)
                        .TransitionTo(State.Done)
                        .Done()
            .For(State.Done)
            .Build();

        var (state, _, _) = machine.Fire(new Trigger.AmountTrigger(1), State.Ready, new Data(15));

        Assert.Equal(State.Done, state);
    }

    [Fact]
    public void If_NoDataTransition_FuncTriggerBool_EvaluatesPredicate()
    {
        var machine = StateMachine<State, Trigger, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Go)
                    .If((Trigger trigger) => true)
                        .TransitionTo(State.Done)
                        .Done()
                    .Done()
            .For(State.Done)
            .Build();

        var (state, _) = machine.Fire(Trigger.Go, State.Ready);

        Assert.Equal(State.Done, state);
    }

    #endregion

    #region ElseIf Overloads

    [Fact]
    public void ElseIf_GenericConditional_FuncDataDerivedTriggerBool_EvaluatesPredicate()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On<Trigger.AmountTrigger>()
                    .If((data, trigger) => trigger.Amount > 100)
                        .Execute(() => new Command("high"))
                        .ElseIf((Data data, Trigger.AmountTrigger trigger) => trigger.Amount > 50)
                        .Execute(() => new Command("medium"))
                        .Done()
            .Build();

        var (_, _, commands) = machine.Fire(new Trigger.AmountTrigger(75), State.Ready, Data.Initial);

        Assert.Single(commands);
        Assert.Equal("medium", commands[0].Message);
    }

    [Fact]
    public void ElseIf_GenericConditional_FuncDataBool_EvaluatesPredicate()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On<Trigger.AmountTrigger>()
                    .If((data, trigger) => trigger.Amount > 100)
                        .Execute(() => new Command("high"))
                        .ElseIf((Data data) => data.Value > 50)
                        .Execute(() => new Command("dataHigh"))
                        .Done()
            .Build();

        var (_, _, commands) = machine.Fire(new Trigger.AmountTrigger(1), State.Ready, new Data(75));

        Assert.Single(commands);
        Assert.Equal("dataHigh", commands[0].Message);
    }

    #endregion

    #region ModifyData Overloads

    [Fact]
    public void ModifyData_Immediate_FuncDataData_UpdatesData()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .Immediately()
                    .ModifyData((Data data) => data with { Value = data.Value + 10 })
                    .TransitionTo(State.Done)
                    .Done()
            .For(State.Done)
            .Build();

        var (_, data, _) = machine.Start(new Data(5));

        Assert.Equal(15, data.Value);
    }

    [Fact]
    public void ModifyData_Transition_FuncDataData_UpdatesData()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Go)
                    .ModifyData((Data data) => data with { Value = data.Value * 2 })
            .Build();

        var (_, data, _) = machine.Fire(Trigger.Go, State.Ready, new Data(7));

        Assert.Equal(14, data.Value);
    }

    [Fact]
    public void ModifyData_GenericTransition_FuncDataDerivedTriggerData_UpdatesData()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On<Trigger.AmountTrigger>()
                    .ModifyData((Data data, Trigger.AmountTrigger trigger) => data with { Value = data.Value + trigger.Amount })
            .Build();

        var (_, data, _) = machine.Fire(new Trigger.AmountTrigger(10), State.Ready, new Data(5));

        Assert.Equal(15, data.Value);
    }

    [Fact]
    public void ModifyData_GenericTransition_FuncDataData_UpdatesData()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On<Trigger.AmountTrigger>()
                    .ModifyData((Data data) => data with { Value = data.Value * 3 })
            .Build();

        var (_, data, _) = machine.Fire(new Trigger.AmountTrigger(1), State.Ready, new Data(4));

        Assert.Equal(12, data.Value);
    }

    [Fact]
    public void ModifyData_GenericConditional_FuncDataDerivedTriggerData_UpdatesData()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On<Trigger.AmountTrigger>()
                    .If((data, trigger) => trigger.Amount > 0)
                        .ModifyData((Data data, Trigger.AmountTrigger trigger) => data with { Value = trigger.Amount })
                        .Done()
            .Build();

        var (_, data, _) = machine.Fire(new Trigger.AmountTrigger(25), State.Ready, new Data(0));

        Assert.Equal(25, data.Value);
    }

    [Fact]
    public void ModifyData_GenericConditional_FuncDataData_UpdatesData()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On<Trigger.AmountTrigger>()
                    .If((data, trigger) => true)
                        .ModifyData((Data data) => data with { Value = 99 })
                        .Done()
            .Build();

        var (_, data, _) = machine.Fire(new Trigger.AmountTrigger(1), State.Ready, new Data(0));

        Assert.Equal(99, data.Value);
    }

    #endregion

    #region Execute Overloads - ImmediateTransitionConfiguration

    [Fact]
    public void Execute_Immediate_FuncIEnumerableCommand_ReturnsCommands()
    {
        var machine = StateMachine<State, Trigger, Data, Command>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .Immediately()
                    .Execute(() => new[] { new Command("a"), new Command("b") })
                    .TransitionTo(State.Done)
                    .Done()
            .For(State.Done)
            .Build();

        var (state, _, commands) = machine.Start(Data.Initial);

        Assert.Equal(State.Done, state);
        Assert.Equal(2, commands.Count);
    }

    #endregion

    #region Test Types

    private enum State
    {
        Ready,
        Done
    }

    private abstract record Trigger
    {
        public sealed record GoTrigger : Trigger;
        public sealed record AmountTrigger(int Amount) : Trigger;

        public static readonly Trigger Go = new GoTrigger();
    }

    private sealed record Data(int Value)
    {
        public static Data Initial => new(0);
    }

    private sealed record Command(string Message);

    #endregion
}
