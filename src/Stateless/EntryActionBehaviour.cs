using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal abstract class EntryActionBehavior
        {
            Reflection.InvocationInfo _description;

            protected EntryActionBehavior(Reflection.InvocationInfo description)
            {
                _description = description;
            }

            public Reflection.InvocationInfo Description => _description;

            public abstract void Execute(Transition transition, object[] args);
            public abstract Task ExecuteAsync(Transition transition, object[] args);

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

                public override Task ExecuteAsync(Transition transition, object[] args)
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

                public override Task ExecuteAsync(Transition transition, object[] args)
                {
                    Execute(transition, args);
                    return TaskResult.Done;
                }
            }

            public class Async : EntryActionBehavior
            {
                readonly Func<Transition, object[], Task> _action;

                public Async(Func<Transition, object[], Task> action, Reflection.InvocationInfo description) : base(description)
                {
                    _action = action;
                }

                public override void Execute(Transition transition, object[] args)
                {
                    throw new InvalidOperationException(
                        $"Cannot execute asynchronous action specified in OnEntry event for '{transition.Destination}' state. " +
                         "Use asynchronous version of Fire [FireAsync]");
                }

                public override Task ExecuteAsync(Transition transition, object[] args)
                {
                    return _action(transition, args);
                }
            }
        }
    }
}
