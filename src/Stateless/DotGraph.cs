using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        /// <summary>
        /// A string representation of the state machine in the DOT graph language.
        /// </summary>
        /// <returns>A description of all simple source states, triggers and destination states.</returns>
        public string ToDotGraph()
        {
            List<string> lines = new List<string>();
            List<string> unknownDestinations = new List<string>();

            foreach (var stateCfg in _stateConfiguration) 
            {
                TState source = stateCfg.Key;
                foreach (var behaviours in stateCfg.Value.TriggerBehaviours) 
                {
                    foreach (TriggerBehaviour behaviour in behaviours.Value) 
                    {
                        string destination;

                        if (behaviour is TransitioningTriggerBehaviour)
                        {
                            destination = ((TransitioningTriggerBehaviour)behaviour).Destination.ToString ();
                        }
                        else if (behaviour is IgnoredTriggerBehaviour)
                        {
                            continue; 
                        }
                        else if (behaviour is InternalTriggerBehaviour)
                        {
                            // Internal transitions are for the moment displayed (re-entrant) self-transitions, even though no exit or entry transition occurs
                            destination = stateCfg.Key.ToString();
                        }
                        else 
                        {
                            destination = "unknownDestination_" + unknownDestinations.Count;
                            unknownDestinations.Add(destination);
                        }

                        string line = (behaviour.Guard.TryGetMethodInfo().DeclaringType.Namespace.Equals("Stateless")) ?
                            string.Format(" {0} -> {1} [label=\"{2}\"];", source, destination, behaviour.Trigger) :
                            string.Format(" {0} -> {1} [label=\"{2} [{3}]\"];", source, destination, behaviour.Trigger, behaviour.GuardDescription);

                        lines.Add(line);
                    }
                }
            }

            if (unknownDestinations.Any())
            {
                string label = string.Format(" {{ node [label=\"?\"] {0} }};", string.Join(" ", unknownDestinations));
                lines.Insert(0, label);
            }

            if (_stateConfiguration.Any(s => s.Value.EntryActions.Any() || s.Value.ExitActions.Any()))
            {
                lines.Add("node [shape=box];");

                foreach (var stateCfg in _stateConfiguration)
                {
                    TState source = stateCfg.Key;

                    foreach (var entryActionBehaviour in stateCfg.Value.EntryActions)
                    {
                        string line = string.Format(" {0} -> \"{1}\" [label=\"On Entry\" style=dotted];", source, entryActionBehaviour.ActionDescription);
                        lines.Add(line);
                    }

                    foreach (var exitActionBehaviour in stateCfg.Value.ExitActions)
                    {
                        string line = string.Format(" {0} -> \"{1}\" [label=\"On Exit\" style=dotted];", source, exitActionBehaviour.ActionDescription);
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
