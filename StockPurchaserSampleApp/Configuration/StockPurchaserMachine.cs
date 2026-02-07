using FunctionalStateMachine.Core;
using FunctionalStateMachine.Diagrams;
using StockPurchaserSampleApp.Domain;

namespace StockPurchaserSampleApp.Configuration;

public static class StockPurchaserMachine
{
    [StateMachineDiagram("diagrams/StockPurchaser.md")]
    public static StateMachine<StockPurchaserState, StockPurchaserTrigger, StockPurchaserData, StockPurchaserCommand> Build()
    {
        return StateMachine<StockPurchaserState, StockPurchaserTrigger, StockPurchaserData, StockPurchaserCommand>
            .Create()
            .StartWith(StockPurchaserState.Idle)

            .For(StockPurchaserState.Idle)
            .On<SetTargetPriceTrigger>()
            .ModifyData((data, trigger) => data with { TargetBuyPrice = trigger.TargetPrice })
            .Execute((data, trigger) =>
            [
                new LogCommand($"[{data.Symbol}] Target buy price set at {trigger.TargetPrice:C}.", ConsoleColor.DarkYellow)    
            ])
            .TransitionTo(StockPurchaserState.Tracking)

            .For(StockPurchaserState.Tracking)
            .On<PriceTickTrigger>()
            .ModifyData((data, trigger) => data with { LastPrice = trigger.Price })
            .If((data, trigger) => data.TargetBuyPrice.HasValue && trigger.Price <= data.TargetBuyPrice)
                .Execute((_, data, trigger) =>
                [
                    new ExecutePurchaseCommand(data.Symbol, trigger.Price),
                    new LogCommand($"[{data.Symbol}] Purchase executed at {trigger.Price:C}.")
                ])
                .ModifyData(data => data with { Purchased = true })
                .TransitionTo(StockPurchaserState.Purchased)
            .Else()
                .Execute((data, trigger) =>
                [
                    new LogCommand($"[{data.Symbol}] Price tick {trigger.Price:C}; waiting for {data.TargetBuyPrice:C}.")
                ])
                .Done()
            .On<ResetTrigger>()
            .ModifyData(data => data with { TargetBuyPrice = null, Purchased = false })
            .TransitionTo(StockPurchaserState.Idle)

        .For(StockPurchaserState.Purchased)
        .On<PriceTickTrigger>()
        .Execute((data, trigger) =>
        [
            new LogCommand($"[{data.Symbol}] Price tick {trigger.Price:C}; already purchased.")
        ])
        .Done()
        .On<ResetTrigger>()
        .ModifyData(data => data with { TargetBuyPrice = null, Purchased = false })
        .TransitionTo(StockPurchaserState.Idle)
        .Build();
    }
}
