using FunctionalStateMachine;
using FunctionalStateMachine.Core;

namespace FunctionalStateMachine.Samples;

public static class FraudReviewSample
{
    public static StateMachine<ReviewState, ReviewTrigger, ReviewData, ReviewCommand> Build()
    {
        var builder = new StateMachineBuilder<ReviewState, ReviewTrigger, ReviewData, ReviewCommand>()
            .StartWith(ReviewState.Pending);

        var pending = builder.For(ReviewState.Pending);
        pending.On(ReviewTrigger.Submit)
            .Guard(state => state.Data.RiskScore > 70)
            .WithData(state => state.Data with { Notes = "High risk" })
            .TransitionTo(ReviewState.Manual)
            .Execute(state => new AuditReviewCommand($"Escalated {state.Data.CaseId}"));

        pending.On(ReviewTrigger.Submit)
            .Guard(state => state.Data.RiskScore <= 70)
            .TransitionTo(ReviewState.AutoApproved)
            .Execute(() => new NotifyCommand("Auto-approved"))
            .Execute((ReviewTrigger trigger) => new AuditReviewCommand($"Trigger:{trigger}"))
            .Execute(state => new AuditReviewCommand($"Case:{state.Data.CaseId}"))
            .Execute(state => new NotifyCommand($"Approved {state.Data.CaseId}"));

        builder.For(ReviewState.Manual)
            .On(ReviewTrigger.Approve)
                .TransitionTo(ReviewState.Completed)
                .Execute(state => new ReviewCompletedCommand(state.Data.CaseId))
                .Execute(() => [new NotifyCommand("Manual approved"), new AuditReviewCommand("Manual path")]);

        return builder.Build();
    }
}

public enum ReviewState
{
    Pending,
    Manual,
    AutoApproved,
    Completed
}

public enum ReviewTrigger
{
    Submit,
    Approve
}

public sealed record ReviewData(string CaseId, int RiskScore, string Notes);

public abstract record ReviewCommand;

public sealed record NotifyCommand(string Message) : ReviewCommand;

public sealed record AuditReviewCommand(string Message) : ReviewCommand;

public sealed record ReviewCompletedCommand(string CaseId) : ReviewCommand;
