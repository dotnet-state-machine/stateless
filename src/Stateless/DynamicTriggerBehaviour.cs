using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class DynamicTriggerBehaviour : TriggerBehaviour
        {
            readonly Func<object[], TState> _destination;
            internal Reflection.InvocationInfo DestinationInfo { get; private set; }  // Not null
            internal TState[] PossibleDestinationStates { get; private set; }       // Can be null if unknown

            public DynamicTriggerBehaviour(TTrigger trigger, Func<object[], TState> destination, Reflection.InvocationInfo destinationInfo
                , TransitionGuard transitionGuard, TState[] possibleDestinationStates)
                : base(trigger, transitionGuard)
            {
                _destination = Enforce.ArgumentNotNull(destination, nameof(destination));
                DestinationInfo = Enforce.ArgumentNotNull(destinationInfo, nameof(destinationInfo));
                PossibleDestinationStates = possibleDestinationStates;     // Can be null if unknown
            }

            public override bool ResultsInTransitionFrom(TState source, object[] args, out TState destination)
            {
                destination = _destination(args);
                return true;
            }
        }
    }
}
