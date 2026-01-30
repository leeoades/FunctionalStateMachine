using FunctionalStateMachine.Core;
using Xunit.Abstractions;

namespace FunctionalStateMachine.Samples;

public static class ShoppingTrolleySample
{
    public static Core.StateMachine<ShopState, CartTrigger, CartSession, ShopCommand> Build()
    {
        return Core.StateMachine<ShopState, CartTrigger, CartSession, ShopCommand>.Create()
            .StartWith(ShopState.Outside)
            .For(ShopState.InStore)
                .StartsWith(ShopState.Shopping)
                .On<CartTrigger.CancelTrigger>()
                    .ModifyData(state => state.Data with { Shop = new ShopData([], 0) })
                    .TransitionTo(ShopState.Outside)
                .On<CartTrigger.PaymentSucceededTrigger>()
                    .Guard(state => state.Value == ShopState.PaymentPending)
                    .Execute(state => new ShopCommand.GrantItemOwnership(state.Data.Shop.Items))
                    .ModifyData(state => state.Data with { Shop = new ShopData([], 0) })
                    .TransitionTo(ShopState.Outside)
            .For(ShopState.Outside)
                .On<CartTrigger.StartShoppingTrigger>()
                    .ModifyData(state => state.Data with { Shop = new ShopData([], 0) })
                    .TransitionTo(ShopState.Shopping)
            .For(ShopState.Shopping)
                .SubStateOf(ShopState.InStore)
                .On<CartTrigger.AddItemTrigger>()
                    .ModifyData((state, trigger) =>
                    {
                        var items = new List<LineItem>(state.Data.Shop.Items);
                        items.Add(trigger.Item);

                        return state.Data with { Shop = state.Data.Shop with { Items = items } };
                    })
                    .Execute(state => new ShopCommand.UpdateCartItems(state.Data.Shop.Items))
                .On<CartTrigger.RemoveItemTrigger>()
                    .ModifyData((state, trigger) =>
                    {
                        var items = new List<LineItem>(state.Data.Shop.Items);
                        items.RemoveAll(item => item.Sku == trigger.Sku);

                        return state.Data with { Shop = state.Data.Shop with { Items = items } };
                    })
                    .Execute(state => new ShopCommand.UpdateCartItems(state.Data.Shop.Items))
                .On<CartTrigger.GoToCheckoutTrigger>()
                    .TransitionTo(ShopState.CheckingOut)
                    .Execute(state => new ShopCommand.RequestPayment(state.Data.Shop.TotalPrice()))
            .For(ShopState.CheckingOut)
                .SubStateOf(ShopState.InStore)
                .On<CartTrigger.PayTrigger>()
                    .TransitionTo(ShopState.PaymentPending)
                    .ModifyData(state => state.Data with
                    {
                        Shop = state.Data.Shop with { PaymentAttempts = state.Data.Shop.PaymentAttempts + 1 }
                    })
                    .Execute(() => new ShopCommand.DisplayPaymentPendingMessage())
            .For(ShopState.PaymentPending)
                .SubStateOf(ShopState.InStore)
                .On<CartTrigger.PaymentFailedTrigger>()
                    .TransitionTo(ShopState.CheckingOut)
                    .Execute(state =>
                    [
                        new ShopCommand.DisplayPaymentFailedMessage(),
                        new ShopCommand.RequestPayment(state.Data.Shop.TotalPrice())
                    ])
            .Build();
    }
}

public enum ShopState
{
    Outside,
    InStore,
    Shopping,
    CheckingOut,
    PaymentPending
}

public sealed record CartSession(ShopData Shop);

public sealed record ShopData(List<LineItem> Items, int PaymentAttempts)
{
    public decimal TotalPrice() => Items.Sum(item => item.Price);
}

public sealed record LineItem(string Sku, decimal Price);

