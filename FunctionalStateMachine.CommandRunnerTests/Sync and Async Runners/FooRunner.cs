using FunctionalStateMachine.CommandRunner;

namespace FunctionalStateMachine.CommandRunnerTests.Sync_and_Async_Runners;

public class FooRunner(CallTracker callTracker) : ICommandRunner<TestCommand.Foo>
{
    public void Run(TestCommand.Foo command)
    {
        callTracker.FooRunnerInvocations++;
    }
}