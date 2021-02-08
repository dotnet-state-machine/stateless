﻿using System;
using System.Collections.Generic;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal abstract class TriggerBehaviour
        {
            private Action<Transition, object []> _triggerAction;

            /// <summary>
            /// If there is no guard function, _guard is set to TransitionGuard.Empty
            /// </summary>
            private TransitionGuard _guard;

            /// <summary>
            /// TriggerBehaviour constructor
            /// </summary>
            /// <param name="trigger"></param>
            /// <param name="guard">TransitionGuard (null if no guard function)</param>
            protected TriggerBehaviour(TTrigger trigger, TransitionGuard guard)
            {
                _guard = guard ?? TransitionGuard.Empty;
                Trigger = trigger;
            }

            public TTrigger Trigger { get; }

            internal void SetGuard(TransitionGuard guard)
            {
                _guard = guard;
            }

            internal void AddAction(Action<Transition, object[]> someAction)
            {
                _triggerAction = someAction;
            }

            internal void AddAction(Action<Transition> someAction)
            {
                _triggerAction = (t, r) => someAction(t);
            }

            /// <summary>
            /// Guard is the transition guard for this trigger.  Equal to
            /// TransitionGuard.Empty if there is no transition guard
            /// </summary>
            internal TransitionGuard Guard => _guard;

            /// <summary>
            /// Guards is the list of guard functions for the transition guard for this trigger
            /// </summary>
            internal ICollection<Func<object[], bool>> Guards =>_guard.Guards;

            /// <summary>
            /// GuardConditionsMet is true if all of the guard functions return true
            /// or if there are no guard functions
            /// </summary>
            public bool GuardConditionsMet(params object[] args) => _guard.GuardConditionsMet(args);

            /// <summary>
            /// UnmetGuardConditions is a list of the descriptions of all guard conditions
            /// whose guard function returns false
            /// </summary>
            public ICollection<string> UnmetGuardConditions(object[] args) => _guard.UnmetGuardConditions(args);

            public abstract bool ResultsInTransitionFrom(TState source, object[] args, out TState destination);

            internal bool HasAction()
            {
                return _triggerAction != null;
            }

            internal void ExecuteAction(Transition transition, object[] args)
            {
                _triggerAction(transition, args);
            }
        }
    }
}
