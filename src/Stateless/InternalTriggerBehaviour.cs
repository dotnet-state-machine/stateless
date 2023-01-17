using System;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class InternalTriggerBehaviour : TriggerBehaviour
        {
            private readonly EventCallback<Transition, object[]> _callback;

            public InternalTriggerBehaviour(TTrigger trigger, Func<object[], bool> guard, Action<Transition, object[]> internalAction, string guardDescription = null)
                : this(trigger, new TransitionGuard(guard, guardDescription))
            {
                _callback = EventCallbackFactory.Create(internalAction);
            }

            // FIXME: inconsistent with synchronous version of StateConfiguration
            public InternalTriggerBehaviour(TTrigger trigger, Func<bool> guard, Func<Transition, object[], Task> internalAction, string guardDescription = null)
                : this(trigger, new TransitionGuard(guard, guardDescription))
            {
                _callback = EventCallbackFactory.Create(internalAction);
            }

            protected InternalTriggerBehaviour(TTrigger trigger, TransitionGuard guard) : base(trigger, guard)
            {
            }

            public override bool ResultsInTransitionFrom(TState source, object[] args, out TState destination)
            {
                destination = source;
                return false;
            }

            public virtual Task ExecuteAsync(Transition transition, object[] args)
            {
                return _callback.InvokeAsync(transition, args);
            }
        }
    }
}