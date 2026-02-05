using VendingMachineSampleApp.Configuration;
using VendingMachineSampleApp.Domain;

namespace VendingMachineSampleApp.Tests;

public class VendingMachineSampleTests
{
    [Fact]
    public void SelectItem_InStock_TransitionsToItemSelected()
    {
        var machine = VendingMachineBuilder.BuildMachine();
        var data = VendingMachineData.Initialize(CreateInventory());

        var (newState, newData, _) = machine.Fire(new SelectItemTrigger("A1"), VendingMachineState.Idle, data);

        Assert.Equal(VendingMachineState.ItemSelected, newState);
        Assert.Equal("A1", newData.SelectedItemCode);
        Assert.Equal(0m, newData.MoneyInserted);
    }

    [Fact]
    public void SelectItem_OutOfStock_TransitionsToOutOfStock()
    {
        var machine = VendingMachineBuilder.BuildMachine();
        var data = VendingMachineData.Initialize(CreateInventory(stock: 0));

        var (newState, newData, _) = machine.Fire(new SelectItemTrigger("A1"), VendingMachineState.Idle, data);

        Assert.Equal(VendingMachineState.OutOfStock, newState);
        Assert.Null(newData.SelectedItemCode);
    }

    [Fact]
    public void InsertMoney_Enough_TransitionsToDispensing()
    {
        var machine = VendingMachineBuilder.BuildMachine();
        var data = VendingMachineData.Initialize(CreateInventory());
        var (selectedState, selectedData, _) = machine.Fire(new SelectItemTrigger("A1"), VendingMachineState.Idle, data);

        var (newState, newData, _) = machine.Fire(new InsertMoneyTrigger(2.00m), selectedState, selectedData);

        Assert.Equal(VendingMachineState.DispensingItem, newState);
        Assert.Equal(2.00m, newData.MoneyInserted);
    }

    [Fact]
    public void DispenseComplete_WithChange_GoesToReturningChangeThenIdle()
    {
        var machine = VendingMachineBuilder.BuildMachine();
        var data = VendingMachineData.Initialize(CreateInventory());
        var (selectedState, selectedData, _) = machine.Fire(new SelectItemTrigger("A1"), VendingMachineState.Idle, data);
        var (dispenseState, dispenseData, _) = machine.Fire(new InsertMoneyTrigger(2.00m), selectedState, selectedData);

        var (returningState, returningData, _) = machine.Fire(new DispenseCompleteTrigger(), dispenseState, dispenseData);
        var (idleState, idleData, _) = machine.Fire(new DispenseCompleteTrigger(), returningState, returningData);

        Assert.Equal(VendingMachineState.ReturningChange, returningState);
        Assert.Equal(VendingMachineState.Idle, idleState);
        Assert.Null(idleData.SelectedItemCode);
        Assert.Equal(0m, idleData.MoneyInserted);
    }

    [Fact]
    public void DispenseComplete_NoChange_GoesDirectlyToIdle()
    {
        var machine = VendingMachineBuilder.BuildMachine();
        var data = VendingMachineData.Initialize(CreateInventory());
        var (selectedState, selectedData, _) = machine.Fire(new SelectItemTrigger("A1"), VendingMachineState.Idle, data);
        var (dispenseState, dispenseData, _) = machine.Fire(new InsertMoneyTrigger(1.50m), selectedState, selectedData);

        var (idleState, idleData, _) = machine.Fire(new DispenseCompleteTrigger(), dispenseState, dispenseData);

        Assert.Equal(VendingMachineState.Idle, idleState);
        Assert.Null(idleData.SelectedItemCode);
        Assert.Equal(0m, idleData.MoneyInserted);
    }

    private static Dictionary<string, VendingItem> CreateInventory(int stock = 5)
    {
        return new Dictionary<string, VendingItem>
        {
            ["A1"] = new("A1", "Chips", 1.50m, stock)
        };
    }
}
