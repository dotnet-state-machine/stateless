using Stateless.Cartography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        /// <param name="stateMachine">The StateMachine to be mapped.</param>
        /// <returns>DOT GraphViz text.</returns>
        public string WriteMap(StateMachine<TState, TTrigger> stateMachine)
        {
            var resources = stateMachine.Explore();

            List<string> lines = new List<string>();
            List<string> unknownDestinations = new List<string>();

            foreach (var stateCfg in resources)
            {
                unknownDestinations.AddRange(stateCfg.DynamicTransitions.Select(t => t.Value.DestinationStateText));

                var source = stateCfg.StateText;
                foreach (var behaviours in stateCfg.Transitions.Concat(stateCfg.InternalTransitions).Concat(stateCfg.DynamicTransitions))
                {
                    var behaviour = behaviours.Value;
                    string destination;

                    destination = behaviour.DestinationStateText;

                    string line = string.IsNullOrWhiteSpace(behaviour.GuardDescription) ?
                        string.Format(" {0} -> {1} [label=\"{2}\"];", source, destination, behaviours.Key) :
                        string.Format(" {0} -> {1} [label=\"{2} [{3}]\"];", source, destination, behaviours.Key, behaviour.GuardDescription);

                    lines.Add(line);
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
                    var source = stateCfg.StateText;

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

    }
}
