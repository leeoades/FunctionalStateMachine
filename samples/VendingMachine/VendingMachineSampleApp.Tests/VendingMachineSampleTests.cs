using VendingMachineSampleApp.Configuration;
using VendingMachineSampleApp.Domain;

namespace VendingMachineSampleApp.Tests;

public class VendingMachineSampleTests
{
    [Fact]
    public void SelectItem_InStock_TransitionsToPayment()
    {
        var machine = VendingMachineBuilder.BuildMachine();
        var data = VendingMachineData.Initialize(CreateInventory());

        var (newState, newData, _) = machine.Fire(new SelectItemTrigger("A1"), VendingMachineState.Idle, data);

        Assert.Equal(VendingMachineState.PaymentMoneyDue, newState);
        Assert.Equal("A1", newData.SelectedItemCode);
        Assert.Equal(0m, newData.MoneyInserted);
    }

    [Fact]
    public void SelectItem_OutOfStock_TransitionsToOutOfStock()
    {
        var machine = VendingMachineBuilder.BuildMachine();
        var data = VendingMachineData.Initialize(CreateInventory(stock: 0));

        var (newState, newData, _) = machine.Fire(new SelectItemTrigger("A1"), VendingMachineState.Idle, data);

        Assert.Equal(VendingMachineState.Idle, newState);
        Assert.Null(newData.SelectedItemCode);
    }

    [Fact]
    public void InsertMoney_PartialPayment_StaysInMoneyDue()
    {
        var machine = VendingMachineBuilder.BuildMachine();
        var data = VendingMachineData.Initialize(CreateInventory());
        var (selectedState, selectedData, _) = machine.Fire(new SelectItemTrigger("A1"), VendingMachineState.Idle, data);

        var (newState, newData, _) = machine.Fire(new InsertMoneyTrigger(1.00m), selectedState, selectedData);

        Assert.Equal(VendingMachineState.PaymentMoneyDue, newState);
        Assert.Equal(1.00m, newData.MoneyInserted);
    }

    [Fact]
    public void InsertMoney_Enough_TransitionsToIdle()
    {
        var machine = VendingMachineBuilder.BuildMachine();
        var data = VendingMachineData.Initialize(CreateInventory());
        var (selectedState, selectedData, _) = machine.Fire(new SelectItemTrigger("A1"), VendingMachineState.Idle, data);

        var (newState, newData, _) = machine.Fire(new InsertMoneyTrigger(2.00m), selectedState, selectedData);

        Assert.Equal(VendingMachineState.Idle, newState);
        Assert.Null(newData.SelectedItemCode);
        Assert.Equal(0m, newData.MoneyInserted);
    }

    [Fact]
    public void InsertMoney_NoChange_GoesToIdle()
    {
        var machine = VendingMachineBuilder.BuildMachine();
        var data = VendingMachineData.Initialize(CreateInventory());
        var (selectedState, selectedData, _) = machine.Fire(new SelectItemTrigger("A1"), VendingMachineState.Idle, data);

        var (newState, newData, _) = machine.Fire(new InsertMoneyTrigger(1.50m), selectedState, selectedData);

        Assert.Equal(VendingMachineState.Idle, newState);
        Assert.Null(newData.SelectedItemCode);
        Assert.Equal(0m, newData.MoneyInserted);
    }

    private static Dictionary<string, VendingItem> CreateInventory(int stock = 5)
    {
        return new Dictionary<string, VendingItem>
        {
            ["A1"] = new("A1", "Chips", 1.50m, stock)
        };
    }
}
