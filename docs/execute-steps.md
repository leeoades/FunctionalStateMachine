# Execute Steps and Multiple Commands

Execute steps let you emit one or more commands during a transition. Commands can access state, data, and trigger information, and you can chain multiple execute steps to emit several commands in sequence.

## Table of Contents

1. [Why Use Execute Steps](#why-use-execute-steps)
2. [Basic Execute Step](#basic-execute-step)
3. [Multiple Execute Steps](#multiple-execute-steps)
4. [Returning Multiple Commands](#returning-multiple-commands)
5. [Execute Signatures](#execute-signatures)
6. [Order of Execution](#order-of-execution)
7. [Complete Example](#complete-example)

---

## Why Use Execute Steps

**Emit commands during transitions** — Generate side effect descriptions as part of state changes  
**Chain multiple actions** — Build up a sequence of commands in readable steps  
**Access context** — Commands can use state, data, and trigger information  
**Keep logic visible** — All commands are explicit in the configuration

---

## Basic Execute Step

Emit a single command during a transition:

```csharp
public enum OrderState { Pending, Confirmed, Shipped }

public abstract record OrderTrigger
{
    public sealed record Confirm : OrderTrigger;
}

public abstract record OrderCommand
{
    public sealed record SendConfirmationEmail(string Email) : OrderCommand;
    public sealed record UpdateInventory(Guid OrderId) : OrderCommand;
}

public sealed record OrderData(Guid OrderId, string Email);

var machine = StateMachine<OrderState, OrderTrigger, OrderData, OrderCommand>.Create()
    .StartWith(OrderState.Pending)
    .For(OrderState.Pending)
        .On<OrderTrigger.Confirm>()
            .Execute(data => new OrderCommand.SendConfirmationEmail(data.Email))
            .TransitionTo(OrderState.Confirmed)
    .Build();

var data = new OrderData(OrderId: Guid.NewGuid(), Email: "user@example.com");
var (newState, newData, commands) = machine.Fire(
    new OrderTrigger.Confirm(),
    OrderState.Pending,
    data);
// newState == OrderState.Confirmed
// commands == [SendConfirmationEmail("user@example.com")]
```

**How it works:**
- `.Execute()` takes a lambda that returns a command
- The lambda can access data, state, and trigger
- Commands are collected and returned from `Fire()`

---

## Multiple Execute Steps

Chain multiple `.Execute()` calls to emit several commands:

```csharp
var machine = StateMachine<OrderState, OrderTrigger, OrderData, OrderCommand>.Create()
    .StartWith(OrderState.Pending)
    .For(OrderState.Pending)
        .On<OrderTrigger.Confirm>()
            .Execute(data => new OrderCommand.SendConfirmationEmail(data.Email))
            .Execute(data => new OrderCommand.UpdateInventory(data.OrderId))
            .Execute(() => new OrderCommand.LogMetric("order_confirmed"))
            .TransitionTo(OrderState.Confirmed)
    .Build();

var (_, _, commands) = machine.Fire(
    new OrderTrigger.Confirm(),
    OrderState.Pending,
    data);
// commands == [
//   SendConfirmationEmail("user@example.com"),
//   UpdateInventory(guid),
//   LogMetric("order_confirmed")
// ]
```

**Benefits:**
- Each command is a separate, clear step
- Easy to read: "send email, then update inventory, then log"
- Easy to modify: add/remove steps without complex refactoring

---

## Returning Multiple Commands

Instead of multiple `.Execute()` calls, return an array from one:

```csharp
var machine = StateMachine<OrderState, OrderTrigger, OrderData, OrderCommand>.Create()
    .For(OrderState.Pending)
        .On<OrderTrigger.Confirm>()
            .Execute(data => new OrderCommand[]
            {
                new OrderCommand.SendConfirmationEmail(data.Email),
                new OrderCommand.UpdateInventory(data.OrderId),
                new OrderCommand.LogMetric("order_confirmed")
            })
            .TransitionTo(OrderState.Confirmed)
    .Build();
```

**When to use each approach:**
- **Multiple Execute calls:** When each command is conceptually separate
- **Array return:** When commands are logically grouped or generated in a loop

---

## Accessing Trigger Data

Triggers can carry information that commands need:

```csharp
public abstract record OrderTrigger
{
    public sealed record Ship(string TrackingNumber, string Carrier) : OrderTrigger;
}

public abstract record OrderCommand
{
    public sealed record SendShippingNotification(string Email, string Tracking) : OrderCommand;
    public sealed record UpdateShippingInfo(Guid OrderId, string Tracking, string Carrier) : OrderCommand;
}

var machine = StateMachine<OrderState, OrderTrigger, OrderData, OrderCommand>.Create()
    .For(OrderState.Confirmed)
        .On<OrderTrigger.Ship>()
            .Execute((data, trigger) => 
                new OrderCommand.SendShippingNotification(data.Email, trigger.TrackingNumber))
            .Execute((data, trigger) => 
                new OrderCommand.UpdateShippingInfo(data.OrderId, trigger.TrackingNumber, trigger.Carrier))
            .TransitionTo(OrderState.Shipped)
    .Build();

var (_, _, commands) = machine.Fire(
    new OrderTrigger.Ship(TrackingNumber: "1Z999", Carrier: "UPS"),
    OrderState.Confirmed,
    data);
// commands == [
//   SendShippingNotification("user@example.com", "1Z999"),
//   UpdateShippingInfo(guid, "1Z999", "UPS")
// ]
```

---

## Execute Signatures

Execute steps support multiple overloads based on what you need:

### 1. No parameters

```csharp
.Execute(() => new Command.Log("Order processed"))
```

Use for commands that don't need context.

### 2. Data only

```csharp
.Execute(data => new Command.SendEmail(data.Email))
```

Use when the command only needs data.

### 3. State and data

```csharp
.Execute((state, data) => new Command.Log($"State {state}, User {data.UserId}"))
```

Use when you need both state and data (rare).

### 4. Data and trigger

```csharp
.Execute((data, trigger) => new Command.Process(data.Id, trigger.Amount))
```

Use when the trigger carries information.

### 5. State, data, and trigger

```csharp
.Execute((state, data, trigger) => 
    new Command.Log($"State {state}, User {data.UserId}, Amount {trigger.Amount}"))
```

Use when you need all three.

---

## Order of Execution

Commands are collected in a specific order:

1. **Exit commands** from the previous state (if state changes)
2. **Entry commands** from the new state (if state changes)
3. **Execute commands** from the transition (in the order defined)

```csharp
var machine = StateMachine<OrderState, OrderTrigger, OrderData, OrderCommand>.Create()
    .For(OrderState.Pending)
        .OnExit(() => new OrderCommand.Log("Exit Pending"))
        .On<OrderTrigger.Confirm>()
            .Execute(() => new OrderCommand.Log("Execute 1"))
            .Execute(() => new OrderCommand.Log("Execute 2"))
            .TransitionTo(OrderState.Confirmed)
    .For(OrderState.Confirmed)
        .OnEntry(() => new OrderCommand.Log("Enter Confirmed"))
    .Build();

var (_, _, commands) = machine.Fire(
    new OrderTrigger.Confirm(),
    OrderState.Pending,
    data);
// commands == [
//   Log("Exit Pending"),         // 1. Exit old state
//   Log("Enter Confirmed"),      // 2. Enter new state
//   Log("Execute 1"),            // 3. First execute step
//   Log("Execute 2")             // 4. Second execute step
// ]
```

---

## Execute with ModifyData

Execute steps see data **after** `ModifyData` has run:

```csharp
public sealed record CounterData(int Count);

var machine = StateMachine<CounterState, CounterTrigger, CounterData, CounterCommand>.Create()
    .For(CounterState.Active)
        .On<CounterTrigger.Increment>()
            .ModifyData(data => data with { Count = data.Count + 1 })
            .Execute(data => new CounterCommand.Display(data.Count))  // Sees updated count
    .Build();

var data = new CounterData(Count: 5);
var (_, newData, commands) = machine.Fire(
    new CounterTrigger.Increment(),
    CounterState.Active,
    data);
// newData.Count == 6
// commands == [Display(6)]  ← Execute sees the NEW count
```

**Order within a transition:**
1. ModifyData updates data
2. Execute steps run with updated data
3. TransitionTo changes state

---

## Complete Example

An e-commerce checkout flow with multiple command types:

```csharp
public enum CheckoutState
{
    Cart,
    PaymentInfo,
    Processing,
    Completed,
    Failed
}

public abstract record CheckoutTrigger
{
    public sealed record EnterPayment : CheckoutTrigger;
    public sealed record SubmitPayment(string CardToken) : CheckoutTrigger;
    public sealed record PaymentSucceeded(string TransactionId) : CheckoutTrigger;
    public sealed record PaymentFailed(string Reason) : CheckoutTrigger;
}

public sealed record CheckoutData(
    Guid OrderId,
    string Email,
    decimal Total,
    List<string> Items,
    string? TransactionId);

public abstract record CheckoutCommand
{
    public sealed record ValidateCart(Guid OrderId) : CheckoutCommand;
    public sealed record SendPaymentLink(string Email) : CheckoutCommand;
    public sealed record ProcessPayment(string CardToken, decimal Amount) : CheckoutCommand;
    public sealed record SendReceipt(string Email, string TransactionId) : CheckoutCommand;
    public sealed record UpdateInventory(List<string> Items) : CheckoutCommand;
    public sealed record LogMetric(string Event, decimal? Amount) : CheckoutCommand;
    public sealed record SendFailureEmail(string Email, string Reason) : CheckoutCommand;
    public sealed record RefundPayment(string TransactionId) : CheckoutCommand;
}

var machine = StateMachine<CheckoutState, CheckoutTrigger, CheckoutData, CheckoutCommand>
    .Create()
    .StartWith(CheckoutState.Cart)
    
    .For(CheckoutState.Cart)
        .On<CheckoutTrigger.EnterPayment>()
            .Execute(data => new CheckoutCommand.ValidateCart(data.OrderId))
            .Execute(data => new CheckoutCommand.SendPaymentLink(data.Email))
            .Execute(() => new CheckoutCommand.LogMetric("entered_payment", null))
            .TransitionTo(CheckoutState.PaymentInfo)
    
    .For(CheckoutState.PaymentInfo)
        .On<CheckoutTrigger.SubmitPayment>()
            .Execute((data, trigger) => 
                new CheckoutCommand.ProcessPayment(trigger.CardToken, data.Total))
            .Execute(data => 
                new CheckoutCommand.LogMetric("payment_submitted", data.Total))
            .TransitionTo(CheckoutState.Processing)
    
    .For(CheckoutState.Processing)
        .On<CheckoutTrigger.PaymentSucceeded>()
            .ModifyData((data, trigger) => data with 
            { 
                TransactionId = trigger.TransactionId 
            })
            .Execute(data => 
                new CheckoutCommand.SendReceipt(data.Email, data.TransactionId!))
            .Execute(data => 
                new CheckoutCommand.UpdateInventory(data.Items))
            .Execute(data => 
                new CheckoutCommand.LogMetric("payment_succeeded", data.Total))
            .TransitionTo(CheckoutState.Completed)
        
        .On<CheckoutTrigger.PaymentFailed>()
            .Execute((data, trigger) => 
                new CheckoutCommand.SendFailureEmail(data.Email, trigger.Reason))
            .Execute((data, trigger) => 
                new CheckoutCommand.LogMetric("payment_failed", data.Total))
            .TransitionTo(CheckoutState.Failed)
    
    .For(CheckoutState.Completed)
        .OnEntry(() => new CheckoutCommand.LogMetric("order_completed", null))
    
    .For(CheckoutState.Failed)
        .OnEntry(() => new CheckoutCommand.LogMetric("order_failed", null))
    
    .Build();

// Usage scenario

var data = new CheckoutData(
    OrderId: Guid.NewGuid(),
    Email: "customer@example.com",
    Total: 129.99m,
    Items: new List<string> { "ITEM-1", "ITEM-2" },
    TransactionId: null);

// Step 1: Enter payment
var (state1, data1, cmds1) = machine.Fire(
    new CheckoutTrigger.EnterPayment(),
    CheckoutState.Cart,
    data);
// state1 == CheckoutState.PaymentInfo
// cmds1 == [
//   ValidateCart(guid),
//   SendPaymentLink("customer@example.com"),
//   LogMetric("entered_payment", null)
// ]

// Step 2: Submit payment
var (state2, data2, cmds2) = machine.Fire(
    new CheckoutTrigger.SubmitPayment(CardToken: "tok_visa_4242"),
    state1,
    data1);
// state2 == CheckoutState.Processing
// cmds2 == [
//   ProcessPayment("tok_visa_4242", 129.99),
//   LogMetric("payment_submitted", 129.99)
// ]

// Step 3: Payment succeeds
var (state3, data3, cmds3) = machine.Fire(
    new CheckoutTrigger.PaymentSucceeded(TransactionId: "txn_12345"),
    state2,
    data2);
// state3 == CheckoutState.Completed
// data3.TransactionId == "txn_12345"
// cmds3 == [
//   LogMetric("order_completed", null),          // OnEntry
//   SendReceipt("customer@example.com", "txn_12345"),  // Execute
//   UpdateInventory(["ITEM-1", "ITEM-2"]),             // Execute
//   LogMetric("payment_succeeded", 129.99)             // Execute
// ]

// Alternate: Payment fails
var (state3alt, data3alt, cmds3alt) = machine.Fire(
    new CheckoutTrigger.PaymentFailed(Reason: "Card declined"),
    state2,
    data2);
// state3alt == CheckoutState.Failed
// cmds3alt == [
//   SendFailureEmail("customer@example.com", "Card declined"),
//   LogMetric("payment_failed", 129.99),
//   LogMetric("order_failed", null)  // OnEntry
// ]
```

**What's happening:**
1. Each transition emits relevant commands
2. Commands use data and trigger information
3. Multiple execute steps build up a command sequence
4. Logging, emails, and business operations all explicit
5. Easy to see what happens in each transition

---

## Best Practices

✅ **Chain multiple Execute for clarity**  
`Execute().Execute().Execute()` is more readable than one complex lambda.

✅ **Use descriptive commands**  
Command names should explain what they do: `SendReceipt`, not `DoThing`.

✅ **Keep execute lambdas simple**  
Just create the command. Don't do complex logic here.

✅ **Return arrays for dynamic commands**  
When building commands in a loop, return an array.

❌ **Don't perform I/O in Execute**  
Execute returns commands to describe work. Don't do the work here.

❌ **Don't mutate data in Execute**  
Use `ModifyData` for data changes. Execute should only create commands.

---

## Common Patterns

### Logging and metrics
```csharp
.Execute(() => new Command.LogEvent("user_action"))
.Execute(data => new Command.RecordMetric("event", data.Value))
```

### Notifications
```csharp
.Execute(data => new Command.SendEmail(data.Email, "Subject"))
.Execute(data => new Command.SendSMS(data.Phone, "Message"))
```

### Batch operations
```csharp
.Execute(data => data.Items
    .Select(item => new Command.ProcessItem(item))
    .ToArray())
```

### Conditional commands
```csharp
.Execute((data, trigger) => trigger.Urgent
    ? new Command.SendUrgentAlert(data.Email)
    : new Command.SendNormalAlert(data.Email))
```

---

## Next Steps

- Combine with [Entry/Exit Commands](entry-exit.md) for lifecycle actions
- Use [ModifyData](state-data.md) to update data before executing commands
- See [Conditional Steps](conditional-steps.md) for branching within execute steps
