# AI Assistant Quick Guide: FunctionalStateMachine

> **For AI coding assistants:** This guide provides essential patterns for using FunctionalStateMachine in external projects.

## Package

```bash
dotnet add package FunctionalStateMachine.Core
```

## Essential Type Pattern

```csharp
// 1. States (enum)
public enum MyState { StateA, StateB, StateC }

// 2. Triggers (abstract record with subtypes)
// Note: 'sealed' is optional but recommended
public abstract record MyTrigger
{
    public record DoAction(string Value) : MyTrigger;
    public record Complete : MyTrigger;
}

// 3. Data (record) - optional
public record MyData(int Counter, string Status);

// 4. Commands (abstract record with subtypes)
public abstract record MyCommand
{
    public record SaveData(string Data) : MyCommand;
    public record SendNotification(string Message) : MyCommand;
}
```

## Builder Pattern

```csharp
var machine = StateMachine<MyState, MyTrigger, MyData, MyCommand>.Create()
    .StartWith(MyState.StateA)
    .For(MyState.StateA)
        .On<MyTrigger.DoAction>()
            .ModifyData((data, trigger) => data with { Status = trigger.Value })
            .Execute(data => new MyCommand.SaveData(data.Status))
            .TransitionTo(MyState.StateB)
    .For(MyState.StateB)
        .On<MyTrigger.Complete>()
            .Execute(() => new MyCommand.SendNotification("Done"))
            .TransitionTo(MyState.StateC)
    .Build();
```

## Execution

```csharp
// Fire a trigger
var (newState, newData, commands) = machine.Fire(
    new MyTrigger.DoAction("value"),
    currentState,
    currentData
);

// Execute commands (your choice how)
foreach (var command in commands)
{
    switch (command)
    {
        case MyCommand.SaveData cmd:
            await repository.SaveAsync(cmd.Data);
            break;
        case MyCommand.SendNotification cmd:
            await notifier.SendAsync(cmd.Message);
            break;
    }
}
```

## Key Patterns

### Internal Transition (No State Change)
```csharp
.On<Trigger.Update>()
    .ModifyData(data => data with { Counter = data.Counter + 1 })
    .Execute(() => new Command.Log())
// No .TransitionTo() = stays in same state
```

### Guards (Conditional Routing)
```csharp
.On<Trigger.Process>()
    .Guard(data => data.Amount > 1000)
    .TransitionTo(State.HighValue)
.On<Trigger.Process>()
    .Guard(data => data.Amount <= 1000)
    .TransitionTo(State.LowValue)
```

### Conditional Steps (If/Else)
```csharp
.On<Trigger.Submit>()
    .If((data, trigger) => data.IsValid)
        .Execute(() => new Command.ProcessSuccess())
        .TransitionTo(State.Success)
    .Else()
        .Execute(() => new Command.ProcessError())
        .TransitionTo(State.Failed)
    .Done()
```

### Entry/Exit Actions
```csharp
.For(State.Active)
    .OnEntry(() => new Command.Start())
    .OnExit(() => new Command.Stop())
    .On<Trigger.Deactivate>()
        .TransitionTo(State.Inactive)
```

### Multiple Commands
```csharp
.On<Trigger.Complete>()
    .Execute(data => new Command.Save(data))
    .Execute(() => new Command.Notify())
    .Execute(() => new Command.UpdateMetrics())
    .TransitionTo(State.Done)
```

## Critical Rules

✅ **DO:**
- Return commands from state machine (describe *what* to do)
- Use immutable updates: `data with { Counter = data.Counter + 1 }`
- Test by asserting on returned (state, data, commands)

❌ **DON'T:**
- Perform I/O or side effects inside state machine
- Mutate data in-place: `data.Counter++`
- Use `.TransitionTo(currentState)` for internal transitions

## Command Dispatching (Optional)

```bash
dotnet add package FunctionalStateMachine.CommandRunner
```

```csharp
// Define runner
public class SaveDataRunner : IAsyncCommandRunner<MyCommand.SaveData>
{
    public async Task RunAsync(MyCommand.SaveData cmd) =>
        await repository.SaveAsync(cmd.Data);
}

// Register (also add Generator package as analyzer)
services.AddCommandRunners<MyCommand>();

// Use
await dispatcher.RunAsync(commands);
```

## Execute Function Signatures

```csharp
.Execute(() => ...)                          // No params
.Execute(data => ...)                        // Data access
.Execute(trigger => ...)                     // Trigger access
.Execute((data, trigger) => ...)            // Both
.Execute((state, data, trigger) => ...)     // All
```

## ModifyData Function Signatures

```csharp
.ModifyData(data => ...)                     // Data only
.ModifyData((data, trigger) => ...)         // Data + trigger
.ModifyData((state, data, trigger) => ...)  // All
```

## Common Pitfalls

| Issue | Cause | Fix |
|-------|-------|-----|
| Entry/exit fire on internal | Used `.TransitionTo(currentState)` | Remove `.TransitionTo()` |
| Data not updating | Mutating in-place | Use `with` expression |
| Commands not dispatching | Generator not configured | Add as `OutputItemType="Analyzer"` |
| Unhandled trigger error | No handler configured | Add `.On<>()` or `.OnUnhandled()` |

## Complete Example

```csharp
public enum OrderState { Cart, Processing, Completed }

public abstract record OrderTrigger
{
    public record AddItem(string Id) : OrderTrigger;
    public record Checkout : OrderTrigger;
}

public record OrderData(List<string> Items, decimal Total);

public abstract record OrderCommand
{
    public record AddToCart(string Id) : OrderCommand;
    public record ChargeCard(decimal Amount) : OrderCommand;
}

var machine = StateMachine<OrderState, OrderTrigger, OrderData, OrderCommand>
    .Create()
    .StartWith(OrderState.Cart)
    .For(OrderState.Cart)
        .On<OrderTrigger.AddItem>()
            .ModifyData((data, trigger) => data with 
            { 
                Items = data.Items.Append(trigger.Id).ToList() 
            })
            .Execute(trigger => new OrderCommand.AddToCart(trigger.Id))
        .On<OrderTrigger.Checkout>()
            .Execute(data => new OrderCommand.ChargeCard(data.Total))
            .TransitionTo(OrderState.Processing)
    .For(OrderState.Processing)
        // ... more configuration
    .Build();

// Usage
var data = new OrderData(new List<string>(), 0m);
var (state, newData, commands) = machine.Fire(
    new OrderTrigger.AddItem("item1"), 
    OrderState.Cart, 
    data);
```

## Documentation

- Full documentation: https://github.com/leeoades/FunctionalStateMachine
- Feature guides: `/docs` folder in repository
- Samples: `/samples` folder in repository

## Version Detection

To provide version-specific guidance, check:
- `CHANGELOG.md` for feature availability
- Package version in project file
- Feature detection via namespace checking

## When Updating for New Features

When the library adds new features:

1. **Check CHANGELOG.md** for version and feature details
2. **Review corresponding feature documentation** in `/docs`
3. **Update implementations** to leverage new capabilities
4. **Validate backwards compatibility** for existing state machines
5. **Consider deprecations** mentioned in release notes

---

For complete documentation and advanced features, see:
- Repository: https://github.com/leeoades/FunctionalStateMachine
- Full AI Guide: `.copilot-instructions.md` in repository root
