// ReSharper disable UnusedParameter.Local
namespace FunctionalStateMachine.Core;

public static partial class StateMachineBuilderExtensions
{
    private static IEnumerable<TCommand> Single<TCommand>(TCommand command) => [command];
}
