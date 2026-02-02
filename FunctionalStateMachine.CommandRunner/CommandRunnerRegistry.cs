using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace FunctionalStateMachine.CommandRunner;

[EditorBrowsable(EditorBrowsableState.Never)]
public static class CommandRunnerRegistry
{
    private static readonly Dictionary<Type, CommandRunnerRegistration> Registrations = new();

    public static void Register<TCommand>(CommandRunnerRegistration registration)
    {
        Registrations[typeof(TCommand)] = registration 
                                          ?? throw new ArgumentNullException(nameof(registration));
    }

    public static bool TryGet<TCommand>(out CommandRunnerRegistration registration)
    {
        return Registrations.TryGetValue(typeof(TCommand), out registration!);
    }
}
