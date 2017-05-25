using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        /// <summary>
        /// Describes a state transition.
        /// </summary>
        public class Transition
        {
            readonly TState _source;
            readonly TState _destination;
            readonly TTrigger _trigger;
            readonly object _tag;

            /// <summary>
            /// Construct a transition.
            /// </summary>
            /// <param name="source">The state transitioned from.</param>
            /// <param name="destination">The state transitioned to.</param>
            /// <param name="trigger">The trigger that caused the transition.</param>
            /// <param name="tag">Optional data associated with the transition.</param>
            public Transition(TState source, TState destination, TTrigger trigger, object tag = null)
            {
                _source = source;
                _destination = destination;
                _trigger = trigger;
                _tag = tag;
            }

            /// <summary>
            /// The state transitioned from.
            /// </summary>
            public TState Source { get { return _source; } }
            
            /// <summary>
            /// The state transitioned to.
            /// </summary>
            public TState Destination { get { return _destination; } }
            
            /// <summary>
            /// The trigger that caused the transition.
            /// </summary>
            public TTrigger Trigger { get { return _trigger; } }

            /// <summary>
            /// Optional data associated with the transition.
            /// </summary>
            public object Tag { get { return _tag; } }

            /// <summary>
            /// True if the transition is a re-entry, i.e. the identity transition.
            /// </summary>
            public bool IsReentry { get { return Source.Equals(Destination); } }
        }
    }
}
