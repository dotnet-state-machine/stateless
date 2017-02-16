using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Stateless.Cartography
{
    /// <summary>
    /// Describes an internal StateRepresentation through the cartography API.
    /// </summary>
    public class StateResource<TState, TTrigger>
    {
        /// <summary>
        /// Constructs an empty StateResource.
        /// </summary>
        public StateResource() { }

        internal StateResource(StateMachine<TState, TTrigger>.StateRepresentation stateReperesentation)
        {
            UnderlyingState = stateReperesentation.UnderlyingState;

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
                        Transitions.Add(triggerBehaviours.Key, new TransitionResource<TState, TTrigger>(triggerBehaviours.Key, (StateMachine<TState, TTrigger>.TransitioningTriggerBehaviour)item));
                    if (item is StateMachine<TState, TTrigger>.InternalTriggerBehaviour)
                        InternalTransitions.Add(triggerBehaviours.Key, new TransitionResource<TState, TTrigger>(triggerBehaviours.Key, (StateMachine<TState, TTrigger>.InternalTriggerBehaviour)item, UnderlyingState));
                    if (item is StateMachine<TState, TTrigger>.IgnoredTriggerBehaviour)
                        IgnoredTriggers.Add(triggerBehaviours.Key);
                    if (item is StateMachine<TState, TTrigger>.DynamicTriggerBehaviour)
                    {
                        var label = $"unknownDestination_{unknowns++}";

                        DynamicTransitions.Add(triggerBehaviours.Key, new DynamicTransitionResource<TState, TTrigger>(triggerBehaviours.Key, (StateMachine<TState, TTrigger>.DynamicTriggerBehaviour)item, label));
                    }
                }
            }
        }

        /// <summary>
        /// The instance or value this resource represents.
        /// </summary>
        public TState UnderlyingState { get; set; }

        /// <summary>
        /// Substates defined for this StateResource.
        /// </summary>
        public ICollection<StateResource<TState, TTrigger>> Substates { get; set; } = new StateResource<TState, TTrigger>[0];

        /// <summary>
        /// Superstate defined, if any, for this StateResource.
        /// </summary>
        public StateResource<TState, TTrigger> Superstate { get; set; }

        /// <summary>
        /// Actions that are defined to be exectuted on state-entry.
        /// </summary>
        public ICollection<string> EntryActions { get; set; } = new string[0];

        /// <summary>
        /// Actions that are defined to be exectuted on state-exit.
        /// </summary>
        public ICollection<string> ExitActions { get; set; } = new string[0]; 

        /// <summary>
        /// Transitions defined for this state.
        /// </summary>
        public IDictionary<TTrigger, TransitionResource<TState, TTrigger>> Transitions { get; set; } = new Dictionary<TTrigger, TransitionResource<TState, TTrigger>>();

        /// <summary>
        /// Transitions defined for this state internally.
        /// </summary>
        public IDictionary<TTrigger, TransitionResource<TState, TTrigger>> InternalTransitions { get; set; } = new Dictionary<TTrigger, TransitionResource<TState, TTrigger>>();

        /// <summary>
        /// Dynamic Transitions defined for this state internally.
        /// </summary>
        public IDictionary<TTrigger, DynamicTransitionResource<TState, TTrigger>> DynamicTransitions { get; set; } = new Dictionary<TTrigger, DynamicTransitionResource<TState, TTrigger>>();

        /// <summary>
        /// Triggers ignored for this state.
        /// </summary>
        public ICollection<TTrigger> IgnoredTriggers { get; set; } = new List<TTrigger>();
    }

    /// <summary>
    /// Describes a transition that can be initiated from a trigger.
    /// </summary>
    public class TransitionResource<TState, TTrigger>
    {
        /// <summary>
        /// Constructs an empty TransitionResource.
        /// </summary>
        public TransitionResource() { }

        internal TransitionResource(TTrigger trigger, StateMachine<TState, TTrigger>.TransitioningTriggerBehaviour b)
        {
            DestinationState = b.Destination;

            GuardDescription = string.IsNullOrWhiteSpace(b.GuardDescription) ? null : b.GuardDescription;
        }

        internal TransitionResource(TTrigger trigger, StateMachine<TState, TTrigger>.InternalTriggerBehaviour b, TState destination)
        {
            DestinationState = destination;

            GuardDescription = string.IsNullOrWhiteSpace(b.GuardDescription) ? null : b.GuardDescription;
        }

        /// <summary>
        /// The trigger whose firing resulted in this transition.
        /// </summary>
        public TTrigger UnderlyingTrigger { get; set; }

        /// <summary>
        /// Textual representation of the State into which will be transitioned.
        /// </summary>
        public TState DestinationState { get; set; }


        /// <summary>
        /// Description of provided guard clause, if any.
        /// </summary>
        public string GuardDescription { get; set; }
    }

    /// <summary>
    /// Describes a transition that can be initiated from a trigger, but whose result is non-deterministic.
    /// </summary>
    public class DynamicTransitionResource<TState, TTrigger>
    {
        /// <summary>
        /// Constructs an empty DynamicTransitionResource.
        /// </summary>
        public DynamicTransitionResource() { }

        internal DynamicTransitionResource(TTrigger trigger, StateMachine<TState, TTrigger>.DynamicTriggerBehaviour b, string desitination)
        {
            Destination = desitination;
        }

        /// <summary>
        /// The trigger whose firing resulted in this transition.
        /// </summary>
        public TTrigger UnderlyingTrigger { get; set; }

        /// <summary>
        /// Friendly text for dynamic transitions.
        /// </summary>
        public string Destination { get; set; }

        /// <summary>
        /// Description of provided guard clause, if any.
        /// </summary>
        public string GuardDescription { get; set; }
    }
}
