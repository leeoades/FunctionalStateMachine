# VendingMachine

```mermaid
flowchart LR
  START((start)) --> S_VendingMachineState_Idle
  S_VendingMachineState_DispensingItem[VendingMachineState.DispensingItem]
  S_VendingMachineState_Idle[VendingMachineState.Idle]
  S_VendingMachineState_ItemSelected[VendingMachineState.ItemSelected]
  S_VendingMachineState_MachineJammed[VendingMachineState.MachineJammed]
  S_VendingMachineState_OutOfStock[VendingMachineState.OutOfStock]
  S_VendingMachineState_PaymentValidation[VendingMachineState.PaymentValidation]
  S_VendingMachineState_ReturningChange[VendingMachineState.ReturningChange]
  S_VendingMachineState_DispensingItem -->|DispenseCompleteTrigger| S_VendingMachineState_Idle
  S_VendingMachineState_DispensingItem -->|JamDetectedTrigger| S_VendingMachineState_MachineJammed
  S_VendingMachineState_DispensingItem -->|DispenseCompleteTrigger| S_VendingMachineState_ReturningChange
  S_VendingMachineState_Idle -->|SelectItemTrigger| S_VendingMachineState_ItemSelected
  S_VendingMachineState_Idle -->|JamDetectedTrigger| S_VendingMachineState_MachineJammed
  S_VendingMachineState_Idle -->|SelectItemTrigger| S_VendingMachineState_OutOfStock
  S_VendingMachineState_ItemSelected -->|InsertMoneyTrigger| S_VendingMachineState_DispensingItem
  S_VendingMachineState_ItemSelected -->|CancelTrigger| S_VendingMachineState_Idle
  S_VendingMachineState_ItemSelected -->|JamDetectedTrigger| S_VendingMachineState_MachineJammed
  S_VendingMachineState_ItemSelected -->|InsertMoneyTrigger| S_VendingMachineState_PaymentValidation
  S_VendingMachineState_OutOfStock -->|CancelTrigger| S_VendingMachineState_Idle
  S_VendingMachineState_OutOfStock -->|SelectItemTrigger| S_VendingMachineState_ItemSelected
  S_VendingMachineState_OutOfStock -->|JamDetectedTrigger| S_VendingMachineState_MachineJammed
  S_VendingMachineState_OutOfStock -->|SelectItemTrigger| S_VendingMachineState_OutOfStock
  S_VendingMachineState_PaymentValidation -->|InsertMoneyTrigger| S_VendingMachineState_DispensingItem
  S_VendingMachineState_PaymentValidation -->|CancelTrigger| S_VendingMachineState_Idle
  S_VendingMachineState_PaymentValidation -->|JamDetectedTrigger| S_VendingMachineState_MachineJammed
  S_VendingMachineState_ReturningChange -->|DispenseCompleteTrigger| S_VendingMachineState_Idle
  S_VendingMachineState_ReturningChange -->|JamDetectedTrigger| S_VendingMachineState_MachineJammed
```
