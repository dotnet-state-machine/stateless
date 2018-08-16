using System;
using System.Threading;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal abstract class EntryActionBehavior
        {
            protected EntryActionBehavior(Reflection.InvocationInfo description)
            {
                Description = description;
            }

            public Reflection.InvocationInfo Description { get; }

            public abstract void Execute(Transition transition, object[] args);
            public abstract Task ExecuteAsync(Transition transition, object[] args, CancellationToken ct);

            public class Sync : EntryActionBehavior
            {
                readonly Action<Transition, object[]> _action;

                public Sync(Action<Transition, object[]> action, Reflection.InvocationInfo description) : base(description)
                {
                    _action = action;
                }

                public override void Execute(Transition transition, object[] args)
                {
                    _action(transition, args);
                }

                public override Task ExecuteAsync(Transition transition, object[] args, CancellationToken ct)
                {
                    Execute(transition, args);
                    return TaskResult.Done;
                }
            }

            public class SyncFrom<TTriggerType> : Sync
            {
                internal TTriggerType Trigger { get; private set; }

                public SyncFrom(TTriggerType trigger, Action<Transition, object[]> action, Reflection.InvocationInfo description)
                    : base(action, description)
                {
                    Trigger = trigger;
                }

                public override void Execute(Transition transition, object[] args)
                {
                    if (transition.Trigger.Equals(Trigger))
                        base.Execute(transition, args);
                }

                public override Task ExecuteAsync(Transition transition, object[] args, CancellationToken ct)
                {
                    Execute(transition, args);
                    return TaskResult.Done;
                }
            }

            public class Async : EntryActionBehavior
            {
                readonly Func<Transition, object[], CancellationToken, Task> _action;

                public Async(Func<Transition, object[], CancellationToken, Task> action, Reflection.InvocationInfo description) : base(description)
                {
                    _action = action;
                }

                public override void Execute(Transition transition, object[] args)
                {
                    throw new InvalidOperationException(
                        $"Cannot execute asynchronous action specified in OnEntry event for '{transition.Destination}' state. " +
                         "Use asynchronous version of Fire [FireAsync]");
                }

                public override Task ExecuteAsync(Transition transition, object[] args, CancellationToken ct)
                {
                    return _action(transition, args, ct);
                }
            }
        }
    }
}
