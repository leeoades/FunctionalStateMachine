# Functional State Machine

**A persistence-friendly state machine for .NET that returns commands instead of executing side effects.**

Build deterministic, testable workflows where transitions produce logical commands that your application decides how and when to execute. Perfect for actor-based systems, event sourcing, and any scenario where state machines need to be persisted and rehydrated.

## Why Choose This Library?

**🎯 Pure & Predictable** — Transitions return commands instead of performing I/O, making every state change deterministic and replayable.

**💾 Persistence-First** — State isn't locked inside the machine. Load state, fire a trigger, save state. Perfect for actors and event-sourced systems.

**✅ Test-Friendly** — Verify behavior by inspecting returned commands. No mocking external dependencies or setting up infrastructure.

**🏗️ Hierarchical States** — Model complex workflows with parent/child state relationships. Parent transitions apply to all children.

**🔍 Design-Time Analysis** — Detect unreachable states, missing transitions, and configuration errors at build time. Generate Mermaid diagrams automatically.

## Perfect For Actor-Based Systems

This library was designed for components that operate within an actor model where state machines aren't kept in memory:

1. **Load** persisted state from storage
2. **Fire** a trigger on the state machine
3. **Execute** returned commands
4. **Save** the new state back to storage

The actor instance may or may not be reused, so the state machine treats persistence as a first-class concern rather than an afterthought. This approach provides a consistent, analyzable pattern across all your stateful components.

## Quick Start

Install the core package:

```bash
dotnet add package FunctionalStateMachine.Core
```

Build a simple door lock state machine:

```csharp
public enum DoorState { Locked, Unlocked }

public abstract record DoorTrigger
{
    public sealed record InsertKey : DoorTrigger;
    public sealed record RemoveKey : DoorTrigger;
}

public abstract record DoorCommand
{
    public sealed record UnlockBolt : DoorCommand;
    public sealed record LockBolt : DoorCommand;
    public sealed record Beep : DoorCommand;
}

// Build the state machine
var machine = StateMachine<DoorState, DoorTrigger, DoorCommand>.Create()
    .StartWith(DoorState.Locked)
    .For(DoorState.Locked)
        .On<DoorTrigger.InsertKey>()
            .Execute(() => new DoorCommand.UnlockBolt())
            .Execute(() => new DoorCommand.Beep())
            .TransitionTo(DoorState.Unlocked)
    .For(DoorState.Unlocked)
        .On<DoorTrigger.RemoveKey>()
            .Execute(() => new DoorCommand.LockBolt())
            .TransitionTo(DoorState.Locked)
    .Build();

// Use the state machine
// Yes, it's a fancy bolt that locks when you remove the key.
var currentState = DoorState.Locked;
var (newState, commands) = machine.Fire(new DoorTrigger.InsertKey(), currentState);

// Execute commands in your application layer
// Note: There is a built-in way to dispatch commands that we'll introduce later...
foreach (var command in commands)
{
    switch (command)
    {
        case DoorCommand.UnlockBolt:
            hardware.UnlockBolt();
            break;
        case DoorCommand.Beep:
            speaker.Beep();
            break;
    }
}
```

---

## Core Concepts

### Commands Over Side Effects

The state machine returns logical commands that describe *what should happen*, not *how to do it*. Your application layer decides when and how to execute them.

**Example:** A payment state machine returns `ChargeCard` and `SendReceipt` commands instead of calling APIs directly.

```csharp
public abstract record PaymentCommand
{
    public sealed record ChargeCard(decimal Amount, string CardToken) : PaymentCommand;
    public sealed record SendReceipt(string Email) : PaymentCommand;
    public sealed record LogFailure(string Reason) : PaymentCommand;
}

var machine = StateMachine<PaymentState, PaymentTrigger, PaymentData, PaymentCommand>.Create()
    .For(PaymentState.Pending)
        .On<PaymentTrigger.Submit>()
            .Execute(data => new PaymentCommand.ChargeCard(data.Amount, data.CardToken))
            .Execute(data => new PaymentCommand.SendReceipt(data.Email))
            .TransitionTo(PaymentState.Completed)
    .Build();
```

