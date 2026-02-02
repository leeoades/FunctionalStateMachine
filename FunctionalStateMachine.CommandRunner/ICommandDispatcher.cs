namespace FunctionalStateMachine.CommandRunner;

// ReSharper disable once TypeParameterCanBeVariant
public interface ICommandDispatcher<TCommand>
{
    void Run(TCommand command);
    void Run(IEnumerable<TCommand> commands);
}
