using FunctionalStateMachine.Core;

namespace FunctionalStateMachine.Samples;

public static class SubMachineSample
{
    public static StateMachine<SessionState, SessionTrigger, SessionData, SessionCommand> Build()
    {
        var authMachine = StateMachine<AuthState, SessionTrigger, AuthData, SessionCommand>.Create()
            .StartWith(AuthState.Anonymous)
            .For(AuthState.Anonymous)
                .On(SessionTrigger.Login)
                    .TransitionTo(AuthState.Authenticated)
                    .Execute(() => new AuthCommand("Login"))
                .For(AuthState.Authenticated)
                    .On(SessionTrigger.Logout)
                        .TransitionTo(AuthState.Anonymous)
                        .Execute(() => new AuthCommand("Logout"))
            .Build();

        return StateMachine<SessionState, SessionTrigger, SessionData, SessionCommand>.Create()
            .StartWith(SessionState.Active)
            .For(SessionState.Active)
                .WithSubStateMachine(
                    authMachine,
                    data => data.Auth,
                    (data, sub) => data with { Auth = sub })
                .On(SessionTrigger.Timeout)
                    .TransitionTo(SessionState.Expired)
                    .Execute(() => new SessionCommandBase("Timeout"))
                .For(SessionState.Expired)
                    .OnEntry(() => new SessionCommandBase("ExpiredEntry"))
            .Build();
    }
}

public enum SessionState
{
    Active,
    Expired
}

public enum AuthState
{
    Anonymous,
    Authenticated
}

public enum SessionTrigger
{
    Login,
    Logout,
    Timeout
}

public sealed record SessionData(SubState<AuthState, AuthData> Auth);

public sealed record AuthData(string UserId);

public abstract record SessionCommand;

public sealed record AuthCommand(string Action) : SessionCommand;

public sealed record SessionCommandBase(string Name) : SessionCommand;