**Benefits:** Pure transitions, easy testing, deterministic replay, natural audit trails.

➡️ **Learn more:** [Commands vs Side Effects](docs/commands-vs-effects.md)

### Packages

Choose only what you need:

- **`FunctionalStateMachine.Core`** — The state machine (required)
- **`FunctionalStateMachine.CommandRunner`** — Optional DI-based command dispatcher
- **`FunctionalStateMachine.Diagrams`** — Optional build-time Mermaid diagram generator

➡️ **Learn more:** [Package Guide](docs/packages.md)

---

## Features

### Fluent Configuration

Build state machines with a readable, chainable API that validates configuration before runtime.

```csharp
var machine = StateMachine<OrderState, OrderTrigger, OrderCommand>.Create()
    .StartWith(OrderState.Cart)
    .For(OrderState.Cart)
        .On<OrderTrigger.Checkout>()
            .TransitionTo(OrderState.Processing)
    .For(OrderState.Processing)
        .On<OrderTrigger.PaymentReceived>()
            .TransitionTo(OrderState.Shipped)
    .Build(); // ✅ Validates: all states reachable, no orphaned states
```

➡️ **Learn more:** [Fluent Configuration](docs/fluent-configuration.md)

---

### State Data

Attach data to your state and update it atomically with transitions. Perfect for tracking counters, timestamps, or domain information.

```csharp
public sealed record GameData(int Score, int Lives, int Level);

var machine = StateMachine<GameState, GameTrigger, GameData, GameCommand>.Create()
    .For(GameState.Playing)
        .On<GameTrigger.ScorePoints>()
            .ModifyData((data, trigger) => data with 
            { 
                Score = data.Score + trigger.Points 
            })
            .Execute(data => new GameCommand.UpdateDisplay(data.Score))
    .Build();

var data = new GameData(Score: 0, Lives: 3, Level: 1);
var (newState, newData, commands) = machine.Fire(
    new GameTrigger.ScorePoints(Points: 100), 
    GameState.Playing, 
    data);
// newData.Score == 100
```

➡️ **Learn more:** [State Data and ModifyData](docs/state-data.md)

---

### Guards

Route triggers down different paths based on state, data, or trigger properties. Guards let you encode business rules directly in your transitions.

```csharp
public sealed record OrderData(decimal Total, bool IsVip);

var machine = StateMachine<OrderState, OrderTrigger, OrderData, OrderCommand>.Create()
    .For(OrderState.Review)
        .On<OrderTrigger.Approve>()
            .Guard(data => data.Total > 1000)
            .TransitionTo(OrderState.ManagerApproval)
        .On<OrderTrigger.Approve>()
            .Guard(data => data.IsVip)
            .TransitionTo(OrderState.Approved)
        .On<OrderTrigger.Approve>()
            .Guard(data => data.Total <= 1000 && !data.IsVip)
            .TransitionTo(OrderState.Approved)
    .Build();
```

**First matching guard wins.** If no guard matches, the trigger is unhandled.

➡️ **Learn more:** [Guards and Conditional Flows](docs/guards.md)

---

### Conditional Steps (If/ElseIf/Else)

Branch *within* a transition to execute different commands, modify data differently, or even transition to different states.

```csharp
var machine = StateMachine<ATMState, ATMTrigger, ATMData, ATMCommand>.Create()
    .For(ATMState.Withdraw)
        .On<ATMTrigger.Confirm>()
            .If((data, trigger) => data.Balance >= trigger.Amount)
                .ModifyData((data, trigger) => data with 
                { 
                    Balance = data.Balance - trigger.Amount 
                })
                .Execute(trigger => new ATMCommand.DispenseCash(trigger.Amount))
                .TransitionTo(ATMState.Idle)
            .Else()
                .Execute(() => new ATMCommand.ShowError("Insufficient funds"))
                .TransitionTo(ATMState.Idle)
            .Done()
    .Build();
```

➡️ **Learn more:** [Conditional Steps](docs/conditional-steps.md)

---

### Entry and Exit Actions

Automatically emit commands when entering or leaving a state. Great for logging, notifications, and lifecycle management.

