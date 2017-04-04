using System;

namespace Stateless.Reflection
{
    /// <summary>
    /// Describes a transition that can be initiated from a trigger.
    /// </summary>
    public class TransitionInfo
    {
        internal static TransitionInfo CreateTransitionInfo<TState, TTrigger>(TTrigger trigger, StateMachine<TState, TTrigger>.TransitioningTriggerBehaviour behavior, Func<TState, StateInfo> lookupState)
        {
            var transition = new TransitionInfo
            {
                Trigger = trigger,
                DestinationState = lookupState(behavior.Destination),
                GuardDescription = string.IsNullOrWhiteSpace(behavior.GuardsDescriptions) ? null : behavior.GuardsDescriptions
            };

            return transition;
        }

        internal static TransitionInfo CreateInternalTransitionInfo<TState, TTrigger>(TTrigger trigger, StateMachine<TState, TTrigger>.InternalTriggerBehaviour behavior, TState destination, Func<TState, StateInfo> lookupState)
        {
            var transition = new TransitionInfo
            {
                Trigger = trigger,
                DestinationState = lookupState(destination),
                GuardDescription = string.IsNullOrWhiteSpace(behavior.GuardsDescriptions) ? null : behavior.GuardsDescriptions
            };

            return transition;
        }

        private TransitionInfo() { }

        /// <summary>
        /// The trigger whose firing resulted in this transition.
        /// </summary>
        public object Trigger { get; private set; }

        /// <summary>
        /// The state that will be transitioned into on activation.
        /// </summary>
        public StateInfo DestinationState { get; private set; }


        /// <summary>
        /// Description of provided guard clause, if any.
        /// </summary>
        public string GuardDescription { get; private set; }
    }
}
