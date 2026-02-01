namespace FunctionalStateMachine.CommandRunner;

public interface ICommandRunner<TCommand>
{
    void Run(TCommand command);
}