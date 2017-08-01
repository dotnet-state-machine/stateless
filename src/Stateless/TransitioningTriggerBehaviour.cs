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

            public bool AllowReentry { get; private set; }

            // transitionGuard can be null if there is no guard function on the transition
            public TransitioningTriggerBehaviour(TTrigger trigger, TState destination, TransitionGuard transitionGuard, bool allowReentry)
                : base(trigger, transitionGuard)
            {
                _destination = destination;
                AllowReentry = allowReentry;
            }

            public override bool ResultsInTransitionFrom(TState source, object[] args, out TState destination)
            {
                destination = _destination;
                return AllowReentry 
                    || !source.Equals(destination); // Reentry not allowed -> src and dest states must differ.
            }
        }
    }
}
