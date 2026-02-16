# Basic State Machine Samples

A collection of four simple, self-contained examples demonstrating core FunctionalStateMachine features. Each sample is defined in a single file and includes unit tests showing how to use the state machine.

## Samples Overview

| Sample | Purpose | Key Features |
|--------|---------|--------------|
| **LightSwitch** | Toggle between two states | Basic transitions, simple commands |
| **SessionLogin** | User authentication flow | Hierarchical states, entry actions |
| **ShoppingTrolley** | Shopping cart workflow | State data, guards, conditional steps |
| **Timer** | Simple counter | State data modification, entry/exit actions |

---

## 1. Light Switch

**File:** `LightSwitchSample.cs`

The simplest possible state machine: a light that toggles between on and off.

### States
- **Off** - Light is off
- **On** - Light is on

### Triggers
- **Toggle** - Switch the light

### Commands
- **TurnOn** - Turn on the light hardware
- **TurnOff** - Turn off the light hardware

### Example Usage

```csharp
var machine = LightSwitchSample.Build();
var state = LightState.Off;

// Toggle the light
(state, var commands) = machine.Fire(LightTrigger.Toggle, state);
// state == LightState.On
// commands == [TurnOn]

// Toggle again
(state, commands) = machine.Fire(LightTrigger.Toggle, state);
// state == LightState.Off
// commands == [TurnOff]
```

### What This Demonstrates
- ✅ Basic state transitions with `TransitionTo`
- ✅ Command generation with `Execute`
- ✅ Ignoring unhandled triggers with `OnUnhandled().Ignore()`

---

## 2. Session Login

**File:** `SessionLoginSample.cs`

A user session with login/logout flow, demonstrating hierarchical states and parent transitions.

### States
- **Active** (parent) - Session is active
  - **Anonymous** (child) - Not logged in
  - **Authenticated** (child) - Logged in
- **Expired** - Session timed out

### Triggers
- **Login** - User logs in
- **Logout** - User logs out
- **Timeout** - Session expires

### Commands
- **PerformLogin** - Execute authentication
- **PerformLogout** - Clear authentication
- **HandleTimeout** - Clean up expired session
- **DisplayExpiredMessage** - Show expiration notice

### Example Usage

```csharp
var machine = SessionLoginSample.Build();
var state = SessionState.Anonymous;
var data = SessionData.Initial;

// User logs in
(state, data, var commands) = machine.Fire(SessionTrigger.Login, state, data);
// state == SessionState.Authenticated
// commands == [PerformLogin]

// User logs out
(state, data, commands) = machine.Fire(SessionTrigger.Logout, state, data);
// state == SessionState.Anonymous
// commands == [PerformLogout]

// Session times out (works from any Active substate)
(state, data, commands) = machine.Fire(SessionTrigger.Timeout, state, data);
// state == SessionState.Expired
// commands == [HandleTimeout]
```

### What This Demonstrates
- ✅ Hierarchical states with `SubStateOf`
- ✅ Parent state initial child with `StartsWith`
- ✅ Parent transitions applying to all children
- ✅ Entry actions with `OnEntry`
- ✅ State data tracking with `SessionData`

---

## 3. Shopping Trolley

**File:** `ShoppingTrolleySample.cs`

A complete shopping cart workflow with item management, checkout, and multiple payment methods.

### States
- **Outside** - Not shopping
- **InStore** (parent) - Customer is in store
  - **Shopping** (child) - Adding/removing items
  - **CheckingOut** (child) - Paying for items
  - **PaymentPending** (child) - Waiting for payment confirmation

### Triggers
- **StartShopping** - Enter the store
- **AddItem** - Add item to cart
- **RemoveItem** - Remove item from cart
- **GoToCheckout** - Proceed to payment
- **Pay** - Pay by card
- **PayByCash** - Pay with cash (may require multiple payments)
- **PaymentSucceeded** - Card payment succeeded
- **PaymentFailed** - Card payment failed
- **Cancel** - Cancel transaction

### Commands
- **UpdateCartItems** - Update cart display
- **RequestPayment** - Show amount due
- **DisplayPaymentPendingMessage** - Show pending status
- **DisplayPaymentFailedMessage** - Show error
- **GrantItemOwnership** - Complete purchase
- **RefundCash** - Return change

