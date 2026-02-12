# Entry and Exit Commands

Entry and exit commands run automatically when a state is entered or left. They're perfect for lifecycle actions like logging, notifications, or setting up/tearing down resources.

## Table of Contents

1. [Why Use Entry and Exit Commands](#why-use-entry-and-exit-commands)
2. [Basic Entry Commands](#basic-entry-commands)
3. [Basic Exit Commands](#basic-exit-commands)
4. [Entry and Exit Together](#entry-and-exit-together)
5. [When Commands Run](#when-commands-run)
6. [Accessing State and Data](#accessing-state-and-data)
7. [Complete Example](#complete-example)

---

## Why Use Entry and Exit Commands

**Automatic lifecycle management** — Actions run whenever a state is entered or left, no matter which trigger caused it  
**Centralized behavior** — Put all entry/exit logic in one place per state  
**Clean transitions** — Keep transition logic focused on the transition itself  
**Audit trail** — Automatically log when states change

---

## Basic Entry Commands

Commands that run when entering a state:

```csharp
public enum ServerState { Stopped, Starting, Running }

public abstract record ServerTrigger
{
    public sealed record Start : ServerTrigger;
}

public abstract record ServerCommand
{
    public sealed record LogMessage(string Message) : ServerCommand;
    public sealed record InitializeResources : ServerCommand;
}

var machine = StateMachine<ServerState, ServerTrigger, ServerCommand>.Create()
    .StartWith(ServerState.Stopped)
    .For(ServerState.Starting)
        .OnEntry(() => new ServerCommand.LogMessage("Server is starting..."))
        .OnEntry(() => new ServerCommand.InitializeResources())
    .Build();
```

**How it works:**
- `.OnEntry()` defines commands that emit when the state is entered
- Multiple `.OnEntry()` calls are allowed—commands emit in order
- Entry commands run **before** any commands from the transition itself

---

## Basic Exit Commands

Commands that run when leaving a state:

```csharp
var machine = StateMachine<ServerState, ServerTrigger, ServerCommand>.Create()
    .StartWith(ServerState.Running)
    .For(ServerState.Running)
        .OnExit(() => new ServerCommand.LogMessage("Server is stopping..."))
    .Build();
```

**How it works:**
- `.OnExit()` defines commands that emit when leaving the state
- Exit commands run **after** entry commands and transition commands
- Multiple `.OnExit()` calls are allowed—commands emit in order

---

## Entry and Exit Together

States can have both entry and exit commands:

```csharp
public enum SessionState { Idle, Active, Expired }

public abstract record SessionTrigger
{
    public sealed record Login : SessionTrigger;
    public sealed record Logout : SessionTrigger;
}

public sealed record SessionData(Guid UserId);

public abstract record SessionCommand
{
    public sealed record LogActivity(string Message) : SessionCommand;
    public sealed record StartSession(Guid UserId) : SessionCommand;
    public sealed record EndSession(Guid UserId) : SessionCommand;
}

var machine = StateMachine<SessionState, SessionTrigger, SessionData, SessionCommand>.Create()
    .StartWith(SessionState.Idle)
    
    .For(SessionState.Active)
        .OnEntry(data => new SessionCommand.LogActivity($"User {data.UserId} logged in"))
        .OnEntry(data => new SessionCommand.StartSession(data.UserId))
        .OnExit(data => new SessionCommand.LogActivity($"User {data.UserId} logged out"))
        .OnExit(data => new SessionCommand.EndSession(data.UserId))
        .On<SessionTrigger.Logout>()
            .TransitionTo(SessionState.Idle)
    
    .Build();

var data = new SessionData(UserId: Guid.NewGuid());
var (newState, newData, commands) = machine.Fire(
    new SessionTrigger.Logout(), 
    SessionState.Active, 
    data);
// Commands emitted in order:
// 1. Exit: LogActivity("User {guid} logged out")
// 2. Exit: EndSession(guid)
```

**Order of execution:**
1. Exit commands from the old state
2. Entry commands from the new state
3. Commands from the transition's `.Execute()` steps

---

## When Commands Run

### Entry commands run when:
- ✅ Transitioning **into** the state from another state
- ✅ Calling `.Start(data)` if the initial state is this state
- ❌ **NOT** on internal transitions (no `TransitionTo`)

### Exit commands run when:
- ✅ Transitioning **out of** the state to another state
- ❌ **NOT** on internal transitions (no `TransitionTo`)

### Example: Internal Transition

```csharp
var machine = StateMachine<ServerState, ServerTrigger, ServerCommand>.Create()
    .For(ServerState.Running)
        .OnEntry(() => new ServerCommand.LogMessage("Entered Running"))
        .OnExit(() => new ServerCommand.LogMessage("Exited Running"))
        .On<ServerTrigger.Heartbeat>()
            // No TransitionTo = internal transition
            .Execute(() => new ServerCommand.LogMessage("Heartbeat"))
    .Build();

var (_, _, commands) = machine.Fire(
    new ServerTrigger.Heartbeat(), 
    ServerState.Running);
// commands == [LogMessage("Heartbeat")]
// Entry and exit NOT run because state didn't change
```

---

## Accessing State and Data

Entry and exit commands can access state and data:

### Simple form (no parameters)

```csharp
.OnEntry(() => new Command.Log("State entered"))
.OnExit(() => new Command.Log("State exited"))
```

### With data

```csharp
.OnEntry(data => new Command.Log($"User {data.UserId} entered"))
.OnExit(data => new Command.Log($"User {data.UserId} exited"))
```

### With state and data

```csharp
.OnEntry((state, data) => new Command.Log($"Entered {state} for user {data.UserId}"))
.OnExit((state, data) => new Command.Log($"Exited {state} for user {data.UserId}"))
```

---

## Multiple Commands per Entry/Exit

Chain multiple entry or exit actions:

```csharp
var machine = StateMachine<OrderState, OrderTrigger, OrderData, OrderCommand>.Create()
    .For(OrderState.Shipped)
        .OnEntry(data => new OrderCommand.SendNotification(data.Email))
        .OnEntry(data => new OrderCommand.UpdateInventory(data.Items))
        .OnEntry(data => new OrderCommand.LogShipment(data.OrderId))
    .Build();
```

All three commands run when entering `Shipped`, in the order defined.

---

## Complete Example

A connection manager with detailed lifecycle logging:

```csharp
public enum ConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting,
    Failed
}

public abstract record ConnectionTrigger
{
    public sealed record Connect : ConnectionTrigger;
    public sealed record ConnectionEstablished : ConnectionTrigger;
    public sealed record ConnectionLost : ConnectionTrigger;
    public sealed record Disconnect : ConnectionTrigger;
    public sealed record RetryFailed : ConnectionTrigger;
}

public sealed record ConnectionData(
    string Host,
    int Port,
    int RetryAttempts,
    DateTime? LastConnectedAt);

public abstract record ConnectionCommand
{
    public sealed record Log(string Message) : ConnectionCommand;
    public sealed record OpenSocket(string Host, int Port) : ConnectionCommand;
    public sealed record CloseSocket : ConnectionCommand;
    public sealed record StartRetryTimer : ConnectionCommand;
    public sealed record StopRetryTimer : ConnectionCommand;
    public sealed record NotifyConnected : ConnectionCommand;
    public sealed record NotifyDisconnected : ConnectionCommand;
}

var machine = StateMachine<ConnectionState, ConnectionTrigger, ConnectionData, ConnectionCommand>
    .Create()
    .StartWith(ConnectionState.Disconnected)
    
    .For(ConnectionState.Disconnected)
        .OnEntry(() => new ConnectionCommand.Log("Connection is disconnected"))
        .OnEntry(() => new ConnectionCommand.NotifyDisconnected())
        .On<ConnectionTrigger.Connect>()
            .TransitionTo(ConnectionState.Connecting)
    
    .For(ConnectionState.Connecting)
        .OnEntry(data => new ConnectionCommand.Log($"Connecting to {data.Host}:{data.Port}"))
        .OnEntry(data => new ConnectionCommand.OpenSocket(data.Host, data.Port))
        .OnExit(() => new ConnectionCommand.Log("Exiting connecting state"))
        .On<ConnectionTrigger.ConnectionEstablished>()
            .ModifyData(data => data with 
            { 
                LastConnectedAt = DateTime.UtcNow,
                RetryAttempts = 0
            })
            .TransitionTo(ConnectionState.Connected)
        .On<ConnectionTrigger.ConnectionLost>()
            .TransitionTo(ConnectionState.Reconnecting)
    
    .For(ConnectionState.Connected)
        .OnEntry(() => new ConnectionCommand.Log("Connection established"))
        .OnEntry(() => new ConnectionCommand.NotifyConnected())
        .OnExit(() => new ConnectionCommand.Log("Leaving connected state"))
        .OnExit(() => new ConnectionCommand.CloseSocket())
        .On<ConnectionTrigger.ConnectionLost>()
            .TransitionTo(ConnectionState.Reconnecting)
        .On<ConnectionTrigger.Disconnect>()
            .TransitionTo(ConnectionState.Disconnected)
    
    .For(ConnectionState.Reconnecting)
        .OnEntry(data => new ConnectionCommand.Log($"Reconnecting (attempt {data.RetryAttempts})"))
        .OnEntry(() => new ConnectionCommand.StartRetryTimer())
        .OnExit(() => new ConnectionCommand.StopRetryTimer())
        .On<ConnectionTrigger.ConnectionEstablished>()
            .ModifyData(data => data with 
            { 
                LastConnectedAt = DateTime.UtcNow,
                RetryAttempts = 0
            })
            .TransitionTo(ConnectionState.Connected)
        .On<ConnectionTrigger.RetryFailed>()
            .ModifyData(data => data with { RetryAttempts = data.RetryAttempts + 1 })
            .TransitionTo(ConnectionState.Failed)
    
    .For(ConnectionState.Failed)
        .OnEntry(() => new ConnectionCommand.Log("Connection failed"))
        .On<ConnectionTrigger.Connect>()
            .TransitionTo(ConnectionState.Connecting)
    
    .Build();

// Usage scenario
var data = new ConnectionData(
    Host: "api.example.com",
    Port: 443,
    RetryAttempts: 0,
    LastConnectedAt: null);

// Connect
var (state1, data1, cmds1) = machine.Fire(
    new ConnectionTrigger.Connect(),
    ConnectionState.Disconnected,
    data);
// state1 == ConnectionState.Connecting
// cmds1 includes:
//   - Log("Connection is disconnected") (exit Disconnected)
//   - NotifyDisconnected() (exit Disconnected)
//   - Log("Connecting to api.example.com:443") (enter Connecting)
//   - OpenSocket("api.example.com", 443) (enter Connecting)

// Connection established
var (state2, data2, cmds2) = machine.Fire(
    new ConnectionTrigger.ConnectionEstablished(),
    state1,
    data1);
// state2 == ConnectionState.Connected
// cmds2 includes:
//   - Log("Exiting connecting state") (exit Connecting)
//   - Log("Connection established") (enter Connected)
//   - NotifyConnected() (enter Connected)

// Connection lost
var (state3, data3, cmds3) = machine.Fire(
    new ConnectionTrigger.ConnectionLost(),
    state2,
    data2);
// state3 == ConnectionState.Reconnecting
// cmds3 includes:
//   - Log("Leaving connected state") (exit Connected)
//   - CloseSocket() (exit Connected)
//   - Log("Reconnecting (attempt 0)") (enter Reconnecting)
//   - StartRetryTimer() (enter Reconnecting)
```

**What's happening:**
1. Every state has clear entry/exit logging
2. Entry commands set up resources (open socket, start timer)
3. Exit commands clean up resources (close socket, stop timer)
4. All lifecycle actions are automatic and consistent
5. Transitions themselves focus only on changing state

---

## Best Practices

✅ **Use entry/exit for lifecycle actions**  
Logging, resource setup/teardown, notifications belong in entry/exit.

✅ **Keep entry/exit commands simple**  
They run on every transition in/out, so they should be lightweight.

✅ **Remember: internal transitions skip entry/exit**  
If you need commands on every trigger, use `.Execute()` on the transition.

✅ **Chain multiple OnEntry/OnExit for clarity**  
Breaking up complex setup into multiple entry commands is more readable.

❌ **Don't put transition-specific logic in entry/exit**  
If logic is specific to one transition, use `.Execute()` on that transition instead.

---

## Entry/Exit vs Execute

### Use OnEntry/OnExit when:
- Action should run **regardless of which trigger** caused the transition
- Setting up or tearing down state-specific resources
- Logging for audit trails

### Use Execute when:
- Action is **specific to one transition**
- Emitting commands based on trigger data
- Business logic tied to a specific trigger

**Example:**

```csharp
.For(State.Processing)
    .OnEntry(() => new Command.StartTimer())      // Always start timer
    .OnExit(() => new Command.StopTimer())        // Always stop timer
    .On<Trigger.Complete>()
        .Execute(() => new Command.SendSuccess())  // Only on Complete trigger
    .On<Trigger.Cancel>()
        .Execute(() => new Command.SendCancellation())  // Only on Cancel trigger
```

---

## Next Steps

- Combine with [Execute Steps](execute-steps.md) for transition-specific commands
- Use with [Hierarchical States](hierarchical-states.md) for parent/child entry/exit
- See [Internal Transitions](internal-transitions.md) to understand when entry/exit skip