```csharp
var machine = StateMachine<SessionState, SessionTrigger, SessionData, SessionCommand>.Create()
    .For(SessionState.Active)
        .OnEntry(data => new SessionCommand.LogActivity($"User {data.UserId} logged in"))
        .OnExit(data => new SessionCommand.LogActivity($"User {data.UserId} logged out"))
        .On<SessionTrigger.Logout>()
            .TransitionTo(SessionState.Idle)
    .Build();
```

Entry/exit commands run *only* when the state actually changes, not on internal transitions.

➡️ **Learn more:** [Entry and Exit Commands](docs/entry-exit.md)

---

### Execute Steps

Emit one or more commands during a transition. Execute steps can access state, data, and trigger information.

```csharp
var machine = StateMachine<OrderState, OrderTrigger, OrderData, OrderCommand>.Create()
    .For(OrderState.Pending)
        .On<OrderTrigger.Submit>()
            .Execute(data => new OrderCommand.ChargeCard(data.Total))
            .Execute(data => new OrderCommand.SendConfirmation(data.Email))
            .Execute(() => new OrderCommand.UpdateMetrics("order_placed"))
            .TransitionTo(OrderState.Completed)
    .Build();
```

Commands are collected and returned in order. Your application executes them sequentially or in parallel.

➡️ **Learn more:** [Execute Steps and Multiple Commands](docs/execute-steps.md)

---

### Internal Transitions

Omit `TransitionTo` to stay in the current state. Useful for high-frequency events like heartbeats or in-place updates.

```csharp
var machine = StateMachine<ServerState, ServerTrigger, ServerData, ServerCommand>.Create()
    .For(ServerState.Running)
        .On<ServerTrigger.Heartbeat>()
            .ModifyData(data => data with { LastSeen = DateTime.UtcNow })
            .Execute(() => new ServerCommand.RecordHeartbeat())
        // No TransitionTo = internal transition
    .Build();
```

**Internal transitions skip entry/exit actions** because the state doesn't change.

➡️ **Learn more:** [Internal Transitions](docs/internal-transitions.md)

---

### Immediate Transitions

Automatically advance from a state without waiting for a trigger. Perfect for initialization or gateway states.

```csharp
var machine = StateMachine<AppState, AppTrigger, AppData, AppCommand>.Create()
    .StartWith(AppState.Initializing)
    .For(AppState.Initializing)
        .OnEntry(() => new AppCommand.LoadConfiguration())
        .Immediately()
            .Guard(data => data.ConfigLoaded)
            .TransitionTo(AppState.Ready)
        .Done()
    .For(AppState.Ready)
        // Application is now ready
    .Build();

// Trigger OnEntry and immediate transition
var (state, data, commands) = machine.Start(new AppData(ConfigLoaded: true));
// state == AppState.Ready
```

➡️ **Learn more:** [Immediate Transitions](docs/immediate-transitions.md)

---

### Hierarchical States

Model parent/child relationships where parent transitions apply to all children, and parents choose which child to enter.

```csharp
var machine = StateMachine<ConnectionState, ConnectionTrigger, ConnectionCommand>.Create()
    .For(ConnectionState.Connected)
        .StartsWith(ConnectionState.Idle) // When entering Connected, start at Idle
        .On<ConnectionTrigger.Disconnect>()
            .TransitionTo(ConnectionState.Disconnected) // Works from any child
    .For(ConnectionState.Idle)
        .SubStateOf(ConnectionState.Connected)
        .On<ConnectionTrigger.SendData>()
            .TransitionTo(ConnectionState.Transmitting)
    .For(ConnectionState.Transmitting)
        .SubStateOf(ConnectionState.Connected)
        .On<ConnectionTrigger.DataSent>()
            .TransitionTo(ConnectionState.Idle)
    .Build();
```

**Parent transitions take precedence** if both parent and child handle the same trigger.

➡️ **Learn more:** [Hierarchical States](docs/hierarchical-states.md)

---

### Ignore and Unhandled Triggers

Explicitly ignore triggers in specific states, or handle all unhandled triggers globally.

