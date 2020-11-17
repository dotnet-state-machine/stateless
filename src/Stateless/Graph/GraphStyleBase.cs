using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless.Graph
{
    /// <summary>
    /// Style definition for StateGraph.
    /// Provides formatting of individual items in a state graph.
    /// </summary>
    public abstract class GraphStyleBase
    {
        /// <summary>
        /// Get the text that must be present at the top of a state graph file.
        /// For example, for DOT files the prefix text would be
        /// digraph {
        /// </summary>
        /// <returns>Prefix text</returns>
        public abstract string GetPrefix();

        /// <summary>
        /// Returns the formatted text for a single state.
        /// For example, for DOT files this would be the description of a single node:
        /// nodename [label="statename"];
        /// Usually the information on exit and entry actions would also be included here.
        /// </summary>
        /// <param name="state">The state to generate text for</param>
        /// <returns>Description of the state in the desired format</returns>
        public abstract string FormatOneState(State state);

        /// <summary>
        /// Returns the formatted text for a single superstate and its substates.
        /// For example, for DOT files this would be a subgraph containing nodes for all the substates.
        /// </summary>
        /// <param name="stateInfo">The superstate to generate text for</param>
        /// <returns>Description of the superstate, and all its substates, in the desired format</returns>
        public abstract string FormatOneCluster(SuperState stateInfo);

        /// <summary>
        /// Returns the formatted text for a single decision node.
        /// A decision node is created each time there is a PermitDynamic() transition.  There will be a transition
        /// from the state that has the dynamic transition to the decision node, and transitions from the decision
        /// node to each of the destination nodes.  If the caller did not specify the possible destination states,
        /// there will be no transitions leaving the decision node.
        /// </summary>
        /// <param name="nodeName">Name of the node</param>
        /// <param name="label">Label for the node</param>
        /// <returns></returns>
        public abstract string FormatOneDecisionNode(string nodeName, string label);

        /// <summary>
        /// Returns the formatted text for all the transitions found in the state graph.
        /// This form, which can be overridden, determines the type of each transition and passes the appropriate
        /// parameters to the virtual FormatOneTransition() function.
        /// </summary>
        /// <param name="transitions">List of all transitions in the state graph</param>
        /// <returns>Description of all transitions, in the desired format</returns>
        public virtual List<string> FormatAllTransitions(List<Transition> transitions)
        {
            List<string> lines = new List<string>();
            if (transitions == null) return lines;

            foreach (var transit in transitions)
            {
                string line = null;
                if (transit is StayTransition stay)
                {
                    if (!stay.ExecuteEntryExitActions)
                    {
                        line = FormatOneTransition(stay.SourceState.NodeName, stay.Trigger.UnderlyingTrigger.ToString(),
                            null, stay.SourceState.NodeName, stay.Guards.Select(x => x.Description));
                    }
                    else if (stay.SourceState.EntryActions.Count == 0)
                    {
                        line = FormatOneTransition(stay.SourceState.NodeName, stay.Trigger.UnderlyingTrigger.ToString(),
                            null, stay.SourceState.NodeName, stay.Guards.Select(x => x.Description));
                    }
                    else
                    {
                        // There are entry functions into the state, so call out that this transition
                        // does invoke them (since normally a transition back into the same state doesn't)
                        line = FormatOneTransition(stay.SourceState.NodeName, stay.Trigger.UnderlyingTrigger.ToString(),
                            stay.SourceState.EntryActions, stay.SourceState.NodeName, stay.Guards.Select(x => x.Description));
                    }
                }
                else
                {
                    if (transit is FixedTransition fix)
                    {
                        line = FormatOneTransition(fix.SourceState.NodeName, fix.Trigger.UnderlyingTrigger.ToString(),
                            fix.DestinationEntryActions.Select(x => x.Method.Description),
                            fix.DestinationState.NodeName, fix.Guards.Select(x => x.Description));
                    }
                    else
                    {
                        if (transit is DynamicTransition dyn)
                        {
                            line = FormatOneTransition(dyn.SourceState.NodeName, dyn.Trigger.UnderlyingTrigger.ToString(),
                                dyn.DestinationEntryActions.Select(x => x.Method.Description),
                                dyn.DestinationState.NodeName, new List<string> { dyn.Criterion });
                        }
                        else
                            throw new ArgumentException("Unexpected transition type");
                    }
                }
                if (line != null)
                    lines.Add(line);
            }

            return lines;
        }

        /// <summary>
        /// Returns the formatted text for a single transition.  Only required if the default version of
        /// FormatAllTransitions() is used.
        /// </summary>
        /// <param name="sourceNodeName">Node name of the source state node</param>
        /// <param name="trigger">Name of the trigger</param>
        /// <param name="actions">List of entry and exit actions (if any)</param>
        /// <param name="destinationNodeName"></param>
        /// <param name="guards">List of guards (if any)</param>
        /// <returns></returns>
        public virtual string FormatOneTransition(string sourceNodeName, string trigger, IEnumerable<string> actions, string destinationNodeName, IEnumerable<string> guards)
        {
            throw new InvalidOperationException("If you use IGraphStyle.FormatAllTransitions() you must implement an override of FormatOneTransition()");
        }
    }
}
