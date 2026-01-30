using FunctionalStateMachine.Core;

namespace FunctionalStateMachine.Samples;

public static class SessionLoginSample
{
    public static StateMachine<SessionState, SessionTrigger, SessionData, SessionCommand> Build()
    {
        return StateMachine<SessionState, SessionTrigger, SessionData, SessionCommand>.Create()
            .StartWith(SessionState.Active)
            .For(SessionState.Active)
                .StartsWith(SessionState.Anonymous)
                .On(SessionTrigger.Timeout)
                    .TransitionTo(SessionState.Expired)
                    .Execute(() => new SessionCommand.HandleTimeout())
                .For(SessionState.Anonymous)
                    .SubStateOf(SessionState.Active)
                    .On(SessionTrigger.Login)
                        .TransitionTo(SessionState.Authenticated)
                        .Execute(() => new AuthCommand.PerformLogin())
                .For(SessionState.Authenticated)
                    .SubStateOf(SessionState.Active)
                    .On(SessionTrigger.Logout)
                        .TransitionTo(SessionState.Anonymous)
                        .Execute(() => new AuthCommand.PerformLogout())
                .For(SessionState.Expired)
                    .OnEntry(() => new SessionCommand.DisplayExpiredMessage())
            .Build();
    }
}

public enum SessionState
{
    Active,
    Expired,
    Anonymous,
    Authenticated
}

public enum SessionTrigger
{
    Login,
    Logout,
    Timeout
}

public sealed record SessionData(string UserId);

public abstract record SessionCommand
{
    public sealed record HandleTimeout : SessionCommand;
    public sealed record DisplayExpiredMessage : SessionCommand;
}

public abstract record AuthCommand : SessionCommand
{
    public sealed record PerformLogin : AuthCommand;
    public sealed record PerformLogout : AuthCommand;
}
