namespace FunctionalStateMachine.CommandRunner;

// ReSharper disable once TypeParameterCanBeVariant
public interface IAsyncCommandRunner<TCommand>
{
    Task RunAsync(TCommand command);
}