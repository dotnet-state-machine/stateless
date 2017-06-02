using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless.Graph
{
    /// <summary>
    /// Generate DOT graphs in basic UML style
    /// </summary>
    public class UmlDotGraphStyle : IGraphStyle
    {
        /// <summary>Get the text that starts a new graph</summary>
        /// <returns></returns>
        override internal string GetPrefix()
        {
            return "digraph {\n"
                      + "compound=true;\n"
                      + "node [shape=Mrecord]\n"
                      + "rankdir=\"LR\"\n";
        }

        internal override string FormatOneCluster(SuperState stateInfo)
        {
            string stateRepresentationString = "";
            var sourceName = stateInfo.StateName;

            StringBuilder label = new StringBuilder(sourceName);

            if ((stateInfo.EntryActions.Count > 0) || (stateInfo.ExitActions.Count > 0))
            {
                label.Append("\\n----------");
                label.Append(string.Concat(stateInfo.EntryActions.Select(act => "\\nentry / " + act)));
                label.Append(string.Concat(stateInfo.ExitActions.Select(act => "\\nexit / " + act)));
            }

            stateRepresentationString = "\n"
                + $"subgraph {stateInfo.NodeName}" + "\n"
                + "\t{" + "\n"
                + $"\tlabel = \"{label.ToString()}\"" + "\n";

            foreach (State subState in stateInfo.SubStates)
            {
                stateRepresentationString += FormatOneState(subState);
            }

            stateRepresentationString += "}\n";

            return stateRepresentationString;
        }

        /// <summary>
        /// Generate the text for a single state
        /// </summary>
        /// <param name="state">The state to generate text for</param>
        /// <returns></returns>
        override internal string FormatOneState(State state)
        {
            if ((state.EntryActions.Count == 0) && (state.ExitActions.Count == 0))
                return state.StateName + " [label=\"" + state.StateName + "\"];\n";

            string f = state.StateName + " [label=\"" + state.StateName + "|";

            List<string> es = new List<string>();
            es.AddRange(state.EntryActions.Select(act => "entry / " + act));
            es.AddRange(state.ExitActions.Select(act => "exit / " + act));

            f += String.Join("\\n", es);

            f += "\"];\n";

            return f;
        }

        /// <summary>
        /// Format all transitions for a graph
        /// </summary>
        /// <param name="transitions"></param>
        /// <returns></returns>
        override internal List<string> FormatAllTransitions(List<Stateless.Graph.Transition> transitions)
        {
            List<string> lines = new List<string>();

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
                    else if (stay.SourceState.EntryActions.Count() == 0)
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
                            throw new System.Exception("Unexpected transition type");
                    }
                }
                if (line != null)
                    lines.Add(line);
            }

            return lines;
        }

        /// <summary>
        /// Generate text for a single transition
        /// </summary>
        /// <param name="source"></param>
        /// <param name="trigger"></param>
        /// <param name="actions"></param>
        /// <param name="destinationString"></param>
        /// <param name="guards"></param>
        /// <returns></returns>
        override internal string FormatOneTransition(string source, string trigger, IEnumerable<string> actions, string destinationString, IEnumerable<string> guards)
        {
            string label = trigger ?? "";

            if (actions?.Count() > 0)
                label += " / " + string.Join(", ", actions);

            if (guards.Count() > 0)
            {
                foreach (var info in guards)
                {
                    if (label.Length > 0)
                        label += " ";
                    label += "[" + info + "]";
                }
            }

            return FormatOneLine(source, destinationString, label);
        }

        /// <summary>
        /// Generate the text for a single decision node
        /// </summary>
        /// <param name="nodeName">Name of the node</param>
        /// <param name="label">Label for the node</param>
        /// <returns></returns>
        override internal string FormatOneDecisionNode(string nodeName, string label)
        {
            return nodeName + " [shape = \"diamond\", label = \"" + label + "\"];\n";
        }

        override internal string FormatOneLine(string fromNodeName, string toNodeName, string label)
        {
            return fromNodeName + " -> " + toNodeName + " " + "[style=\"solid\", label=\"" + label + "\"];";
        }
    }
}
