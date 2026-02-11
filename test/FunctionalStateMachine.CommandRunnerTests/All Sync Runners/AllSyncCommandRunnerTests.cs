using FunctionalStateMachine.CommandRunner;
using Microsoft.Extensions.DependencyInjection;

namespace FunctionalStateMachine.CommandRunnerTests.All_Sync_Runners;

public class AllSyncCommandRunnerTests
{
    [Fact]
    public void GivenOnlySyncCommandRunners_WhenCommandsAreFired_ThenSyncProviderIsUsed()
    {
        var serviceProvider = new ServiceCollection()
            .AddCommandRunners<SyncCommand>()
            .AddSingleton<SyncCallTracker>()
            .BuildServiceProvider();

        var syncCommandRunner = serviceProvider.GetRequiredService<ICommandDispatcher<SyncCommand>>();
        syncCommandRunner.Run(new SyncCommand.Alpha());
        syncCommandRunner.Run([new SyncCommand.Beta()]);

        Assert.Null(serviceProvider.GetService<IAsyncCommandDispatcher<SyncCommand>>());

        var callTracker = serviceProvider.GetRequiredService<SyncCallTracker>();
        Assert.Equal(1, callTracker.AlphaRunnerInvocations);
        Assert.Equal(1, callTracker.BetaRunnerInvocations);
    }
}