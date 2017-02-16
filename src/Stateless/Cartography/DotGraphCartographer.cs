using Stateless.Cartography;
using System.Collections.Generic;
using System.Linq;

namespace Stateless.Core.Cartography
{
    /// <summary>
    /// DOT GraphViz text writer for cartographer API.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TTrigger"></typeparam>
    public class DotGraphCartographer<TState, TTrigger> : ICartographer<TState, TTrigger>
    {
        /// <summary>
        /// Produces a DOT GraphViz graph.
        /// </summary>
        /// <param name="stateMachineInfo">The StateMachineInfo to be mapped.</param>
        /// <returns>DOT GraphViz text.</returns>
        public string WriteMap(StateMachineInfo<TState, TTrigger> stateMachineInfo)
        {
            var resources = stateMachineInfo.StateResources;

            List<string> lines = new List<string>();
            List<string> unknownDestinations = new List<string>();

            foreach (var stateCfg in resources)
            {
                unknownDestinations.AddRange(stateCfg.DynamicTransitions.Select(t => t.Value.Destination));

                var source = stateCfg.UnderlyingState.ToString();
                foreach (var behaviours in stateCfg.Transitions.Concat(stateCfg.InternalTransitions))
                {
                    HandleTransitions(ref lines, source, behaviours.Key.ToString(), behaviours.Value.DestinationState.ToString(), behaviours.Value.GuardDescription);
                }

                foreach (var behaviours in stateCfg.DynamicTransitions)
                {
                    HandleTransitions(ref lines, source, behaviours.Key.ToString(), behaviours.Value.Destination, behaviours.Value.GuardDescription);
                }
            }

            if (unknownDestinations.Any())
            {
                string label = string.Format(" {{ node [label=\"?\"] {0} }};", string.Join(" ", unknownDestinations));
                lines.Insert(0, label);
            }

            if (resources.Any(s => s.EntryActions.Any() || s.ExitActions.Any()))
            {
                lines.Add("node [shape=box];");

                foreach (var stateCfg in resources)
                {
                    var source = stateCfg.UnderlyingState.ToString();

                    foreach (var entryActionBehaviour in stateCfg.EntryActions)
                    {
                        string line = string.Format(" {0} -> \"{1}\" [label=\"On Entry\" style=dotted];", source, entryActionBehaviour);
                        lines.Add(line);
                    }

                    foreach (var exitActionBehaviour in stateCfg.ExitActions)
                    {
                        string line = string.Format(" {0} -> \"{1}\" [label=\"On Exit\" style=dotted];", source, exitActionBehaviour);
                        lines.Add(line);
                    }
                }
            }

            return "digraph {" + System.Environment.NewLine +
                     string.Join(System.Environment.NewLine, lines) + System.Environment.NewLine +
                   "}";
        }

        private static void HandleTransitions(ref List<string> lines, string sourceState, string trigger, string destination, string guardDescription)
        {
            string line = string.IsNullOrWhiteSpace(guardDescription) ?
                string.Format(" {0} -> {1} [label=\"{2}\"];", sourceState, destination, trigger) :
                string.Format(" {0} -> {1} [label=\"{2} [{3}]\"];", sourceState, destination, trigger, guardDescription);

            lines.Add(line);
        }
    }
}
