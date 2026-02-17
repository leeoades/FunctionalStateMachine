# Hierarchical States

Hierarchical states let you model parent/child relationships where parent transitions apply to all child states, and parents choose which child state to enter by default. This reduces duplication and better represents real-world state structures.

## Table of Contents

1. [Why Use Hierarchical States](#why-use-hierarchical-states)
2. [Basic Parent-Child Relationship](#basic-parent-child-relationship)
3. [Parent Transitions](#parent-transitions)
4. [Initial Substates](#initial-substates)
5. [Multiple Levels of Hierarchy](#multiple-levels-of-hierarchy)
6. [Entry and Exit with Hierarchies](#entry-and-exit-with-hierarchies)
7. [Complete Example](#complete-example)

---

## Why Use Hierarchical States

**Reduce duplication** — Define common transitions once on the parent instead of on every child  
**Model real hierarchies** — Match your domain's natural structure (authenticated user has multiple substates)  
**Cleaner configuration** — Group related states under a parent  
**Reusable behavior** — Parent actions apply to all children automatically

---

## Basic Parent-Child Relationship

Define parent states and their children:

```csharp
public enum ConnectionState
{
    Disconnected,
    Connected,      // Parent
    Idle,           // Child of Connected
    Transmitting    // Child of Connected
}

public abstract record ConnectionTrigger
{
    public sealed record Connect : ConnectionTrigger;
    public sealed record Disconnect : ConnectionTrigger;
    public sealed record StartTransmit : ConnectionTrigger;
    public sealed record StopTransmit : ConnectionTrigger;
}

public abstract record ConnectionCommand
{
    public sealed record OpenConnection : ConnectionCommand;
    public sealed record CloseConnection : ConnectionCommand;
    public sealed record Log(string Message) : ConnectionCommand;
}

var machine = StateMachine<ConnectionState, ConnectionTrigger, ConnectionCommand>.Create()
    .StartWith(ConnectionState.Disconnected)
    
    .For(ConnectionState.Connected)
        .StartsWith(ConnectionState.Idle)  // When entering Connected, start in Idle
    
    .For(ConnectionState.Idle)
        .SubStateOf(ConnectionState.Connected)  // Declare parent relationship
        .On<ConnectionTrigger.StartTransmit>()
            .TransitionTo(ConnectionState.Transmitting)
    
    .For(ConnectionState.Transmitting)
        .SubStateOf(ConnectionState.Connected)  // Also a child of Connected
        .On<ConnectionTrigger.StopTransmit>()
            .TransitionTo(ConnectionState.Idle)
    
    .Build();
```

**Key points:**
- `Connected` is the parent state
- `Idle` and `Transmitting` are children (declared with `.SubStateOf()`)
- Parent chooses initial child with `.StartsWith()`
- When transitioning to `Connected`, machine actually enters `Idle`

---

## Parent Transitions

Transitions defined on the parent work from **all** child states:

```csharp
var machine = StateMachine<ConnectionState, ConnectionTrigger, ConnectionCommand>.Create()
    .StartWith(ConnectionState.Disconnected)
    
    .For(ConnectionState.Disconnected)
        .On<ConnectionTrigger.Connect>()
            .Execute(() => new ConnectionCommand.OpenConnection())
            .TransitionTo(ConnectionState.Connected)  // Goes to Connected.Idle
    
    .For(ConnectionState.Connected)
        .StartsWith(ConnectionState.Idle)
        .On<ConnectionTrigger.Disconnect>()           // Parent transition
            .Execute(() => new ConnectionCommand.CloseConnection())
            .TransitionTo(ConnectionState.Disconnected)
    
    .For(ConnectionState.Idle)
        .SubStateOf(ConnectionState.Connected)
        .On<ConnectionTrigger.StartTransmit>()
            .TransitionTo(ConnectionState.Transmitting)
    
    .For(ConnectionState.Transmitting)
        .SubStateOf(ConnectionState.Connected)
        .On<ConnectionTrigger.StopTransmit>()
            .TransitionTo(ConnectionState.Idle)
    
    .Build();

// Disconnect works from BOTH Idle and Transmitting
var (state1, cmds1) = machine.Fire(
    new ConnectionTrigger.Disconnect(),
    ConnectionState.Idle);
// state1 == ConnectionState.Disconnected ✅

var (state2, cmds2) = machine.Fire(
    new ConnectionTrigger.Disconnect(),
    ConnectionState.Transmitting);
// state2 == ConnectionState.Disconnected ✅
```

**How it works:**
- `Disconnect` trigger is defined on `Connected` parent
- When in `Idle` (child), `Disconnect` trigger is still available
- When in `Transmitting` (child), `Disconnect` trigger is still available
- **Don't need to define `Disconnect` on each child**

---

## Initial Substates

Use `.StartsWith()` to define which child state to enter:

```csharp
public enum SessionState
{
    Inactive,
    Active,         // Parent
    Anonymous,      // Child of Active
    Authenticated   // Child of Active
}

var machine = StateMachine<SessionState, SessionTrigger, SessionCommand>.Create()
    .StartWith(SessionState.Inactive)
    
    .For(SessionState.Active)
        .StartsWith(SessionState.Anonymous)  // Default child
    
    .For(SessionState.Anonymous)
        .SubStateOf(SessionState.Active)
        .On<SessionTrigger.Login>()
            .TransitionTo(SessionState.Authenticated)
    
    .For(SessionState.Authenticated)
        .SubStateOf(SessionState.Active)
    
    .Build();

// When transitioning to Active, machine enters Anonymous
var (state, _) = machine.Fire(
    new SessionTrigger.Start(),
    SessionState.Inactive);
// Transition says "go to Active"
// Machine actually goes to Active.Anonymous (the initial substate)
```

**Without `.StartsWith()`:** 
Trying to transition to a parent state without specifying the initial child will cause a validation error at build time.

---

## Transition Priority

When both parent and child handle the same trigger, **child takes precedence**:

```csharp
var machine = StateMachine<State, Trigger, Command>.Create()
    .For(State.Parent)
        .StartsWith(State.Child1)
        .On<Trigger.Action>()
            .Execute(() => new Command.ParentAction())
            .TransitionTo(State.SomewhereElse)
    
    .For(State.Child1)
        .SubStateOf(State.Parent)
        .On<Trigger.Action>()                         // Child also handles Action
            .Execute(() => new Command.ChildAction())
            .TransitionTo(State.Child2)
    
    .For(State.Child2)
        .SubStateOf(State.Parent)
        // Child2 doesn't handle Action, so parent's Action applies here
    
    .Build();

// From Child1: child handler wins
var (state1, cmds1) = machine.Fire(new Trigger.Action(), State.Child1);
// cmds1 == [ChildAction()]  ← Child handler used
// state1 == State.Child2

// From Child2: parent handler used
var (state2, cmds2) = machine.Fire(new Trigger.Action(), State.Child2);
// cmds2 == [ParentAction()] ← Parent handler used
// state2 == State.SomewhereElse
```

**Rule:** Child transitions override parent transitions for the same trigger.

---

## Multiple Levels of Hierarchy

You can nest multiple levels of substates:

```csharp
public enum AppState
{
    Offline,
    Online,             // Level 1
    Authenticated,      // Level 2 (child of Online)
    Viewing,            // Level 3 (child of Authenticated)
    Editing             // Level 3 (child of Authenticated)
}

var machine = StateMachine<AppState, AppTrigger, AppCommand>.Create()
    .StartWith(AppState.Offline)
    
    .For(AppState.Online)
        .StartsWith(AppState.Authenticated)
        .On<AppTrigger.NetworkLost>()              // Applies to all descendants
            .TransitionTo(AppState.Offline)
    
    .For(AppState.Authenticated)
        .SubStateOf(AppState.Online)
        .StartsWith(AppState.Viewing)
        .On<AppTrigger.Logout>()                   // Applies to Viewing and Editing
            .TransitionTo(AppState.Online)  // Back to Online (will enter Authenticated)
    
    .For(AppState.Viewing)
        .SubStateOf(AppState.Authenticated)
        .On<AppTrigger.StartEdit>()
            .TransitionTo(AppState.Editing)
    
    .For(AppState.Editing)
        .SubStateOf(AppState.Authenticated)
        .On<AppTrigger.StopEdit>()
            .TransitionTo(AppState.Viewing)
    
    .Build();

// NetworkLost works from BOTH Viewing and Editing (grandchildren of Online)
var (state, _) = machine.Fire(new AppTrigger.NetworkLost(), AppState.Editing);
// state == AppState.Offline ✅
// Parent's parent transition still works
```

---

## Entry and Exit with Hierarchies

Entry and exit commands run when entering/leaving the actual (leaf) state, not the parent:

```csharp
var machine = StateMachine<State, Trigger, Command>.Create()
    .For(State.Parent)
        .StartsWith(State.Child1)
        .OnEntry(() => new Command.Log("Enter Parent"))    // Runs when entering from outside hierarchy
        .OnExit(() => new Command.Log("Exit Parent"))      // Runs when leaving hierarchy
    
    .For(State.Child1)
        .SubStateOf(State.Parent)
        .OnEntry(() => new Command.Log("Enter Child1"))
        .OnExit(() => new Command.Log("Exit Child1"))
        .On<Trigger.Switch>()
            .TransitionTo(State.Child2)
    
    .For(State.Child2)
        .SubStateOf(State.Parent)
        .OnEntry(() => new Command.Log("Enter Child2"))
        .OnExit(() => new Command.Log("Exit Child2"))
    
    .Build();

// Entering the hierarchy from outside
var (_, cmds1) = machine.Fire(new Trigger.GoToParent(), State.Outside);
// cmds1 == [
//   Log("Enter Parent"),   // Parent entry
//   Log("Enter Child1")    // Child entry (initial substate)
// ]

// Moving within the hierarchy (Child1 → Child2)
var (_, cmds2) = machine.Fire(new Trigger.Switch(), State.Child1);
// cmds2 == [
//   Log("Exit Child1"),    // Exit old child
//   Log("Enter Child2")    // Enter new child
// ]
// Parent entry/exit NOT called (staying within Parent)

// Leaving the hierarchy
var (_, cmds3) = machine.Fire(new Trigger.Leave(), State.Child2);
// cmds3 == [
//   Log("Exit Child2"),    // Exit child
//   Log("Exit Parent")     // Exit parent
// ]
```

**Rules:**
- Entering hierarchy = parent entry + initial child entry
- Moving between siblings = old child exit + new child entry (parent entry/exit skip)
- Leaving hierarchy = child exit + parent exit

---

## Complete Example

A media player with hierarchical playback states:

```csharp
public enum PlayerState
{
    Stopped,
    Playing,        // Parent
    PlayingAudio,   // Child of Playing
    PlayingVideo,   // Child of Playing
    Paused,         // Parent
    PausedAudio,    // Child of Paused
    PausedVideo     // Child of Paused
}

public abstract record PlayerTrigger
{
    public sealed record Play : PlayerTrigger;
    public sealed record Pause : PlayerTrigger;
    public sealed record Stop : PlayerTrigger;
    public sealed record SwitchToAudio : PlayerTrigger;
    public sealed record SwitchToVideo : PlayerTrigger;
}

public sealed record PlayerData(
    string CurrentTrack,
    MediaType Type,
    int Position);

public enum MediaType { Audio, Video }

public abstract record PlayerCommand
{
    public sealed record StartPlayback(string Track, MediaType Type) : PlayerCommand;
    public sealed record PausePlayback : PlayerCommand;
    public sealed record ResumePlayback : PlayerCommand;
    public sealed record StopPlayback : PlayerCommand;
    public sealed record SwitchMedia(MediaType Type) : PlayerCommand;
}

var machine = StateMachine<PlayerState, PlayerTrigger, PlayerData, PlayerCommand>.Create()
    .StartWith(PlayerState.Stopped)
    
    .For(PlayerState.Stopped)
        .On<PlayerTrigger.Play>()
            .Execute(data => new PlayerCommand.StartPlayback(data.CurrentTrack, data.Type))
            .TransitionTo(PlayerState.Playing)
    
    // Playing parent state
    .For(PlayerState.Playing)
        .StartsWith(PlayerState.PlayingAudio)
        
        // Parent transitions - apply to BOTH PlayingAudio and PlayingVideo
        .On<PlayerTrigger.Pause>()
            .Execute(() => new PlayerCommand.PausePlayback())
            .TransitionTo(PlayerState.Paused)
        
        .On<PlayerTrigger.Stop>()
            .Execute(() => new PlayerCommand.StopPlayback())
            .TransitionTo(PlayerState.Stopped)
    
    // Playing children
    .For(PlayerState.PlayingAudio)
        .SubStateOf(PlayerState.Playing)
        .On<PlayerTrigger.SwitchToVideo>()
            .ModifyData(data => data with { Type = MediaType.Video })
            .Execute(() => new PlayerCommand.SwitchMedia(MediaType.Video))
            .TransitionTo(PlayerState.PlayingVideo)
    
    .For(PlayerState.PlayingVideo)
        .SubStateOf(PlayerState.Playing)
        .On<PlayerTrigger.SwitchToAudio>()
            .ModifyData(data => data with { Type = MediaType.Audio })
            .Execute(() => new PlayerCommand.SwitchMedia(MediaType.Audio))
            .TransitionTo(PlayerState.PlayingAudio)
    
    // Paused parent state
    .For(PlayerState.Paused)
        .StartsWith(PlayerState.PausedAudio)
        
        // Parent transitions - apply to BOTH PausedAudio and PausedVideo
        .On<PlayerTrigger.Play>()
            .Execute(() => new PlayerCommand.ResumePlayback())
            .TransitionTo(PlayerState.Playing)
        
        .On<PlayerTrigger.Stop>()
            .Execute(() => new PlayerCommand.StopPlayback())
            .TransitionTo(PlayerState.Stopped)
    
    // Paused children
    .For(PlayerState.PausedAudio)
        .SubStateOf(PlayerState.Paused)
        .On<PlayerTrigger.SwitchToVideo>()
            .ModifyData(data => data with { Type = MediaType.Video })
            .TransitionTo(PlayerState.PausedVideo)
    
    .For(PlayerState.PausedVideo)
        .SubStateOf(PlayerState.Paused)
        .On<PlayerTrigger.SwitchToAudio>()
            .ModifyData(data => data with { Type = MediaType.Audio })
            .TransitionTo(PlayerState.PausedAudio)
    
    .Build();

// Usage scenarios

var data = new PlayerData(
    CurrentTrack: "song.mp3",
    Type: MediaType.Audio,
    Position: 0);

// Scenario 1: Play audio
var (state1, data1, cmds1) = machine.Fire(
    new PlayerTrigger.Play(),
    PlayerState.Stopped,
    data);
// state1 == PlayerState.PlayingAudio (Playing's initial substate)
// cmds1 == [StartPlayback("song.mp3", Audio)]

// Scenario 2: Pause (parent transition works from child state)
var (state2, data2, cmds2) = machine.Fire(
    new PlayerTrigger.Pause(),
    state1,
    data1);
// state2 == PlayerState.PausedAudio (Paused's initial substate)
// cmds2 == [PausePlayback()]
// Pause trigger defined on Playing parent, works from PlayingAudio child

// Scenario 3: Switch to video while paused
var (state3, data3, cmds3) = machine.Fire(
    new PlayerTrigger.SwitchToVideo(),
    state2,
    data2);
// state3 == PlayerState.PausedVideo
// data3.Type == MediaType.Video

// Scenario 4: Resume (parent transition)
var (state4, data4, cmds4) = machine.Fire(
    new PlayerTrigger.Play(),
    state3,
    data3);
// state4 == PlayerState.PlayingVideo (Playing's default is Audio, but we're coming from PausedVideo)
// Wait, this is a problem! Let me fix this...
```

**Note:** The above example has a subtle issue—when resuming from `PausedVideo`, we want to go to `PlayingVideo`, not `PlayingAudio`. This requires a more sophisticated design. Here's the fix:

```csharp
// Better approach: Map paused states to their playing counterparts

.For(PlayerState.PausedAudio)
    .SubStateOf(PlayerState.Paused)
    .On<PlayerTrigger.Play>()  // Override parent
        .Execute(() => new PlayerCommand.ResumePlayback())
        .TransitionTo(PlayerState.PlayingAudio)
    // other transitions...

.For(PlayerState.PausedVideo)
    .SubStateOf(PlayerState.Paused)
    .On<PlayerTrigger.Play>()  // Override parent
        .Execute(() => new PlayerCommand.ResumePlayback())
        .TransitionTo(PlayerState.PlayingVideo)
    // other transitions...
```

**What's happening:**
1. Two hierarchies: Playing (Audio/Video) and Paused (Audio/Video)
2. Common actions (Stop, Pause, Play) defined on parents
3. Media switching defined on children
4. Children override parent's Play to resume to correct playing state
5. Greatly reduces duplication—Stop works from all 4 substates without repeating

---

## Best Practices

✅ **Use hierarchies for related states**  
Group states that share common transitions or behavior.

✅ **Define common transitions on parent**  
Avoid repeating the same transition on every child.

✅ **Always specify initial substate**  
Use `.StartsWith()` on every parent.

✅ **Let children override when needed**  
Child transitions take precedence over parent transitions.

❌ **Don't overuse hierarchies**  
Not every group of states needs to be hierarchical. Use when it reduces duplication.

❌ **Avoid deep nesting**  
Two levels is usually enough. Deeper hierarchies get complex.

---

## Common Patterns

### Authenticated vs Unauthenticated
```csharp
.For(State.Authenticated)
    .StartsWith(State.ViewingProfile)
    .On<Trigger.Logout>()  // Works from all authenticated substates
        .TransitionTo(State.Unauthenticated)
```

### Connection with substates
```csharp
.For(State.Connected)
    .StartsWith(State.Idle)
    .On<Trigger.Disconnect>()  // Works from all connection substates
        .TransitionTo(State.Disconnected)
```

### Error handling from any substate
```csharp
.For(State.Processing)
    .StartsWith(State.Step1)
    .On<Trigger.Error>()  // Works from all processing steps
        .TransitionTo(State.Failed)
```

---

## Next Steps

- Use with [Entry/Exit Commands](Entry-and-Exit-Commands.md) to understand how entry/exit work in hierarchies
- Combine with [Guards](Guards-and-Conditional-Flows.md) for conditional parent transitions
- See [Internal Transitions](Internal-Transitions.md) which children can have without affecting parent
