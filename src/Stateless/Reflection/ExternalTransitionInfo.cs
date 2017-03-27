using System;

namespace Stateless.Reflection
{
    /// <summary>
    /// Describes a transition that can be initiated from a trigger.
    /// </summary>
    public class ExternalTransitionInfo : TransitionInfo
    {
        internal static ExternalTransitionInfo Create<TState, TTrigger>(TTrigger trigger, StateMachine<TState, TTrigger>.TransitioningTriggerBehaviour behavior, Func<TState, StateInfo> lookupState)
        {
            var transition = new ExternalTransitionInfo
            {
                Trigger = new TriggerInfo(trigger, trigger.GetType()),
                DestinationState = lookupState(behavior.Destination),
                GuardDescription = string.IsNullOrWhiteSpace(behavior.GuardsDescriptions) ? null : behavior.GuardsDescriptions
            };

            return transition;
        }

        private ExternalTransitionInfo() { }

        /// <summary>
        /// The state that will be transitioned into on activation.
        /// </summary>
        public StateInfo DestinationState { get; private set; }
    }
}