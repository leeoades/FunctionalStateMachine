using Microsoft.Extensions.DependencyInjection;

namespace FunctionalStateMachine.CommandRunner;

public enum CommandRunnerMissingBehavior
{
    Throw,
    NoOp
}

public sealed class CommandRunnerOptions
{
    public CommandRunnerMissingBehavior MissingBehavior { get; set; } = CommandRunnerMissingBehavior.Throw;
    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Transient;
}
