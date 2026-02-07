namespace FunctionalStateMachine.Core.Tests;

public class StateMachineConditionalTests
{
    [Fact]
    public void If_TrueBranchExecutesStepsInOrder()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On<Trigger.PayTrigger>()
                    .If((data, trigger) => trigger.Amount >= 10m)
                        .ModifyData((data, trigger) => data with { Total = data.Total + trigger.Amount })
                        .Execute(data => new LogCommand($"Paid:{data.Total}"))
                        .Done()
            .Build();
        var currentState = State.Ready;
        var currentData = new Data(0m);

        var (_, _, commands) = machine.Fire(new Trigger.PayTrigger(12m), currentState, currentData);

        Assert.Single(commands);
        Assert.Equal("Paid:12", ((LogCommand)commands[0]).Message);
    }

    [Fact]
    public void If_ElseBranchExecutesWhenPredicateIsFalse()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On<Trigger.PayTrigger>()
                    .If((data, trigger) => trigger.Amount >= 10m)
                        .Execute(() => new LogCommand("Accepted"))
                        .Else()
                        .Execute((data, trigger) => new LogCommand($"Need:{10m - trigger.Amount}"))
                        .Done()
            .Build();
        var currentState = State.Ready;
        var currentData = new Data(0m);

        var (_, _, commands) = machine.Fire(new Trigger.PayTrigger(7m), currentState, currentData);

        Assert.Single(commands);
        Assert.Equal("Need:3", ((LogCommand)commands[0]).Message);
    }

    [Fact]
    public void ElseIf_FirstConditionMatches()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On<Trigger.PayTrigger>()
                    .If((data, trigger) => trigger.Amount >= 20m)
                        .Execute(() => new LogCommand("Premium"))
                        .ElseIf((data, trigger) => trigger.Amount >= 10m)
                        .Execute(() => new LogCommand("Standard"))
                        .ElseIf((data, trigger) => trigger.Amount >= 5m)
                        .Execute(() => new LogCommand("Basic"))
                        .Else()
                        .Execute(() => new LogCommand("Minimum"))
                        .Done()
            .Build();
        var currentState = State.Ready;
        var currentData = new Data(0m);

        var (_, _, commands) = machine.Fire(new Trigger.PayTrigger(25m), currentState, currentData);

        Assert.Single(commands);
        Assert.Equal("Premium", ((LogCommand)commands[0]).Message);
    }

    [Fact]
    public void ElseIf_SecondConditionMatches()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On<Trigger.PayTrigger>()
                    .If((data, trigger) => trigger.Amount >= 20m)
                        .Execute(() => new LogCommand("Premium"))
                        .ElseIf((data, trigger) => trigger.Amount >= 10m)
                        .Execute(() => new LogCommand("Standard"))
                        .ElseIf((data, trigger) => trigger.Amount >= 5m)
                        .Execute(() => new LogCommand("Basic"))
                        .Else()
                        .Execute(() => new LogCommand("Minimum"))
                        .Done()
            .Build();
        var currentState = State.Ready;
        var currentData = new Data(0m);

        var (_, _, commands) = machine.Fire(new Trigger.PayTrigger(15m), currentState, currentData);

        Assert.Single(commands);
        Assert.Equal("Standard", ((LogCommand)commands[0]).Message);
    }

    [Fact]
    public void ElseIf_ThirdConditionMatches()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On<Trigger.PayTrigger>()
                    .If((data, trigger) => trigger.Amount >= 20m)
                        .Execute(() => new LogCommand("Premium"))
                        .ElseIf((data, trigger) => trigger.Amount >= 10m)
                        .Execute(() => new LogCommand("Standard"))
                        .ElseIf((data, trigger) => trigger.Amount >= 5m)
                        .Execute(() => new LogCommand("Basic"))
                        .Else()
                        .Execute(() => new LogCommand("Minimum"))
                        .Done()
            .Build();
        var currentState = State.Ready;
        var currentData = new Data(0m);

        var (_, _, commands) = machine.Fire(new Trigger.PayTrigger(7m), currentState, currentData);

        Assert.Single(commands);
        Assert.Equal("Basic", ((LogCommand)commands[0]).Message);
    }

    [Fact]
    public void ElseIf_ElseExecutesWhenNoConditionMatches()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On<Trigger.PayTrigger>()
                    .If((data, trigger) => trigger.Amount >= 20m)
                        .Execute(() => new LogCommand("Premium"))
                        .ElseIf((data, trigger) => trigger.Amount >= 10m)
                        .Execute(() => new LogCommand("Standard"))
                        .ElseIf((data, trigger) => trigger.Amount >= 5m)
                        .Execute(() => new LogCommand("Basic"))
                        .Else()
                        .Execute(() => new LogCommand("Minimum"))
                        .Done()
            .Build();
        var currentState = State.Ready;
        var currentData = new Data(0m);

        var (_, _, commands) = machine.Fire(new Trigger.PayTrigger(2m), currentState, currentData);

        Assert.Single(commands);
        Assert.Equal("Minimum", ((LogCommand)commands[0]).Message);
    }

    [Fact]
    public void ElseIf_WithModifyData()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On<Trigger.PayTrigger>()
                    .If((data, trigger) => trigger.Amount >= 20m)
                        .ModifyData(data => data with { Total = data.Total + 100 })
                        .Execute(data => new LogCommand($"Total:{data.Total}"))
                        .ElseIf((data, trigger) => trigger.Amount >= 10m)
                        .ModifyData(data => data with { Total = data.Total + 50 })
                        .Execute(data => new LogCommand($"Total:{data.Total}"))
                        .Else()
                        .ModifyData(data => data with { Total = data.Total + 10 })
                        .Execute(data => new LogCommand($"Total:{data.Total}"))
                        .Done()
            .Build();
        var currentState = State.Ready;
        var currentData = new Data(0m);

        var (_, newData, commands) = machine.Fire(new Trigger.PayTrigger(15m), currentState, currentData);

        Assert.Single(commands);
        Assert.Equal("Total:50", ((LogCommand)commands[0]).Message);
        Assert.Equal(50m, newData.Total);
    }

    [Fact]
    public void ElseIf_OnSpecificTriggerType()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On<Trigger.PayTrigger>()
                    .If((data, trigger) => trigger.Amount >= 10m)
                        .Execute(() => new LogCommand("Large"))
                        .ElseIf((data, trigger) => trigger.Amount >= 5m)
                        .Execute(() => new LogCommand("Medium"))
                        .Else()
                        .Execute(() => new LogCommand("Small"))
                        .Done()
            .Build();
        var currentState = State.Ready;
        var currentData = new Data(0m);

        var result1 = machine.Fire(new Trigger.PayTrigger(3m), currentState, currentData);
        var result2 = machine.Fire(new Trigger.PayTrigger(7m), currentState, currentData);
        var result3 = machine.Fire(new Trigger.PayTrigger(15m), currentState, currentData);

        Assert.Equal("Small", ((LogCommand)result1.Commands[0]).Message);
        Assert.Equal("Medium", ((LogCommand)result2.Commands[0]).Message);
        Assert.Equal("Large", ((LogCommand)result3.Commands[0]).Message);
    }

    [Fact]
    public void ElseIf_StopsAtFirstMatch()
    {
        var executionLog = new List<string>();
        
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .For(State.Ready)
                .On<Trigger.PayTrigger>()
                    .If((data, trigger) => { executionLog.Add("If"); return trigger.Amount >= 10m; })
                        .Execute(() => new LogCommand("First"))
                        .ElseIf((data, trigger) => { executionLog.Add("ElseIf1"); return trigger.Amount >= 5m; })
                        .Execute(() => new LogCommand("Second"))
                        .ElseIf((data, trigger) => { executionLog.Add("ElseIf2"); return trigger.Amount >= 1m; })
                        .Execute(() => new LogCommand("Third"))
                        .Done()
            .Build();
        var currentState = State.Ready;
        var currentData = new Data(0m);

        executionLog.Clear();
        var (_, _, commands) = machine.Fire(new Trigger.PayTrigger(7m), currentState, currentData);

        Assert.Equal("Second", ((LogCommand)commands[0]).Message);
        Assert.Contains("If", executionLog);
        Assert.Contains("ElseIf1", executionLog);
        Assert.DoesNotContain("ElseIf2", executionLog);
    }

    [Fact]
    public void If_ConditionalTransitionToRequiresSingleTarget()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
                .StartWith(State.Ready)
                .For(State.Ready)
                    .On<Trigger.PayTrigger>()
                        .If((data, trigger) => trigger.Amount >= 10m)
                            .TransitionTo(State.Approved)
                            .TransitionTo(State.Ready)
                            .Done()
                .For(State.Approved)
                    .On<Trigger.PayTrigger>()
                        .Ignore()
                .Build();
        });

        Assert.Contains("TransitionTo", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void If_AllowsSingleConditionalTransitionTo()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On<Trigger.PayTrigger>()
                    .If((data, trigger) => trigger.Amount >= 10m)
                        .TransitionTo(State.Approved)
                        .Else()
                        .Execute(() => new LogCommand("Declined"))
                        .Done()
            .For(State.Approved)
                .On<Trigger.PayTrigger>()
                    .Ignore()
            .Build();

        var (newState, _, _) = machine.Fire(new Trigger.PayTrigger(4m), State.Ready, new Data(0m));

        Assert.Equal(State.Ready, newState);
    }

    #region Non-Generic ConditionalTransitionConfiguration Tests

    [Fact]
    public void If_NonGeneric_ModifyData_UpdatesDataInBranch()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Go)
                    .If((state, data, trigger) => data.Total < 50m)
                        .ModifyData((state, data, trigger) => data with { Total = data.Total + 25m })
                        .Execute((state, data, trigger) => new LogCommand($"Total:{data.Total}"))
                        .Done()
                    .Done()
            .Build();

        var (_, newData, commands) = machine.Fire(Trigger.Go, State.Ready, new Data(10m));

        Assert.Equal(35m, newData.Total);
        Assert.Single(commands);
        Assert.Equal("Total:35", ((LogCommand)commands[0]).Message);
    }

    [Fact]
    public void If_NonGeneric_Else_ExecutesWhenPredicateFalse()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Go)
                    .If((state, data, trigger) => data.Total >= 100m)
                        .Execute((state, data, trigger) => new LogCommand("Rich"))
                        .Else()
                        .Execute((state, data, trigger) => new LogCommand("Poor"))
                        .Done()
                    .Done()
            .Build();

        var (_, _, commands) = machine.Fire(Trigger.Go, State.Ready, new Data(50m));

        Assert.Single(commands);
        Assert.Equal("Poor", ((LogCommand)commands[0]).Message);
    }

    [Fact]
    public void If_NonGeneric_ElseIf_EvaluatesMultipleConditions()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Go)
                    .If((state, data, trigger) => data.Total >= 100m)
                        .Execute((state, data, trigger) => new LogCommand("Gold"))
                        .ElseIf((state, data, trigger) => data.Total >= 50m)
                        .Execute((state, data, trigger) => new LogCommand("Silver"))
                        .ElseIf((state, data, trigger) => data.Total >= 25m)
                        .Execute((state, data, trigger) => new LogCommand("Bronze"))
                        .Else()
                        .Execute((state, data, trigger) => new LogCommand("None"))
                        .Done()
                    .Done()
            .Build();

        var result1 = machine.Fire(Trigger.Go, State.Ready, new Data(150m));
        var result2 = machine.Fire(Trigger.Go, State.Ready, new Data(75m));
        var result3 = machine.Fire(Trigger.Go, State.Ready, new Data(30m));
        var result4 = machine.Fire(Trigger.Go, State.Ready, new Data(10m));

        Assert.Equal("Gold", ((LogCommand)result1.Commands[0]).Message);
        Assert.Equal("Silver", ((LogCommand)result2.Commands[0]).Message);
        Assert.Equal("Bronze", ((LogCommand)result3.Commands[0]).Message);
        Assert.Equal("None", ((LogCommand)result4.Commands[0]).Message);
    }

    [Fact]
    public void If_NonGeneric_ElseIf_ModifyDataInEachBranch()
    {
        var machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Go)
                    .If((state, data, trigger) => data.Total >= 100m)
                        .ModifyData((state, data, trigger) => data with { Total = data.Total * 2 })
                        .ElseIf((state, data, trigger) => data.Total >= 50m)
                        .ModifyData((state, data, trigger) => data with { Total = data.Total * 1.5m })
                        .Else()
                        .ModifyData((state, data, trigger) => data with { Total = data.Total + 10m })
                        .Done()
                    .Done()
            .Build();

        var result1 = machine.Fire(Trigger.Go, State.Ready, new Data(100m));
        var result2 = machine.Fire(Trigger.Go, State.Ready, new Data(60m));
        var result3 = machine.Fire(Trigger.Go, State.Ready, new Data(20m));

        Assert.Equal(200m, result1.NewData.Total);
        Assert.Equal(90m, result2.NewData.Total);
        Assert.Equal(30m, result3.NewData.Total);
    }

    #endregion

    private enum State
    {
        Ready,
        Approved
    }

    private abstract record Trigger
    {
        public sealed record PayTrigger(decimal Amount) : Trigger;
        public sealed record GoTrigger : Trigger;

        public static readonly Trigger Go = new GoTrigger();
    }

    private sealed record Data(decimal Total)
    {
        public static Data Initial => new(0m);
    }

    private abstract record CommandBase;

    private sealed record LogCommand(string Message) : CommandBase;
}
