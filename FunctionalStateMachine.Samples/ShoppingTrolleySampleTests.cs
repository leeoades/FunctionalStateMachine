using FunctionalStateMachine.Core;

namespace FunctionalStateMachine.Samples;

public class ShoppingTrolleySampleTests
{
    [Fact]
    public void PayByCash_UnderpaymentRequestsRemaining()
    {
        var machine = ShoppingTrolleySample.Build();
        var (state, data) = StartState(machine);

        (state, data) = Fire(CartTrigger.StartShopping(), state, data, machine);
        (state, data) = Fire(CartTrigger.AddItem(new LineItem("Milk", 1.30m)), state, data, machine);
        (state, data) = Fire(CartTrigger.AddItem(new LineItem("Bread", 0.80m)), state, data, machine);
        (state, data) = Fire(CartTrigger.GoToCheckout(), state, data, machine);
        var (nextState, nextData, commands) = FireWithCommands(CartTrigger.PayByCash(1.00m), state, data, machine);

        Assert.Equal(ShopState.CheckingOut, nextState);
        Assert.Equal(2, nextData.Shop.Items.Count);
        var request = Assert.Single(commands) as ShopCommand.RequestPayment;
        Assert.NotNull(request);
        Assert.Equal(1.10m, request.Total);
    }

    [Fact]
    public void PayByCash_ExactPaymentGrantsOwnership()
    {
        var machine = ShoppingTrolleySample.Build();
        var (state, data) = StartState(machine);

        (state, data) = Fire(CartTrigger.StartShopping(), state, data, machine);
        (state, data) = Fire(CartTrigger.AddItem(new LineItem("Milk", 1.30m)), state, data, machine);
        (state, data) = Fire(CartTrigger.AddItem(new LineItem("Bread", 0.80m)), state, data, machine);
        (state, data) = Fire(CartTrigger.GoToCheckout(), state, data, machine);
        var (nextState, nextData, commands) = FireWithCommands(CartTrigger.PayByCash(2.10m), state, data, machine);

        Assert.Equal(ShopState.Outside, nextState);
        Assert.Empty(nextData.Shop.Items);
        var grant = Assert.Single(commands) as ShopCommand.GrantItemOwnership;
        Assert.NotNull(grant);
        Assert.Equal(2, grant.Items.Count);
    }

    [Fact]
    public void Pay_GrantsOwnership()
    {
        var machine = ShoppingTrolleySample.Build();
        var (state, data) = StartState(machine);

        (state, data) = Fire(CartTrigger.StartShopping(), state, data, machine);
        (state, data) = Fire(CartTrigger.AddItem(new LineItem("Milk", 1.30m)), state, data, machine);
        (state, data) = Fire(CartTrigger.AddItem(new LineItem("Bread", 0.80m)), state, data, machine);
        (state, data) = Fire(CartTrigger.GoToCheckout(), state, data, machine);
        (state, data) = Fire(CartTrigger.Pay(), state, data, machine);
        var (nextState, nextData, commands) = FireWithCommands(CartTrigger.PaymentSucceeded(), state, data, machine);

        Assert.Equal(ShopState.Outside, nextState);
        Assert.Empty(nextData.Shop.Items);
        var grant = Assert.Single(commands) as ShopCommand.GrantItemOwnership;
        Assert.NotNull(grant);
        Assert.Equal(2, grant.Items.Count);
    }

    [Fact]
    public void PayByCash_OverpaymentIssuesRefund()
    {
        var machine = ShoppingTrolleySample.Build();
        var (state, data) = StartState(machine);

        (state, data) = Fire(CartTrigger.StartShopping(), state, data, machine);
        (state, data) = Fire(CartTrigger.AddItem(new LineItem("Milk", 1.30m)), state, data, machine);
        (state, data) = Fire(CartTrigger.AddItem(new LineItem("Bread", 0.80m)), state, data, machine);
        (state, data) = Fire(CartTrigger.GoToCheckout(), state, data, machine);
        var (nextState, nextData, commands) = FireWithCommands(CartTrigger.PayByCash(3.00m), state, data, machine);

        Assert.Equal(ShopState.Outside, nextState);
        Assert.Empty(nextData.Shop.Items);
        Assert.Equal(2, commands.Count);
        var grant = commands[0] as ShopCommand.GrantItemOwnership;
        var refund = commands[1] as ShopCommand.RefundCash;
        Assert.NotNull(grant);
        Assert.NotNull(refund);
        Assert.Equal(2, grant.Items.Count);
        Assert.Equal(0.90m, refund.Amount);
    }

    private static (ShopState State, CartSession Data) StartState(
        StateMachine<ShopState, CartTrigger, CartSession, ShopCommand> machine)
    {
        return (machine.InitialState, new CartSession(new ShopData([], 0)));
    }

    private static (ShopState State, CartSession Data) Fire(
        CartTrigger trigger,
        ShopState state,
        CartSession data,
        StateMachine<ShopState, CartTrigger, CartSession, ShopCommand> machine)
    {
        var (newState, newData, _) = machine.Fire(trigger, state, data);
        return (newState, newData);
    }

    private static (ShopState State, CartSession Data, IReadOnlyList<ShopCommand> Commands) FireWithCommands(
        CartTrigger trigger,
        ShopState state,
        CartSession data,
        StateMachine<ShopState, CartTrigger, CartSession, ShopCommand> machine)
    {
        return machine.Fire(trigger, state, data);
    }
}
