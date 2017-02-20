using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless.Reflection
{
    /// <summary>
    /// Describes an internal StateRepresentation through the reflection API.
    /// </summary>
    public class StateBindingInfo
    {
        /// <summary>
        /// Constructs an empty StateResource.
        /// </summary>
        public StateBindingInfo() { }

        internal static StateBindingInfo CreateStateBindingInfo<TState, TTrigger>(StateMachine<TState, TTrigger>.StateRepresentation stateReperesentation)
        {
            var stateBinding = new StateBindingInfo();

            stateBinding.State = new StateInfo { Value = stateReperesentation.UnderlyingState, StateType = typeof(TState) };

            stateBinding.Substates = stateReperesentation.GetSubstates().Select(s => CreateStateBindingInfo(s)).ToList();

            if (stateReperesentation.Superstate != null)
                stateBinding.Superstate = CreateStateBindingInfo(stateReperesentation.Superstate);

            stateBinding.EntryActions = stateReperesentation.EntryActions.Select(e => e.ActionDescription).ToList();

            stateBinding.ExitActions = stateReperesentation.ExitActions.Select(e => e.ActionDescription).ToList();

            foreach (var triggerBehaviours in stateReperesentation.TriggerBehaviours)
            {
                var triggerText = triggerBehaviours.Key.ToString();

                int unknowns = 0;

                foreach (var item in triggerBehaviours.Value)
                {
                    if (item is StateMachine<TState, TTrigger>.TransitioningTriggerBehaviour)
                        stateBinding.Transitions.Add(TransitionInfo.CreateTransitionInfo(triggerBehaviours.Key, (StateMachine<TState, TTrigger>.TransitioningTriggerBehaviour)item));
                    if (item is StateMachine<TState, TTrigger>.InternalTriggerBehaviour)
                        stateBinding.Transitions.Add(TransitionInfo.CreateInternalTransitionInfo(triggerBehaviours.Key, (StateMachine<TState, TTrigger>.InternalTriggerBehaviour)item, stateReperesentation.UnderlyingState));
                    if (item is StateMachine<TState, TTrigger>.IgnoredTriggerBehaviour)
                        stateBinding.IgnoredTriggers.Add(new TriggerInfo { Value = triggerBehaviours.Key, TriggerType = typeof(TTrigger) });
                    if (item is StateMachine<TState, TTrigger>.DynamicTriggerBehaviour)
                    {
                        var label = $"unknownDestination_{unknowns++}";

                        stateBinding.DynamicTransitions.Add(DynamicTransitionInfo.Create(triggerBehaviours.Key, (StateMachine<TState, TTrigger>.DynamicTriggerBehaviour)item, label));
                    }
                }
            }

            return stateBinding;
        }

        /// <summary>
        /// The state to which this binding belongs.
        /// </summary>
        public StateInfo State { get; set; }

        /// <summary>
        /// Substates defined for this StateResource.
        /// </summary>
        public ICollection<StateBindingInfo> Substates { get; set; } = new List<StateBindingInfo>();

        /// <summary>
        /// Superstate defined, if any, for this StateResource.
        /// </summary>
        public StateBindingInfo Superstate { get; set; }

        /// <summary>
        /// Actions that are defined to be exectuted on state-entry.
        /// </summary>
        public ICollection<string> EntryActions { get; set; } = new List<string>();

        /// <summary>
        /// Actions that are defined to be exectuted on state-exit.
        /// </summary>
        public ICollection<string> ExitActions { get; set; } = new List<string>(); 

        /// <summary>
        /// Transitions defined for this state.
        /// </summary>
        public ICollection<TransitionInfo> Transitions { get; set; } = new List<TransitionInfo>();

        /// <summary>
        /// Transitions defined for this state internally.
        /// </summary>
        public ICollection<TransitionInfo> InternalTransitions { get; set; } = new List<TransitionInfo>();

        /// <summary>
        /// Dynamic Transitions defined for this state internally.
        /// </summary>
        public ICollection<DynamicTransitionInfo> DynamicTransitions { get; set; } = new List<DynamicTransitionInfo>();

        /// <summary>
        /// Triggers ignored for this state.
        /// </summary>
        public ICollection<TriggerInfo> IgnoredTriggers { get; set; } = new List<TriggerInfo>();
    }

    /// <summary>
    /// Represents a state in a statemachine.
    /// </summary>
    public class StateInfo
    {
        /// <summary>
        /// The instance or value this state represents.
        /// </summary>
        public object Value { get; set; }
        
        /// <summary>
        /// The type of the underlying state.
        /// </summary>
        /// <returns></returns>
        public Type StateType { get; set; }

        /// <summary>
        /// Passes through to the value's ToString.
        /// </summary>
        public override string ToString()
        {
            return Value.ToString();
        }
    }

    /// <summary>
    /// Represents a trigger in a statemachine.
    /// </summary>
    public class TriggerInfo
    {
        /// <summary>
        /// The instance or value this trigger represents.
        /// </summary>
        public object Value { get; set; }
        
        /// <summary>
        /// The type of the underlying trigger.
        /// </summary>
        /// <returns></returns>
        public Type TriggerType { get; set; }

        /// <summary>
        /// Passes through to the value's ToString.
        /// </summary>
        public override string ToString()
        {
            return Value.ToString();
        }
    }

    /// <summary>
    /// Describes a transition that can be initiated from a trigger.
    /// </summary>
    public class TransitionInfo
    {
        /// <summary>
        /// Constructs an empty TransitionInfo.
        /// </summary>
        public TransitionInfo() { }

        internal static TransitionInfo CreateTransitionInfo<TState, TTrigger>(TTrigger trigger, StateMachine<TState, TTrigger>.TransitioningTriggerBehaviour b)
        {
            var transition = new TransitionInfo
            {
                Trigger = new TriggerInfo
                {
                    Value = trigger,
                    TriggerType = trigger.GetType() 
                },
                DestinationState = new StateInfo { Value = b.Destination, StateType = typeof(TState) },
                GuardDescription = string.IsNullOrWhiteSpace(b.GuardsDescriptions) ? null : b.GuardsDescriptions
            };

            return transition;
        }

        internal static TransitionInfo CreateInternalTransitionInfo<TState, TTrigger>(TTrigger trigger, StateMachine<TState, TTrigger>.InternalTriggerBehaviour b, TState destination)
        {
            var transition = new TransitionInfo
            {
                Trigger = new TriggerInfo
                {
                    Value = trigger,
                    TriggerType = typeof(TTrigger) 
                },
                DestinationState = new StateInfo { Value = destination, StateType = typeof(TState) },
                GuardDescription = string.IsNullOrWhiteSpace(b.GuardsDescriptions) ? null : b.GuardsDescriptions
            };

            return transition;
        }

        /// <summary>
        /// The trigger whose firing resulted in this transition.
        /// </summary>
        public TriggerInfo Trigger { get; set; }

        /// <summary>
        /// The state that will be transitioned into on activation.
        /// </summary>
        public StateInfo DestinationState { get; set; }


        /// <summary>
        /// Description of provided guard clause, if any.
        /// </summary>
        public string GuardDescription { get; set; }
    }

    /// <summary>
    /// Describes a transition that can be initiated from a trigger, but whose result is non-deterministic.
    /// </summary>
    public class DynamicTransitionInfo
    {
        /// <summary>
        /// Constructs an empty DynamicTransitionInfo.
        /// </summary>
        public DynamicTransitionInfo() { }

        internal static DynamicTransitionInfo Create<TState, TTrigger>(TTrigger trigger, StateMachine<TState, TTrigger>.DynamicTriggerBehaviour b, string destination)
        {
            var transition = new DynamicTransitionInfo
            {
                Trigger = new TriggerInfo
                {
                    Value = trigger,
                    TriggerType = typeof(TTrigger)
                },
                Destination = destination
            };

            return transition;
        }

        /// <summary>
        /// The trigger whose firing resulted in this transition.
        /// </summary>
        public TriggerInfo Trigger { get; set; }

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
