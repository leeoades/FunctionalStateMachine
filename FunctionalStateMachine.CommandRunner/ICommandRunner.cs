namespace FunctionalStateMachine.CommandRunner;

// ReSharper disable once TypeParameterCanBeVariant
public interface ICommandRunner<TCommand>
{
    void Run(TCommand command);
}