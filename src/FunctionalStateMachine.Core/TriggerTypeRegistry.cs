using System.ComponentModel;

namespace FunctionalStateMachine.Core;

/// <summary>
/// Registry for source-generated trigger type mappings.
/// Populated automatically by the FunctionalStateMachine.Core source generator.
/// </summary>
/// <remarks>
/// <see cref="Register{TTrigger}"/> is called exclusively from <c>[ModuleInitializer]</c>
/// methods, which the .NET runtime guarantees run single-threaded before any module code
/// executes.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class TriggerTypeRegistry
{
    private static readonly Dictionary<Type, Type[]> Registrations = new();

    /// <summary>
    /// Registers the concrete trigger types for a given trigger base type.
    /// Called automatically by the source-generated module initializer.
    /// </summary>
    public static void Register<TTrigger>(Type[] triggerTypes)
    {
        Registrations[typeof(TTrigger)] = triggerTypes ?? throw new ArgumentNullException(nameof(triggerTypes));
    }

    /// <summary>
    /// Tries to retrieve the registered concrete trigger types for a given trigger base type.
    /// Returns false if the generator was not active for this trigger type.
    /// </summary>
    public static bool TryGet<TTrigger>(out Type[] triggerTypes)
    {
        return Registrations.TryGetValue(typeof(TTrigger), out triggerTypes!);
    }
}
