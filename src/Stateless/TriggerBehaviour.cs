using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal abstract class TriggerBehaviour
        {
            /// <summary>
            /// TriggerBehaviour constructor
            /// </summary>
            /// <param name="trigger"></param>
            /// <param name="guard">TransitionGuard (null if no guard function)</param>
            protected TriggerBehaviour(TTrigger trigger, TransitionGuard guard)
            {
                Guard = guard ?? TransitionGuard.Empty;
                Trigger = trigger;
            }

            public TTrigger Trigger { get; }

            /// <summary>
            /// Guard is the transition guard for this trigger.  Equal to
            /// TransitionGuard.Empty if there is no transition guard
            /// </summary>
            internal TransitionGuard Guard { get; set; }

            /// <summary>
            /// Guards is the list of guard functions for the transition guard for this trigger
            /// </summary>
            internal ICollection<Func<bool>> Guards => Guard.Guards;

            /// <summary>
            /// GuardConditionsMet is true if all of the guard functions return true
            /// or if there are no guard functions
            /// </summary>
            public bool GuardConditionsMet => Guard.GuardConditionsMet;

            /// <summary>
            /// UnmetGuardConditions is a list of the descriptions of all guard conditions
            /// whose guard function returns false
            /// </summary>
            public ICollection<string> UnmetGuardConditions => Guard.UnmetGuardConditions;

            public abstract bool ResultsInTransitionFrom(TState source, object[] args, out TState destination);

            public virtual void IsFiring()
            { }
        }
    }
}
