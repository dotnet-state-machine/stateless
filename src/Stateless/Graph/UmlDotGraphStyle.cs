using Stateless.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless.Graph
{
    /// <summary>
    /// Generate DOT graphs in basic UML style.
    /// </summary>
    public class UmlDotGraphStyle : GraphStyleBase
    {
        /// <summary>Get the text that starts a new graph.</summary>
        /// <returns>The prefix for the DOT graph document.</returns>
        public override string GetPrefix()
        {
            return "digraph {\n"
                      + "compound=true;\n"
                      + "node [shape=Mrecord]\n"
                      + "rankdir=\"LR\"\n";
        }

        /// <summary>
        /// Returns the formatted text for a single superstate and its substates.
        /// </summary>
        /// <returns>A DOT graph representation of the superstate and all its substates.</returns>
        /// <inheritdoc/>
        public override string FormatOneCluster(SuperState stateInfo)
        {
            string stateRepresentationString = "";

            StringBuilder label = new StringBuilder($"{EscapeLabel(stateInfo.StateName)}");

            if (stateInfo.EntryActions.Count > 0 || stateInfo.ExitActions.Count > 0)
            {
                label.Append("\\n----------");
                label.Append(string.Concat(stateInfo.EntryActions.Select(act => "\\nentry / " + EscapeLabel(act))));
                label.Append(string.Concat(stateInfo.ExitActions.Select(act => "\\nexit / " + EscapeLabel(act))));
            }

            stateRepresentationString = "\n"
                + $"subgraph \"cluster{EscapeLabel(stateInfo.NodeName)}\"" + "\n"
                + "\t{" + "\n"
                + $"\tlabel = \"{label}\"" + "\n";

            foreach (var subState in stateInfo.SubStates)
            {
                stateRepresentationString += FormatOneState(subState);
            }

            stateRepresentationString += "}\n";

            return stateRepresentationString;
        }

        /// <summary>
        /// Generate the text for a single state.
        /// </summary>
        /// <returns>A DOT graph representation of the state.</returns>
        /// <inheritdoc/>
        public override string FormatOneState(State state)
        {
            var escapedStateName = EscapeLabel(state.StateName);

            if (state.EntryActions.Count == 0 && state.ExitActions.Count == 0)
                return $"\"{escapedStateName}\" [label=\"{escapedStateName}\"];\n";

            string f = $"\"{escapedStateName}\" [label=\"{escapedStateName}|";

            List<string> es = new List<string>();
            es.AddRange(state.EntryActions.Select(act => "entry / " + EscapeLabel(act)));
            es.AddRange(state.ExitActions.Select(act => "exit / " + EscapeLabel(act)));

            f += string.Join("\\n", es);

            f += "\"];\n";

            return f;
        }

        /// <summary>
        /// Generate text for a single transition.
        /// </summary>
        /// <returns>A DOT graph representation of a state transition.</returns>
        /// <inheritdoc/>
        public override string FormatOneTransition(string sourceNodeName, string trigger, IEnumerable<string> actions, string destinationNodeName, IEnumerable<string> guards)
        {
            string label = trigger ?? "";

            if (actions?.Count() > 0)
                label += " / " + string.Join(", ", actions);

            if (guards.Any())
            {
                foreach (var info in guards)
                {
                    if (label.Length > 0)
                        label += " ";
                    label += "[" + info + "]";
                }
            }

            return FormatOneLine(sourceNodeName, destinationNodeName, label);
        }

        /// <summary>
        /// Generate the text for a single decision node.
        /// </summary>
        /// <returns>A DOT graph representation of the decision node for a dynamic transition.</returns>
        /// <inheritdoc/>
        public override string FormatOneDecisionNode(string nodeName, string label)
        {
            return $"\"{EscapeLabel(nodeName)}\" [shape = \"diamond\", label = \"{EscapeLabel(label)}\"];\n";
        }

        /// <summary>
        /// Get initial transition if present.
        /// </summary>
        /// <returns>A DOT graph representation of the initial state transition.</returns>
        /// <inheritdoc/>
        public override string GetInitialTransition(StateInfo initialState)
        {
            var initialStateName = initialState.UnderlyingState.ToString();
            string dirgraphText = Environment.NewLine + $" init [label=\"\", shape=point];";
            dirgraphText += Environment.NewLine + $" init -> \"{EscapeLabel(initialStateName)}\"[style = \"solid\"]";

            dirgraphText += Environment.NewLine + "}";

            return dirgraphText;
        }

        internal string FormatOneLine(string fromNodeName, string toNodeName, string label)
        {
            return $"\"{EscapeLabel(fromNodeName)}\" -> \"{EscapeLabel(toNodeName)}\" [style=\"solid\", label=\"{EscapeLabel(label)}\"];";
        }

        private static string EscapeLabel(string label)
        {
            return label.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
