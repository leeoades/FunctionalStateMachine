using FunctionalStateMachine.Core;

namespace FunctionalStateMachine.Samples;

public static class IgnoreAndUnhandledSample
{
    public static StateMachine<QueueState, QueueTrigger, QueueData, QueueCommand> Build()
    {
        return StateMachine<QueueState, QueueTrigger, QueueData, QueueCommand>.Create()
            .StartWith(QueueState.Empty)
            .OnUnhandled((trigger, state) => state.Data.Log.Add($"Unhandled:{trigger}"))
            .For(QueueState.Empty)
                .On(QueueTrigger.Enqueue)
                    .TransitionTo(QueueState.HasItems)
                    .Execute(state => new EnqueueCommand(state.Data.QueueId))
                .On(QueueTrigger.Peek)
                    .Ignore()
                .For(QueueState.HasItems)
                    .On(QueueTrigger.Dequeue)
                        .TransitionTo(QueueState.Empty)
                        .Execute(state => new DequeueCommand(state.Data.QueueId))
            .Build();
    }
}

public enum QueueState
{
    Empty,
    HasItems
}

public enum QueueTrigger
{
    Enqueue,
    Dequeue,
    Peek
}

public sealed record QueueData(string QueueId, List<string> Log);

public abstract record QueueCommand;

public sealed record EnqueueCommand(string QueueId) : QueueCommand;

public sealed record DequeueCommand(string QueueId) : QueueCommand;
