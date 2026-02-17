using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using FunctionalStateMachine.Core;

namespace FunctionalStateMachine.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<StateMachineBenchmarks>();
    }
}

public enum TestState
{
    Initial,
    Active,
    Processing,
    Complete,
    Failed
}

public abstract record TestTrigger
{
    public sealed record Start : TestTrigger;
    public sealed record Process : TestTrigger;
    public sealed record Complete : TestTrigger;
    public sealed record Fail : TestTrigger;
    public sealed record Reset : TestTrigger;
}

public sealed record TestData(int Counter, string Message);

public abstract record TestCommand
{
    public sealed record Log(string Message) : TestCommand;
    public sealed record Increment : TestCommand;
    public sealed record Reset : TestCommand;
}

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 10)]
public class StateMachineBenchmarks
{
    private StateMachine<TestState, TestTrigger, TestData, TestCommand> _simpleMachine = null!;
    private StateMachine<TestState, TestTrigger, TestData, TestCommand> _complexMachine = null!;
    private TestData _testData = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Simple state machine with basic transitions
        _simpleMachine = StateMachine<TestState, TestTrigger, TestData, TestCommand>.Create()
            .StartWith(TestState.Initial)
            .For(TestState.Initial)
                .On<TestTrigger.Start>()
                    .TransitionTo(TestState.Active)
            .For(TestState.Active)
                .On<TestTrigger.Complete>()
                    .TransitionTo(TestState.Complete)
            .For(TestState.Complete)
                .On<TestTrigger.Reset>()
                    .TransitionTo(TestState.Initial)
            .Build();

        // Complex state machine with guards, data modifications, and multiple commands
        _complexMachine = StateMachine<TestState, TestTrigger, TestData, TestCommand>.Create()
            .StartWith(TestState.Initial)
            .For(TestState.Initial)
                .OnEntry(() => new TestCommand.Log("Entered Initial"))
                .On<TestTrigger.Start>()
                    .Execute(() => new TestCommand.Log("Starting"))
                    .ModifyData(data => data with { Counter = data.Counter + 1 })
                    .TransitionTo(TestState.Active)
            .For(TestState.Active)
                .OnEntry(() => new TestCommand.Log("Entered Active"))
                .OnExit(() => new TestCommand.Log("Exiting Active"))
                .On<TestTrigger.Process>()
                    .Guard((data) => data.Counter < 100)
                    .Execute(() => new TestCommand.Increment())
                    .ModifyData(data => data with { Counter = data.Counter + 1 })
                    .TransitionTo(TestState.Processing)
                .On<TestTrigger.Process>()
                    .Execute(() => new TestCommand.Log("Counter limit reached"))
                    .TransitionTo(TestState.Failed)
            .For(TestState.Processing)
                .OnEntry(() => new TestCommand.Log("Processing"))
                .On<TestTrigger.Complete>()
                    .Execute(() => new TestCommand.Log("Completing"))
                    .Execute(() => new TestCommand.Log("Done"))
                    .TransitionTo(TestState.Complete)
                .On<TestTrigger.Fail>()
                    .Execute(() => new TestCommand.Log("Failed"))
                    .TransitionTo(TestState.Failed)
            .For(TestState.Complete)
                .OnEntry(() => new TestCommand.Log("Completed"))
                .On<TestTrigger.Reset>()
                    .Execute(() => new TestCommand.Reset())
                    .ModifyData(data => data with { Counter = 0, Message = "Reset" })
                    .TransitionTo(TestState.Initial)
            .For(TestState.Failed)
                .OnEntry(() => new TestCommand.Log("Failed"))
                .On<TestTrigger.Reset>()
                    .Execute(() => new TestCommand.Reset())
                    .ModifyData(data => data with { Counter = 0, Message = "Reset" })
                    .TransitionTo(TestState.Initial)
            .Build();

        _testData = new TestData(0, "Initial");
    }

    [Benchmark]
    public void SimpleFire()
    {
        var (_, _, _) = _simpleMachine.Fire(new TestTrigger.Start(), TestState.Initial, _testData);
    }

    [Benchmark]
    public void ComplexFire_WithGuard()
    {
        var (_, _, _) = _complexMachine.Fire(new TestTrigger.Process(), TestState.Active, _testData);
    }

    [Benchmark]
    public void ComplexFire_WithEntryExit()
    {
        var (_, _, _) = _complexMachine.Fire(new TestTrigger.Start(), TestState.Initial, _testData);
    }

    [Benchmark]
    public void ComplexFire_MultipleCommands()
    {
        var (_, _, _) = _complexMachine.Fire(new TestTrigger.Complete(), TestState.Processing, _testData);
    }

    [Benchmark]
    public void Build_SimpleMachine()
    {
        var machine = StateMachine<TestState, TestTrigger, TestData, TestCommand>.Create()
            .StartWith(TestState.Initial)
            .For(TestState.Initial)
                .On<TestTrigger.Start>()
                    .TransitionTo(TestState.Active)
            .For(TestState.Active)
                .On<TestTrigger.Complete>()
                    .TransitionTo(TestState.Complete)
            .Build();
    }

    [Benchmark]
    public void Build_ComplexMachine()
    {
        var machine = StateMachine<TestState, TestTrigger, TestData, TestCommand>.Create()
            .StartWith(TestState.Initial)
            .For(TestState.Initial)
                .OnEntry(() => new TestCommand.Log("Entered Initial"))
                .On<TestTrigger.Start>()
                    .Execute(() => new TestCommand.Log("Starting"))
                    .ModifyData(data => data with { Counter = data.Counter + 1 })
                    .TransitionTo(TestState.Active)
            .For(TestState.Active)
                .OnEntry(() => new TestCommand.Log("Entered Active"))
                .OnExit(() => new TestCommand.Log("Exiting Active"))
                .On<TestTrigger.Process>()
                    .Guard((data) => data.Counter < 100)
                    .Execute(() => new TestCommand.Increment())
                    .ModifyData(data => data with { Counter = data.Counter + 1 })
                    .TransitionTo(TestState.Processing)
            .For(TestState.Processing)
                .On<TestTrigger.Complete>()
                    .TransitionTo(TestState.Complete)
            .Build();
    }
}
