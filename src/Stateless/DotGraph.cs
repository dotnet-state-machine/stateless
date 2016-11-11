using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Stateless.Dot;

namespace Stateless
{

    public partial class StateMachine<TState, TTrigger>
    {

        private Dictionary<string, string> clusterDictionary;
        private List<string> _handledStates;
        private List<string> _formattedNodesList;
        private FormatList _nodeShapeState;
        private FormatList _clusterFormat;
        private FormatList _edgeShape;
        private FormatList _exitEdges;
        private Rank _ranks;

        private void BuildFormatters()
        {
            _nodeShapeState = new FormatList()
                .Add(new Shape(ShapeType.Mrecord))
                .Add(new Color(ShapeColor.blue));
            _clusterFormat = new FormatList()
                .Add(new Label("placeholder"));
            _edgeShape = new FormatList()
                .Add(new Label("placeholder"))
                .Add(new Style(ShapeStyle.solid));
            _exitEdges = new FormatList()
                .Add(new Label("placeholder"))
                .Add(new Style(ShapeStyle.dotted));
        }

        /// <summary>
        /// A string representation of the stateRepresentation machine in the DOT graph language.
        /// </summary>
        /// <returns>A description of all simple source states, triggers and destination states.</returns>
        public string ToDotGraph()
        {
            clusterDictionary = new Dictionary<string, string>();
            _handledStates  =new List<string>();
            _formattedNodesList = new List<string>();
            _ranks = new Rank();
            BuildFormatters();
            FillClusterNames();

            string dirgraphText = $"digraph {{{System.Environment.NewLine}compound=true;{System.Environment.NewLine}rankdir=\"LR\"{System.Environment.NewLine}";

            // Start with the clusters
            foreach (var stateRep in _stateConfiguration.Values.Where(v => v.GetSubstates().Count > 0))
            {
                dirgraphText += BuildNodesRepresentation(stateRep);
            }

            // Next process all non-cluster elements
            foreach (var stateRep in _stateConfiguration.Values.Where(v => v.Superstate == null && v.GetSubstates().Count == 0))
            {
                dirgraphText += BuildNodesRepresentation(stateRep);
            }
            // now build behaviours
            foreach (var stateRep in _stateConfiguration.Values)
            {
                var behaviours = ProcessTriggerBehaviour(stateRep).ToArray();
                if (behaviours.Length > 0)
                    dirgraphText += $"{System.Environment.NewLine}{string.Join(System.Environment.NewLine, behaviours)} ";
            }
            dirgraphText += _ranks;

            dirgraphText += $"{System.Environment.NewLine}}}";
            return dirgraphText;

        }
 

        private List<string> ProcessEntries(StateRepresentation stateRepresentation)
        {
            if (stateRepresentation.EntryActions.Count > 0)
                return ProcessBehaviors( (ICollection) stateRepresentation.EntryActions);
            return new List<string> ();
        }
        private List<string> ProcessExits(StateRepresentation stateRepresentation)
        {
            if (stateRepresentation.ExitActions.Count > 0)
                return ProcessBehaviors((ICollection) stateRepresentation.ExitActions);
            return new List<string> (); 
        }
        private List<string> ProcessBehaviors(ICollection behaviors)
        {
            List<string> lines = new List<string>();
            if (behaviors.Count > 0)
            {
                foreach (var actionBehavior in behaviors)
                {
                    string actionDescription = "";
                    if (actionBehavior is ExitActionBehavior)
                    {
                        actionDescription = (actionBehavior as ExitActionBehavior).ActionDescription;
                    }
                    else if (actionBehavior is EntryActionBehavior)
                    {
                        actionDescription = (actionBehavior as EntryActionBehavior).ActionDescription;
                    }
                    lines.Add(actionDescription);
                }
            }
            return lines;
        }

        private void FillClusterNames()
        {
            int i = 0;

            foreach (var state in _stateConfiguration.Where(sc=> sc.Value.GetSubstates()!=null && sc.Value.GetSubstates().Count>0))
            {
                var clusterName = $"cluster{i++}";
                clusterDictionary.Add(state.Key.ToString(), clusterName);
                Rank.TryAdd(_ranks.ClustersList, clusterName);
            }
        }
        /// <summary>
        /// This function builds a representation for nodes and clusters, grouping states together under
        /// a super state as required.  It is implemented as a recursive that drills through the 
        /// the sub states when they are found.
        /// </summary>
        /// <param name="stateRepresentation"></param>
        /// <returns></returns>
        private string BuildNodesRepresentation(StateRepresentation stateRepresentation)
        {
            string stateRepresentationString = "";
            var sourceName = stateRepresentation.UnderlyingState.ToString();

            if (_handledStates.Any(hs=>hs == sourceName))
                return string.Empty;

            if (stateRepresentation.GetSubstates() != null && stateRepresentation.GetSubstates().Count > 0)
            {
                stateRepresentationString +=
                    $"{System.Environment.NewLine}subgraph {ResolveEntityName(sourceName)}  {{{System.Environment.NewLine} \tlabel = \"{sourceName}\"  {System.Environment.NewLine}";
                // The parent node needs to be added to the cluster so that we can draw edges on the cluster borders
                stateRepresentationString += StateRepresentationString(stateRepresentation,sourceName);
                stateRepresentationString = stateRepresentation.GetSubstates().Aggregate(stateRepresentationString,
                    (current, subStateRepresentation) => current + BuildNodesRepresentation(subStateRepresentation));
                
                stateRepresentationString += $"}}{System.Environment.NewLine}";
            }
            else
            {
                // Single node representation
                stateRepresentationString = StateRepresentationString(stateRepresentation, sourceName);
            }
            _handledStates.Add(sourceName);

            return stateRepresentationString;
        }

