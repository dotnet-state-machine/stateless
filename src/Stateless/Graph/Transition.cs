using System;
using System.Collections.Generic;

using Stateless.Reflection;

namespace Stateless.Graph
{
    /// <summary>
    /// Used to keep track of transitions between states
    /// </summary>
    public class Transition
    {
        /// <summary>
        /// The trigger that causes this transition
        /// </summary>
        public TriggerInfo Trigger { get; private set; }

        /// <summary>
        /// List of actions to be performed by the destination state (the one being entered)
        /// </summary>
        public List<ActionInfo> DestinationEntryActions = new List<ActionInfo>();

        /// <summary>
        /// Should the entry and exit actions be executed when this transition takes place
        /// </summary>
        public bool ExecuteEntryExitActions { get; protected set; } = true;

        /// <summary>
        /// The state where this transition starts
        /// </summary>
        public State SourceState { get; private set; }

        /// <summary>
        /// Base class of transitions
        /// </summary>
        /// <param name="sourceState"></param>
        /// <param name="trigger"></param>
        public Transition(State sourceState, TriggerInfo trigger)
        {
            SourceState = sourceState;
            Trigger = trigger;
        }
    }

    class FixedTransition : Transition
    {
        /// <summary>
        /// The state where this transition finishes
        /// </summary>
        public State DestinationState { get; private set; }

        /// <summary>
        /// Guard functions for this transition (null if none)
        /// </summary>
        public IEnumerable<InvocationInfo> Guards { get; private set; }

        public FixedTransition(State sourceState, State destinationState, TriggerInfo trigger, IEnumerable<InvocationInfo> guards)
            : base(sourceState, trigger)
        {
            DestinationState = destinationState;
            Guards = guards;
        }
    }

    class DynamicTransition : Transition
    {
        /// <summary>
        /// The state where this transition finishes
        /// </summary>
        public State DestinationState { get; private set; }

        /// <summary>
        /// When is this transition followed
        /// </summary>
        public string Criterion { get; private set; }

        public DynamicTransition(State sourceState, State destinationState, TriggerInfo trigger, string criterion)
            : base(sourceState, trigger)
        {
            DestinationState = destinationState;
            Criterion = criterion;
        }
    }

    class StayTransition : Transition
    {
        public IEnumerable<InvocationInfo> Guards { get; private set; }

        public StayTransition(State sourceState, TriggerInfo trigger, IEnumerable<InvocationInfo> guards, bool executeEntryExitActions)
            : base(sourceState, trigger)
        {
            ExecuteEntryExitActions = executeEntryExitActions;
            Guards = guards;
        }
    }
}
