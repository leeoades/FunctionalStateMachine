using FunctionalStateMachine.CommandRunner;
using Microsoft.Extensions.DependencyInjection;

namespace FunctionalStateMachine.CommandRunnerTests;

public class CommandRunnerTests
{
    public abstract record TestCommand
    {
        public record Foo : TestCommand;
        public record Bar : TestCommand;
    }

    public class FooRunner(CallTracker callTracker) : ICommandRunner<TestCommand.Foo>
    {
        public void Run(TestCommand.Foo command)
        {
            callTracker.FooRunnerInvocations++;
        }
    }

    public class AsyncBarRunner(CallTracker callTracker) : IAsyncCommandRunner<TestCommand.Bar>
    {
        public Task RunAsync(TestCommand.Bar command)
        {
            callTracker.BarRunnerInvocations++;
            return Task.CompletedTask;
        }
    }

    public class CallTracker
    {
        public int FooRunnerInvocations { get; set; }
        public int BarRunnerInvocations { get; set; }
    }

    public abstract record SyncCommand
    {
        public record Alpha : SyncCommand;
        public record Beta : SyncCommand;
    }

    public class AlphaRunner(SyncCallTracker callTracker) : ICommandRunner<SyncCommand.Alpha>
    {
        public void Run(SyncCommand.Alpha command)
        {
            callTracker.AlphaRunnerInvocations++;
        }
    }

    public class BetaRunner(SyncCallTracker callTracker) : ICommandRunner<SyncCommand.Beta>
    {
        public void Run(SyncCommand.Beta command)
        {
            callTracker.BetaRunnerInvocations++;
        }
    }

    public class SyncCallTracker
    {
        public int AlphaRunnerInvocations { get; set; }
        public int BetaRunnerInvocations { get; set; }
    }

    public abstract record MissingCommand
    {
        public record HasRunner : MissingCommand;
        public record MissingRunner : MissingCommand;
    }

    public class HasRunnerHandler(MissingCallTracker callTracker) : ICommandRunner<MissingCommand.HasRunner>
    {
        public void Run(MissingCommand.HasRunner command)
        {
            callTracker.Invocations++;
        }
    }

    public class MissingCallTracker
    {
        public int Invocations { get; set; }
    }

    [Fact]
    public async Task GivenImplementedCommandRunners_WhenCommandsAreFired_ThenRunnersAreInvoked()
    {
        var serviceProvider = new ServiceCollection()
            .AddCommandRunners<TestCommand>()
            .AddSingleton<CallTracker>()
            .BuildServiceProvider();

        var asyncCommandRunner = serviceProvider.GetRequiredService<IAsyncCommandRunnerProvider<TestCommand>>();
        await asyncCommandRunner.RunAsync(new TestCommand.Foo());
        await asyncCommandRunner.RunAsync(new[] { new TestCommand.Bar() });

        var callTracker = serviceProvider.GetRequiredService<CallTracker>();
        Assert.Equal(1, callTracker.FooRunnerInvocations);
        Assert.Equal(1, callTracker.BarRunnerInvocations);
    }

    [Fact]
    public void GivenOnlySyncCommandRunners_WhenCommandsAreFired_ThenSyncProviderIsUsed()
    {
        var serviceProvider = new ServiceCollection()
            .AddCommandRunners<SyncCommand>()
            .AddSingleton<SyncCallTracker>()
            .BuildServiceProvider();

        var syncCommandRunner = serviceProvider.GetRequiredService<ICommandRunnerProvider<SyncCommand>>();
        syncCommandRunner.Run(new SyncCommand.Alpha());
        syncCommandRunner.Run(new[] { new SyncCommand.Beta() });

        Assert.Null(serviceProvider.GetService<IAsyncCommandRunnerProvider<SyncCommand>>());

        var callTracker = serviceProvider.GetRequiredService<SyncCallTracker>();
        Assert.Equal(1, callTracker.AlphaRunnerInvocations);
        Assert.Equal(1, callTracker.BetaRunnerInvocations);
    }

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
