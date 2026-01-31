using FunctionalStateMachine.Core;
using FunctionalStateMachine.Samples;

namespace FunctionalStateMachine.Core.Tests;

public class ShoppingTrolleySampleTests
{
    [Fact]
    public void PayByCash_UnderpaymentRequestsRemaining()
    {
        var machine = ShoppingTrolleySample.Build();
        var state = StartState(machine);

        state = Fire(CartTrigger.StartShopping(), state, machine);
        state = Fire(CartTrigger.AddItem(new LineItem("Milk", 1.30m)), state, machine);
        state = Fire(CartTrigger.AddItem(new LineItem("Bread", 0.80m)), state, machine);
        state = Fire(CartTrigger.GoToCheckout(), state, machine);
        var (next, commands) = FireWithCommands(CartTrigger.PayByCash(1.00m), state, machine);

        Assert.Equal(ShopState.CheckingOut, next.Value);
        Assert.Equal(2, next.Data.Shop.Items.Count);
        var request = Assert.Single(commands) as ShopCommand.RequestPayment;
        Assert.NotNull(request);
        Assert.Equal(1.10m, request.Total);
    }

    [Fact]
    public void PayByCash_ExactPaymentGrantsOwnership()
    {
        var machine = ShoppingTrolleySample.Build();
        var state = StartState(machine);

        state = Fire(CartTrigger.StartShopping(), state, machine);
        state = Fire(CartTrigger.AddItem(new LineItem("Milk", 1.30m)), state, machine);
        state = Fire(CartTrigger.AddItem(new LineItem("Bread", 0.80m)), state, machine);
        state = Fire(CartTrigger.GoToCheckout(), state, machine);
        var (next, commands) = FireWithCommands(CartTrigger.PayByCash(2.10m), state, machine);

        Assert.Equal(ShopState.Outside, next.Value);
        Assert.Empty(next.Data.Shop.Items);
        var grant = Assert.Single(commands) as ShopCommand.GrantItemOwnership;
        Assert.NotNull(grant);
        Assert.Equal(2, grant.Items.Count);
    }

    [Fact]
    public void PayByCash_OverpaymentIssuesRefund()
    {
        var machine = ShoppingTrolleySample.Build();
        var state = StartState(machine);

        state = Fire(CartTrigger.StartShopping(), state, machine);
        state = Fire(CartTrigger.AddItem(new LineItem("Milk", 1.30m)), state, machine);
        state = Fire(CartTrigger.AddItem(new LineItem("Bread", 0.80m)), state, machine);
        state = Fire(CartTrigger.GoToCheckout(), state, machine);
        var (next, commands) = FireWithCommands(CartTrigger.PayByCash(3.00m), state, machine);

        Assert.Equal(ShopState.Outside, next.Value);
        Assert.Empty(next.Data.Shop.Items);
        Assert.Equal(2, commands.Count);
        var grant = commands[0] as ShopCommand.GrantItemOwnership;
        var refund = commands[1] as ShopCommand.RefundCash;
        Assert.NotNull(grant);
        Assert.NotNull(refund);
        Assert.Equal(2, grant.Items.Count);
        Assert.Equal(0.90m, refund.Amount);
    }

    private static State<ShopState, CartSession> StartState(
        StateMachine<ShopState, CartTrigger, CartSession, ShopCommand> machine)
    {
        return new State<ShopState, CartSession>(
            machine.InitialStateOrDefault(),
            new CartSession(new ShopData([], 0)));
    }

    private static State<ShopState, CartSession> Fire(
        CartTrigger trigger,
        State<ShopState, CartSession> state,
        StateMachine<ShopState, CartTrigger, CartSession, ShopCommand> machine)
    {
        var (newState, _) = machine.Fire(trigger, state);
        return newState;
    }

    private static (State<ShopState, CartSession> State, IReadOnlyList<ShopCommand> Commands) FireWithCommands(
        CartTrigger trigger,
        State<ShopState, CartSession> state,
        StateMachine<ShopState, CartTrigger, CartSession, ShopCommand> machine)
    {
        return machine.Fire(trigger, state);
    }
}
