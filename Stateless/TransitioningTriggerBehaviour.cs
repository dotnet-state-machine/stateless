﻿using System;
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

            public TransitioningTriggerBehaviour(TTrigger trigger, TState destination, Func<bool> guard)
                : this(trigger, destination, guard, string.Empty)
            {
            }

            public TransitioningTriggerBehaviour(TTrigger trigger, TState destination, Func<bool> guard, string description)
                : base(trigger, guard, description)
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
