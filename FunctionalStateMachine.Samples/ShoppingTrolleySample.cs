namespace FunctionalStateMachine.Samples;

public static class ShoppingTrolleySample
{
    public static Core.StateMachine<ShopState, CartTrigger, CartSession, ShopCommand> Build()
    {
        var shopMachine = Core.StateMachine<ShopPhase, CartTrigger, ShopData, ShopCommand>.Create()
            .StartWith(ShopPhase.Shopping)
            .For(ShopPhase.Shopping)
                .On(CartTrigger.ForKind(CartTriggerKind.AddItem))
                    .WithData((state, trigger) =>
                    {
                        var items = new List<LineItem>(state.Data.Items);
                        if (trigger.Item != null)
                        {
                            items.Add(trigger.Item);
                        }
                        return state.Data with { Items = items };
                    })
                    .Execute(state => new CartUpdatedCommand(state.Data.Items))
                .On(CartTrigger.ForKind(CartTriggerKind.RemoveItem))
                    .WithData((state, trigger) =>
                    {
                        var items = new List<LineItem>(state.Data.Items);
                        if (!string.IsNullOrWhiteSpace(trigger.Sku))
                        {
                            items.RemoveAll(item => item.Sku == trigger.Sku);
                        }
                        return state.Data with { Items = items };
                    })
                    .Execute(state => new CartUpdatedCommand(state.Data.Items))
                .On(CartTrigger.ForKind(CartTriggerKind.GoToCheckout))
                    .TransitionTo(ShopPhase.CheckingOut)
                    .Execute(state => new TotalCalculatedCommand(state.Data.TotalPrice()))
                .For(ShopPhase.CheckingOut)
                    .On(CartTrigger.ForKind(CartTriggerKind.Pay))
                        .TransitionTo(ShopPhase.PaymentPending)
                        .WithData(state => state.Data with { PaymentAttempts = state.Data.PaymentAttempts + 1 })
                        .Execute(() => new PaymentRequestedCommand())
                    .For(ShopPhase.PaymentPending)
                        .On(CartTrigger.ForKind(CartTriggerKind.PaymentFailed))
                            .TransitionTo(ShopPhase.CheckingOut)
                            .Execute(() => new PaymentFailedCommand())
            .Build();

        return Core.StateMachine<ShopState, CartTrigger, CartSession, ShopCommand>.Create()
            .StartWith(ShopState.Outside)
            .For(ShopState.Outside)
                .On(CartTrigger.ForKind(CartTriggerKind.StartShopping))
                    .WithData(state => state.Data with
                    {
                        Shop = new Core.SubState<ShopPhase, ShopData>(
                            ShopPhase.Shopping,
                            new ShopData([], 0))
                    })
                    .TransitionTo(ShopState.InStore)
                .For(ShopState.InStore)
                    .WithSubStateMachine(
                        shopMachine,
                        data => data.Shop,
                        (data, sub) => data with { Shop = sub })
                    .On(CartTrigger.ForKind(CartTriggerKind.Cancel))
                        .WithData(state => state.Data with
                        {
                            Shop = new Core.SubState<ShopPhase, ShopData>(
                                ShopPhase.Shopping,
                                new ShopData([], 0))
                        })
                        .TransitionTo(ShopState.Outside)
                    .On(CartTrigger.ForKind(CartTriggerKind.PaymentSucceeded))
                        .Guard(state => state.Data.Shop.Value == ShopPhase.PaymentPending)
                        .TransitionTo(ShopState.Outside)
                        .Execute(state => new OwnershipGrantedCommand(state.Data.Shop.Data.Items))
            .Build();
    }
}

public enum ShopState
{
    Outside,
    InStore
}

public enum ShopPhase
{
    Shopping,
    CheckingOut,
    PaymentPending
}

public sealed record CartSession(Core.SubState<ShopPhase, ShopData> Shop);

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

public abstract record ShopCommand;

public sealed record CartUpdatedCommand(IReadOnlyList<LineItem> Items) : ShopCommand;

public sealed record TotalCalculatedCommand(decimal Total) : ShopCommand;

public sealed record PaymentRequestedCommand() : ShopCommand;

public sealed record PaymentFailedCommand() : ShopCommand;

public sealed record OwnershipGrantedCommand(IReadOnlyList<LineItem> Items) : ShopCommand;
