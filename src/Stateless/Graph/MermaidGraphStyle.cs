using Stateless.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless.Graph
{
    /// <summary>
    /// Class to generate a graph in mermaid format
    /// </summary>
    public class MermaidGraphStyle : GraphStyleBase
    {
        private readonly StateGraph _graph;
        private readonly MermaidGraphDirection? _direction;
        private readonly Dictionary<string, State> _stateMap = new Dictionary<string, State>();
        private bool _stateMapInitialized = false;

        /// <summary>
        /// Create a new instance of <see cref="MermaidGraphStyle"/>
        /// </summary>
        /// <param name="graph">The state graph</param>
        /// <param name="direction">When non-null, sets the flow direction in the output.</param>
        public MermaidGraphStyle(StateGraph graph, MermaidGraphDirection? direction) 
            : base()
        {
            _graph = graph;
            _direction = direction;
        }

        /// <inheritdoc/>
        public override string FormatOneCluster(SuperState stateInfo)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine($"\tstate {GetSanitizedStateName(stateInfo.StateName)} {{");
            foreach (var subState in stateInfo.SubStates)
            {
                sb.AppendLine($"\t\t{GetSanitizedStateName(subState.StateName)}");
            }

            sb.Append("\t}");

            return sb.ToString();
        }

        /// <summary>
        /// Generate the text for a single decision node
        /// </summary>
        /// <param name="nodeName">Name of the node</param>
        /// <param name="label">Label for the node</param>
        /// <returns></returns>
        public override string FormatOneDecisionNode(string nodeName, string label)
        {
            return $"{Environment.NewLine}\tstate {nodeName} <<choice>>";
        }

        /// <inheritdoc/>
        public override string FormatOneState(State state)
        {
            return string.Empty;
        }

        /// <summary>Get the text that starts a new graph</summary>
        /// <returns></returns>
        public override string GetPrefix()
        {
            BuildSanitizedNamedStateMap();
            string prefix = "stateDiagram-v2";
            if (_direction.HasValue)
            {
                prefix += $"{Environment.NewLine}\tdirection {GetDirectionCode(_direction.Value)}";
            }

            foreach (var state in _stateMap.Where(x => !x.Key.Equals(x.Value.StateName, StringComparison.Ordinal)))
            {
                prefix += $"{Environment.NewLine}\t{state.Key} : {state.Value.StateName}";
            }

            return prefix;
        }

        /// <inheritdoc/>
        public override string GetInitialTransition(StateInfo initialState)
        {
            var sanitizedStateName = GetSanitizedStateName(initialState.ToString());

            return $"{Environment.NewLine}[*] --> {sanitizedStateName}";
        }

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

            var sanitizedSourceNodeName = GetSanitizedStateName(sourceNodeName);
            var sanitizedDestinationNodeName = GetSanitizedStateName(destinationNodeName);

            return FormatOneLine(sanitizedSourceNodeName, sanitizedDestinationNodeName, label);
        }

        internal string FormatOneLine(string fromNodeName, string toNodeName, string label)
        {
            return $"\t{fromNodeName} --> {toNodeName} : {label}";
        }

        private static string GetDirectionCode(MermaidGraphDirection direction)
        {
            switch(direction)
            {
                case MermaidGraphDirection.TopToBottom:
                    return "TB";
                case MermaidGraphDirection.BottomToTop:
                    return "BT";
                case MermaidGraphDirection.LeftToRight:
                    return "LR";
                case MermaidGraphDirection.RightToLeft:
                    return "RL";
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, $"Unsupported {nameof(MermaidGraphDirection)}: {direction}.");
            }
        }

        private void BuildSanitizedNamedStateMap()
        {
            if (_stateMapInitialized)
            {
                return;
            }

            // Ensures that state names are unique and do not contain characters that would cause an invalid Mermaid graph.
            var uniqueAliases = new HashSet<string>();
            foreach (var state in _graph.States)
            {
                var sanitizedStateName = string.Concat(state.Value.StateName.Where(c => !(char.IsWhiteSpace(c) || c == ':' || c == '-')));
                if (!sanitizedStateName.Equals(state.Value.StateName, StringComparison.Ordinal))
                {
                    int count = 1;
                    var tempName = sanitizedStateName;
                    while (uniqueAliases.Contains(tempName) || _graph.States.ContainsKey(tempName))
                    {
                        tempName = $"{sanitizedStateName}_{count++}";
                    }

                    sanitizedStateName = tempName;
                    uniqueAliases.Add(sanitizedStateName);
                }

                _stateMap[sanitizedStateName] = state.Value;
            }

            _stateMapInitialized = true;
        }

        private string GetSanitizedStateName(string stateName)
        {
            return _stateMap.FirstOrDefault(x => x.Value.StateName == stateName).Key ?? stateName;
        }
    }
}
