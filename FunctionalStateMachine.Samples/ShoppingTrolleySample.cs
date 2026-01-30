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
                .On(CartTrigger.ForKind(CartTriggerKind.Cancel))
                    .WithData(state => state.Data with { Shop = new ShopData([], 0) })
                    .TransitionTo(ShopState.Outside)
                .On(CartTrigger.ForKind(CartTriggerKind.PaymentSucceeded))
                    .Guard(state => state.Value == ShopState.PaymentPending)
                    .WithData(state => state.Data with { Shop = new ShopData([], 0) })
                    .TransitionTo(ShopState.Outside)
                    .Execute(state => new ShopCommand.GrantItemOwnership(state.Data.Shop.Items))
            .For(ShopState.Outside)
                .On(CartTrigger.ForKind(CartTriggerKind.StartShopping))
                    .WithData(state => state.Data with { Shop = new ShopData([], 0) })
                    .TransitionTo(ShopState.Shopping)
            .For(ShopState.Shopping)
                .SubStateOf(ShopState.InStore)
                .On(CartTrigger.ForKind(CartTriggerKind.AddItem))
                    .WithData((state, trigger) =>
                    {
                        var items = new List<LineItem>(state.Data.Shop.Items);
                        if (trigger.Item != null)
                        {
                            items.Add(trigger.Item);
                        }

                        return state.Data with { Shop = state.Data.Shop with { Items = items } };
                    })
                    .Execute(state => new ShopCommand.UpdateCartItems(state.Data.Shop.Items))
                .On(CartTrigger.ForKind(CartTriggerKind.RemoveItem))
                    .WithData((state, trigger) =>
                    {
                        var items = new List<LineItem>(state.Data.Shop.Items);
                        if (!string.IsNullOrWhiteSpace(trigger.Sku))
                        {
                            items.RemoveAll(item => item.Sku == trigger.Sku);
                        }

                        return state.Data with { Shop = state.Data.Shop with { Items = items } };
                    })
                    .Execute(state => new ShopCommand.UpdateCartItems(state.Data.Shop.Items))
                .On(CartTrigger.ForKind(CartTriggerKind.GoToCheckout))
                    .TransitionTo(ShopState.CheckingOut)
                    .Execute(state => new ShopCommand.RequestPayment(state.Data.Shop.TotalPrice()))
            .For(ShopState.CheckingOut)
                .SubStateOf(ShopState.InStore)
                .On(CartTrigger.ForKind(CartTriggerKind.Pay))
                    .TransitionTo(ShopState.PaymentPending)
                    .WithData(state => state.Data with
                    {
                        Shop = state.Data.Shop with { PaymentAttempts = state.Data.Shop.PaymentAttempts + 1 }
                    })
                    .Execute(() => new ShopCommand.DisplayPaymentPendingMessage())
            .For(ShopState.PaymentPending)
                .SubStateOf(ShopState.InStore)
                .On(CartTrigger.ForKind(CartTriggerKind.PaymentFailed))
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

public enum CartTriggerKind
{
    StartShopping,
    AddItem,
    RemoveItem,
    GoToCheckout,
    Pay,
    PaymentFailed,
    PaymentSucceeded,
    Cancel
}

public sealed class CartTrigger : IEquatable<CartTrigger>
{
    public CartTrigger(CartTriggerKind kind, LineItem? item = null, string? sku = null)
    {
        Kind = kind;
        Item = item;
        Sku = sku;
    }

    public CartTriggerKind Kind { get; }

    public LineItem? Item { get; }

    public string? Sku { get; }

    public static CartTrigger ForKind(CartTriggerKind kind) => new(kind);

    public static CartTrigger AddItem(LineItem item) => new(CartTriggerKind.AddItem, item);

    public static CartTrigger RemoveItem(string sku) => new(CartTriggerKind.RemoveItem, null, sku);

    public static CartTrigger StartShopping() => new(CartTriggerKind.StartShopping);

    public static CartTrigger GoToCheckout() => new(CartTriggerKind.GoToCheckout);

    public static CartTrigger Pay() => new(CartTriggerKind.Pay);

    public static CartTrigger PaymentFailed() => new(CartTriggerKind.PaymentFailed);

    public static CartTrigger PaymentSucceeded() => new(CartTriggerKind.PaymentSucceeded);

    public static CartTrigger Cancel() => new(CartTriggerKind.Cancel);

    public bool Equals(CartTrigger? other) => other is not null && Kind == other.Kind;

    public override bool Equals(object? obj) => obj is CartTrigger other && Equals(other);

    public override int GetHashCode() => (int)Kind;
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
