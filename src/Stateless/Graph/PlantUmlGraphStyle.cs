using Stateless.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless.Graph
{
    /// <summary>
    /// Class to generate a graph in PlantUML format
    /// </summary>
    public class PlantUmlGraphStyle : GraphStyleBase
    {
        private const string Indent = "\t";

        // Words that PlantUML treats as keywords in state diagrams, and which therefore cannot be used as a bare state name.
        private static readonly HashSet<string> PlantUmlReservedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "as",
            "bottom",
            "caption",
            "direction",
            "end",
            "footer",
            "header",
            "hide",
            "left",
            "legend",
            "note",
            "remove",
            "right",
            "scale",
            "show",
            "skinparam",
            "state",
            "title",
            "top"
        };

        private readonly StateGraph _graph;
        private readonly PlantUmlGraphDirection? _direction;
        private readonly Dictionary<string, string> _aliases = new Dictionary<string, string>(StringComparer.Ordinal);
        private bool _aliasesInitialized = false;

        /// <summary>
        /// Create a new instance of <see cref="PlantUmlGraphStyle"/>
        /// </summary>
        /// <param name="graph">The state graph</param>
        /// <param name="direction">When non-null, sets the flow direction in the output.</param>
        public PlantUmlGraphStyle(StateGraph graph, PlantUmlGraphDirection? direction)
            : base()
        {
            _graph = graph;
            _direction = direction;
        }

        /// <summary>Get the text that starts a new graph</summary>
        /// <returns>The <c>@startuml</c> line, followed by the direction setting if one was requested.</returns>
        public override string GetPrefix()
        {
            BuildAliasMap();

            var sb = new StringBuilder("@startuml");
            if (_direction.HasValue)
            {
                sb.AppendLine();
                sb.Append(GetDirectionText(_direction.Value));
            }

            return sb.ToString();
        }

        /// <summary>Get the text that ends the graph</summary>
        /// <returns>The <c>@enduml</c> line.</returns>
        public override string GetSuffix()
        {
            return $"{Environment.NewLine}@enduml";
        }

        /// <summary>
        /// Returns the formatted text for a superstate, its substates (including nested superstates) and their actions.
        /// Only top-level superstates produce output; nested superstates are written inside their parent.
        /// </summary>
        /// <param name="stateInfo">The superstate to generate text for.</param>
        /// <returns>A PlantUML composite state.</returns>
        public override string FormatOneCluster(SuperState stateInfo)
        {
            if (stateInfo.SuperState != null)
                return string.Empty;

            var sb = new StringBuilder();
            AppendState(sb, stateInfo, string.Empty);
            return sb.ToString();
        }

        /// <summary>
        /// Returns the formatted text for a single state which is neither a superstate nor a substate,
        /// including its entry and exit actions.
        /// </summary>
        /// <param name="state">The state to generate text for.</param>
        /// <returns>A PlantUML state declaration.</returns>
        public override string FormatOneState(State state)
        {
            var sb = new StringBuilder();
            AppendState(sb, state, string.Empty);
            return sb.ToString();
        }

        /// <summary>
        /// Generate the text for a single decision node
        /// </summary>
        /// <param name="nodeName">Name of the node</param>
        /// <param name="label">Label for the node</param>
        /// <returns>A PlantUML choice pseudostate.</returns>
        public override string FormatOneDecisionNode(string nodeName, string label)
        {
            return $"{Environment.NewLine}state {nodeName} <<choice>>";
        }

        /// <inheritdoc/>
        public override string GetInitialTransition(StateInfo initialState)
        {
            return $"{Environment.NewLine}[*] --> {GetStateId(initialState.ToString())}";
        }

        /// <inheritdoc/>
        public override string FormatOneTransition(string sourceNodeName, string trigger, IEnumerable<string> actions, string destinationNodeName, IEnumerable<string> guards)
        {
            var label = new StringBuilder(trigger ?? string.Empty);

            if (actions?.Count() > 0)
            {
                label.Append(" / ");
                label.Append(string.Join(", ", actions));
            }

            if (guards != null)
            {
                foreach (var info in guards)
                {
                    if (label.Length > 0)
                        label.Append(" ");

                    label.Append("[" + info + "]");
                }
            }

            var line = $"{GetStateId(sourceNodeName)} --> {GetStateId(destinationNodeName)}";

            return label.Length == 0 ? line : $"{line} : {EscapeText(label.ToString())}";
        }

        private void AppendState(StringBuilder sb, State state, string indent)
        {
            var stateId = GetStateId(state.StateName);

            sb.AppendLine();
            sb.Append(indent);
            sb.Append("state ");
            if (!stateId.Equals(state.StateName, StringComparison.Ordinal))
                sb.Append($"\"{EscapeDisplayName(state.StateName)}\" as ");
            sb.Append(stateId);

            if (state is SuperState superState)
            {
                sb.Append(" {");
                foreach (var subState in superState.SubStates)
                {
                    AppendState(sb, subState, indent + Indent);
                }

                sb.AppendLine();
                sb.Append(indent);
                sb.Append("}");
            }

            foreach (var action in state.EntryActions)
            {
                sb.AppendLine();
                sb.Append($"{indent}{stateId} : entry / {EscapeText(action)}");
            }

            foreach (var action in state.ExitActions)
            {
                sb.AppendLine();
                sb.Append($"{indent}{stateId} : exit / {EscapeText(action)}");
            }
        }

        private static string GetDirectionText(PlantUmlGraphDirection direction)
        {
            switch (direction)
            {
                case PlantUmlGraphDirection.TopToBottom:
                    return "top to bottom direction";
                case PlantUmlGraphDirection.LeftToRight:
                    return "left to right direction";
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, $"Unsupported {nameof(PlantUmlGraphDirection)}: {direction}.");
            }
        }

        // PlantUML labels run to the end of the line, so line breaks are written as PlantUML's \n token.
        // A backslash starts an escape sequence in PlantUML text (\n, \t, ...) and doubling it does not prevent
        // \t from being read as a tab, so literal backslashes are written as the Unicode code <U+005C>.
        private static string EscapeText(string text)
        {
            var escaped = new StringBuilder(text.Length);
            for (var index = 0; index < text.Length; index++)
            {
                switch (text[index])
                {
                    case '\r':
                        if (index + 1 < text.Length && text[index + 1] == '\n')
                        {
                            index++;
                        }

                        escaped.Append("\\n");
                        break;
                    case '\n':
                        escaped.Append("\\n");
                        break;
                    case '\\':
                        escaped.Append("<U+005C>");
                        break;
                    default:
                        escaped.Append(text[index]);
                        break;
                }
            }

            return escaped.ToString();
        }

        // A display name is written between double quotes, so literal double quotes are written as the Unicode code <U+0022>.
        private static string EscapeDisplayName(string name)
        {
            return EscapeText(name).Replace("\"", "<U+0022>");
        }

        private static bool IsValidIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name) || char.IsDigit(name[0]))
                return false;

            return name.All(IsIdentifierChar) && !PlantUmlReservedNames.Contains(name);
        }

        private static bool IsIdentifierChar(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_';
        }

        // Ensures that every state has an identifier that PlantUML accepts and that identifiers are unique.
        // States whose names are not valid identifiers are given an alias and declared with their name as display text.
        private void BuildAliasMap()
        {
            if (_aliasesInitialized)
                return;

            var usedIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var state in _graph.States.Values)
            {
                if (IsValidIdentifier(state.StateName))
                    usedIds.Add(state.StateName);
            }

            foreach (var decision in _graph.Decisions)
            {
                usedIds.Add(decision.NodeName);
            }

            foreach (var state in _graph.States.Values)
            {
                var name = state.StateName;
                if (IsValidIdentifier(name))
                {
                    _aliases[name] = name;
                    continue;
                }

                var aliasRoot = string.Concat(name.Where(IsIdentifierChar));
                if (aliasRoot.Length == 0)
                    aliasRoot = "StateAlias";
                else if (char.IsDigit(aliasRoot[0]))
                    aliasRoot = "_" + aliasRoot;

                var alias = aliasRoot;
                var count = 1;
                while (usedIds.Contains(alias) || PlantUmlReservedNames.Contains(alias))
                {
                    alias = $"{aliasRoot}_{count++}";
                }

                usedIds.Add(alias);
                _aliases[name] = alias;
            }

            _aliasesInitialized = true;
        }

        private string GetStateId(string stateName)
        {
            BuildAliasMap();

            return stateName != null && _aliases.TryGetValue(stateName, out var alias) ? alias : stateName;
        }
    }
}
