# Command Runners

Command runners are an optional layer that dispatches commands using DI. They are useful when you want to keep the state machine pure but still have a structured way to execute commands.

1. Declare either `ICommandRunner<TCommand>` or `IAsyncCommandRunner<TCommand>` for each command type.
2. Call `serviceCollection.AddCommandRunners<TCommand>()` to automatically discover and register them.
3. Resolve `ICommandRunnerProvider<TCommand>` or `IAsyncCommandRunnerProvider<TCommand>` to dispatch commands.

## Why it is useful

- Keeps command handling modular and testable.
- Uses DI for dependency management.
- Supports sync-only or async-capable command execution.

## Simple example

```csharp
public abstract record UserCommand
{
    public sealed record SendWelcomeEmail(Guid UserId) : UserCommand;
}

public sealed class SendWelcomeEmailRunner : ICommandRunner<UserCommand.SendWelcomeEmail>
{
    public void Run(UserCommand.SendWelcomeEmail command)
    {
        // send email
    }
}

services.AddCommandRunners<UserCommand>();

var provider = services.BuildServiceProvider()
    .GetRequiredService<ICommandRunnerProvider<UserCommand>>();

provider.Run(new UserCommand.SendWelcomeEmail(Guid.NewGuid()));
```

## Async-capable example

If any runner implements `IAsyncCommandRunner<TCommand>`, resolve `IAsyncCommandRunnerProvider<TCommand>` instead.

```csharp
public abstract record BillingCommand
{
    public sealed record ChargeCard(decimal Amount) : BillingCommand;
}

public sealed class ChargeCardRunner : IAsyncCommandRunner<BillingCommand.ChargeCard>
{
    public Task RunAsync(BillingCommand.ChargeCard command)
    {
        return Task.CompletedTask;
    }
}

services.AddCommandRunners<BillingCommand>();

var provider = services.BuildServiceProvider()
    .GetRequiredService<IAsyncCommandRunnerProvider<BillingCommand>>();

await provider.RunAsync(new BillingCommand.ChargeCard(42m));
```

## More complex options

```csharp
services.AddCommandRunners<UserCommand>(new CommandRunnerOptions
{
    MissingBehavior = CommandRunnerMissingBehavior.NoOp,
    Lifetime = ServiceLifetime.Scoped,
    AutoRegisterRunners = false
});

services.AddScoped<SendWelcomeEmailRunner>();
```

- `MissingBehavior`: defaults to throw if a command has no runner.
- `Lifetime`: controls runner and provider lifetime.
- `AutoRegisterRunners`: set to `false` to register runners manually.

## End-to-end dispatch example

This example shows a state machine producing commands and a command runner provider executing them.

```csharp
public abstract record OrderTrigger
{
    public sealed record Submit : OrderTrigger;
}

public enum OrderState
{
    Draft,
    Submitted
}

public sealed record OrderData(Guid OrderId, decimal Total);

public abstract record OrderCommand
{
    public sealed record Charge(decimal Amount) : OrderCommand;
    public sealed record SendReceipt(Guid OrderId) : OrderCommand;
}

public sealed class ChargeRunner : IAsyncCommandRunner<OrderCommand.Charge>
{
    public Task RunAsync(OrderCommand.Charge command)
    {
        return Task.CompletedTask;
    }
}

public sealed class SendReceiptRunner : ICommandRunner<OrderCommand.SendReceipt>
{
    public void Run(OrderCommand.SendReceipt command)
    {
    }
}

var machine = StateMachine<OrderState, OrderTrigger, OrderData, OrderCommand>.Create()
    .StartWith(OrderState.Draft)
    .For(OrderState.Draft)
        .On<OrderTrigger.Submit>()
            .Execute(data => new OrderCommand.Charge(data.Total))
            .Execute(data => new OrderCommand.SendReceipt(data.OrderId))
            .TransitionTo(OrderState.Submitted)
    .Build();

var services = new ServiceCollection()
    .AddCommandRunners<OrderCommand>()
    .AddTransient<ChargeRunner>()
    .AddTransient<SendReceiptRunner>();

var provider = services.BuildServiceProvider()
    .GetRequiredService<IAsyncCommandRunnerProvider<OrderCommand>>();

var (nextState, nextData, commands) = machine.Fire(
    new OrderTrigger.Submit(),
    OrderState.Draft,
    new OrderData(Guid.NewGuid(), 99m));

await provider.RunAsync(commands);
```
