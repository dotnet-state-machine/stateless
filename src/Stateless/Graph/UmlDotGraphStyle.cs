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
            var sb = new StringBuilder();
            sb.AppendLine("digraph {")
                .AppendLine("compound=true;")
                .AppendLine("node [shape=Mrecord]")
                .AppendLine("rankdir=\"LR\"");

            return sb.ToString();
        }

        /// <summary>
        /// Returns the formatted text for a single superstate and its substates.
        /// </summary>
        /// <returns>A DOT graph representation of the superstate and all its substates.</returns>
        /// <inheritdoc/>
        public override string FormatOneCluster(SuperState stateInfo)
        {
            var sb = new StringBuilder();
            var sourceName = stateInfo.StateName;

            StringBuilder label = new StringBuilder($"{EscapeLabel(stateInfo.StateName)}");

            if (stateInfo.EntryActions.Any() || stateInfo.ExitActions.Any())
            {
                label.Append("\\n----------");
                label.Append(string.Concat(stateInfo.EntryActions.Select(act => "\\nentry / " + EscapeLabel(act))));
                label.Append(string.Concat(stateInfo.ExitActions.Select(act => "\\nexit / " + EscapeLabel(act))));
            }

            sb.AppendLine()
                .AppendLine($"subgraph \"cluster{EscapeLabel(stateInfo.NodeName)}\"")
                .AppendLine("\t{")
                .AppendLine($"\tlabel = \"{label.ToString()}\"");

            foreach (var subState in stateInfo.SubStates)
            {
                sb.Append(FormatOneState(subState));
            }

            sb.AppendLine("}");

            return sb.ToString();
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
                return $"\"{escapedStateName}\" [label=\"{escapedStateName}\"];{Environment.NewLine}";

            StringBuilder sb = new StringBuilder($"\"{escapedStateName}\" [label=\"{escapedStateName}|");

            List<string> es = new List<string>();
            es.AddRange(state.EntryActions.Select(act => "entry / " + EscapeLabel(act)));
            es.AddRange(state.ExitActions.Select(act => "exit / " + EscapeLabel(act)));

            sb.Append(string.Join("\\n", es));
            sb.Append($"\"];{Environment.NewLine}");

            return sb.ToString();
        }

        /// <summary>
        /// Generate text for a single transition.
        /// </summary>
        /// <returns>A DOT graph representation of a state transition.</returns>
        /// <inheritdoc/>
        public override string FormatOneTransition(string sourceNodeName, string trigger, IEnumerable<string> actions, string destinationNodeName, IEnumerable<string> guards)
        {
            StringBuilder sb = new StringBuilder(trigger ?? string.Empty);

            if (actions?.Count() > 0)
            {
                sb.Append(" / ");
                sb.Append(string.Join(", ", actions));
            }

            if (guards.Any())
            {
                foreach (var info in guards)
                {
                    if (sb.Length > 0)
                        sb.Append(" ");

                    sb.Append("[" + info + "]");
                }
            }

            return FormatOneLine(sourceNodeName, destinationNodeName, sb.ToString());
        }

        /// <summary>
        /// Generate the text for a single decision node
        /// </summary>
        /// <returns>A DOT graph representation of the decision node for a dynamic transition.</returns>
        /// <inheritdoc/>
        public override string FormatOneDecisionNode(string nodeName, string label)
        {
            return $"\"{EscapeLabel(nodeName)}\" [shape = \"diamond\", label = \"{EscapeLabel(label)}\"];{Environment.NewLine}";
        }

        /// <summary>
        /// Get initial transition if present.
        /// </summary>
        /// <returns>A DOT graph representation of the initial state transition.</returns>
        /// <inheritdoc/>
        public override string GetInitialTransition(StateInfo initialState)
        {
            var initialStateName = initialState.UnderlyingState.ToString();

            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.Append($" init [label=\"\", shape=point];");
            sb.AppendLine();
            sb.Append($" init -> \"{EscapeLabel(initialStateName)}\"[style = \"solid\"]");
            sb.AppendLine();
            sb.Append("}");

            return sb.ToString();
        }

        internal static string FormatOneLine(string fromNodeName, string toNodeName, string label)
        {
            return $"\"{EscapeLabel(fromNodeName)}\" -> \"{EscapeLabel(toNodeName)}\" [style=\"solid\", label=\"{EscapeLabel(label)}\"];";
        }

        private static string EscapeLabel(string label)
        {
            return label.Replace("\\", "\\\\").Replace("\"", "\\\"");
        }
    }
}
