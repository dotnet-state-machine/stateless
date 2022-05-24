using System.Diagnostics.CodeAnalysis;

namespace Stateless;

public partial class StateMachine<TState, TTrigger> {
    internal abstract class TriggerBehaviour {
        public TTrigger Trigger { get; }

        /// <summary>
        ///     Guard is the transition guard for this trigger.  Equal to
        ///     TransitionGuard.Empty if there is no transition guard
        /// </summary>
        /// <remarks>Defaults to TransitionGuard.Empty</remarks>
        internal TransitionGuard Guard { get; }

        /// <summary>
        ///     TriggerBehaviour constructor
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="guard">TransitionGuard (null if no guard function)</param>
        protected TriggerBehaviour(TTrigger trigger, TransitionGuard? guard) {
            Guard   = guard ?? TransitionGuard.Empty;
            Trigger = trigger;
        }

        /// <summary>
        ///     UnmetGuardConditions is a list of the descriptions of all guard conditions
        ///     whose guard function returns false
        /// </summary>
        public ICollection<string> UnmetGuardConditions(object?[] args) => Guard.UnmetGuardConditions(args);

        public abstract bool ResultsInTransitionFrom(TState                          source, object?[] args,
                                                     [NotNullWhen(true)] out TState? destination);
    }
}