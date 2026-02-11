namespace FunctionalStateMachine.CommandRunnerTests.Sync_and_Async_Runners;

public abstract record TestCommand
{
    public record Foo : TestCommand;
    public record Bar : TestCommand;
}