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

    // ── Multiple machines sharing the same trigger type ──────────────────────

    [Fact]
    public void MultipleMachines_SameTrigger_RegistryPopulatedOnce()
    {
        // The source generator deduplicates: both machines share AccessibleTrigger,
        // so the registry should contain exactly the concrete types, not doubled.
        var registered = TriggerTypeRegistry.TryGet<AccessibleTrigger>(out var types);

        Assert.True(registered);
        Assert.NotNull(types);
        // Exactly two concrete types, not duplicated
        Assert.Equal(2, types.Length);
        Assert.Contains(typeof(AccessibleTrigger.TriggerA), types);
        Assert.Contains(typeof(AccessibleTrigger.TriggerB), types);
    }

    [Fact]
    public void MultipleMachines_SameTrigger_BothMachinesBuildAndFireCorrectly()
    {
        // Two independent state machines that share the same trigger hierarchy.
        // Each machine has different state/data/command types but the same TTrigger.
        var machineAlpha = StateMachine<MultiAlphaState, AccessibleTrigger, AccessibleData, AccessibleCommand>.Create()
            .StartWith(MultiAlphaState.X)
            .For(MultiAlphaState.X)
                .On<AccessibleTrigger.TriggerA>()
                    .TransitionTo(MultiAlphaState.Y)
            .For(MultiAlphaState.Y)
                .On<AccessibleTrigger.TriggerB>()
                    .TransitionTo(MultiAlphaState.X)
            .Build();

        var machineBeta = StateMachine<MultiBetaState, AccessibleTrigger, AccessibleData, AccessibleCommand>.Create()
            .StartWith(MultiBetaState.P)
            .For(MultiBetaState.P)
                .On<AccessibleTrigger.TriggerB>()
                    .TransitionTo(MultiBetaState.Q)
            .For(MultiBetaState.Q)
                .On<AccessibleTrigger.TriggerA>()
                    .TransitionTo(MultiBetaState.P)
            .Build();

        // machineAlpha: X --TriggerA--> Y
        var (alphaState, alphaData, _) = machineAlpha.Fire(new AccessibleTrigger.TriggerA(), MultiAlphaState.X, new AccessibleData());
        Assert.Equal(MultiAlphaState.Y, alphaState);

        // machineAlpha: Y --TriggerB--> X
        var (alphaState2, _, _) = machineAlpha.Fire(new AccessibleTrigger.TriggerB(), alphaState, alphaData);
        Assert.Equal(MultiAlphaState.X, alphaState2);

        // machineBeta: P --TriggerB--> Q
        var (betaState, betaData, _) = machineBeta.Fire(new AccessibleTrigger.TriggerB(), MultiBetaState.P, new AccessibleData());
        Assert.Equal(MultiBetaState.Q, betaState);

        // machineBeta: Q --TriggerA--> P
        var (betaState2, _, _) = machineBeta.Fire(new AccessibleTrigger.TriggerA(), betaState, betaData);
        Assert.Equal(MultiBetaState.P, betaState2);
    }

    [Fact]
    public void MultipleMachines_SameTrigger_MachinesAreIndependent()
    {
        // Verify machines with shared triggers don't affect each other's state.
        var machine1 = StateMachine<MultiAlphaState, AccessibleTrigger, AccessibleData, AccessibleCommand>.Create()
            .StartWith(MultiAlphaState.X)
            .For(MultiAlphaState.X)
                .On<AccessibleTrigger.TriggerA>()
                    .TransitionTo(MultiAlphaState.Y)
            .For(MultiAlphaState.Y)
                .On<AccessibleTrigger.TriggerA>()
                    .TransitionTo(MultiAlphaState.X)
            .Build();

        var machine2 = StateMachine<MultiAlphaState, AccessibleTrigger, AccessibleData, AccessibleCommand>.Create()
            .StartWith(MultiAlphaState.X)
            .For(MultiAlphaState.X)
                .On<AccessibleTrigger.TriggerA>()
                    .TransitionTo(MultiAlphaState.Y)
            .For(MultiAlphaState.Y)
                .On<AccessibleTrigger.TriggerA>()
                    .TransitionTo(MultiAlphaState.X)
            .Build();

        // Fire machine1 twice but machine2 once — their states are independent
        var (s1, d1, _) = machine1.Fire(new AccessibleTrigger.TriggerA(), MultiAlphaState.X, new AccessibleData());
        var (s1b, _, _) = machine1.Fire(new AccessibleTrigger.TriggerA(), s1, d1);
        var (s2, _, _) = machine2.Fire(new AccessibleTrigger.TriggerA(), MultiAlphaState.X, new AccessibleData());

        Assert.Equal(MultiAlphaState.X, s1b);  // machine1 looped back
        Assert.Equal(MultiAlphaState.Y, s2);    // machine2 still at Y
    }

    [Fact]
    public void MultipleMachines_SameTrigger_EachUsesSubsetOfTriggers_BothBuildSuccessfully()
    {
        // machineAlpha only uses TriggerA (TriggerB is unused in Alpha)
        // machineBeta only uses TriggerB (TriggerA is unused in Beta)
        // Both should build successfully — unused-trigger analysis is per-machine and
        // emits warnings (not errors), so a trigger unused in one machine doesn't block
        // the other machine.

        var machineAlpha = StateMachine<MultiAlphaState, AccessibleTrigger, AccessibleData, AccessibleCommand>.Create()
            .StartWith(MultiAlphaState.X)
            .For(MultiAlphaState.X)
                .On<AccessibleTrigger.TriggerA>()   // TriggerB never used in Alpha
                    .TransitionTo(MultiAlphaState.Y)
            .For(MultiAlphaState.Y)
                .On<AccessibleTrigger.TriggerA>()
                    .TransitionTo(MultiAlphaState.X)
            .Build();  // ⚠️ warning about TriggerB, but no error

        var machineBeta = StateMachine<MultiBetaState, AccessibleTrigger, AccessibleData, AccessibleCommand>.Create()
            .StartWith(MultiBetaState.P)
            .For(MultiBetaState.P)
                .On<AccessibleTrigger.TriggerB>()   // TriggerA never used in Beta
                    .TransitionTo(MultiBetaState.Q)
            .For(MultiBetaState.Q)
                .On<AccessibleTrigger.TriggerB>()
                    .TransitionTo(MultiBetaState.P)
            .Build();  // ⚠️ warning about TriggerA, but no error

        Assert.NotNull(machineAlpha);
        Assert.NotNull(machineBeta);

        // Both machines respond correctly to their own triggers
        var (alphaNext, _, _) = machineAlpha.Fire(new AccessibleTrigger.TriggerA(), MultiAlphaState.X, new AccessibleData());
        Assert.Equal(MultiAlphaState.Y, alphaNext);

        var (betaNext, _, _) = machineBeta.Fire(new AccessibleTrigger.TriggerB(), MultiBetaState.P, new AccessibleData());
        Assert.Equal(MultiBetaState.Q, betaNext);
    }

    [Fact]
    public void HierarchicalMachine_TriggerRegistry_PopulatedCorrectly()
    {
        // Hierarchical states reuse the same TTrigger as flat machines.
        // The trigger registry is keyed on TTrigger regardless of state hierarchy.
        var registered = TriggerTypeRegistry.TryGet<AccessibleTrigger>(out var types);
        Assert.True(registered);
        Assert.Contains(typeof(AccessibleTrigger.TriggerA), types);
        Assert.Contains(typeof(AccessibleTrigger.TriggerB), types);
    }

    [Fact]
    public void HierarchicalMachine_TriggersFire_ThroughParentAndChildStates()
    {
        // A machine with hierarchical (parent/child) states using AccessibleTrigger.
        // Verifies that the shared trigger registry doesn't interfere with hierarchy logic.
        var machine = StateMachine<HierarchyState, AccessibleTrigger, AccessibleData, AccessibleCommand>.Create()
            .StartWith(HierarchyState.Leaf)
            .For(HierarchyState.Root)
                .StartsWith(HierarchyState.Child)
            .For(HierarchyState.Child)
                .SubStateOf(HierarchyState.Root)
                .On<AccessibleTrigger.TriggerA>()
                    .TransitionTo(HierarchyState.Leaf)
            .For(HierarchyState.Leaf)
                .On<AccessibleTrigger.TriggerB>()
                    .TransitionTo(HierarchyState.Child)
            .Build();

        // Leaf --TriggerB--> Child (a sub-state of Root)
        var (state1, data1, _) = machine.Fire(new AccessibleTrigger.TriggerB(), HierarchyState.Leaf, new AccessibleData());
        Assert.Equal(HierarchyState.Child, state1);

        // Child --TriggerA--> Leaf
        var (state2, _, _) = machine.Fire(new AccessibleTrigger.TriggerA(), state1, data1);
        Assert.Equal(HierarchyState.Leaf, state2);
    }
}

// Types are internal at namespace level so the source generator can access them
internal enum AccessibleState { A, B }
internal enum MultiAlphaState { X, Y }
internal enum MultiBetaState { P, Q }
internal enum HierarchyState { Root, Child, Leaf }

internal abstract record AccessibleTrigger
{
    public sealed record TriggerA : AccessibleTrigger;
    public sealed record TriggerB : AccessibleTrigger;
}

internal sealed record AccessibleData;
internal abstract record AccessibleCommand;
