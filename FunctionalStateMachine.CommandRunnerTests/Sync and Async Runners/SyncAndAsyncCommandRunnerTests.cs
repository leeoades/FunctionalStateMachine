using FunctionalStateMachine.CommandRunner;
using Microsoft.Extensions.DependencyInjection;

namespace FunctionalStateMachine.CommandRunnerTests.Sync_and_Async_Runners;

public class SyncAndAsyncCommandRunnerTests
{
    [Fact]
    public async Task GivenImplementedCommandRunners_WhenCommandsAreFired_ThenRunnersAreInvoked()
    {
        var serviceProvider = new ServiceCollection()
            .AddCommandRunners<TestCommand>()
            .AddSingleton<CallTracker>()
            .BuildServiceProvider();

        var asyncCommandRunner = serviceProvider.GetRequiredService<IAsyncCommandRunnerProvider<TestCommand>>();
        await asyncCommandRunner.RunAsync(new TestCommand.Foo());
        await asyncCommandRunner.RunAsync([new TestCommand.Bar()]);

        var callTracker = serviceProvider.GetRequiredService<CallTracker>();
        Assert.Equal(1, callTracker.FooRunnerInvocations);
        Assert.Equal(1, callTracker.BarRunnerInvocations);
    }
}