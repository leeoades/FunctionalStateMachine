namespace FunctionalStateMachine.CommandRunner;

public interface IAsyncCommandDispatcher<TCommand>
{
    Task RunAsync(TCommand command);
    Task RunAsync(IEnumerable<TCommand> commands);
}
