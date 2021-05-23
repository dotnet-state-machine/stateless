using System;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class EntryActionBehavior
        {
            private readonly EventCallback<Transition, object[]> _callback;

            public EntryActionBehavior(Action<Transition, object[]> action, Reflection.InvocationInfo description)
                : this(description)
            {
                _callback = EventCallbackFactory.Create(action);
            }

            public EntryActionBehavior(Func<Transition, object[], Task> action, Reflection.InvocationInfo description)
                : this(description)
            {
                _callback = EventCallbackFactory.Create(action);
            }

            protected EntryActionBehavior(Reflection.InvocationInfo description)
            {
                Description = description;
            }

            public Reflection.InvocationInfo Description { get; }

            public virtual Task ExecuteAsync(Transition transition, object[] args)
            {
                return _callback.InvokeAsync(transition, args);
            }

            public class From<TTriggerType> : EntryActionBehavior
            {
                internal TTriggerType Trigger { get; private set; }

                public From(TTriggerType trigger, Action<Transition, object[]> action, Reflection.InvocationInfo description)
                    : base(action, description)
                {
                    Trigger = trigger;
                }

                public From(TTriggerType trigger, Func<Transition, object[], Task> action, Reflection.InvocationInfo description)
                    : base(action, description)
                {
                    Trigger = trigger;
                }

                public override Task ExecuteAsync(Transition transition, object[] args)
                {
                    if (transition.Trigger.Equals(Trigger))
                        return base.ExecuteAsync(transition, args);

                    return TaskResult.Done;
                }
            }
        }
    }
}
