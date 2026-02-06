namespace VendingMachineSampleApp;

/// <summary>
/// Tracks whether the host should exit after processing commands.
/// </summary>
public sealed class ExitSignal
{
    public bool ShouldExit { get; private set; }

    public void RequestExit()
    {
        ShouldExit = true;
    }
}
