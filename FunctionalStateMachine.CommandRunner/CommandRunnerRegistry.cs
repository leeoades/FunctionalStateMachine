using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace FunctionalStateMachine.CommandRunner;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class CommandRunnerRegistration
{
    public CommandRunnerRegistration(Action<IServiceCollection, CommandRunnerOptions> register)
    {
        Register = register ?? throw new ArgumentNullException(nameof(register));
    }

    public Action<IServiceCollection, CommandRunnerOptions> Register { get; }
}

[EditorBrowsable(EditorBrowsableState.Never)]
public static class CommandRunnerRegistry
{
    private static readonly Dictionary<Type, CommandRunnerRegistration> Registrations = new();

    public static void Register<TCommand>(CommandRunnerRegistration registration)
    {
        if (registration is null)
        {
            throw new ArgumentNullException(nameof(registration));
        }

        Registrations[typeof(TCommand)] = registration;
    }

    public static bool TryGet<TCommand>(out CommandRunnerRegistration registration)
    {
        return Registrations.TryGetValue(typeof(TCommand), out registration!);
    }
}
