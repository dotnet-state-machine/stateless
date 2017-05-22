using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless.Reflection
{
    /// <summary>
    /// Describes a transition that can be initiated from a trigger, but whose result is non-deterministic.
    /// </summary>
    public class DynamicTransitionInfo : TransitionInfo
    {
        internal InvocationInfo DestinationStateSelectorDescription { get; private set; }
        internal string[] PossibleDestinationStates { get; private set; }

        internal static DynamicTransitionInfo Create<TState, TTrigger>(TTrigger trigger, StateMachine<TState, TTrigger>.DynamicTriggerBehaviour behaviour)
        {
            ;

            var transition = new DynamicTransitionInfo
            {
                Trigger = new TriggerInfo(trigger),
                GuardConditionsMethodDescriptions = (behaviour.Guard == null)
                    ? new List<InvocationInfo>() : behaviour.Guard.Conditions.Select(c => c.MethodDescription),
                DestinationStateSelectorDescription = behaviour.DestinationInfo,
                PossibleDestinationStates = behaviour.PossibleDestinationStates?.Select(x => x.ToString()).ToArray()
            };

            return transition;
        }

        private DynamicTransitionInfo() { }
    }
}
