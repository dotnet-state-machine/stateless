using System.Collections.Generic;

namespace Stateless.Graph
{
    /// <summary>
    /// Used to keep track of a state that has substates
    /// </summary>
    public class SuperState : State
    {
        /// <summary>
        /// List of states that are a substate of this state
        /// </summary>
        public List<State> SubStates { get; } = new List<State>();

        /// <summary>
        /// Constructs a new instance of SuperState.
        /// </summary>
        /// <param name="stateInfo">The super state to be represented.</param>
        public SuperState(Reflection.StateInfo stateInfo)
            : base(stateInfo)
        {

        }
    }
}
