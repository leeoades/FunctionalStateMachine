# FunctionalStateMachine.Samples

Each sample builds a state machine and documents which features it demonstrates.

## Samples

- OrderProcessingSample
  - Features: fluent builder, StartWith, OnEntry/OnExit, TransitionTo, Execute(state, trigger), command base record
- FraudReviewSample
  - Features: Guard, WithData, Execute(state), Execute(trigger), Execute(), multiple commands
- IgnoreAndUnhandledSample
  - Features: Ignore, OnUnhandled handler, TryFire-friendly behavior
- InternalTransitionSample
  - Features: internal transition (no TransitionTo), Execute(state, trigger), no entry/exit on internal transition
- SessionLoginSample
  - Features: SubStateOf, StartsWith, parent transitions
- NoDataSample
  - Features: StateMachineBuilder without extra data payload (NoData)
- ShoppingTrolleySample
  - Features: SubStateOf, StartsWith, guards based on leaf state, data updates, cancel from parent, checkout total command
