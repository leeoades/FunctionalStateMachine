using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace FunctionalStateMachine.CommandRunner;

[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class CommandRunnerRegistration(Action<IServiceCollection, CommandRunnerOptions> register)
{
    public Action<IServiceCollection, CommandRunnerOptions> Register { get; } = register ?? throw new ArgumentNullException(nameof(register));
}