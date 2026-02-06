namespace StockPurchaserSampleApp.Domain;

public abstract record StockPurchaserTrigger;

public record SetTargetPriceTrigger(decimal TargetPrice) : StockPurchaserTrigger;

public record PriceTickTrigger(decimal Price) : StockPurchaserTrigger;

public record ResetTrigger : StockPurchaserTrigger;
