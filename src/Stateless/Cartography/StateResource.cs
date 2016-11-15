using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stateless.Cartography
{
    /// <summary>
    /// Describes an internal StateRepresentation through the cartography API.
    /// </summary>
    public class StateResource<TState, TTrigger>
    {
        internal StateResource(StateMachine<TState, TTrigger>.StateRepresentation stateReperesentation)
        {
            StateText = stateReperesentation.UnderlyingState.ToString();

            Substates = stateReperesentation.GetSubstates().Select(s => new StateResource<TState, TTrigger>(s)).ToList();

            if (stateReperesentation.Superstate != null)
                Superstate = new StateResource<TState, TTrigger>(stateReperesentation.Superstate);

            EntryActions = stateReperesentation.EntryActions.Select(e => e.ActionDescription).ToList();

            ExitActions = stateReperesentation.ExitActions.Select(e => e.ActionDescription).ToList();

            foreach (var triggerBehaviours in stateReperesentation.TriggerBehaviours)
            {
                var triggerText = triggerBehaviours.Key.ToString();

                int unknowns = 0;

                foreach (var item in triggerBehaviours.Value)
                {
                    if (item is StateMachine<TState, TTrigger>.TransitioningTriggerBehaviour)
                        Transitions.Add(triggerText, new TransitionResource<TState, TTrigger>((StateMachine<TState, TTrigger>.TransitioningTriggerBehaviour)item));
                    if (item is StateMachine<TState, TTrigger>.InternalTriggerBehaviour)
                        InternalTransitions.Add(triggerText, new TransitionResource<TState, TTrigger>((StateMachine<TState, TTrigger>.InternalTriggerBehaviour)item, StateText));
                    if (item is StateMachine<TState, TTrigger>.IgnoredTriggerBehaviour)
                        IgnoredTriggers.Add(triggerText);
                    if (item is StateMachine<TState, TTrigger>.DynamicTriggerBehaviour)
                    {
                        var label = $"unknownDestination_{unknowns++}";

                        DynamicTransitions.Add(triggerText, new TransitionResource<TState, TTrigger>((StateMachine<TState, TTrigger>.DynamicTriggerBehaviour)item, label));
                    }
                }
            }
        }

        /// <summary>
        /// Textual representation of the State.
        /// </summary>
        public string StateText { get; }

        /// <summary>
        /// Substates defined for this StateResource.
        /// </summary>
        public ICollection<StateResource<TState, TTrigger>> Substates { get; } = new StateResource<TState, TTrigger>[0];

        /// <summary>
        /// Superstate defined, if any, for this StateResource.
        /// </summary>
        public StateResource<TState, TTrigger> Superstate { get; }

        /// <summary>
        /// Actions that are defined to be exectuted on state-entry.
        /// </summary>
        public ICollection<string> EntryActions { get; } = new string[0];

        /// <summary>
        /// Actions that are defined to be exectuted on state-exit.
        /// </summary>
        public ICollection<string> ExitActions { get; } = new string[0]; 

        /// <summary>
        /// Transitions defined for this state.
        /// </summary>
        public IDictionary<string, TransitionResource<TState, TTrigger>> Transitions { get; } = new Dictionary<string, TransitionResource<TState, TTrigger>>();

        /// <summary>
        /// Transitions defined for this state internally.
        /// </summary>
        public IDictionary<string, TransitionResource<TState, TTrigger>> InternalTransitions { get; } = new Dictionary<string, TransitionResource<TState, TTrigger>>();

        /// <summary>
        /// Dynamic Transitions defined for this state internally.
        /// </summary>
        public IDictionary<string, TransitionResource<TState, TTrigger>> DynamicTransitions { get; } = new Dictionary<string, TransitionResource<TState, TTrigger>>();

        /// <summary>
        /// Triggers ignored for this state.
        /// </summary>
        public ICollection<string> IgnoredTriggers { get; } = new List<string>();
    }

    /// <summary>
    /// Describes a transition that can be initiated from a trigger.
    /// </summary>
    public class TransitionResource<TState, TTrigger>
    {
        internal TransitionResource(StateMachine<TState, TTrigger>.TransitioningTriggerBehaviour b)
        {
            DestinationStateText = b.Destination.ToString();

            GuardDescription = string.IsNullOrWhiteSpace(b.GuardDescription) ? null : b.GuardDescription;
        }

        internal TransitionResource(StateMachine<TState, TTrigger>.InternalTriggerBehaviour b, string sourceState)
        {
            DestinationStateText = sourceState;

            GuardDescription = string.IsNullOrWhiteSpace(b.GuardDescription) ? null : b.GuardDescription;
        }

        internal TransitionResource(StateMachine<TState, TTrigger>.DynamicTriggerBehaviour b, string label)
        {
            DestinationStateText = label;
        }

        /// <summary>
        /// Textual representation of the State into which will be transitioned.
        /// </summary>
        public string DestinationStateText { get; }


        /// <summary>
        /// Description of provided guard clause, if any.
        /// </summary>
        public string GuardDescription { get; }
    }
}
