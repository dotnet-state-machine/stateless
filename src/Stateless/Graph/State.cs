using System.Collections.Generic;

using Stateless.Reflection;

namespace Stateless.Graph
{
    /// <summary>
    /// Used to keep track of a state that has substates
    /// </summary>
    public class State
    {
        /// <summary>
        /// The superstate of this state (null if none)
        /// </summary>
        public SuperState SuperState { get; set; }

        /// <summary>
        /// List of all transitions that leave this state (never null)
        /// </summary>
        public List<Transition> Leaving { get; } = new List<Transition>();

        /// <summary>
        /// List of all transitions that enter this state (never null)
        /// </summary>
        public List<Transition> Arriving { get; } = new List<Transition>();

        /// <summary>
        /// Unique name of this object
        /// </summary>
        public string NodeName { get; private set; }

        /// <summary>
        /// Name of the state represented by this object
        /// </summary>
        public string StateName { get; private set; }

        /// <summary>
        /// Actions that are executed when you enter this state from any trigger
        /// </summary>
        public List<string> EntryActions { get; private set; } = new List<string>();

        /// <summary>
        /// Actions that are executed when you exit this state
        /// </summary>
        public List<string> ExitActions { get; private set; } = new List<string>();

        /// <summary>
        /// Constructs a new instance of State.
        /// </summary>
        /// <param name="stateInfo">The state to be represented.</param>
        public State(StateInfo stateInfo)
        {
            NodeName = stateInfo.UnderlyingState.ToString();
            StateName = stateInfo.UnderlyingState.ToString();

            // Only include entry actions that aren't specific to a trigger
            foreach (var entryAction in stateInfo.EntryActions)
            {
                if (entryAction.FromTrigger == null)
                    EntryActions.Add(entryAction.Method.Description);
            }

            foreach (var exitAction in stateInfo.ExitActions)
                ExitActions.Add(exitAction.Description);
        }

        /// <summary>
        /// Constructs a new instance of State.
        /// </summary>
        /// <param name="nodeName">The node name.</param>
        public State(string nodeName)
        {
            NodeName = nodeName;
            StateName = null;
        }
    }
}
