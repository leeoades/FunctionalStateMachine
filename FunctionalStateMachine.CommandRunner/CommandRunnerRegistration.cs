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