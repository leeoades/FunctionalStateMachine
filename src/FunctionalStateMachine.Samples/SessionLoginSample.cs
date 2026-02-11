using FunctionalStateMachine.Core;
using FunctionalStateMachine.Diagrams;

namespace FunctionalStateMachine.Samples;

public static class SessionLoginSample
{
[StateMachineDiagram("diagrams/SessionLogin.md")]
    public static StateMachine<SessionState, SessionTrigger, SessionData, SessionCommand> Build()
    {
        return StateMachine<SessionState, SessionTrigger, SessionData, SessionCommand>.Create()
            .StartWith(SessionState.Active)
            .For(SessionState.Active)
                .StartsWith(SessionState.Anonymous)
                .On<SessionTrigger.TimeoutTrigger>()
                    .TransitionTo(SessionState.Expired)
                    .Execute(() => new SessionCommand.HandleTimeout())
                .For(SessionState.Anonymous)
                    .SubStateOf(SessionState.Active)
                    .On<SessionTrigger.LoginTrigger>()
                        .TransitionTo(SessionState.Authenticated)
                        .Execute(() => new AuthCommand.PerformLogin())
                .For(SessionState.Authenticated)
                    .SubStateOf(SessionState.Active)
                    .On<SessionTrigger.LogoutTrigger>()
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

public abstract record SessionTrigger
{
    public sealed record LoginTrigger : SessionTrigger;
    public sealed record LogoutTrigger : SessionTrigger;
    public sealed record TimeoutTrigger : SessionTrigger;

    public static readonly SessionTrigger Login = new LoginTrigger();
    public static readonly SessionTrigger Logout = new LogoutTrigger();
    public static readonly SessionTrigger Timeout = new TimeoutTrigger();
}

public sealed record SessionData(string UserId)
{
    public static SessionData Initial => new(string.Empty);
}

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