```csharp
var machine = StateMachine<State, Trigger, Data, Command>.Create()
    .OnUnhandled()
        .Execute((trigger, state) => 
            new Command.LogWarning($"Unhandled {trigger} in {state}"))
    .For(State.Processing)
        .On<Trigger.Cancel>()
            .Ignore() // Explicitly do nothing
    .Build();
```

**Unhandled triggers throw an exception by default.** Use `.OnUnhandled()` to customize behavior.

➡️ **Learn more:** [Ignore and Unhandled Triggers](docs/ignore-unhandled.md)

---

### No-Data Builder

Build state machines without attaching data. All the same features, just simpler types.

```csharp
var machine = StateMachine<LightState, LightTrigger, LightCommand>.Create()
    .StartWith(LightState.Off)
    .For(LightState.Off)
        .On<LightTrigger.Toggle>()
            .Execute(() => new LightCommand.TurnOn())
            .TransitionTo(LightState.On)
    .Build();

var (newState, commands) = machine.Fire(new LightTrigger.Toggle(), LightState.Off);
```

➡️ **Learn more:** [No-Data Builder](docs/no-data.md)

---

## Tooling & Utilities

### Static Analysis

Detect configuration errors at build time:

- **Unreachable states** — States with no inbound transitions
- **Dead-end states** — States with no outbound transitions
- **Duplicate transitions** — Same trigger configured multiple times
- **Orphaned states** — States neither initial, substate, nor reachable

Analysis runs automatically when you call `.Build()`. All issues are reported before the state machine is created.

➡️ **Learn more:** [Static Analysis](docs/static-analysis.md)

---

### Mermaid Diagram Generation

Annotate your builder methods to automatically generate Mermaid flowcharts at compile time:

```csharp
using FunctionalStateMachine.Diagrams;

[StateMachineDiagram("diagrams/OrderFlow.md")]
public static StateMachine<OrderState, OrderTrigger, OrderCommand> Build()
{
    return StateMachine<OrderState, OrderTrigger, OrderCommand>.Create()
        .StartWith(OrderState.Cart)
        .For(OrderState.Cart)
            .On<OrderTrigger.Checkout>()
                .TransitionTo(OrderState.Processing)
        .Build();
}
```

**Diagrams update automatically** whenever you change the state machine. Perfect for documentation and PRs.

➡️ **Learn more:** [Mermaid Diagram Generation](docs/diagrams.md)

---

### Command Runners (DI Integration)

Optional package that dispatches commands through dependency injection:

```csharp
public sealed class ChargeCardRunner : IAsyncCommandRunner<PaymentCommand.ChargeCard>
{
    private readonly IPaymentGateway _gateway;
    
    public ChargeCardRunner(IPaymentGateway gateway) => _gateway = gateway;
    
    public Task RunAsync(PaymentCommand.ChargeCard command) =>
        _gateway.ChargeAsync(command.Amount, command.CardToken);
}

// Registration
services.AddCommandRunners<PaymentCommand>();

// Usage
var dispatcher = serviceProvider.GetRequiredService<IAsyncCommandDispatcher<PaymentCommand>>();
await dispatcher.RunAsync(commands);
```

**Zero reflection.** The dispatcher is source-generated for maximum performance.

➡️ **Learn more:** [Command Runners](docs/command-runners.md)

---

## Examples

Explore complete, runnable examples in the `/samples` directory:

- **[Basic Samples](samples/Basic)** — Light switch, login session, shopping cart, timer
- **[Vending Machine](samples/VendingMachine/VendingMachineSampleApp)** — Complex hierarchical states, payment flow, inventory management
- **[Stock Purchaser](samples/StockPurchaser/StockPurchaserSampleApp)** — Actor-model example with persistence simulation

---

## Installation

```bash
# Core state machine (required)
dotnet add package FunctionalStateMachine.Core

# Optional: DI-based command dispatcher
dotnet add package FunctionalStateMachine.CommandRunner

# Optional: Build-time diagram generator
dotnet add package FunctionalStateMachine.Diagrams
```

---

## Contributing

Contributions welcome! Please open an issue or PR on GitHub.

---

## License

MIT
