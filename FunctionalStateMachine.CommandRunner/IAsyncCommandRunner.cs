namespace FunctionalStateMachine.CommandRunner;

public interface IAsyncCommandRunner<TCommand>
{
    Task RunAsync(TCommand command);
}