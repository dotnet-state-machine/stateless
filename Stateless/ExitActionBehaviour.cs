using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class ExitActionBehavior
        {
            readonly string _actionDescription;
            readonly Action<Transition> _action;

            public ExitActionBehavior(Action<Transition> action, string actionDescription)
            {
                _action = action;
                _actionDescription = actionDescription;
            }

            internal string ActionDescription { get { return string.IsNullOrEmpty(_actionDescription) ? _action.Method.Name : _actionDescription; } }
            internal Action<Transition> Action { get { return _action; } }
        }
    }
}
