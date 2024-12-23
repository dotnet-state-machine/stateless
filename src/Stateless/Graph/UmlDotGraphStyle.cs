using Stateless.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless.Graph
{
    /// <summary>
    /// Generate DOT graphs in basic UML style
    /// </summary>
    public class UmlDotGraphStyle : GraphStyleBase
    {
        /// <summary>Get the text that starts a new graph</summary>
        /// <returns></returns>
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
        /// For example, for DOT files this would be a subgraph containing nodes for all the substates.
        /// </summary>
        /// <param name="stateInfo">The superstate to generate text for</param>
        /// <returns>Description of the superstate, and all its substates, in the desired format</returns>
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
        /// Generate the text for a single state
        /// </summary>
        /// <param name="state">The state to generate text for</param>
        /// <returns></returns>
        public override string FormatOneState(State state)
        {
            var escapedStateName = EscapeLabel(state.StateName);

            if (state.EntryActions.Count == 0 && state.ExitActions.Count == 0)
                return $"\"{escapedStateName}\" [label=\"{escapedStateName}\"];{Environment.NewLine}";

            string f = $"\"{escapedStateName}\" [label=\"{escapedStateName}|";

            List<string> es = new List<string>();
            es.AddRange(state.EntryActions.Select(act => "entry / " + EscapeLabel(act)));
            es.AddRange(state.ExitActions.Select(act => "exit / " + EscapeLabel(act)));

            f += string.Join("\\n", es);

            f += $"\"];{Environment.NewLine}";

            return f;
        }

        /// <summary>
        /// Generate text for a single transition
        /// </summary>
        /// <param name="sourceNodeName"></param>
        /// <param name="trigger"></param>
        /// <param name="actions"></param>
        /// <param name="destinationNodeName"></param>
        /// <param name="guards"></param>
        /// <returns></returns>
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
        /// Generate the text for a single decision node
        /// </summary>
        /// <param name="nodeName">Name of the node</param>
        /// <param name="label">Label for the node</param>
        /// <returns></returns>
        public override string FormatOneDecisionNode(string nodeName, string label)
        {
            return $"\"{EscapeLabel(nodeName)}\" [shape = \"diamond\", label = \"{EscapeLabel(label)}\"];{Environment.NewLine}";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="initialState"></param>
        /// <returns></returns>
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
