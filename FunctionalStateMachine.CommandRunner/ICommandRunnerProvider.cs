namespace FunctionalStateMachine.CommandRunner;

public interface ICommandRunnerProvider<TCommand>
{
    void Run(TCommand command);
    void Run(IEnumerable<TCommand> commands);
}
