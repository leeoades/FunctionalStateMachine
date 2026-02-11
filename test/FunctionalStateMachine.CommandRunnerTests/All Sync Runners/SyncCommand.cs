namespace FunctionalStateMachine.CommandRunnerTests.All_Sync_Runners;

public abstract record SyncCommand
{
    public record Alpha : SyncCommand;
    public record Beta : SyncCommand;
}