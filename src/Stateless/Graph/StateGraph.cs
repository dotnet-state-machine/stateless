using System.Collections.Generic;
using System.Linq;
using Stateless.Reflection;

namespace Stateless.Graph
{
    /// <summary>
    /// This class is used to generate a symbolic representation of the
    /// graph structure, in preparation for feeding it to a diagram
    /// generator 
    /// </summary>
    public class StateGraph
    {
        private StateInfo initialState;

        /// <summary>
        /// List of all states in the graph, indexed by the string representation of the underlying State object.
        /// </summary>
        public Dictionary<string, State> States { get; private set; } = new Dictionary<string, State>();

        /// <summary>
        /// List of all transitions in the graph
        /// </summary>
        public List<Transition> Transitions { get; private set; } = new List<Transition>();

        /// <summary>
        /// List of all decision nodes in the graph.  A decision node is generated each time there
        /// is a PermitDynamic() transition.
        /// </summary>
        public List<Decision> Decisions { get; private set; } = new List<Decision>();

        /// <summary>
        /// Creates a new instance of <see cref="StateGraph"/>.
        /// </summary>
        /// <param name="machineInfo">An object which exposes the states, transitions, and actions of this machine.</param>
        public StateGraph(StateMachineInfo machineInfo)
        {
            // Add initial state
            initialState = machineInfo.InitialState;

            // Start with top-level superstates
            AddSuperstates(machineInfo);

            // Now add any states that aren't part of a tree
            AddSingleStates(machineInfo);

            // Now grab transitions
            AddTransitions(machineInfo);

            // Handle "OnEntryFrom"
            ProcessOnEntryFrom(machineInfo);
        }

        /// <summary>
        /// Convert the graph into a string representation, using the specified style.
        /// </summary>
        /// <param name="style"></param>
        /// <returns></returns>
        public string ToGraph(GraphStyleBase style)
        {
            string dirgraphText = style.GetPrefix().Replace("\n", System.Environment.NewLine);

            // Start with the clusters
            foreach (var state in States.Values.Where(x => x is SuperState))
            {
                dirgraphText += style.FormatOneCluster((SuperState)state).Replace("\n", System.Environment.NewLine);
            }

            // Next process all non-cluster states
            foreach (var state in States.Values)
            {
                if ((state is SuperState) || (state is Decision) || (state.SuperState != null))
                    continue;
                dirgraphText += style.FormatOneState(state).Replace("\n", System.Environment.NewLine);
            }

            // Finally, add decision nodes
            foreach (var dec in Decisions)
            {
                dirgraphText += style.FormatOneDecisionNode(dec.NodeName, dec.Method.Description)
                    .Replace("\n", System.Environment.NewLine);
            }

            // now build behaviours
            List<string> transits = style.FormatAllTransitions(Transitions);
            foreach (var transit in transits)
                dirgraphText += System.Environment.NewLine + transit;

            // Add initial transition if present
            var initialStateName = initialState.UnderlyingState.ToString();
            dirgraphText += System.Environment.NewLine + $" init [label=\"\", shape=point];";
            dirgraphText += System.Environment.NewLine + $" init -> \"{initialStateName}\"[style = \"solid\"]";

            dirgraphText += System.Environment.NewLine + "}";

            return dirgraphText;
        }

