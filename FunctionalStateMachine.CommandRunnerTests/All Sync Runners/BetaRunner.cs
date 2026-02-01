using FunctionalStateMachine.CommandRunner;

namespace FunctionalStateMachine.CommandRunnerTests.All_Sync_Runners;

public class BetaRunner(SyncCallTracker callTracker) : ICommandRunner<SyncCommand.Beta>
{
    public void Run(SyncCommand.Beta command)
    {
        callTracker.BetaRunnerInvocations++;
    }
}