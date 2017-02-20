using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class TransitioningTriggerBehaviour : TriggerBehaviour
        {
            readonly TState _destination;

            internal TState Destination { get { return _destination; } }

            public TransitioningTriggerBehaviour(TTrigger trigger, TState destination, Func<bool> guard = null, string guardDescription = null)
                : base(trigger, new TransitionGuard(guard, guardDescription))
            {
                _destination = destination;
            }

            public TransitioningTriggerBehaviour(TTrigger trigger, TState destination, TransitionGuard transitionGuard)
                : base(trigger, transitionGuard)
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
