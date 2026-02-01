namespace FunctionalStateMachine.CommandRunner;

public interface IAsyncCommandRunnerProvider<TCommand>
{
    Task RunAsync(TCommand command);
    Task RunAsync(IEnumerable<TCommand> commands);
}
