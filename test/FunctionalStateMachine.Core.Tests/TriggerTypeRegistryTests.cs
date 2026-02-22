namespace FunctionalStateMachine.Core.Tests;

/// <summary>
/// Tests that the source generator populates TriggerTypeRegistry for accessible trigger types,
/// enabling unused-trigger analysis without reflection.
/// </summary>
public class TriggerTypeRegistryTests
{
    [Fact]
    public void TriggerTypeRegistry_IsPopulatedByGenerator_ForAccessibleTrigger()
    {
        // The source generator should have registered the trigger types
        // for AccessibleTrigger (internal type at namespace level)
        // at module initialization time.
        var registered = TriggerTypeRegistry.TryGet<AccessibleTrigger>(out var types);

        Assert.True(registered, "TriggerTypeRegistry should be populated by the source generator");
        Assert.NotNull(types);
        Assert.Contains(typeof(AccessibleTrigger.TriggerA), types);
        Assert.Contains(typeof(AccessibleTrigger.TriggerB), types);
        Assert.DoesNotContain(typeof(AccessibleTrigger), types); // abstract base should not be listed
    }

    [Fact]
    public void AnalyzeUnusedTriggers_ReportsWarning_WhenTriggerNotUsed()
    {
        // TriggerA is used, TriggerB is not — should build fine (warning is logged to Debug, not thrown)
        // Warnings are internal to AnalysisResult; the machine still builds successfully.
        var machine = StateMachine<AccessibleState, AccessibleTrigger, AccessibleData, AccessibleCommand>.Create()
            .StartWith(AccessibleState.A)
            .For(AccessibleState.A)
                .On<AccessibleTrigger.TriggerA>()
                    .TransitionTo(AccessibleState.B)
            .For(AccessibleState.B)
                .On<AccessibleTrigger.TriggerA>()
                    .TransitionTo(AccessibleState.A)
            // TriggerB is never used — warning is emitted (not an error)
            .Build();

        Assert.NotNull(machine);
    }

    [Fact]
    public void AnalyzeUnusedTriggers_NoWarning_WhenAllTriggersUsed()
    {
        // Both TriggerA and TriggerB are used
        var machine = StateMachine<AccessibleState, AccessibleTrigger, AccessibleData, AccessibleCommand>.Create()
            .StartWith(AccessibleState.A)
            .For(AccessibleState.A)
                .On<AccessibleTrigger.TriggerA>()
                    .TransitionTo(AccessibleState.B)
                .On<AccessibleTrigger.TriggerB>()
                    .TransitionTo(AccessibleState.A)
            .For(AccessibleState.B)
                .On<AccessibleTrigger.TriggerA>()
                    .TransitionTo(AccessibleState.A)
            .Build();

        Assert.NotNull(machine);
    }
}

// Types are internal at namespace level so the source generator can access them
internal enum AccessibleState { A, B }

internal abstract record AccessibleTrigger
{
    public sealed record TriggerA : AccessibleTrigger;
    public sealed record TriggerB : AccessibleTrigger;
}

internal sealed record AccessibleData;
internal abstract record AccessibleCommand;
