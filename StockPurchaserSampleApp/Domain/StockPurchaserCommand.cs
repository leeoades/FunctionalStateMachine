namespace StockPurchaserSampleApp.Domain;

public abstract record StockPurchaserCommand;

public record LogCommand(string Message, ConsoleColor? Color = null) : StockPurchaserCommand;

public record ExecutePurchaseCommand(string Symbol, decimal Price) : StockPurchaserCommand;
