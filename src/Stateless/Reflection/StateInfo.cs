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
        internal static StateInfo CreateStateInfo<TState, TTrigger>(StateMachine<TState, TTrigger>.StateRepresentation stateRepresentation)
        {
            if (stateRepresentation == null)
                throw new ArgumentException(nameof(stateRepresentation));

            var ignoredTriggers = new List<IgnoredTransitionInfo>();

            // stateRepresentation.TriggerBehaviours maps from TTrigger to ICollection<TriggerBehaviour>
            foreach (var triggerBehaviours in stateRepresentation.TriggerBehaviours)
            {
                foreach (var item in triggerBehaviours.Value)
                {
                    if (item is StateMachine<TState, TTrigger>.IgnoredTriggerBehaviour behaviour)
                    {
                        ignoredTriggers.Add(IgnoredTransitionInfo.Create(behaviour));
                    }
                }
            }

            return new StateInfo(stateRepresentation.UnderlyingState, ignoredTriggers,
                stateRepresentation.EntryActions.Select(e => ActionInfo.Create(e)).ToList(),
                stateRepresentation.ActivateActions.Select(e => e.Description).ToList(),
                stateRepresentation.DeactivateActions.Select(e => e.Description).ToList(),
                stateRepresentation.ExitActions.Select(e => e.Description).ToList());
        }
 
        internal static void AddRelationships<TState, TTrigger>(StateInfo info, StateMachine<TState, TTrigger>.StateRepresentation stateRepresentation, Func<TState, StateInfo> lookupState)
        {
            if (lookupState == null) throw new ArgumentNullException(nameof(lookupState));

            var substates = stateRepresentation.GetSubstates().Select(s => lookupState(s.UnderlyingState)).ToList();

            StateInfo superstate = null;
            if (stateRepresentation.Superstate != null)
                superstate = lookupState(stateRepresentation.Superstate.UnderlyingState);

            var fixedTransitions = new List<FixedTransitionInfo>();
            var dynamicTransitions = new List<DynamicTransitionInfo>();

            foreach (var triggerBehaviours in stateRepresentation.TriggerBehaviours)
            {
                // First add all the deterministic transitions
                foreach (var item in triggerBehaviours.Value.Where(behaviour => (behaviour is StateMachine<TState, TTrigger>.TransitioningTriggerBehaviour)))
                {
                    var destinationInfo = lookupState(((StateMachine<TState, TTrigger>.TransitioningTriggerBehaviour)item).Destination);
                    fixedTransitions.Add(FixedTransitionInfo.Create(item, destinationInfo));
                }
                foreach (var item in triggerBehaviours.Value.Where(behaviour => (behaviour is StateMachine<TState, TTrigger>.ReentryTriggerBehaviour)))
                {
                    var destinationInfo = lookupState(((StateMachine<TState, TTrigger>.ReentryTriggerBehaviour)item).Destination);
                    fixedTransitions.Add(FixedTransitionInfo.Create(item, destinationInfo));
                }
                //Then add all the internal transitions
                foreach (var item in triggerBehaviours.Value.Where(behaviour => (behaviour is StateMachine<TState, TTrigger>.InternalTriggerBehaviour)))
                {
                    var destinationInfo = lookupState(stateRepresentation.UnderlyingState);
                    fixedTransitions.Add(FixedTransitionInfo.Create(item, destinationInfo));
                }
                // Then add all the dynamic transitions
                foreach (var item in triggerBehaviours.Value.Where(behaviour => behaviour is StateMachine<TState, TTrigger>.DynamicTriggerBehaviour))
                {
                    dynamicTransitions.Add(((StateMachine<TState, TTrigger>.DynamicTriggerBehaviour)item).TransitionInfo);
                }
            }

            info.AddRelationships(superstate, substates, fixedTransitions, dynamicTransitions);
        }

        private StateInfo(
            object underlyingState,
            IEnumerable<IgnoredTransitionInfo> ignoredTriggers,
            IEnumerable<ActionInfo> entryActions,
            IEnumerable<InvocationInfo> activateActions,
            IEnumerable<InvocationInfo> deactivateActions,
            IEnumerable<InvocationInfo> exitActions)
        {
            UnderlyingState = underlyingState;
            IgnoredTriggers = ignoredTriggers ?? throw new ArgumentNullException(nameof(ignoredTriggers));
            EntryActions = entryActions;
            ActivateActions = activateActions;
            DeactivateActions = deactivateActions;
            ExitActions = exitActions;
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
        /// Actions that are defined to be executed on state-entry.
        /// </summary>
        public IEnumerable<ActionInfo> EntryActions { get; private set; }

        /// <summary>
        /// Actions that are defined to be executed on activation.
        /// </summary>
        public IEnumerable<InvocationInfo> ActivateActions { get; private set; }

        /// <summary>
        /// Actions that are defined to be executed on deactivation.
        /// </summary>
        public IEnumerable<InvocationInfo> DeactivateActions { get; private set; }

        /// <summary>
        /// Actions that are defined to be exectuted on state-exit.
        /// </summary>
        public IEnumerable<InvocationInfo> ExitActions { get; private set; }

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
        public IEnumerable<IgnoredTransitionInfo> IgnoredTriggers { get; private set; }

        /// <summary>
        /// Passes through to the value's ToString.
        /// </summary>
        public override string ToString()
        {
            return UnderlyingState?.ToString() ?? "<null>";
        }
    }
}
