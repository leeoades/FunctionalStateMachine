using Microsoft.Extensions.DependencyInjection;

namespace FunctionalStateMachine.CommandRunner;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommandRunners<TCommand>(
        this IServiceCollection services,
        CommandRunnerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var resolvedOptions = options ?? new CommandRunnerOptions();
        if (!CommandRunnerRegistry.TryGet<TCommand>(out var registration))
        {
            throw new InvalidOperationException(
                $"No command runner registrations were generated for {typeof(TCommand).FullName}.");
        }

        registration.Register(services, resolvedOptions);
        return services;
    }
}
