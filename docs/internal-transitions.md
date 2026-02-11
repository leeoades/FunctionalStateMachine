# Internal Transitions

Internal transitions let you handle triggers without changing state. When you omit `TransitionTo()`, the state machine stays in the current state, skipping entry and exit commands while still allowing data updates and command execution.

## Table of Contents

1. [Why Use Internal Transitions](#why-use-internal-transitions)
2. [Basic Internal Transition](#basic-internal-transition)
3. [When to Use Internal Transitions](#when-to-use-internal-transitions)
4. [Internal vs External Transitions](#internal-vs-external-transitions)
5. [Updating Data Without Changing State](#updating-data-without-changing-state)
6. [Complete Example](#complete-example)

---

## Why Use Internal Transitions

**High-frequency updates** — Handle events like heartbeats or ticks without state change overhead  
**In-place modifications** — Update data without triggering entry/exit commands  
**Performance** — Skip unnecessary entry/exit logic when state doesn't actually change  
**Clear intent** — Explicitly show that an event doesn't cause a state change

---

## Basic Internal Transition

Simply omit `.TransitionTo()` to create an internal transition:

```csharp
public enum ServerState { Stopped, Running }

public abstract record ServerTrigger
{
    public sealed record Heartbeat : ServerTrigger;
    public sealed record Stop : ServerTrigger;
}

public abstract record ServerCommand
{
    public sealed record RecordHeartbeat(DateTime Timestamp) : ServerCommand;
    public sealed record Shutdown : ServerCommand;
}

var machine = StateMachine<ServerState, ServerTrigger, ServerCommand>.Create()
    .StartWith(ServerState.Running)
    .For(ServerState.Running)
        .On<ServerTrigger.Heartbeat>()
            .Execute(() => new ServerCommand.RecordHeartbeat(DateTime.UtcNow))
            // No TransitionTo = internal transition
        .On<ServerTrigger.Stop>()
            .Execute(() => new ServerCommand.Shutdown())
            .TransitionTo(ServerState.Stopped)
    .Build();

var (newState, commands) = machine.Fire(
    new ServerTrigger.Heartbeat(),
    ServerState.Running);
// newState == ServerState.Running (unchanged)
// commands == [RecordHeartbeat(timestamp)]
```

**Key points:**
- No `.TransitionTo()` means the state stays the same
- Commands can still be emitted
- Entry and exit commands **do not run**

---

## When to Use Internal Transitions

### ✅ Use internal transitions for:

**High-frequency events** — Heartbeats, ticks, status updates
```csharp
.On<Trigger.Heartbeat>()
    .Execute(() => new Command.UpdateLastSeen())
// Runs frequently, no need for state change
```

**Data updates in place** — Modifying counters, timestamps, or cached values
```csharp
.On<Trigger.IncrementCounter>()
    .ModifyData(data => data with { Count = data.Count + 1 })
```

**State-specific actions without state change** — Actions that happen within a state
```csharp
.On<Trigger.LogActivity>()
    .Execute(data => new Command.Log($"Activity in {data.CurrentState}"))
```

### ❌ Avoid internal transitions when:

- You actually need to change state
- Entry/exit commands are important for the action
- The semantic meaning is a state transition

---

## Internal vs External Transitions

### External Transition (with TransitionTo)

```csharp
var machine = StateMachine<State, Trigger, Command>.Create()
    .For(State.Active)
        .OnEntry(() => new Command.Log("Entered Active"))
        .OnExit(() => new Command.Log("Exited Active"))
        .On<Trigger.Refresh>()
            .Execute(() => new Command.Log("Refreshing"))
            .TransitionTo(State.Active)  // ← Transitions to SAME state
    .Build();

var (_, commands) = machine.Fire(new Trigger.Refresh(), State.Active);
// commands == [
//   Log("Exited Active"),    // Exit runs
//   Log("Entered Active"),   // Entry runs
//   Log("Refreshing")        // Execute runs
// ]
```

### Internal Transition (without TransitionTo)

```csharp
var machine = StateMachine<State, Trigger, Command>.Create()
    .For(State.Active)
        .OnEntry(() => new Command.Log("Entered Active"))
        .OnExit(() => new Command.Log("Exited Active"))
        .On<Trigger.Refresh>()
            .Execute(() => new Command.Log("Refreshing"))
            // No TransitionTo = internal transition
    .Build();

var (_, commands) = machine.Fire(new Trigger.Refresh(), State.Active);
// commands == [
//   Log("Refreshing")        // Only execute runs
// ]
// Entry and exit DO NOT run
```

**The difference:**
- **External to same state:** Entry and exit both run
- **Internal:** Entry and exit skipped, only execute steps run

---

## Updating Data Without Changing State

Internal transitions can still modify data:

```csharp
public enum SessionState { Active, Expired }

public abstract record SessionTrigger
{
    public sealed record Activity : SessionTrigger;
    public sealed record Expire : SessionTrigger;
}

public sealed record SessionData(
    Guid UserId,
    DateTime LastActivity,
    int ActivityCount);

public abstract record SessionCommand
{
    public sealed record UpdateMetrics(int Count) : SessionCommand;
}

var machine = StateMachine<SessionState, SessionTrigger, SessionData, SessionCommand>.Create()
    .StartWith(SessionState.Active)
    .For(SessionState.Active)
        .On<SessionTrigger.Activity>()
            .ModifyData(data => data with
            {
                LastActivity = DateTime.UtcNow,
                ActivityCount = data.ActivityCount + 1
            })
            .Execute(data => new SessionCommand.UpdateMetrics(data.ActivityCount))
            // No TransitionTo = stays in Active
        .On<SessionTrigger.Expire>()
            .TransitionTo(SessionState.Expired)
    .Build();

var data = new SessionData(
    UserId: Guid.NewGuid(),
    LastActivity: DateTime.UtcNow.AddMinutes(-5),
    ActivityCount: 10);

var (newState, newData, commands) = machine.Fire(
    new SessionTrigger.Activity(),
    SessionState.Active,
    data);
// newState == SessionState.Active (unchanged)
// newData.ActivityCount == 11 (incremented)
// newData.LastActivity updated to now
// commands == [UpdateMetrics(11)]
```

**What's happening:**
1. Trigger fires while in `Active` state
2. Data is modified (timestamp and counter updated)
3. Command is emitted with new data
4. State remains `Active`
5. No entry/exit commands run

---

## Complete Example

A download manager that tracks progress without changing state:

```csharp
public enum DownloadState
{
    Idle,
    Downloading,
    Paused,
    Completed,
    Failed
}

public abstract record DownloadTrigger
{
    public sealed record Start : DownloadTrigger;
    public sealed record ProgressUpdate(int BytesDownloaded) : DownloadTrigger;
    public sealed record Pause : DownloadTrigger;
    public sealed record Resume : DownloadTrigger;
    public sealed record Complete : DownloadTrigger;
    public sealed record Error(string Message) : DownloadTrigger;
}

public sealed record DownloadData(
    string FileName,
    int TotalBytes,
    int DownloadedBytes,
    DateTime StartedAt,
    DateTime LastUpdate);

public abstract record DownloadCommand
{
    public sealed record UpdateUI(int Downloaded, int Total, int Percent) : DownloadCommand;
    public sealed record LogProgress(int Percent) : DownloadCommand;
    public sealed record StartDownload(string FileName) : DownloadCommand;
    public sealed record PauseDownload : DownloadCommand;
    public sealed record ResumeDownload : DownloadCommand;
    public sealed record NotifyComplete : DownloadCommand;
    public sealed record NotifyError(string Message) : DownloadCommand;
}

var machine = StateMachine<DownloadState, DownloadTrigger, DownloadData, DownloadCommand>
    .Create()
    .StartWith(DownloadState.Idle)
    
    .For(DownloadState.Idle)
        .On<DownloadTrigger.Start>()
            .ModifyData(data => data with { StartedAt = DateTime.UtcNow })
            .Execute(data => new DownloadCommand.StartDownload(data.FileName))
            .TransitionTo(DownloadState.Downloading)
    
    .For(DownloadState.Downloading)
        .OnEntry(() => new DownloadCommand.LogProgress(0))
        .OnExit(data => new DownloadCommand.LogProgress(
            data.DownloadedBytes * 100 / data.TotalBytes))
        
        // Internal transition - happens many times per second
        .On<DownloadTrigger.ProgressUpdate>()
            .ModifyData((data, trigger) => data with
            {
                DownloadedBytes = trigger.BytesDownloaded,
                LastUpdate = DateTime.UtcNow
            })
            .Execute(data => new DownloadCommand.UpdateUI(
                data.DownloadedBytes,
                data.TotalBytes,
                data.DownloadedBytes * 100 / data.TotalBytes))
            // No TransitionTo = stays in Downloading
            // Entry/Exit NOT called on every progress update
        
        .On<DownloadTrigger.Pause>()
            .Execute(() => new DownloadCommand.PauseDownload())
            .TransitionTo(DownloadState.Paused)
        
        .On<DownloadTrigger.Complete>()
            .Execute(() => new DownloadCommand.NotifyComplete())
            .TransitionTo(DownloadState.Completed)
        
        .On<DownloadTrigger.Error>()
            .Execute(trigger => new DownloadCommand.NotifyError(trigger.Message))
            .TransitionTo(DownloadState.Failed)
    
    .For(DownloadState.Paused)
        .On<DownloadTrigger.Resume>()
            .Execute(() => new DownloadCommand.ResumeDownload())
            .TransitionTo(DownloadState.Downloading)
    
    .For(DownloadState.Completed)
        // Terminal state
    
    .For(DownloadState.Failed)
        // Terminal state
    
    .Build();

// Usage scenario

var data = new DownloadData(
    FileName: "large-file.zip",
    TotalBytes: 100_000_000,  // 100 MB
    DownloadedBytes: 0,
    StartedAt: DateTime.MinValue,
    LastUpdate: DateTime.MinValue);

// Start download
var (state1, data1, cmds1) = machine.Fire(
    new DownloadTrigger.Start(),
    DownloadState.Idle,
    data);
// state1 == DownloadState.Downloading
// cmds1 == [
//   LogProgress(0),                          // OnEntry
//   StartDownload("large-file.zip")          // Execute
// ]

// Progress update 1 (10% complete) - INTERNAL TRANSITION
var (state2, data2, cmds2) = machine.Fire(
    new DownloadTrigger.ProgressUpdate(BytesDownloaded: 10_000_000),
    state1,
    data1);
// state2 == DownloadState.Downloading (unchanged)
// data2.DownloadedBytes == 10_000_000
// cmds2 == [UpdateUI(10000000, 100000000, 10)]
// OnEntry/OnExit NOT called

// Progress update 2 (25% complete) - INTERNAL TRANSITION
var (state3, data3, cmds3) = machine.Fire(
    new DownloadTrigger.ProgressUpdate(BytesDownloaded: 25_000_000),
    state2,
    data2);
// state3 == DownloadState.Downloading (unchanged)
// data3.DownloadedBytes == 25_000_000
// cmds3 == [UpdateUI(25000000, 100000000, 25)]
// OnEntry/OnExit NOT called

// ... many more progress updates ...

// Complete download - EXTERNAL TRANSITION
var (state4, data4, cmds4) = machine.Fire(
    new DownloadTrigger.Complete(),
    state3,
    data3);
// state4 == DownloadState.Completed
// cmds4 == [
//   LogProgress(25),              // OnExit (with current progress)
//   NotifyComplete()              // Execute
// ]
```

**What's happening:**
1. **Start** transitions from Idle → Downloading (external transition)
2. **ProgressUpdate** stays in Downloading (internal transition)
   - Happens potentially hundreds of times
   - Updates data with new byte count
   - Emits UI update command
   - **Does NOT** trigger entry/exit (performance win!)
3. **Complete** transitions to Completed (external transition)
   - Exit command runs with final progress

**Why internal transitions are perfect here:**
- Progress updates happen very frequently (every 100ms, maybe)
- We don't want to log entry/exit hundreds of times per download
- We only care about logging when actually entering or leaving the state
- Data still gets updated, UI still gets commands
- Much more efficient than external transitions

---

## Best Practices

✅ **Use for high-frequency events**  
Heartbeats, progress updates, ticks—events that happen often without changing meaning.

✅ **Update data freely**  
Internal transitions can still modify data with `ModifyData`.

✅ **Emit commands as needed**  
Use `.Execute()` to generate commands during internal transitions.

✅ **Skip entry/exit overhead**  
When you don't need setup/teardown, internal transitions are more efficient.

❌ **Don't use when entry/exit matter**  
If entering/leaving the state has semantic meaning, use an external transition.

❌ **Don't mix semantics**  
If an event logically means "leave and re-enter", use `.TransitionTo(sameState)`.

---

## When Entry/Exit Matter

### Example: State machine with important entry action

```csharp
.For(State.Connected)
    .OnEntry(() => new Command.OpenConnection())   // Important setup
    .OnExit(() => new Command.CloseConnection())   // Important cleanup
    .On<Trigger.Reconnect>()
        .TransitionTo(State.Connected)  // ← External transition
        // We WANT entry/exit to run (close old, open new)
```

### Example: State machine where entry/exit don't matter

```csharp
.For(State.Connected)
    .On<Trigger.Ping>()
        // No TransitionTo = internal
        .Execute(() => new Command.RecordPing())
        // We DON'T want entry/exit overhead for every ping
```

---

## Common Patterns

### Heartbeat tracking
```csharp
.On<Trigger.Heartbeat>()
    .ModifyData(data => data with { LastSeen = DateTime.UtcNow })
```

### Counter increment
```csharp
.On<Trigger.Increment>()
    .ModifyData(data => data with { Count = data.Count + 1 })
    .Execute(data => new Command.Display(data.Count))
```

### Activity logging without state change
```csharp
.On<Trigger.LogActivity>()
    .Execute(data => new Command.Log($"Activity: {data.Description}"))
```

### Progress updates
```csharp
.On<Trigger.Progress>()
    .ModifyData((data, trigger) => data with { Percent = trigger.Percent })
    .Execute(data => new Command.UpdateUI(data.Percent))
```

---

## Next Steps

- Learn about [Entry and Exit Commands](entry-exit.md) to understand what gets skipped
- See [Execute Steps](execute-steps.md) for emitting commands in transitions
- Compare with [Hierarchical States](hierarchical-states.md) where substates can have internal transitions
