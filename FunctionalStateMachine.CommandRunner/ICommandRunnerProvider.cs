namespace FunctionalStateMachine.CommandRunner;

public interface ICommandDispatcher<TCommand>
{
    void Run(TCommand command);
    void Run(IEnumerable<TCommand> commands);
}
