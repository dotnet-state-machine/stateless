using Stateless.Reflection;

namespace Stateless.Graph; 

/// <summary>
/// This class is used to generate a symbolic representation of the
/// graph structure, in preparation for feeding it to a diagram
/// generator 
/// </summary>
public class StateGraph
{
    private readonly StateInfo _initialState;

    /// <summary>
    /// List of all states in the graph, indexed by the string representation of the underlying State object.
    /// </summary>
    public Dictionary<string, State> States { get; } = new();

    /// <summary>
    /// List of all transitions in the graph
    /// </summary>
    public List<Transition> Transitions { get; } = new();

    /// <summary>
    /// List of all decision nodes in the graph.  A decision node is generated each time there
    /// is a PermitDynamic() transition.
    /// </summary>
    public List<Decision> Decisions { get; } = new();

    /// <summary>
    /// Creates a new instance of <see cref="StateGraph"/>.
    /// </summary>
    /// <param name="machineInfo">An object which exposes the states, transitions, and actions of this machine.</param>
    public StateGraph(StateMachineInfo machineInfo)
    {
        // Add initial state
        _initialState = machineInfo.InitialState;

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
        var dirGraphText = style.GetPrefix().Replace("\n", Environment.NewLine);

        // Start with the clusters
        foreach (var state in States.Values.Where(x => x is SuperState))
        {
            dirGraphText += style.FormatOneCluster((SuperState)state).Replace("\n", Environment.NewLine);
        }

        // Next process all non-cluster states
        foreach (var state in States.Values)
        {
            if ((state is SuperState) || (state is Decision) || state.SuperState is { })
                continue;
            dirGraphText += style.FormatOneState(state).Replace("\n", Environment.NewLine);
        }

        // Finally, add decision nodes
        foreach (var dec in Decisions)
        {
            dirGraphText += style.FormatOneDecisionNode(dec.NodeName, dec.Method.Description)
                                 .Replace("\n", Environment.NewLine);
        }

        // now build behaviours
        var transits = style.FormatAllTransitions(Transitions);
        foreach (var transit in transits)
            dirGraphText += Environment.NewLine + transit;

        // Add initial transition if present
        var initialStateName = _initialState.UnderlyingState.ToString();
        dirGraphText += $"{Environment.NewLine} init [label=\"\", shape=point];";
        dirGraphText += $"{Environment.NewLine} init -> \"{initialStateName}\"[style = \"solid\"]";

        dirGraphText += $"{Environment.NewLine}}}";

        return dirGraphText;
    }

    /// <summary>
    /// Process all entry actions that have a "FromTrigger" (meaning they are
    /// only executed when the state is entered because the specified trigger
    /// was fired).
    /// </summary>
    /// <param name="machineInfo"></param>
    private void ProcessOnEntryFrom(StateMachineInfo machineInfo)
    {
        foreach (var stateInfo in machineInfo.States)
        {
            var state = States[stateInfo.ToString()];
            foreach (var entryAction in stateInfo.EntryActions)
            {
                if (entryAction.FromTrigger is { })
                {
                    // This 'state' has an 'entryAction' that only fires when it gets the trigger 'entryAction.FromTrigger'
                    // Does it have any incoming transitions that specify that trigger?
                    foreach (var transit in state.Arriving)
                    {
                        if ((transit.ExecuteEntryExitActions)
                         && (transit.Trigger.UnderlyingTrigger?.ToString() == entryAction.FromTrigger))
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
    private void AddTransitions(StateMachineInfo machineInfo)
    {
        foreach (var stateInfo in machineInfo.States)
        {
            var fromState = States[stateInfo.ToString()];
            foreach (var fix in stateInfo.FixedTransitions)
            {
                var toState = States[fix.DestinationState.ToString()];
                if (fromState == toState)
                {
                    var stay = new StayTransition(fromState, fix.Trigger, fix.GuardConditionsMethodDescriptions, true);
                    Transitions.Add(stay);
                    fromState.Leaving.Add(stay);
                    fromState.Arriving.Add(stay);
                }
                else
                {
                    var trans = new FixedTransition(fromState, toState, fix.Trigger, fix.GuardConditionsMethodDescriptions);
                    Transitions.Add(trans);
                    fromState.Leaving.Add(trans);
                    toState.Arriving.Add(trans);
                }
            }
            foreach (var dyno in stateInfo.DynamicTransitions)
            {
                var decide = new Decision(dyno.DestinationStateSelectorDescription, Decisions.Count + 1);
                Decisions.Add(decide);
                var trans = new FixedTransition(fromState, decide, dyno.Trigger,
                                                dyno.GuardConditionsMethodDescriptions);
                Transitions.Add(trans);
                fromState.Leaving.Add(trans);
                decide.Arriving.Add(trans);
                if (dyno.PossibleDestinationStates is { })
                {
                    foreach (var dynamicStateInfo in dyno.PossibleDestinationStates)
                    {
                        States.TryGetValue(dynamicStateInfo.DestinationState, out var toState);
                        if (toState is { })
                        {
                            var dTrans = new DynamicTransition(decide, toState, dyno.Trigger, dynamicStateInfo.Criterion);
                            Transitions.Add(dTrans);
                            decide.Leaving.Add(dTrans);
                            toState.Arriving.Add(dTrans);
                        }
                    }
                }
            }
            foreach (var ignored in stateInfo.IgnoredTriggers)
            {
                var stay = new StayTransition(fromState, ignored.Trigger, ignored.GuardConditionsMethodDescriptions, false);
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
    private void AddSingleStates(StateMachineInfo machineInfo)
    {
        foreach (var stateInfo in machineInfo.States)
        {
            if (!States.ContainsKey(stateInfo.ToString()))
                States[stateInfo.ToString()] = new State(stateInfo);
        }
    }

    /// <summary>
    /// Add superstates to the graph (states that have substates)
    /// </summary>
    /// <param name="machineInfo"></param>
    private void AddSuperstates(StateMachineInfo machineInfo)
    {
        foreach (var stateInfo in machineInfo.States.Where(sc => (sc.Substates.Any()) && (sc.Superstate == null)))
        {
            var state = new SuperState(stateInfo);
            States[stateInfo.ToString()] = state;
            AddSubstates(state, stateInfo.Substates);
        }
    }

    private void AddSubstates(SuperState superState, IEnumerable<StateInfo> substates)
    {
        foreach (var subState in substates)
        {
            if (States.ContainsKey(subState.ToString()))
            {
                // This shouldn't happen
            }
            else if (subState.Substates.Any())
            {
                var sub = new SuperState(subState);
                States[subState.ToString()] = sub;
                superState.SubStates.Add(sub);
                sub.SuperState = superState;
                AddSubstates(sub, subState.Substates);
            }
            else
            {
                var sub = new State(subState);
                States[subState.ToString()] = sub;
                superState.SubStates.Add(sub);
                sub.SuperState = superState;
            }
        }
    }
}