public abstract record CartTrigger
{
    public sealed record StartShoppingTrigger : CartTrigger;
    public sealed record AddItemTrigger(LineItem Item) : CartTrigger;
    public sealed record RemoveItemTrigger(string Sku) : CartTrigger;
    public sealed record GoToCheckoutTrigger : CartTrigger;
    public sealed record PayTrigger : CartTrigger;
    public sealed record PaymentFailedTrigger : CartTrigger;
    public sealed record PaymentSucceededTrigger : CartTrigger;
    public sealed record CancelTrigger : CartTrigger;

    private static readonly CartTrigger StartShoppingInstance = new StartShoppingTrigger();
    private static readonly CartTrigger GoToCheckoutInstance = new GoToCheckoutTrigger();
    private static readonly CartTrigger PayInstance = new PayTrigger();
    private static readonly CartTrigger PaymentFailedInstance = new PaymentFailedTrigger();
    private static readonly CartTrigger PaymentSucceededInstance = new PaymentSucceededTrigger();
    private static readonly CartTrigger CancelInstance = new CancelTrigger();

    public static CartTrigger StartShopping() => StartShoppingInstance;

    public static CartTrigger AddItem(LineItem item) => new AddItemTrigger(item);

    public static CartTrigger RemoveItem(string sku) => new RemoveItemTrigger(sku);

    public static CartTrigger GoToCheckout() => GoToCheckoutInstance;

    public static CartTrigger Pay() => PayInstance;

    public static CartTrigger PaymentFailed() => PaymentFailedInstance;

    public static CartTrigger PaymentSucceeded() => PaymentSucceededInstance;

    public static CartTrigger Cancel() => CancelInstance;
}

public abstract record ShopCommand
{
    public sealed record UpdateCartItems(IReadOnlyList<LineItem> Items) : ShopCommand;

    public sealed record RequestPayment(decimal Total) : ShopCommand;

    public sealed record DisplayPaymentPendingMessage : ShopCommand;

    public sealed record DisplayPaymentFailedMessage : ShopCommand;

    public sealed record GrantItemOwnership(IReadOnlyList<LineItem> Items) : ShopCommand;
}

public class ShoppingTrolleyDemo(ITestOutputHelper output)
{
    [Fact]
    public void Demo()
    {
        var machine = ShoppingTrolleySample.Build();
        var state = new State<ShopState, CartSession>(machine.InitialStateOrDefault(), new CartSession(new ShopData([], 0)));

        state = Fire(CartTrigger.StartShopping(), state, machine);
        state = Fire(CartTrigger.AddItem(new LineItem("Milk", 1.30m)), state, machine);
        state = Fire(CartTrigger.AddItem(new LineItem("Bread", 0.80m)), state, machine);
        state = Fire(CartTrigger.GoToCheckout(), state, machine);
        state = Fire(CartTrigger.Pay(), state, machine);
        state = Fire(CartTrigger.PaymentSucceeded(), state, machine);
    }

    private State<ShopState, CartSession> Fire(
        CartTrigger trigger,
        State<ShopState, CartSession> state,
        StateMachine<ShopState, CartTrigger, CartSession, ShopCommand> machine)
    {
        var (newState, commands) = machine.Fire(trigger, state);
        Run(commands);
        return newState;
    }

    private void Run(IReadOnlyList<ShopCommand> commands)
    {
        foreach (var command in commands)
        {
            Print(command switch        
            {
                ShopCommand.UpdateCartItems update => $"Updated cart items: {string.Join(", ", update.Items)}",
                ShopCommand.RequestPayment request => $"Requesting payment for {request.Total}",
                ShopCommand.DisplayPaymentPendingMessage => "Payment pending...",
                ShopCommand.DisplayPaymentFailedMessage => "Payment failed...",
                ShopCommand.GrantItemOwnership grant => $"Purchase complete of {string.Join(", ", grant.Items)}",
                _ => throw new ArgumentOutOfRangeException()
            });
        }
    }
    
    private void Print(string s) => output.WriteLine(s);
}
