using System.Collections.Generic;
using System.Linq;

namespace Stateless.Reflection
{
    /// <summary>
    /// Describes a transition that can be initiated from a trigger.
    /// </summary>
    public class FixedTransitionInfo : TransitionInfo
    {
        internal static FixedTransitionInfo Create<TState, TTrigger>(StateMachine<TState, TTrigger>.TriggerBehaviour behaviour, StateInfo destinationStateInfo)
        {
            var transition = new FixedTransitionInfo
            {
                Trigger = new TriggerInfo(behaviour.Trigger),
                DestinationState = destinationStateInfo,
                GuardConditionsMethodDescriptions = (behaviour.Guard == null)
                    ? new List<InvocationInfo>() : behaviour.Guard.Conditions.Select(c => c.MethodDescription)
            };

            return transition;
        }

        private FixedTransitionInfo() { }

        /// <summary>
        /// The state that will be transitioned into on activation.
        /// </summary>
        public StateInfo DestinationState { get; private set; }
    }
}