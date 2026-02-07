namespace FunctionalStateMachine.Core.Tests;

public class StateMachineNoDataConditionalTests
{
    [Fact]
    public void IfElseIfElse_NoData_SelectsExpectedBranch()
    {
        var machine = StateMachine<State, Trigger, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Step)
                    .If((State state, Trigger trigger) => state == State.Ready && trigger == Trigger.Step)
                        .Execute((State state, Trigger trigger) => new CommandBase[]
                        {
                            new Command("If")
                        })
                        .ElseIf((State state, Trigger trigger) => state == State.Ready)
                        .Execute((State state, Trigger trigger) => new CommandBase[]
                        {
                            new Command("ElseIf")
                        })
                        .Else()
                        .Execute((State state, Trigger trigger) => new CommandBase[]
                        {
                            new Command("Else")
                        })
                        .Done()
                    .Done()
            .Build();

        var (state, commands) = machine.Fire(Trigger.Step, State.Ready);

        Assert.Equal(State.Ready, state);
        Assert.Single(commands);
        Assert.Equal("If", ((Command)commands[0]).Message);
    }

    [Fact]
    public void IfElseIfElse_DerivedTrigger_NoData_SelectsExpectedBranch()
    {
        var machine = StateMachine<State, Trigger, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On<Trigger.DerivedTrigger>()
                    .If((State state, Trigger.DerivedTrigger trigger) => trigger.Amount > 10)
                        .Execute((State state, Trigger.DerivedTrigger trigger) => new CommandBase[]
                        {
                            new Command("If")
                        })
                        .ElseIf((State state, Trigger.DerivedTrigger trigger) => trigger.Amount > 5)
                        .Execute((State state, Trigger.DerivedTrigger trigger) => new CommandBase[]
                        {
                            new Command("ElseIf")
                        })
                        .Else()
                        .Execute((State state, Trigger.DerivedTrigger trigger) => new CommandBase[]
                        {
                            new Command("Else")
                        })
                        .Done()
                    .Done()
            .Build();

        var (state, commands) = machine.Fire(new Trigger.DerivedTrigger(7), State.Ready);

        Assert.Equal(State.Ready, state);
        Assert.Single(commands);
        Assert.Equal("ElseIf", ((Command)commands[0]).Message);
    }

    [Fact]
    public void If_WithTriggerPredicate_NoData_EvaluatesTrigger()
    {
        var machine = StateMachine<State, Trigger, CommandBase>.Create()
            .StartWith(State.Ready)
            .For(State.Ready)
                .On(Trigger.Step)
                    .If((Trigger trigger) => trigger == Trigger.Step)
                        .Execute((State state, Trigger trigger) => new CommandBase[]
                        {
                            new Command("Match")
                        })
                        .Else()
                        .Execute((State state, Trigger trigger) => new CommandBase[]
                        {
                            new Command("NoMatch")
                        })
                        .Done()
                    .Done()
            .Build();

        var (_, commands) = machine.Fire(Trigger.Step, State.Ready);

        Assert.Single(commands);
        Assert.Equal("Match", ((Command)commands[0]).Message);
    }

    private enum State
    {
        Ready,
        Done
    }

    private abstract record Trigger
    {
        public sealed record StepTrigger : Trigger;
        public sealed record DerivedTrigger(int Amount) : Trigger;

        public static readonly Trigger Step = new StepTrigger();
    }

    private abstract record CommandBase;

    private sealed record Command(string Message) : CommandBase;
}
