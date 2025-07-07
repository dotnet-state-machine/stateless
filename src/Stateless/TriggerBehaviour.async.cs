using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal abstract class TriggerBehaviourAsync : TriggerBehaviourBase
        {
            /// <summary>
            /// If there is no guard function, _guard is set to TransitionGuardAsync.Empty
            /// </summary>
            readonly TransitionGuardAsync _guard;

            /// <summary>
            /// TriggerBehaviour constructor
            /// </summary>
            /// <param name="trigger"></param>
            /// <param name="guard">TransitionGuard (null if no guard function)</param>
            protected TriggerBehaviourAsync(TTrigger trigger, TransitionGuardAsync guard)
            {
                _guard = guard ?? TransitionGuardAsync.Empty;
                Trigger = trigger;
            }

            public TTrigger Trigger { get; }

            /// <summary>
            /// Guard is the transition guard for this trigger.  Equal to
            /// TransitionGuard.Empty if there is no transition guard
            /// </summary>
            internal TransitionGuardAsync Guard => _guard;

            /// <summary>
            /// Guards is the list of guard functions for the transition guard for this trigger
            /// </summary>
            internal ICollection<Func<object[], Task<bool>>> Guards =>_guard.Guards;

            /// <summary>
            /// GuardConditionsMet is true if all the guard functions return true
            /// or if there are no guard functions
            /// </summary>
            public async Task<bool> GuardConditionsMet(params object[] args) => await _guard.GuardConditionsMet(args);

            /// <summary>
            /// UnmetGuardConditions is a list of the descriptions of all guard conditions
            /// whose guard function returns false
            /// </summary>
            public async Task<ICollection<string>> UnmetGuardConditions(object[] args) => await _guard.UnmetGuardConditions(args);
        }
    }
}
