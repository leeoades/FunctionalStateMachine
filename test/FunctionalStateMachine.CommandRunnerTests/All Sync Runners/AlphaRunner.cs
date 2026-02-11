using FunctionalStateMachine.CommandRunner;

namespace FunctionalStateMachine.CommandRunnerTests.All_Sync_Runners;

public class AlphaRunner(SyncCallTracker callTracker) : ICommandRunner<SyncCommand.Alpha>
{
    public void Run(SyncCommand.Alpha command)
    {
        callTracker.AlphaRunnerInvocations++;
    }
}