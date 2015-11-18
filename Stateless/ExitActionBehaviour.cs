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
            readonly string _description;
            readonly Action<Transition> _action;

            public ExitActionBehavior(Action<Transition> action, string description)
            {
                _action = action;
                _description = description;
            }

            internal string Description { get { return string.IsNullOrEmpty(_description) ? _action.Method.Name : _description; } }
            internal Action<Transition> Action { get { return _action; } }
        }
    }
}
