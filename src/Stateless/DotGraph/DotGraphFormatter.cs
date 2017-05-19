using System.Collections.Generic;
using System.Linq;
using Stateless.Reflection;

namespace Stateless.DotGraph
{
    /// <summary>
    /// DOT GraphViz text writer for reflection API.
    /// </summary>
    public class DotGraphFormatter
    {
        IDotGraphStyle _style;

        Dictionary<string, string> clusterDictionary;
        List<string> _handledStates;
        List<string> _formattedNodesList;
        FormatList _clusterFormat;
        FormatList _edgeShape;
        FormatList _exitEdges;

        /// <summary>
        /// Constructor
        /// </summary>
        public DotGraphFormatter(IDotGraphStyle style)
        {
            _style = style;

            _clusterFormat = new FormatList()
                .Add(new Label("placeholder"));
            _edgeShape = new FormatList()
                .Add(new Label("placeholder"))
                .Add(new Style(ShapeStyle.solid));
            _exitEdges = new FormatList()
                .Add(new Label("placeholder"))
                .Add(new Style(ShapeStyle.dotted));
        }

        // TODO: Is this the way we want to create DOT graphs?  new DotGraphFormatter().ToDotGraph(sm)?

        /// <summary>
        /// A string representation of the stateRepresentation machine in the DOT graph language.
        /// </summary>
        /// <returns>A description of all simple source states, triggers and destination states.</returns>
        public string ToDotGraph(StateMachineInfo machineInfo)
        {
            clusterDictionary = new Dictionary<string, string>();
            _handledStates = new List<string>();
            _formattedNodesList = new List<string>();

            FillClusterNames(machineInfo);

            string dirgraphText = _style.GetPrefix().Replace("\n", System.Environment.NewLine);

            // Start with the clusters
            foreach (StateInfo stateInfo in machineInfo.States.Where(v => v.Substates.Count() > 0))
            {
                dirgraphText += BuildNodesRepresentation(stateInfo);
            }

            // Next process all non-cluster elements
            foreach (var stateRep in machineInfo.States.Where(v => v.Superstate == null && v.Substates.Count() == 0))
            {
                dirgraphText += BuildNodesRepresentation(stateRep);
            }
            // now build behaviours
            foreach (StateInfo stateInfo in machineInfo.States)
            {
                var behaviours = ProcessTriggerBehaviour(stateInfo).ToArray();
                if (behaviours.Length > 0)
                    dirgraphText += $"{System.Environment.NewLine}{string.Join(System.Environment.NewLine, behaviours)} ";
            }

            dirgraphText += $"{System.Environment.NewLine}}}";
            return dirgraphText;

        }


        private List<string> ProcessEntries(StateInfo stateInfo)
        {
            List<string> lines = new List<string>();
            foreach (InvocationInfo entryAction in stateInfo.EntryActions)
                lines.Add(entryAction.Description);
            return lines;
        }
        private List<string> ProcessExits(StateInfo stateInfo)
        {
            List<string> lines = new List<string>();
            foreach (InvocationInfo exitAction in stateInfo.ExitActions)
                lines.Add(exitAction.Description);
            return lines;
        }

        private void FillClusterNames(StateMachineInfo machineInfo)
        {
            int i = 0;

            foreach (StateInfo stateInfo in machineInfo.States.Where(sc => sc.Substates != null && sc.Substates.Count() > 0))
            {
                var clusterName = $"cluster{i++}";
                clusterDictionary.Add(stateInfo.UnderlyingState.ToString(), clusterName);
            }
        }
        /// <summary>
        /// This function builds a representation for nodes and clusters, grouping states together under
        /// a super state as required.  It is implemented as a recursive that drills through the 
        /// the sub states when they are found.
        /// </summary>
        /// <param name="stateInfo"></param>
        /// <returns></returns>
        private string BuildNodesRepresentation(StateInfo stateInfo)
        {
            string stateRepresentationString = "";
            var sourceName = stateInfo.UnderlyingState.ToString();

            if (_handledStates.Any(hs => hs == sourceName))
                return string.Empty;

            if (stateInfo.Substates != null && stateInfo.Substates.Count() > 0)
            {
                stateRepresentationString = System.Environment.NewLine
                    + $"subgraph {ResolveEntityName(sourceName)}" + System.Environment.NewLine
                    + "\t{" + System.Environment.NewLine
                    + $"\tlabel = \"{sourceName}\"" + System.Environment.NewLine;

                // The parent node needs to be added to the cluster so that we can draw edges on the cluster borders
                stateRepresentationString += StateRepresentationString(stateInfo, sourceName);

                stateRepresentationString = stateInfo.Substates.Aggregate(stateRepresentationString,
                    (current, subStateRepresentation) => current + BuildNodesRepresentation(subStateRepresentation));

                stateRepresentationString += $"}}{System.Environment.NewLine}";
            }
            else
            {
                // Single node representation
                stateRepresentationString = StateRepresentationString(stateInfo, sourceName);
            }
            _handledStates.Add(sourceName);

            return stateRepresentationString;
        }

        private string StateRepresentationString(StateInfo stateInfo, string sourceName)
        {
            return _style.FormatOneState(sourceName, ProcessEntries(stateInfo), ProcessExits(stateInfo)).Replace("\n", System.Environment.NewLine);
        }

        private List<string> ProcessTriggerBehaviour(StateInfo stateInfo, string sourceName = "")
        {

            List<string> lines = new List<string>();
            List<string> unknownDestinations = new List<string>();

            var source = string.IsNullOrEmpty(sourceName) ? stateInfo.UnderlyingState.ToString() : sourceName;

            foreach (FixedTransitionInfo t in stateInfo.FixedTransitions)
                lines.Add(ProcessTransition(source, t));

            foreach (DynamicTransitionInfo t in stateInfo.DynamicTransitions)
                lines.Add(ProcessTransition(source, t));

            return lines;
        }

        private string ProcessTransition(string source, FixedTransitionInfo t)
        {
            string label;
            string destinationString = t.DestinationState.ToString();
            if (t.GuardConditionsMethodDescriptions.Count() == 0)
            {
                label = t.Trigger.ToString();
            }
            else
            {
                InvocationInfo first = null;
                foreach (InvocationInfo info in t.GuardConditionsMethodDescriptions)
                {
                    first = info;
                    break;
                }
                label = t.Trigger + " " + first.Description;
            }
            FormatList head = _edgeShape
                .SetLHead(destinationString, ResolveEntityName(destinationString))
                .SetLTail(source, ResolveEntityName(source))
                .Add(new Label(label));
            return source + " -> " + destinationString + " " + head;
        }

        // TODO: How do we want to handle dynamic transitions?
        private string ProcessTransition(string source, DynamicTransitionInfo t)
        {
            string label;

            if (t.GuardConditionsMethodDescriptions.Count() == 0)
            {
                label = t.Trigger.ToString();
            }
            else
            {
                InvocationInfo first = null;
                foreach (InvocationInfo info in t.GuardConditionsMethodDescriptions)
                {
                    first = info;
                    break;
                }
                label = t.Trigger + " " + first.Description;
            }

            FormatList head = _edgeShape
                .SetLHead("Dynamic", ResolveEntityName("Dynamic"))
                .SetLTail(source, ResolveEntityName(source)).Add(new Label(label));
            return source + " -> " + "Dynamic" + " " + head;
        }

        private string ResolveEntityName(string entityName) => clusterDictionary.All(cd => cd.Key != entityName)
            ? entityName
            : clusterDictionary.First(cd => cd.Key == entityName).Value;


    }
}