### Example Usage

```csharp
var machine = ShoppingTrolleySample.Build();
var state = ShopState.Outside;
var data = CartSession.Initial;

// Start shopping
(state, data, var commands) = machine.Fire(CartTrigger.StartShopping(), state, data);
// state == ShopState.Shopping

// Add items
(state, data, commands) = machine.Fire(
    CartTrigger.AddItem(new LineItem("Milk", 1.30m)), 
    state, 
    data);
// commands == [UpdateCartItems]

(state, data, commands) = machine.Fire(
    CartTrigger.AddItem(new LineItem("Bread", 0.80m)), 
    state, 
    data);

// Checkout
(state, data, commands) = machine.Fire(CartTrigger.GoToCheckout(), state, data);
// state == ShopState.CheckingOut
// commands == [RequestPayment(2.10)]

// Pay with cash - first payment
(state, data, commands) = machine.Fire(CartTrigger.PayByCash(2.00m), state, data);
// Still in CheckingOut - not enough money
// commands == [RequestPayment(0.10)]

// Pay with cash - second payment
(state, data, commands) = machine.Fire(CartTrigger.PayByCash(1.00m), state, data);
// state == ShopState.Outside
// commands == [GrantItemOwnership, RefundCash(0.90)]
```

### What This Demonstrates
- ✅ Complex state data with `CartSession` and `ShopData`
- ✅ Data modification with `ModifyData`
- ✅ Guards with different conditions on the same trigger
- ✅ Conditional steps with `If/Else/Done`
- ✅ Parent transition canceling from any child state
- ✅ Multiple commands in a single transition
- ✅ Entry actions with data access

---

## 4. Timer

**File:** `TimerSample.cs`

A simple timer that counts ticks, demonstrating state data updates and entry/exit actions.

### States
- **Running** - Timer is active
- **Paused** - Timer is paused (not shown in build, but referenced)

### Triggers
- **Tick** - Increment the counter
- **Resume** - Resume from paused (state exists for completeness)

### Commands
- **WriteLog** - Log timer events

### Example Usage

```csharp
var machine = TimerSample.Build();
var state = TimerState.Running;
var data = TimerData.Initial;

// Timer starts - entry action fires
var (newState, newData, commands) = machine.Start(data);
// commands == [WriteLog("Start")]

// Tick the timer
(state, data, commands) = machine.Fire(TimerTrigger.Tick, state, data);
// data.Ticks == 1
// commands == [WriteLog("Tick:1")]

// Tick again
(state, data, commands) = machine.Fire(TimerTrigger.Tick, state, data);
// data.Ticks == 2
// commands == [WriteLog("Tick:2")]
```

### What This Demonstrates
- ✅ State data updates without state transitions (internal transitions)
- ✅ Entry actions with `OnEntry`
- ✅ Exit actions with `OnExit`
- ✅ Accessing updated data in execute steps

---

## Running the Samples

All samples include unit tests demonstrating their usage. Run them with:

```bash
dotnet test samples/Basic/FunctionalStateMachine.Samples/FunctionalStateMachine.Samples.csproj
```

---

## Diagrams

Each sample includes an auto-generated Mermaid diagram in the `diagrams/` subdirectory:

- `diagrams/LightSwitch.md` - Simple toggle flow
- `diagrams/SessionLogin.md` - Hierarchical authentication
- `diagrams/ShoppingTrolley.md` - Complex payment workflow
- `diagrams/Timer.md` - Counter state machine

Diagrams are automatically updated when the state machine definition changes, thanks to the `[StateMachineDiagram]` attribute.

---

## Key Takeaways

### For Beginners
Start with **LightSwitch** to understand basic transitions, then move to **Timer** to see state data in action.

### For Intermediate Users
Explore **SessionLogin** for hierarchical states and **ShoppingTrolley** for guards and conditional logic.

### Common Patterns
- **Pure functions** - All state machines are deterministic
- **Immutable data** - State data uses records and `with` expressions
- **Command pattern** - Transitions return what to do, not how to do it
- **Type safety** - Triggers and commands are sealed record hierarchies
