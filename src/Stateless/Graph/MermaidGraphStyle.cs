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
        // Characters that Mermaid treats as syntax (separators, comments, entity codes, quoting, brackets and markup)
        // and which therefore cannot appear in a state alias.
        private const string AliasExcludedCharacters = ":-%#;\"{}[]<>&";
        private static readonly HashSet<string> MermaidReservedAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "accDescr",
            "accTitle",
            "class",
            "classDef",
            "click",
            "default",
            "href",
            "note",
            "scale",
            "state",
            "stateDiagram",
            "style"
        };

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

            StringBuilder sb = new StringBuilder("stateDiagram-v2");
            if (_direction.HasValue)
            {
                sb.AppendLine();
                sb.Append($"\tdirection {GetDirectionCode(_direction.Value)}");
            }

            foreach (var state in _stateMap.Where(x => !x.Key.Equals(x.Value.StateName, StringComparison.Ordinal)))
            {
                sb.AppendLine();
                sb.Append($"\t{state.Key} : {EscapeLabel(state.Value.StateName)}");
            }

            return sb.ToString();
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

            var sanitizedSourceNodeName = GetSanitizedStateName(sourceNodeName);
            var sanitizedDestinationNodeName = GetSanitizedStateName(destinationNodeName);

            return FormatOneLine(sanitizedSourceNodeName, sanitizedDestinationNodeName, sb.ToString());
        }

        internal string FormatOneLine(string fromNodeName, string toNodeName, string label)
        {
            return $"\t{fromNodeName} --> {toNodeName} : {EscapeLabel(label)}";
        }

        // Mermaid decodes #name; and #123; sequences in labels as entity codes, so literal '#' characters are encoded.
        // Each input character is handled once so that the entity codes emitted here are not themselves re-encoded.
        // Mermaid passes label text through an HTML sanitiser, so '&', '<' and '>' become entity codes. Line breaks
        // are written directly as <br/>, Mermaid's line break token, so that the token itself is kept.
        // A line break would otherwise end the label and let the rest start a new statement.
        // Colons and semicolons delimit Mermaid syntax, and %% starts a comment, so these are written as entity codes.
        private static string EscapeLabel(string label)
        {
            var escaped = new StringBuilder(label.Length);
            for (var index = 0; index < label.Length; index++)
            {
                switch (label[index])
                {
                    case '\r':
                        if (index + 1 < label.Length && label[index + 1] == '\n')
                        {
                            index++;
                        }

                        escaped.Append("<br/>");
                        break;
                    case '\n':
                        escaped.Append("<br/>");
                        break;
                    case '#':
                        escaped.Append("#35;");
                        break;
                    case '&':
                        escaped.Append("#amp;");
                        break;
                    case '<':
                        escaped.Append("#lt;");
                        break;
                    case '>':
                        escaped.Append("#gt;");
                        break;
                    case ':':
                        escaped.Append("#58;");
                        break;
                    case '%':
                        escaped.Append("#37;");
                        break;
                    case ';':
                        escaped.Append("#59;");
                        break;
                    default:
                        escaped.Append(label[index]);
                        break;
                }
            }

            return escaped.ToString();
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
                var sanitizedStateName = string.Concat(state.Value.StateName.Where(c =>
                    !char.IsWhiteSpace(c) && AliasExcludedCharacters.IndexOf(c) < 0));
                if (sanitizedStateName.Length == 0 ||
                    !sanitizedStateName.Equals(state.Value.StateName, StringComparison.Ordinal) ||
                    MermaidReservedAliases.Contains(sanitizedStateName))
                {
                    int count = 1;
                    var aliasRoot = sanitizedStateName.Length == 0 ? "StateAlias" : sanitizedStateName;
                    var tempName = aliasRoot;
                    while (MermaidReservedAliases.Contains(tempName) ||
                           uniqueAliases.Contains(tempName) ||
                           _graph.States.ContainsKey(tempName))
                    {
                        tempName = $"{aliasRoot}_{count++}";
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
