using FunctionalStateMachine.CommandRunner;
using Microsoft.Extensions.DependencyInjection;

namespace FunctionalStateMachine.CommandRunnerTests.Missing_Runner;

public class MissingCommandRunnerTests
{
    [Fact]
    public void GivenMissingCommandRunner_WhenRegistered_ThenRegistrationFails()
    {
        var services = new ServiceCollection();

        Assert.Throws<InvalidOperationException>(() => services.AddCommandRunners<MissingCommand>());
    }

    [Fact]
    public void GivenMissingCommandRunner_WhenNoOpConfigured_ThenMissingCommandsAreIgnored()
    {
        var serviceProvider = new ServiceCollection()
            .AddCommandRunners<MissingCommand>(new CommandRunnerOptions
            {
                MissingBehavior = CommandRunnerMissingBehavior.NoOp
            })
            .AddSingleton<MissingCallTracker>()
            .BuildServiceProvider();

        var syncCommandRunner = serviceProvider.GetRequiredService<ICommandRunnerProvider<MissingCommand>>();
        syncCommandRunner.Run(new MissingCommand.HasRunner());
        syncCommandRunner.Run(new MissingCommand.MissingRunner());

        var callTracker = serviceProvider.GetRequiredService<MissingCallTracker>();
        Assert.Equal(1, callTracker.Invocations);
    }
}