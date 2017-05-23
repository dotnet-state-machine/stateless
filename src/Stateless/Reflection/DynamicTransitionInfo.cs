using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless.Reflection
{
    /// <summary>
    /// 
    /// </summary>
    public class DynamicStateInfo
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DynamicStateInfo(string destinationState, string criterion)
        {
            DestinationState = destinationState;
            Criterion = criterion;
        }

        /// <summary>
        /// The name of the destination state
        /// </summary>
        public string DestinationState { get; set; }

        /// <summary>
        /// The reason this destination state was chosen
        /// </summary>
        public string Criterion { get; set; }
    }

    /// <summary>
    /// List of DynamicStateInfo objects, with "add" function for ease of definition
    /// </summary>
    public class DynamicStateInfos : List<DynamicStateInfo>
    {
        /// <summary>
        /// Add a DynamicStateInfo with less typing
        /// </summary>
        /// <param name="destinationState"></param>
        /// <param name="criterion"></param>
        public void Add(string destinationState, string criterion)
        {
            base.Add(new DynamicStateInfo(destinationState, criterion));
        }

        /// <summary>
        /// Add a DynamicStateInfo with less typing
        /// </summary>
        /// <param name="destinationState"></param>
        /// <param name="criterion"></param>
        public void Add<TState>(TState destinationState, string criterion)
        {
            base.Add(new DynamicStateInfo(destinationState.ToString(), criterion));
        }
    }

    /// <summary>
    /// Describes a transition that can be initiated from a trigger, but whose result is non-deterministic.
    /// </summary>
    public class DynamicTransitionInfo : TransitionInfo
    {
        internal InvocationInfo DestinationStateSelectorDescription { get; private set; }
        internal DynamicStateInfos PossibleDestinationStates { get; private set; }

        internal static DynamicTransitionInfo Create<TTrigger>(TTrigger trigger, IEnumerable<InvocationInfo> guards,
            InvocationInfo selector, DynamicStateInfos possibleStates)
        {
            var transition = new DynamicTransitionInfo
            {
                Trigger = new TriggerInfo(trigger),
                GuardConditionsMethodDescriptions = guards ?? new List<InvocationInfo>(),
                DestinationStateSelectorDescription = selector,
                PossibleDestinationStates = possibleStates // behaviour.PossibleDestinationStates?.Select(x => x.ToString()).ToArray()
            };

            return transition;
        }

        private DynamicTransitionInfo() { }
    }
}
