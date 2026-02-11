using FunctionalStateMachine.CommandRunner;

namespace FunctionalStateMachine.CommandRunnerTests.Missing_Runner;


public class HasRunnerHandler(MissingCallTracker callTracker) : ICommandRunner<MissingCommand.HasRunner>
{
    public void Run(MissingCommand.HasRunner command)
    {
        callTracker.Invocations++;
    }
}