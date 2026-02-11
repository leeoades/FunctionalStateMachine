namespace FunctionalStateMachine.CommandRunner;

// ReSharper disable once TypeParameterCanBeVariant
public interface IAsyncCommandDispatcher<TCommand>
{
    Task RunAsync(TCommand command);
    Task RunAsync(IEnumerable<TCommand> commands);
}
