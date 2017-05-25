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
            Action _triggerAction = null;
            Reflection.InvocationInfo _triggerActionInfo = null;

            public object Tag { get; internal set; } = null;

            internal TState Destination { get { return _destination; } }

            // transitionGuard can be null if there is no guard function on the transition
            public TransitioningTriggerBehaviour(TTrigger trigger, TState destination, TransitionGuard transitionGuard)
                : base(trigger, transitionGuard)
            {
                _destination = destination;
            }

            public TransitioningTriggerBehaviour(TTrigger trigger, TState destination, TransitionGuard transitionGuard,
                Action action, Reflection.InvocationInfo invocationInfo)
                : base(trigger, transitionGuard)
            {
                _destination = destination;
                _triggerAction = action;
                _triggerActionInfo = invocationInfo;
            }

            public void SetAction(Action action, Reflection.InvocationInfo info)
            {
                _triggerAction = action;
                _triggerActionInfo = info;
            }

            public override bool ResultsInTransitionFrom(TState source, object[] args, out TState destination)
            {
                destination = _destination;
                return true;
            }

            public override void IsFiring()
            {
                if (_triggerAction != null)
                    _triggerAction();
            }
        }
    }
}
