using FunctionalStateMachine.Core;

namespace FunctionalStateMachine.Samples;

public static class FraudReviewSample
{
    public static StateMachine<ReviewState, ReviewTrigger, ReviewData, ReviewCommand> Build()
    {
        return StateMachine<ReviewState, ReviewTrigger, ReviewData, ReviewCommand>.Create()
            .StartWith(ReviewState.Pending)
            .For(ReviewState.Pending)
                .On(ReviewTrigger.Submit)
                    .Guard(state => state.Data.RiskScore > 70)
                    .WithData(state => state.Data with { Notes = "High risk" })
                    .TransitionTo(ReviewState.Manual)
                    .Execute(state => new ReviewCommand.Audit($"Escalated {state.Data.CaseId}"))
            .On(ReviewTrigger.Submit)
                .Guard(state => state.Data.RiskScore <= 70)
                .TransitionTo(ReviewState.AutoApproved)
                .Execute(() => new ReviewCommand.Notify("Auto-approved"))
                .Execute((ReviewTrigger trigger) => new ReviewCommand.Audit($"Trigger:{trigger}"))
                .Execute(state => new ReviewCommand.Audit($"Case:{state.Data.CaseId}"))
                .Execute(state => new ReviewCommand.Notify($"Approved {state.Data.CaseId}"))
            .For(ReviewState.Manual)
                .On(ReviewTrigger.Approve)
                    .TransitionTo(ReviewState.Completed)
                    .Execute(state => new ReviewCommand.Completed(state.Data.CaseId))
                    .Execute(() => [new ReviewCommand.Notify("Manual approved"), new ReviewCommand.Audit("Manual path")])
            .Build();
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

public abstract record ReviewCommand
{
    public sealed record Notify(string Message) : ReviewCommand;
    public sealed record Audit(string Message) : ReviewCommand;
    public sealed record Completed(string CaseId) : ReviewCommand;
}
