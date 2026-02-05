using FunctionalStateMachine.CommandRunner;
using VendingMachineSampleApp.Domain;

namespace VendingMachineSampleApp.CommandRunners;

/// <summary>
/// Handles UpdateSalesMetricsCommand by tracking sales analytics.
/// In a real system, this would send data to a monitoring/analytics service.
/// </summary>
public class UpdateSalesMetricsHandler : ICommandRunner<UpdateSalesMetricsCommand>
{
    private decimal _totalRevenue = 0m;
    private int _successfulTransactions = 0;
    private int _failedTransactions = 0;

    public void Run(UpdateSalesMetricsCommand command)
    {
        _totalRevenue += command.Revenue;
        
        if (command.Success)
            _successfulTransactions++;
        else
            _failedTransactions++;

        var successRate = _successfulTransactions + _failedTransactions > 0
            ? (decimal)_successfulTransactions / (_successfulTransactions + _failedTransactions) * 100
            : 0;

        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine($"📈 [METRICS] Revenue: ${_totalRevenue:F2} | Success Rate: {successRate:F1}% | Successful: {_successfulTransactions} | Failed: {_failedTransactions}");
        Console.ResetColor();
    }
}
