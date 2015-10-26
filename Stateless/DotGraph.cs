using System.Collections.Generic;
using System.Linq;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        /// <summary>
        /// A string representation of the state machine in the DOT graph language.
        /// </summary>
        /// <returns>A description of all simple source states, triggers and destination states except those defined with PermitDynamic.</returns>
        public string ToDotGraph()
        {
            List<string> transitions = new List<string>();

            foreach (var stateCfg in _stateConfiguration) {
                TState source = stateCfg.Key;
                foreach (var behaviours in stateCfg.Value._triggerBehaviours) {
                    foreach (TransitioningTriggerBehaviour behaviour in behaviours.Value.Where(b => b is TransitioningTriggerBehaviour)) {
                        string line = (behaviour._guard.Method.DeclaringType.Namespace.Equals("Stateless")) ?
                            string.Format(" {0} -> {1} [label=\"{2}\"];", source, behaviour._destination, behaviour._trigger) :
                            string.Format(" {0} -> {1} [label=\"{2} [{3}]\"];", source, behaviour._destination, behaviour._trigger, behaviour._guard.Method.Name);

                        transitions.Add(line);
                    }
                }
            }

            return "digraph {" + System.Environment.NewLine +
                     string.Join(System.Environment.NewLine, transitions) + System.Environment.NewLine +
                   "}";
        }
    }
}
