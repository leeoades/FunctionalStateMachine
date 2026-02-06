using FunctionalStateMachine.CommandRunner;
using VendingMachineSampleApp.Domain;

namespace VendingMachineSampleApp.CommandRunners;

/// <summary>
/// Handles ExitApplicationCommand by notifying the host to exit.
/// </summary>
public class ExitApplicationHandler(ExitSignal exitSignal) : ICommandRunner<ExitApplicationCommand>
{
    public void Run(ExitApplicationCommand command)
    {
        exitSignal.RequestExit();
    }
}
