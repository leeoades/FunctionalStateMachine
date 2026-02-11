# Guards and Conditional Flows

Guards let you choose between multiple transitions for the same trigger based on state, data, or trigger properties. They encode business rules directly in your transitions.

## Table of Contents

1. [Why Use Guards](#why-use-guards)
2. [Basic Guard](#basic-guard)
3. [Multiple Guarded Transitions](#multiple-guarded-transitions)
4. [Guard Signatures](#guard-signatures)
5. [Guards vs Conditional Steps](#guards-vs-conditional-steps)
6. [Complete Example](#complete-example)

---

## Why Use Guards

**Encode business rules** — Put decision logic in the state machine, not scattered in calling code  
**Branch naturally** — Route the same trigger to different states based on conditions  
**Keep transitions pure** — Guards use data, not side effects, to make decisions  
**Explicit behavior** — All possible paths visible in configuration

---

## Basic Guard

Start with a simple guard that checks a condition:

```csharp
public sealed record LoanData(decimal Amount, int CreditScore);

public enum LoanState { Application, Approved, Rejected }

public abstract record LoanTrigger
{
    public sealed record Submit : LoanTrigger;
}

public abstract record LoanCommand
{
    public sealed record SendApproval : LoanCommand;
    public sealed record SendRejection : LoanCommand;
}

var machine = StateMachine<LoanState, LoanTrigger, LoanData, LoanCommand>.Create()
    .StartWith(LoanState.Application)
    .For(LoanState.Application)
        .On<LoanTrigger.Submit>()
            .Guard(data => data.CreditScore >= 700)  // Condition must be true
            .Execute(() => new LoanCommand.SendApproval())
            .TransitionTo(LoanState.Approved)
    .Build();

var data = new LoanData(Amount: 50000, CreditScore: 750);
var (newState, newData, commands) = machine.Fire(
    new LoanTrigger.Submit(), 
    LoanState.Application, 
    data);
// newState == LoanState.Approved ✅
```

**How it works:**
- Guard evaluates the predicate `data.CreditScore >= 700`
- If **true**, the transition executes
- If **false**, this transition is skipped

**What happens if the guard fails?** If no other transition handles this trigger, it's **unhandled** and throws an exception.

---

## Multiple Guarded Transitions

Handle different cases by defining multiple transitions for the same trigger:

```csharp
var machine = StateMachine<LoanState, LoanTrigger, LoanData, LoanCommand>.Create()
    .StartWith(LoanState.Application)
    .For(LoanState.Application)
        .On<LoanTrigger.Submit>()
            .Guard(data => data.CreditScore >= 700)        // First guard
            .Execute(() => new LoanCommand.SendApproval())
            .TransitionTo(LoanState.Approved)
        .On<LoanTrigger.Submit>()
            .Guard(data => data.CreditScore < 700)         // Second guard
            .Execute(() => new LoanCommand.SendRejection())
            .TransitionTo(LoanState.Rejected)
    .Build();
```

**Evaluation order:**  
Guards are evaluated **in the order you define them**. The **first matching guard wins**.

```csharp
// CreditScore = 650
var (newState, _, commands) = machine.Fire(
    new LoanTrigger.Submit(), 
    LoanState.Application, 
    new LoanData(50000, 650));
// First guard fails (650 < 700 is false)
// Second guard passes (650 < 700 is true) ✅
// newState == LoanState.Rejected
```

---

## Guard with Multiple Conditions

Guards can check multiple properties:

```csharp
public sealed record LoanData(decimal Amount, int CreditScore, bool HasCollateral);

var machine = StateMachine<LoanState, LoanTrigger, LoanData, LoanCommand>.Create()
    .StartWith(LoanState.Application)
    .For(LoanState.Application)
        // Premium loans: high credit score OR has collateral
        .On<LoanTrigger.Submit>()
            .Guard(data => data.CreditScore >= 750 || data.HasCollateral)
            .Execute(() => new LoanCommand.SendApproval())
            .TransitionTo(LoanState.Approved)
        
        // Standard loans: good credit score and low amount
        .On<LoanTrigger.Submit>()
            .Guard(data => data.CreditScore >= 650 && data.Amount < 100000)
            .Execute(() => new LoanCommand.SendApproval())
            .TransitionTo(LoanState.Approved)
        
        // Everything else rejected
        .On<LoanTrigger.Submit>()
            .Guard(data => true)  // Catch-all guard
            .Execute(() => new LoanCommand.SendRejection())
            .TransitionTo(LoanState.Rejected)
    .Build();
```

**Pattern:** Use a catch-all guard (`data => true`) as the last option to ensure all cases are handled.

---

## Guard Signatures

Guards support multiple overloads depending on what information you need:

### 1. Data only

```csharp
.Guard(data => data.CreditScore >= 700)
```

Use when the decision depends only on state data.

### 2. State and data

```csharp
.Guard((state, data) => 
    state == LoanState.Application && data.CreditScore >= 700)
```

Use when the decision depends on both current state and data (rare, but useful with hierarchical states).

### 3. Data and trigger

```csharp
public abstract record LoanTrigger
{
    public sealed record Submit(decimal RequestedAmount) : LoanTrigger;
}

.Guard((data, trigger) => 
    data.CreditScore >= 700 && trigger.RequestedAmount <= data.MaxLoanAmount)
```

Use when the trigger carries information needed for the decision.

### 4. State, data, and trigger

```csharp
.Guard((state, data, trigger) => 
    state == LoanState.Application && 
    data.CreditScore >= 700 && 
    trigger.RequestedAmount <= 500000)
```

Use when you need all three pieces of information.

---

## Guards vs Conditional Steps

**When to use Guards:**
- You want to transition to **different states** based on a condition
- Each path is a distinct transition
- Useful for diverging workflows

```csharp
.On<Trigger.Submit>()
    .Guard(data => data.IsValid)
    .TransitionTo(State.Approved)       // Goes to Approved
.On<Trigger.Submit>()
    .Guard(data => !data.IsValid)
    .TransitionTo(State.Rejected)       // Goes to Rejected
```

**When to use Conditional Steps (If/ElseIf/Else):**
- You want to emit **different commands** or modify data differently
- All paths go to the **same state** (or no transition at all)
- Useful for variations within a single transition

```csharp
.On<Trigger.Submit>()
    .If(data => data.IsValid)
        .Execute(() => new Command.SendApproval())
    .Else()
        .Execute(() => new Command.SendRejection())
    .Done()
    .TransitionTo(State.Complete)       // Both paths go to Complete
```

See [Conditional Steps](conditional-steps.md) for more details on If/ElseIf/Else.

---

## Complete Example

An ATM withdrawal with multiple guarded paths:

```csharp
public enum ATMState
{
    Idle,
    SelectAmount,
    Dispensing,
    InsufficientFunds,
    DailyLimitReached
}

public abstract record ATMTrigger
{
    public sealed record SelectWithdraw : ATMTrigger;
    public sealed record ConfirmAmount(decimal Amount) : ATMTrigger;
    public sealed record Cancel : ATMTrigger;
}

public sealed record ATMData(
    decimal Balance, 
    decimal DailyLimit, 
    decimal WithdrawnToday);

public abstract record ATMCommand
{
    public sealed record ShowMessage(string Message) : ATMCommand;
    public sealed record DispenseCash(decimal Amount) : ATMCommand;
    public sealed record UpdateBalance(decimal NewBalance) : ATMCommand;
}

var machine = StateMachine<ATMState, ATMTrigger, ATMData, ATMCommand>.Create()
    .StartWith(ATMState.Idle)
    
    .For(ATMState.Idle)
        .On<ATMTrigger.SelectWithdraw>()
            .TransitionTo(ATMState.SelectAmount)
    
    .For(ATMState.SelectAmount)
        // Guard 1: Check if amount exceeds daily limit
        .On<ATMTrigger.ConfirmAmount>()
            .Guard((data, trigger) => 
                data.WithdrawnToday + trigger.Amount > data.DailyLimit)
            .Execute(() => new ATMCommand.ShowMessage("Daily limit exceeded"))
            .TransitionTo(ATMState.DailyLimitReached)
        
        // Guard 2: Check if insufficient funds
        .On<ATMTrigger.ConfirmAmount>()
            .Guard((data, trigger) => trigger.Amount > data.Balance)
            .Execute(() => new ATMCommand.ShowMessage("Insufficient funds"))
            .TransitionTo(ATMState.InsufficientFunds)
        
        // Guard 3: Successful withdrawal (catch-all)
        .On<ATMTrigger.ConfirmAmount>()
            .Guard((data, trigger) => true)  // If we got here, all checks passed
            .ModifyData((data, trigger) => data with 
            {
                Balance = data.Balance - trigger.Amount,
                WithdrawnToday = data.WithdrawnToday + trigger.Amount
            })
            .Execute(trigger => new ATMCommand.DispenseCash(trigger.Amount))
            .Execute(data => new ATMCommand.UpdateBalance(data.Balance))
            .TransitionTo(ATMState.Dispensing)
        
        .On<ATMTrigger.Cancel>()
            .TransitionTo(ATMState.Idle)
    
    .For(ATMState.Dispensing)
        // Automatically return to Idle (could use immediate transition here)
        .On<ATMTrigger.Cancel>()
            .TransitionTo(ATMState.Idle)
    
    .For(ATMState.InsufficientFunds)
        .On<ATMTrigger.Cancel>()
            .TransitionTo(ATMState.Idle)
    
    .For(ATMState.DailyLimitReached)
        .On<ATMTrigger.Cancel>()
            .TransitionTo(ATMState.Idle)
    
    .Build();

// Test scenarios

// Scenario 1: Successful withdrawal
var data1 = new ATMData(Balance: 1000, DailyLimit: 500, WithdrawnToday: 0);
var (state1, data1New, commands1) = machine.Fire(
    new ATMTrigger.ConfirmAmount(200), 
    ATMState.SelectAmount, 
    data1);
// state1 == ATMState.Dispensing
// data1New.Balance == 800
// data1New.WithdrawnToday == 200
// commands1 == [DispenseCash(200), UpdateBalance(800)]

// Scenario 2: Insufficient funds
var data2 = new ATMData(Balance: 100, DailyLimit: 500, WithdrawnToday: 0);
var (state2, _, commands2) = machine.Fire(
    new ATMTrigger.ConfirmAmount(200), 
    ATMState.SelectAmount, 
    data2);
// state2 == ATMState.InsufficientFunds
// commands2 == [ShowMessage("Insufficient funds")]

// Scenario 3: Daily limit reached
var data3 = new ATMData(Balance: 1000, DailyLimit: 500, WithdrawnToday: 400);
var (state3, _, commands3) = machine.Fire(
    new ATMTrigger.ConfirmAmount(200), 
    ATMState.SelectAmount, 
    data3);
// state3 == ATMState.DailyLimitReached
// commands3 == [ShowMessage("Daily limit exceeded")]
```

**What's happening:**
1. Three guards check conditions in order: daily limit, insufficient funds, success
2. Each guard routes to a different state based on the condition
3. The success guard modifies data and emits commands
4. All guards use the same trigger but produce different outcomes

---

## Best Practices

✅ **Order guards from most specific to most general**  
Put stricter conditions first, catch-all guards last.

✅ **Use a catch-all guard for completeness**  
`Guard(data => true)` ensures all cases are handled.

✅ **Keep guards pure**  
Don't perform I/O or side effects in guard predicates. Only inspect data.

✅ **Consider If/ElseIf/Else for single-state variations**  
Use guards when you need to go to different states. Use If/Else when you stay in the same state.

❌ **Avoid overlapping guards without intention**  
If two guards can both be true, only the first will execute.

---

## Next Steps

- Combine guards with [Conditional Steps](conditional-steps.md) for complex branching
- Learn about [State Data](state-data.md) to understand what guards can inspect
- See [Execute Steps](execute-steps.md) for emitting commands after guard evaluation
