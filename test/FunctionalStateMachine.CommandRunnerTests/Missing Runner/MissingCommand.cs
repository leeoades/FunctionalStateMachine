namespace FunctionalStateMachine.CommandRunnerTests.Missing_Runner;



public abstract record MissingCommand
{
    public record HasRunner : MissingCommand;
    public record MissingRunner : MissingCommand;
}