        /// <summary>
        /// Process all entry actions that have a "FromTrigger" (meaning they are
        /// only executed when the state is entered because the specified trigger
        /// was fired).
        /// </summary>
        /// <param name="machineInfo"></param>
        void ProcessOnEntryFrom(StateMachineInfo machineInfo)
        {
            foreach (var stateInfo in machineInfo.States)
            {
                State state = States[stateInfo.UnderlyingState.ToString()];
                foreach (var entryAction in stateInfo.EntryActions)
                {
                    if (entryAction.FromTrigger != null)
                    {
                        // This 'state' has an 'entryAction' that only fires when it gets the trigger 'entryAction.FromTrigger'
                        // Does it have any incoming transitions that specify that trigger?
                        foreach (var transit in state.Arriving)
                        {
                            if ((transit.ExecuteEntryExitActions)
                                && (transit.Trigger.UnderlyingTrigger.ToString() == entryAction.FromTrigger))
                            {
                                transit.DestinationEntryActions.Add(entryAction);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Add all transitions to the graph
        /// </summary>
        /// <param name="machineInfo"></param>
        void AddTransitions(StateMachineInfo machineInfo)
        {
            foreach (var stateInfo in machineInfo.States)
            {
                State fromState = States[stateInfo.UnderlyingState.ToString()];
                foreach (var fix in stateInfo.FixedTransitions)
                {
                    State toState = States[fix.DestinationState.UnderlyingState.ToString()];
                    if (fromState == toState)
                    {
                        StayTransition stay = new StayTransition(fromState, fix.Trigger, fix.GuardConditionsMethodDescriptions, true);
                        Transitions.Add(stay);
                        fromState.Leaving.Add(stay);
                        fromState.Arriving.Add(stay);
                    }
                    else
                    {
                        FixedTransition trans = new FixedTransition(fromState, toState, fix.Trigger, fix.GuardConditionsMethodDescriptions);
                        Transitions.Add(trans);
                        fromState.Leaving.Add(trans);
                        toState.Arriving.Add(trans);
                    }
                }
                foreach (var dyno in stateInfo.DynamicTransitions)
                {
                    Decision decide = new Decision(dyno.DestinationStateSelectorDescription, Decisions.Count + 1);
                    Decisions.Add(decide);
                    FixedTransition trans = new FixedTransition(fromState, decide, dyno.Trigger,
                        dyno.GuardConditionsMethodDescriptions);
                    Transitions.Add(trans);
                    fromState.Leaving.Add(trans);
                    decide.Arriving.Add(trans);
                    if (dyno.PossibleDestinationStates != null)
                    {
                        foreach (var dynamicStateInfo in dyno.PossibleDestinationStates)
                        {
                            States.TryGetValue(dynamicStateInfo.DestinationState, out State toState);
                            if (toState != null)
                            {
                                DynamicTransition dtrans = new DynamicTransition(decide, toState, dyno.Trigger, dynamicStateInfo.Criterion);
                                Transitions.Add(dtrans);
                                decide.Leaving.Add(dtrans);
                                toState.Arriving.Add(dtrans);
                            }
                        }
                    }
                }
                foreach (var igno in stateInfo.IgnoredTriggers)
                {
                    StayTransition stay = new StayTransition(fromState, igno.Trigger, igno.GuardConditionsMethodDescriptions, false);
                    Transitions.Add(stay);
                    fromState.Leaving.Add(stay);
                    fromState.Arriving.Add(stay);
                }
            }
        }


        /// <summary>
        /// Add states to the graph that are neither superstates, nor substates of a superstate.
        /// </summary>
        /// <param name="machineInfo"></param>
        void AddSingleStates(StateMachineInfo machineInfo)
        {
            foreach (var stateInfo in machineInfo.States)
            {
                if (!States.ContainsKey(stateInfo.UnderlyingState.ToString()))
                    States[stateInfo.UnderlyingState.ToString()] = new State(stateInfo);
            }
        }

        /// <summary>
        /// Add superstates to the graph (states that have substates)
        /// </summary>
        /// <param name="machineInfo"></param>
        void AddSuperstates(StateMachineInfo machineInfo)
        {
            foreach (var stateInfo in machineInfo.States.Where(sc => (sc.Substates?.Count() > 0) && (sc.Superstate == null)))
            {
                SuperState state = new SuperState(stateInfo);
                States[stateInfo.UnderlyingState.ToString()] = state;
                AddSubstates(state, stateInfo.Substates);
            }
        }

        void AddSubstates(SuperState superState, IEnumerable<StateInfo> substates)
        {
            foreach (var subState in substates)
            {
                if (States.ContainsKey(subState.UnderlyingState.ToString()))
                {
                    // This shouldn't happen
                }
                else if (subState.Substates.Any())
                {
                    SuperState sub = new SuperState(subState);
                    States[subState.UnderlyingState.ToString()] = sub;
                    superState.SubStates.Add(sub);
                    sub.SuperState = superState;
                    AddSubstates(sub, subState.Substates);
                }
                else
                {
                    State sub = new State(subState);
                    States[subState.UnderlyingState.ToString()] = sub;
                    superState.SubStates.Add(sub);
                    sub.SuperState = superState;
                }
            }
        }
    }
}
