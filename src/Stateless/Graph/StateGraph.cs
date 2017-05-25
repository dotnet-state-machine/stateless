using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Stateless.Reflection;

namespace Stateless.Graph
{
    /// <summary>
    /// This class is used to generate a symbolic representation of the
    /// graph structure, in preparation for feeding it to a diagram
    /// generator 
    /// </summary>
    class StateGraph
    {
        public Dictionary<string, State> States { get; private set; } = new Dictionary<string, State>();
        public List<Transition> Transitions { get; private set; } = new List<Transition>();
        public List<Decision> Decisions { get; private set; } = new List<Decision>();

        public StateGraph(StateMachineInfo machineInfo)
        {
            Debug.WriteLine("Graph.Graph(machineInfo)");

            // Start with top-level superstates
            foreach (var stateInfo in machineInfo.States.Where(sc => (sc.Substates?.Count() > 0) && (sc.Superstate == null)))
            {
                SuperState state = new SuperState(stateInfo);
                States[stateInfo.UnderlyingState.ToString()] = state;
                AddSubstates(state, stateInfo.Substates);
            }

            // Now add any states that aren't part of a tree
            foreach (var stateInfo in machineInfo.States)
            {
                if (!States.ContainsKey(stateInfo.UnderlyingState.ToString()))
                    States[stateInfo.UnderlyingState.ToString()] = new State(stateInfo);
            }

            // Now grab transitions
            foreach (var stateInfo in machineInfo.States)
            {
                State fromState = States[stateInfo.UnderlyingState.ToString()];
                foreach (var fix in stateInfo.FixedTransitions)
                {
                    Debug.WriteLine("   Processing fixed transition from " + stateInfo.UnderlyingState + " to " + fix.DestinationState);
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
                            State toState = null;
                            States.TryGetValue(dynamicStateInfo.DestinationState, out toState);
                            if (toState == null)
                                Debug.WriteLine("*** Dynamic state info specifies unexpected destination state \"" + dynamicStateInfo.DestinationState + "\"");
                            else
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

            // Handle "OnEntryFrom"
            foreach (var stateInfo in machineInfo.States)
            {
                State state = States[stateInfo.UnderlyingState.ToString()];
                foreach (var entryAction in stateInfo.EntryActions)
                {
                    if (entryAction.FromTrigger != null)
                    {
                        Debug.WriteLine("   Processing EntryAction into state " + stateInfo.UnderlyingState + " with trigger " + entryAction.FromTrigger);
                        // This 'state' has an 'entryAction' that only fires when it gets the trigger 'entryAction.FromTrigger'
                        // Does it have any incoming transitions that specify that trigger?
                        int numMatches = 0;
                        foreach (var transit in state.Arriving)
                        {
                            if ( (transit.ExecuteEntryExitActions)
                                && (transit.Trigger.UnderlyingTrigger.ToString() == entryAction.FromTrigger) )
                            {
                                transit.DestinationEntryActions.Add(entryAction);
                                numMatches++;
                            }
                        }
                        if (numMatches == 0)
                        {
                            Debug.WriteLine("Warning: State(" + state.NodeName + ").OnEntryFrom(" + entryAction.FromTrigger.ToString()
                                + ") found no matching transitions");
                        }
                    }
                }
            }
        }

        public string ToGraph(IGraphStyle style)
        {
            string dirgraphText = style.GetPrefix().Replace("\n", System.Environment.NewLine);

            // Start with the clusters
            foreach (State state in States.Values.Where(x => x is SuperState))
            {
                dirgraphText += style.FormatOneCluster((SuperState)state).Replace("\n", System.Environment.NewLine);
            }

            // Next process all non-cluster states
            foreach (State state in States.Values)
            {
                if ((state is SuperState) || (state is Decision) || (state.SuperState != null))
                    continue;
                dirgraphText += style.FormatOneState(state).Replace("\n", System.Environment.NewLine);
            }

            // Finally, add decision nodes
            foreach (Decision dec in Decisions)
            {
                dirgraphText += style.FormatOneDecisionNode(dec.NodeName, dec.Method.Description)
                    .Replace("\n", System.Environment.NewLine);
            }

            // now build behaviours
            List<string> transits = style.FormatAllTransitions(Transitions);
            foreach (string transit in transits)
                dirgraphText += System.Environment.NewLine + transit;

            dirgraphText += System.Environment.NewLine + "}";

            return dirgraphText;
        }

        void AddSubstates(SuperState superState, IEnumerable<StateInfo> substates)
        {
            foreach (var subState in substates)
            {
                if (States.ContainsKey(subState.UnderlyingState.ToString()))
                {
                    Debug.WriteLine("*** Attempt to add state multiple times");
                }
                else if (subState.Substates.Count() > 0)
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
