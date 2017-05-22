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

        Dictionary<string, string> _clusters = new Dictionary<string, string>();
        List<string> _handledStates = new List<string>();
        List<string> _formattedNodes = new List<string>();
        FormatList _clusterFormat;
        FormatList _exitEdges;
        int _numDecisionNodes = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        DotGraphFormatter(IDotGraphStyle style)
        {
            _style = style;

            _clusterFormat = new FormatList()
                .Add(new Label("placeholder"));
            _exitEdges = new FormatList()
                .Add(new Label("placeholder"))
                .Add(new Style(ShapeStyle.dotted));
        }

        /// <summary>
        /// Generates a string representation of the stateRepresentation machine in the DOT graph language.
        /// </summary>
        /// <returns>A description of all simple source states, triggers and destination states.</returns>
        public static string Format(StateMachineInfo machineInfo, IDotGraphStyle style)
        {
            DotGraphFormatter fmtr = new DotGraphFormatter(style);

            return fmtr.Format(machineInfo);
        }

        string Format(StateMachineInfo machineInfo)
        { 
            FillClusterNames(machineInfo);

            string dirgraphText = _style.GetPrefix().Replace("\n", System.Environment.NewLine);

            // Start with the clusters
            foreach (var stateInfo in machineInfo.States.Where(v => v.Substates.Count() > 0))
            {
                dirgraphText += BuildNodesRepresentation(stateInfo);
            }

            // Next process all non-cluster elements
            foreach (var stateRep in machineInfo.States.Where(v => v.Superstate == null && v.Substates.Count() == 0))
            {
                dirgraphText += BuildNodesRepresentation(stateRep);
            }
            // now build behaviours
            foreach (var stateInfo in machineInfo.States)
            {
                var behaviours = ProcessTriggerBehaviour(stateInfo).ToArray();
                if (behaviours.Length > 0)
                    dirgraphText += $"{System.Environment.NewLine}{string.Join(System.Environment.NewLine, behaviours)} ";
            }

            dirgraphText += System.Environment.NewLine + "}";

            return dirgraphText;
        }


        List<string> ProcessEntries(StateInfo stateInfo)
        {
            List<string> lines = new List<string>();

            foreach (var entryAction in stateInfo.EntryActions)
                lines.Add(entryAction.Description);

            return lines;
        }

        List<string> ProcessExits(StateInfo stateInfo)
        {
            List<string> lines = new List<string>();

            foreach (var exitAction in stateInfo.ExitActions)
                lines.Add(exitAction.Description);

            return lines;
        }

        void FillClusterNames(StateMachineInfo machineInfo)
        {
            int i = 0;

            foreach (var stateInfo in machineInfo.States.Where(sc => sc.Substates?.Count() > 0))
            {
                var clusterName = $"cluster{i++}";
                _clusters.Add(stateInfo.UnderlyingState.ToString(), clusterName);
            }
        }
        /// <summary>
        /// This function builds a representation for nodes and clusters, grouping states together under
        /// a super state as required.  It is implemented as a recursive that drills through the 
        /// the sub states when they are found.
        /// </summary>
        /// <param name="stateInfo"></param>
        /// <returns></returns>
        string BuildNodesRepresentation(StateInfo stateInfo)
        {
            string stateRepresentationString = "";
            var sourceName = stateInfo.UnderlyingState.ToString();

            if (_handledStates.Any(hs => hs == sourceName))
                return string.Empty;

            if (stateInfo.Substates?.Count() > 0)
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

        string StateRepresentationString(StateInfo stateInfo, string sourceName)
        {
            return _style.FormatOneState(sourceName, ProcessEntries(stateInfo), ProcessExits(stateInfo)).Replace("\n", System.Environment.NewLine);
        }

        List<string> ProcessTriggerBehaviour(StateInfo stateInfo, string sourceName = "")
        {

            List<string> lines = new List<string>();
            List<string> unknownDestinations = new List<string>();

            var source = string.IsNullOrEmpty(sourceName) ? stateInfo.UnderlyingState.ToString() : sourceName;

            foreach (var t in stateInfo.FixedTransitions)
                lines.Add(ProcessTransition(source, t));

            foreach (var t in stateInfo.DynamicTransitions)
                lines.Add(ProcessTransition(source, t));

            return lines;
        }

        string ProcessTransition(string source, FixedTransitionInfo t)
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
                foreach (var info in t.GuardConditionsMethodDescriptions)
                {
                    first = info;
                    break;
                }
                label = t.Trigger + " " + first.Description;
            }

            return _style.FormatOneLine(source, destinationString, label);
        }

        // TODO: How do we want to handle dynamic transitions?
        string ProcessTransition(string source, DynamicTransitionInfo t)
        {
            string label;
            string nodeName = $"Decision{++_numDecisionNodes}";

            string node = _style.FormatOneDecisionNode(nodeName, t.DestinationStateSelectorDescription.Description)
                .Replace("\n", System.Environment.NewLine);

            // ;

            if (t.GuardConditionsMethodDescriptions.Count() == 0)
            {
                label = t.Trigger.ToString();
            }
            else
            {
                InvocationInfo first = null;
                foreach (var info in t.GuardConditionsMethodDescriptions)
                {
                    first = info;
                    break;
                }
                label = t.Trigger + " " + first.Description;
            }

            string s = node + System.Environment.NewLine
                + _style.FormatOneLine(source, nodeName, label);

            if (t.PossibleDestinationStates != null)
            {
                foreach (string possibleState in t.PossibleDestinationStates)
                {
                    s += System.Environment.NewLine + _style.FormatOneLine(nodeName, possibleState, null);
                }
            }

            return s;
        }

        string ResolveEntityName(string entityName) => _clusters.All(cd => cd.Key != entityName)
            ? entityName
            : _clusters.First(cd => cd.Key == entityName).Value;


    }
}
