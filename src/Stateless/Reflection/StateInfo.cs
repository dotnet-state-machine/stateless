using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless.Reflection
{
    /// <summary>
    /// Describes an internal StateRepresentation through the reflection API.
    /// </summary>
    public class StateInfo
    {
        internal static StateInfo CreateStateInfo<TState, TTrigger>(StateMachine<TState, TTrigger>.StateRepresentation stateReperesentation)
        {
            if (stateReperesentation == null)
                throw new ArgumentException(nameof(stateReperesentation));

            var entryActions = stateReperesentation.EntryActions.Select(e => e.ActionDescription).ToList();
            var exitActions = stateReperesentation.ExitActions.Select(e => e.ActionDescription).ToList();
            var ignoredTriggers = new List<TriggerInfo>();

            foreach (var triggerBehaviours in stateReperesentation.TriggerBehaviours)
            {
                foreach (var item in triggerBehaviours.Value)
                {
                    if (item is StateMachine<TState, TTrigger>.IgnoredTriggerBehaviour)
                        ignoredTriggers.Add(new TriggerInfo(triggerBehaviours.Key));
                }
            }

            return new StateInfo(stateReperesentation.UnderlyingState, entryActions, exitActions, ignoredTriggers);
        }

        internal static void AddRelationships<TState, TTrigger>(StateInfo info, StateMachine<TState, TTrigger>.StateRepresentation stateReperesentation, Func<TState, StateInfo> lookupState)
        {
            var substates = stateReperesentation.GetSubstates().Select(s => lookupState(s.UnderlyingState)).ToList();

            StateInfo superstate = null;
            if (stateReperesentation.Superstate != null)
                superstate = lookupState(stateReperesentation.Superstate.UnderlyingState);

            var fixedTransitions = new List<FixedTransitionInfo>();
            var dynamicTransitions = new List<DynamicTransitionInfo>();

            foreach (var triggerBehaviours in stateReperesentation.TriggerBehaviours)
            {
                int unknowns = 0;
                // First add all the deterministic transitions
                foreach (var item in triggerBehaviours.Value.Where(behaviour => ((behaviour is StateMachine<TState, TTrigger>.TransitioningTriggerBehaviour)
                                                                              || (behaviour is StateMachine<TState, TTrigger>.InternalTriggerBehaviour))))
                {
                    var destinationInfo = lookupState(((StateMachine<TState, TTrigger>.TransitioningTriggerBehaviour)item).Destination);
                    fixedTransitions.Add(FixedTransitionInfo.Create(item, destinationInfo));
                }
                // Then add all the dynamic transitions
                foreach (var item in triggerBehaviours.Value.Where(behaviour => behaviour is StateMachine<TState, TTrigger>.DynamicTriggerBehaviour))
                {
                    var label = $"unknownDestination_{unknowns++}";
                    dynamicTransitions.Add(DynamicTransitionInfo.Create(triggerBehaviours.Key, (StateMachine<TState, TTrigger>.DynamicTriggerBehaviour)item, label));
                }
            }

            info.AddRelationships(superstate, substates, fixedTransitions, dynamicTransitions);
        }

        private StateInfo(
            object underlyingState, 
            IEnumerable<string> entryActions,
            IEnumerable<string> exitActions,
            IEnumerable<TriggerInfo> ignoredTriggers)
        {
            UnderlyingState = underlyingState;
            EntryActions = entryActions ?? throw new ArgumentNullException(nameof(entryActions));
            ExitActions = exitActions ?? throw new ArgumentNullException(nameof(exitActions));
            IgnoredTriggers = ignoredTriggers ?? throw new ArgumentNullException(nameof(ignoredTriggers));
        }

        private void AddRelationships(
            StateInfo superstate,
            IEnumerable<StateInfo> substates,
            IEnumerable<FixedTransitionInfo> transitions,
            IEnumerable<DynamicTransitionInfo> dynamicTransitions)
        {
            Superstate = superstate;
            Substates = substates ?? throw new ArgumentNullException(nameof(substates));
            FixedTransitions = transitions ?? throw new ArgumentNullException(nameof(transitions));
            DynamicTransitions = dynamicTransitions ?? throw new ArgumentNullException(nameof(dynamicTransitions));
        }

        /// <summary>
        /// The instance or value this state represents.
        /// </summary>
        public object UnderlyingState { get; }

        /// <summary>
        /// Substates defined for this StateResource.
        /// </summary>
        public IEnumerable<StateInfo> Substates { get; private set; }

        /// <summary>
        /// Superstate defined, if any, for this StateResource.
        /// </summary>
        public StateInfo Superstate { get; private set; }

        /// <summary>
        /// Actions that are defined to be exectuted on state-entry.
        /// </summary>
        public IEnumerable<string> EntryActions { get; private set; }

        /// <summary>
        /// Actions that are defined to be exectuted on state-exit.
        /// </summary>
        public IEnumerable<string> ExitActions { get; private set; }

        /// <summary> 
        /// Transitions defined for this state. 
        /// </summary> 
        public IEnumerable<TransitionInfo> Transitions { get { return FixedTransitions.Concat<TransitionInfo>(DynamicTransitions); } }
        /// <summary>
        /// Transitions defined for this state.
        /// </summary>
        public IEnumerable<FixedTransitionInfo> FixedTransitions { get; private set; }

        /// <summary>
        /// Dynamic Transitions defined for this state internally.
        /// </summary>
        public IEnumerable<DynamicTransitionInfo> DynamicTransitions { get; private set; }

        /// <summary>
        /// Triggers ignored for this state.
        /// </summary>
        public IEnumerable<TriggerInfo> IgnoredTriggers { get; private set; }

        /// <summary>
        /// Passes through to the value's ToString.
        /// </summary>
        public override string ToString()
        {
            return UnderlyingState?.ToString() ?? "<null>";
        }
    }
}
