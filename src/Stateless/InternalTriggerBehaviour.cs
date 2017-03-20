using System;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class InternalTriggerBehaviour : TriggerBehaviour
        {
            public InternalTriggerBehaviour(TTrigger trigger, Func<bool> guard)
                : base(trigger, new TransitionGuard(guard, "Internal Transition"))
            {
            }
            public override bool ResultsInTransitionFrom(TState source, object[] args, out TState destination)
            {
                destination = source;
                return false;
            }
        }
    }
}
