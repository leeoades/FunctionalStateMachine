# Stock Purchaser Actor Sample

Console app demonstrating the actor-model flow with the FunctionalStateMachine library. Each stock symbol acts as an actor id: state and data are loaded from storage, a trigger is fired, the new state/data are persisted, and commands are executed.

## Scenario

Three stock symbols (ACME, BETA, COHO) receive price ticks every 5–10 seconds. Each symbol has its own target buy price. When the price meets or beats the target, a purchase command is emitted.

## Key Loop

```
load state/data by actor id
fire trigger
persist new state/data
execute commands
```

## Run

```bash
dotnet run --project samples\StockPurchaser\StockPurchaserSampleApp\StockPurchaserSampleApp.csproj
```

## Files

- `Configuration/StockPurchaserMachine.cs` - state machine definition
- `Runtime/InMemoryStockStore.cs` - fake persistence
- `Runtime/CommandExecutor.cs` - command execution
- `Program.cs` - timer-driven event loop
