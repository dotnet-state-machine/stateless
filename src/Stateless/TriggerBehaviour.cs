using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Stateless; 

public partial class StateMachine<TState, TTrigger>
{
    internal abstract class TriggerBehaviour
    {
        /// <summary>
        /// TriggerBehaviour constructor
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="guard">TransitionGuard (null if no guard function)</param>
        protected TriggerBehaviour(TTrigger trigger, TransitionGuard? guard)
        {
            Guard  = guard ?? TransitionGuard.Empty;
            Trigger = trigger;
        }

        public TTrigger Trigger { get; }

        /// <summary>
        /// Guard is the transition guard for this trigger.  Equal to
        /// TransitionGuard.Empty if there is no transition guard
        /// </summary>
        /// <remarks>Defaults to TransitionGuard.Empty</remarks>
        internal TransitionGuard Guard { get; }

        /// <summary>
        /// Guards is the list of guard functions for the transition guard for this trigger
        /// </summary>
        internal ICollection<Func<object?[], bool>> Guards =>Guard.Guards;

        /// <summary>
        /// GuardConditionsMet is true if all of the guard functions return true
        /// or if there are no guard functions
        /// </summary>
        public bool GuardConditionsMet(params object[] args) => Guard.GuardConditionsMet(args);

        /// <summary>
        /// UnmetGuardConditions is a list of the descriptions of all guard conditions
        /// whose guard function returns false
        /// </summary>
        public ICollection<string> UnmetGuardConditions(object?[] args) => Guard.UnmetGuardConditions(args);

        public abstract bool ResultsInTransitionFrom(TState source, object?[] args, [NotNullWhen(true)]out TState? destination);
    }
}