        private string StateRepresentationString(StateRepresentation stateRepresentation, string sourceName)
        {
            string label = $"{System.Environment.NewLine}\t<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >";
            label +=$"{System.Environment.NewLine}\t<tr><td>";
            if (stateRepresentation !=null && ProcessEntries(stateRepresentation).Any())
            {
                label += $"{System.Environment.NewLine}\t\t<TABLE BORDER=\"0\" CELLBORDER=\"1\" CELLSPACING=\"0\" BGCOLOR=\"yellow\">";
                label += string.Join("",ProcessEntries(stateRepresentation).Select(l => $"{System.Environment.NewLine}\t\t<TR><TD><sup>{l}</sup></TD></TR>"));
                label += $"{System.Environment.NewLine}\t\t</TABLE>{System.Environment.NewLine}";
            }

            label += $"{System.Environment.NewLine}\t</td></tr>";
            label += $"{System.Environment.NewLine}\t<TR><TD PORT=\"{sourceName}\">{sourceName}</TD></TR>";
            label +="<tr><td>";
            if (stateRepresentation!=null && ProcessExits(stateRepresentation).Any())
            {
                label += $"{System.Environment.NewLine}\t\t<TABLE BORDER=\"0\" CELLBORDER=\"1\" CELLSPACING=\"0\" BGCOLOR=\"yellow\">";
                label += string.Join("", ProcessExits(stateRepresentation).Select(l => $"{System.Environment.NewLine}\t\t<TR><TD><sup>{l}</sup></TD></TR>"));
                label += $"{System.Environment.NewLine}\t\t</TABLE>{System.Environment.NewLine}";
            }
            label += $"{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t</TABLE>";

            _nodeShapeState = new FormatList()
                .Add(new Label(label).IsHTML())
                .Add(new Shape(ShapeType.plaintext))
                .Add(new Color(ShapeColor.blue))
                ;
            return $"\t{sourceName} {_nodeShapeState}{System.Environment.NewLine}";
        }

        private List<string> ProcessTriggerBehaviour( StateRepresentation stateRepresentation,string sourceName="")
        {
            
            List<string> lines = new List<string>();
            List<string> unknownDestinations = new List<string>();
            var source = string.IsNullOrEmpty(sourceName) ? stateRepresentation.UnderlyingState.ToString() : sourceName;
            foreach (var triggerBehaviours in stateRepresentation.TriggerBehaviours)
            {
                string destinationString = "";
                foreach (TriggerBehaviour behaviour in triggerBehaviours.Value)
                {
                    TState destination = default(TState);

                    var triggerBehaviour = behaviour as TransitioningTriggerBehaviour;
                    if (triggerBehaviour != null)
                    {
                        destination = triggerBehaviour.Destination;
                    }
                    else if (behaviour is IgnoredTriggerBehaviour)
                    {
                        continue;
                    }
                    else if (behaviour is InternalTriggerBehaviour)
                    {
                        // Internal transitions are for the moment displayed (re-entrant) self-transitions, even though no exit or entry transition occurs
                        destination = stateRepresentation.UnderlyingState;
                    }
                    else
                    {
                        var unk = $"Unk{unknownDestinations.Count}_{source}" ;
                        destinationString = unk;
                        unknownDestinations.Add(unk);
                    }
                    destinationString = !string.IsNullOrEmpty(destinationString)
                        ? destinationString
                        : (destination == null ? string.Empty : destination.ToString());
                    
                    string line = (behaviour.Guard.TryGetMethodInfo().DeclaringType.Namespace.Equals("Stateless"))
                        ? $"{source} -> {destinationString} {_edgeShape.SetLHead(destinationString, ResolveEntityName(destinationString)).SetLTail(source, ResolveEntityName(source)).Add(new Label(behaviour.Trigger.ToString()))} "
                        : $"{source} -> {destinationString} {_edgeShape.SetLHead(destinationString, ResolveEntityName(destinationString)).SetLTail(source, ResolveEntityName(source)).Add(new Label($"{behaviour.Trigger} {behaviour.GuardDescription}"))}";

                    lines.Add(line);

                }
                if (unknownDestinations.Any())
                {
                    foreach (var unknownDestination in unknownDestinations)
                    {
                        lines.Insert(unknownDestinations.FindIndex(d=>d == unknownDestination), StateRepresentationString(null, unknownDestination));
                    }                   
                }
            }
            return lines;
        }

        private string ResolveEntityName(string entityName) => clusterDictionary.All(cd => cd.Key != entityName)
            ? entityName
            : clusterDictionary.First(cd => cd.Key == entityName).Value;


    }



}
