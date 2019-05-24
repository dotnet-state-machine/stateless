using System;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class ReentryTriggerBehaviour : TriggerBehaviour
        {
            readonly TState _destination;

            internal TState Destination { get { return _destination; } }

            // transitionGuard can be null if there is no guard function on the transition
            public ReentryTriggerBehaviour(TTrigger trigger, TState destination, TransitionGuard transitionGuard, Action transitionFunction)
                : base(trigger, transitionGuard, transitionFunction)
            {
                _destination = destination;
            }

            public override bool ResultsInTransitionFrom(TState source, object[] args, out TState destination)
            {
                destination = _destination;
                return true;
            }
        }
        
    }
}
