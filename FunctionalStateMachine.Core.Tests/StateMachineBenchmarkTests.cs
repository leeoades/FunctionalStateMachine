using System.Diagnostics;
using Xunit.Abstractions;

namespace FunctionalStateMachine.Core.Tests;

public class StateMachineBenchmarkTests(ITestOutputHelper output)
{
    [Fact]
    public void Benchmark_StateMachineDeclarationAndFire()
    {
        const int declarationIterations = 1000;
        const int fireIterations = 10000;

        var declarationWatch = Stopwatch.StartNew();
        StateMachine<State, Trigger, Data, CommandBase>? machine = null;
        for (int i = 0; i < declarationIterations; i++)
        {
            machine = StateMachine<State, Trigger, Data, CommandBase>.Create()
                .StartWith(State.Ready)
                .For(State.Ready)
                    .On(Trigger.Go)
                        .TransitionTo(State.Done)
                        .Execute(() => new LogCommand("Done"))
                .For(State.Done)
                    .On(Trigger.Reset)
                        .TransitionTo(State.Ready)
                .Build();
        }
        declarationWatch.Stop();

        var fireState = State.Ready;
        var fireData = new Data(0);
        var fireWatch = Stopwatch.StartNew();
        for (int i = 0; i < fireIterations; i++)
        {
            (fireState, fireData, _) = machine!.Fire(Trigger.Go, fireState, fireData);
            (fireState, fireData, _) = machine.Fire(Trigger.Reset, fireState, fireData);
        }
        fireWatch.Stop();

        var declarationAvg = declarationWatch.Elapsed.TotalMilliseconds / declarationIterations;
        var fireTotalIterations = fireIterations * 2;
        var fireAvg = fireWatch.Elapsed.TotalMilliseconds / fireTotalIterations;

        output.WriteLine($"Declaration iterations: {declarationIterations}");
        output.WriteLine($"Total declaration time: {declarationWatch.ElapsedMilliseconds} ms");
        output.WriteLine($"Avg declaration time: {declarationAvg:F4} ms");
        output.WriteLine($"Fire iterations: {fireTotalIterations}");
        output.WriteLine($"Total fire time: {fireWatch.ElapsedMilliseconds} ms");
        output.WriteLine($"Avg fire time: {fireAvg:F6} ms");
        Console.WriteLine($"Benchmark declaration avg (ms): {declarationAvg:F4}");
        Console.WriteLine($"Benchmark fire avg (ms): {fireAvg:F6}");

        Assert.True(declarationWatch.ElapsedMilliseconds >= 0);
    }

    private enum State
    {
        Ready,
        Done
    }

    private abstract record Trigger
    {
        public sealed record GoTrigger : Trigger;
        public sealed record ResetTrigger : Trigger;

        public static readonly Trigger Go = new GoTrigger();
        public static readonly Trigger Reset = new ResetTrigger();
    }

    private sealed record Data(int Count)
    {
        public static Data Initial => new(0);
    }

    private abstract record CommandBase;

    private sealed record LogCommand(string Message) : CommandBase;
}
