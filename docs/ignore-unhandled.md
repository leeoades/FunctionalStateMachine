# Ignore and Unhandled Triggers

Control how your state machine handles triggers that aren't explicitly configured. You can explicitly ignore specific triggers in certain states, or define global behavior for all unhandled triggers.

## Table of Contents

1. [Why Handle Unhandled Triggers](#why-handle-unhandled-triggers)
2. [Default Behavior](#default-behavior)
3. [Ignoring Specific Triggers](#ignoring-specific-triggers)
4. [Global Unhandled Handler](#global-unhandled-handler)
5. [Ignore vs Unhandled](#ignore-vs-unhandled)
6. [Complete Example](#complete-example)

---

## Why Handle Unhandled Triggers

**Explicit behavior** — Make it clear when a trigger is intentionally ignored vs accidentally missing  
**Graceful degradation** — Handle unexpected triggers without crashing  
**Logging and metrics** — Track which triggers happen in which states  
**Flexible design** — Some states need to ignore triggers that other states handle

---

## Default Behavior

By default, firing an unhandled trigger throws an exception:

```csharp
public enum DocumentState { Draft, Published }

public abstract record DocumentTrigger
{
    public sealed record Publish : DocumentTrigger;
    public sealed record Archive : DocumentTrigger;
}

public abstract record DocumentCommand
{
    public sealed record PublishDocument : DocumentCommand;
}

var machine = StateMachine<DocumentState, DocumentTrigger, DocumentCommand>.Create()
    .StartWith(DocumentState.Draft)
    .For(DocumentState.Draft)
        .On<DocumentTrigger.Publish>()
            .Execute(() => new DocumentCommand.PublishDocument())
            .TransitionTo(DocumentState.Published)
    .For(DocumentState.Published)
        // Archive trigger NOT configured here
    .Build();

// This works
var (state1, cmds1) = machine.Fire(
    new DocumentTrigger.Publish(),
    DocumentState.Draft);
// state1 == DocumentState.Published ✅

// This throws an exception
try
{
    var (state2, cmds2) = machine.Fire(
        new DocumentTrigger.Archive(),  // Not configured!
        DocumentState.Published);
}
catch (InvalidOperationException ex)
{
    // Exception: No transition defined for trigger 'Archive' in state 'Published'
}
```

**Why this is good:** Prevents silent failures. You know immediately when a trigger isn't handled.

**Why you might want different behavior:** Sometimes triggers should be silently ignored in certain states.

---

## Ignoring Specific Triggers

Use `.Ignore()` to explicitly mark a trigger as "do nothing" in a specific state:

```csharp
var machine = StateMachine<DocumentState, DocumentTrigger, DocumentCommand>.Create()
    .StartWith(DocumentState.Draft)
    .For(DocumentState.Draft)
        .On<DocumentTrigger.Publish>()
            .Execute(() => new DocumentCommand.PublishDocument())
            .TransitionTo(DocumentState.Published)
        .On<DocumentTrigger.Archive>()
            .Ignore()  // Explicitly ignore Archive in Draft state
    .For(DocumentState.Published)
        .On<DocumentTrigger.Archive>()
            .Execute(() => new DocumentCommand.ArchiveDocument())
            .TransitionTo(DocumentState.Archived)
    .Build();

// Archive in Draft state now does nothing (no exception)
var (state, commands) = machine.Fire(
    new DocumentTrigger.Archive(),
    DocumentState.Draft);
// state == DocumentState.Draft (unchanged)
// commands == [] (empty)
// No exception! ✅
```

**When to use `.Ignore()`:**
- Trigger makes sense in some states but not others
- You want to be explicit that ignoring is intentional
- Different from forgetting to configure the trigger

---

## Global Unhandled Handler

Use `.OnUnhandled()` to define behavior for **all** unhandled triggers:

```csharp
public abstract record DocumentCommand
{
    public sealed record PublishDocument : DocumentCommand;
    public sealed record LogWarning(string Message, string TriggerName, string StateName) : DocumentCommand;
}

var machine = StateMachine<DocumentState, DocumentTrigger, DocumentCommand>.Create()
    .OnUnhandled()
        .Execute((trigger, state) => 
            new DocumentCommand.LogWarning(
                $"Unhandled trigger {trigger.GetType().Name} in state {state}",
                trigger.GetType().Name,
                state.ToString()))
    .StartWith(DocumentState.Draft)
    .For(DocumentState.Draft)
        .On<DocumentTrigger.Publish>()
            .Execute(() => new DocumentCommand.PublishDocument())
            .TransitionTo(DocumentState.Published)
    .For(DocumentState.Published)
        // No Archive configured
    .Build();

// Unhandled trigger now logs instead of throwing
var (state, commands) = machine.Fire(
    new DocumentTrigger.Archive(),
    DocumentState.Published);
// state == DocumentState.Published (unchanged)
// commands == [LogWarning("Unhandled trigger Archive in state Published", ...)]
// No exception! ✅
```

**How it works:**
- `.OnUnhandled()` is defined at the machine level (before `.For()` calls)
- When a trigger has no configured transition, the unhandled handler runs
- The handler can emit commands
- The state doesn't change

---

## OnUnhandled with Ignore

You can use `.OnUnhandled().Ignore()` to silently ignore all unhandled triggers:

```csharp
var machine = StateMachine<DocumentState, DocumentTrigger, DocumentCommand>.Create()
    .OnUnhandled()
        .Ignore()  // Ignore ALL unhandled triggers globally
    .StartWith(DocumentState.Draft)
    .For(DocumentState.Draft)
        .On<DocumentTrigger.Publish>()
            .TransitionTo(DocumentState.Published)
    .Build();

// Any unhandled trigger is silently ignored
var (state, commands) = machine.Fire(
    new DocumentTrigger.Archive(),  // Not configured anywhere
    DocumentState.Draft);
// state == DocumentState.Draft (unchanged)
// commands == [] (empty)
// No exception, no logging ✅
```

**Use with caution:** This can hide configuration mistakes. Usually better to log unhandled triggers.

---

## Ignore vs Unhandled

### `.Ignore()` on a specific trigger

```csharp
.For(State.Locked)
    .On<Trigger.Lock>()
        .Ignore()  // Lock is already locked, do nothing
```

**Scope:** Only this specific trigger in this specific state  
**Meaning:** "This trigger is intentionally a no-op here"  
**Best for:** Explicit cases where a trigger shouldn't do anything

### `.OnUnhandled()`

```csharp
.OnUnhandled()
    .Execute((trigger, state) => new Command.Log($"Unexpected: {trigger} in {state}"))
```

**Scope:** All triggers not explicitly configured anywhere  
**Meaning:** "Catch-all for anything I forgot or didn't expect"  
**Best for:** Defensive programming, logging, metrics

---

## Precedence Rules

The machine checks in this order:

1. **State-specific handler** — `.On<Trigger>()` defined for current state
2. **Parent handler** — If using hierarchical states, checks parent state handlers
3. **Explicit ignore** — `.On<Trigger>().Ignore()` for current state
4. **Unhandled handler** — `.OnUnhandled()` at machine level
5. **Default behavior** — Throw exception

**Example:**

```csharp
var machine = StateMachine<State, Trigger, Command>.Create()
    .OnUnhandled()
        .Execute((trigger, state) => new Command.Log("Unhandled"))  // #4
    .For(State.Draft)
        .On<Trigger.Action1>()
            .TransitionTo(State.Done)     // #1 - Used for Action1
        .On<Trigger.Action2>()
            .Ignore()                     // #3 - Used for Action2
    // Trigger.Action3 falls through to #4 (OnUnhandled)
    .Build();
```

---

## Complete Example

A game state machine with comprehensive trigger handling:

```csharp
public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}

public abstract record GameTrigger
{
    public sealed record StartGame : GameTrigger;
    public sealed record PauseGame : GameTrigger;
    public sealed record ResumeGame : GameTrigger;
    public sealed record QuitGame : GameTrigger;
    public sealed record PlayerDied : GameTrigger;
    public sealed record SaveGame : GameTrigger;
    public sealed record LoadGame : GameTrigger;
}

public abstract record GameCommand
{
    public sealed record InitializeGame : GameCommand;
    public sealed record Pause : GameCommand;
    public sealed record Resume : GameCommand;
    public sealed record ShowMainMenu : GameCommand;
    public sealed record ShowGameOver : GameCommand;
    public sealed record SaveProgress : GameCommand;
    public sealed record LoadProgress : GameCommand;
    public sealed record LogUnhandled(string Trigger, string State) : GameCommand;
}

var machine = StateMachine<GameState, GameTrigger, GameCommand>.Create()
    // Global handler for unhandled triggers
    .OnUnhandled()
        .Execute((trigger, state) => 
            new GameCommand.LogUnhandled(trigger.GetType().Name, state.ToString()))
    
    .StartWith(GameState.MainMenu)
    
    .For(GameState.MainMenu)
        .On<GameTrigger.StartGame>()
            .Execute(() => new GameCommand.InitializeGame())
            .TransitionTo(GameState.Playing)
        .On<GameTrigger.LoadGame>()
            .Execute(() => new GameCommand.LoadProgress())
            .TransitionTo(GameState.Playing)
        // Pause/Resume don't make sense in menu, explicitly ignore
        .On<GameTrigger.PauseGame>()
            .Ignore()
        .On<GameTrigger.ResumeGame>()
            .Ignore()
        // QuitGame not configured - will use OnUnhandled()
    
    .For(GameState.Playing)
        .On<GameTrigger.PauseGame>()
            .Execute(() => new GameCommand.Pause())
            .TransitionTo(GameState.Paused)
        .On<GameTrigger.SaveGame>()
            .Execute(() => new GameCommand.SaveProgress())
            // Internal transition - stays in Playing
        .On<GameTrigger.PlayerDied>()
            .Execute(() => new GameCommand.ShowGameOver())
            .TransitionTo(GameState.GameOver)
        .On<GameTrigger.QuitGame>()
            .Execute(() => new GameCommand.ShowMainMenu())
            .TransitionTo(GameState.MainMenu)
        // ResumeGame ignored - already playing
        .On<GameTrigger.ResumeGame>()
            .Ignore()
    
    .For(GameState.Paused)
        .On<GameTrigger.ResumeGame>()
            .Execute(() => new GameCommand.Resume())
            .TransitionTo(GameState.Playing)
        .On<GameTrigger.SaveGame>()
            .Execute(() => new GameCommand.SaveProgress())
            // Internal transition
        .On<GameTrigger.QuitGame>()
            .Execute(() => new GameCommand.ShowMainMenu())
            .TransitionTo(GameState.MainMenu)
        // PauseGame ignored - already paused
        .On<GameTrigger.PauseGame>()
            .Ignore()
    
    .For(GameState.GameOver)
        .On<GameTrigger.StartGame>()
            .Execute(() => new GameCommand.InitializeGame())
            .TransitionTo(GameState.Playing)
        .On<GameTrigger.QuitGame>()
            .Execute(() => new GameCommand.ShowMainMenu())
            .TransitionTo(GameState.MainMenu)
        // Most triggers don't make sense when game is over
        .On<GameTrigger.PauseGame>()
            .Ignore()
        .On<GameTrigger.ResumeGame>()
            .Ignore()
        .On<GameTrigger.PlayerDied>()
            .Ignore()  // Already dead
    
    .Build();

// Test scenarios

// Scenario 1: Valid trigger
var (state1, cmds1) = machine.Fire(
    new GameTrigger.StartGame(),
    GameState.MainMenu);
// state1 == GameState.Playing
// cmds1 == [InitializeGame()]

// Scenario 2: Explicitly ignored trigger
var (state2, cmds2) = machine.Fire(
    new GameTrigger.PauseGame(),
    GameState.MainMenu);
// state2 == GameState.MainMenu (unchanged)
// cmds2 == [] (no commands)

// Scenario 3: Unhandled trigger (logs)
var (state3, cmds3) = machine.Fire(
    new GameTrigger.LoadGame(),
    GameState.Playing);
// state3 == GameState.Playing (unchanged)
// cmds3 == [LogUnhandled("LoadGame", "Playing")]
// LoadGame makes sense in MainMenu, but not during Playing

// Scenario 4: Save game while playing (internal transition)
var (state4, cmds4) = machine.Fire(
    new GameTrigger.SaveGame(),
    GameState.Playing);
// state4 == GameState.Playing (unchanged - internal transition)
// cmds4 == [SaveProgress()]
```

**What's happening:**
1. Global `.OnUnhandled()` catches unexpected triggers and logs them
2. Explicit `.Ignore()` for triggers that are intentionally no-ops
3. Different states handle different triggers appropriately
4. Clear distinction between "ignored on purpose" and "unexpected"

---

## Best Practices

✅ **Use OnUnhandled for logging**  
Track unexpected triggers in production for debugging.

✅ **Use Ignore for explicit no-ops**  
Makes it clear the trigger was considered and intentionally does nothing.

✅ **Log unhandled triggers**  
Don't silently ignore them—you want to know when something unexpected happens.

✅ **Consider metrics**  
Count unhandled triggers to detect integration issues.

❌ **Don't use OnUnhandled().Ignore() blindly**  
This hides configuration mistakes. Usually better to log.

❌ **Don't overuse Ignore**  
If most states ignore a trigger, maybe it's not a good trigger.

---

## Common Patterns

### Logging unhandled triggers
```csharp
.OnUnhandled()
    .Execute((trigger, state) => 
        new Command.LogWarning($"Unexpected {trigger} in {state}"))
```

### Metrics for unhandled triggers
```csharp
.OnUnhandled()
    .Execute((trigger, state) => 
        new Command.RecordMetric("unhandled_trigger", trigger.GetType().Name))
```

### Ignore redundant triggers
```csharp
.For(State.Paused)
    .On<Trigger.Pause>()
        .Ignore()  // Already paused
```

### Graceful degradation
```csharp
.OnUnhandled()
    .Execute((trigger, state) => 
        new Command.LogWarning($"Ignoring {trigger}"))
    // Continues without exception
```

---

## Next Steps

- See [Fluent Configuration](fluent-configuration.md) for validation that catches missing triggers
- Use with [Hierarchical States](hierarchical-states.md) where parent can handle unhandled child triggers
- Combine with [Execute Steps](execute-steps.md) for sophisticated unhandled handlers
