# FunctionalStateMachine.CommandRunner

`FunctionalStateMachine.CommandRunner` is an optional DI layer for executing commands produced by a state machine. It discovers and registers `ICommandRunner<TCommand>`/`IAsyncCommandRunner<TCommand>` implementations and provides dispatchers for running commands.

## Quick start

### 1) Define your command hierarchy

```csharp
public abstract record UserCommand
{
    public sealed record SendWelcomeEmail(Guid UserId) : UserCommand;
}
```

### 2) Implement runners

```csharp
public sealed class SendWelcomeEmailRunner : ICommandRunner<UserCommand.SendWelcomeEmail>
{
    public void Run(UserCommand.SendWelcomeEmail command)
    {
        // send email
    }
}
```

### 3) Register and dispatch

```csharp
var services = new ServiceCollection()
    .AddCommandRunners<UserCommand>();

var dispatcher = services
    .BuildServiceProvider()
    .GetRequiredService<ICommandDispatcher<UserCommand>>();

dispatcher.Run(new UserCommand.SendWelcomeEmail(Guid.NewGuid()));
```

## Async runners

If any runner implements `IAsyncCommandRunner<TCommand>`, resolve `IAsyncCommandDispatcher<TCommand>` and call `RunAsync`:

```csharp
public sealed class ChargeCardRunner : IAsyncCommandRunner<BillingCommand.ChargeCard>
{
    public Task RunAsync(BillingCommand.ChargeCard command)
        => Task.CompletedTask;
}

services.AddCommandRunners<BillingCommand>();
var dispatcher = services.BuildServiceProvider()
    .GetRequiredService<IAsyncCommandDispatcher<BillingCommand>>();

await dispatcher.RunAsync(new BillingCommand.ChargeCard(42m));
```

## Options

```csharp
services.AddCommandRunners<UserCommand>(new CommandRunnerOptions
{
    MissingBehavior = CommandRunnerMissingBehavior.NoOp,
    Lifetime = ServiceLifetime.Scoped,
    AutoRegisterRunners = false
});
```
### Missing Behavior
- `MissingBehavior`: Define how to deal with missing command runners 
  - `Throw` - `[default]` throw when a command has no runner.
  - `NoOp` - executing a command that has no runner does nothing.

### Lifetime
- `Lifetime`: Sets the lifetime that the command runners are registered.
  - `Transient` - `[default]`
  - `Singleton`
  - `Scoped`

### Suppress Autoregistration
- `AutoRegisterRunners`: Suppress the auto-registration
  - `true` : `[default]` 
  - `false` : set to `false` to register runners manually.

## Notes

- The dispatcher is source-generated; ensure `FunctionalStateMachine.CommandRunner.Generator` is referenced as an analyzer in projects that call `AddCommandRunners<T>()`.
- See `docs/command-runners.md` for full guidance and examples.
