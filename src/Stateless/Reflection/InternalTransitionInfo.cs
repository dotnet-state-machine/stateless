using System;

namespace Stateless.Reflection
{
    /// <summary>
    /// Describes a transition that can be initiated from a trigger, but whose result does noit cause a state change.
    /// </summary>
    public class InternalTransitionInfo : TransitionInfo
    {
        internal static InternalTransitionInfo Create<TState, TTrigger>(TTrigger trigger, StateMachine<TState, TTrigger>.InternalTriggerBehaviour behavior, TState destination, Func<TState, StateInfo> lookupState)
        {
            var transition = new InternalTransitionInfo
            {
                Trigger = new TriggerInfo(trigger, typeof(TTrigger)),
                DestinationState = lookupState(destination),
                GuardDescription = String.IsNullOrWhiteSpace(behavior.GuardsDescriptions) ? null : behavior.GuardsDescriptions
            };

            return transition;
        }
        /// <summary>
        /// The state that will be transitioned into on activation.
        /// </summary>
        public StateInfo DestinationState { get; private set; }
    }
}
