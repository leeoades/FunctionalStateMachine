using FunctionalStateMachine.CommandRunner;

namespace FunctionalStateMachine.CommandRunnerTests.Sync_and_Async_Runners;

public class AsyncBarRunner(CallTracker callTracker) : IAsyncCommandRunner<TestCommand.Bar>
{
    public Task RunAsync(TestCommand.Bar command)
    {
        callTracker.BarRunnerInvocations++;
        return Task.CompletedTask;
    }
}