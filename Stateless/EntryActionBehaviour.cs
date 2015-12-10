using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class EntryActionBehavior
        {
            readonly string _actionDescription;
            readonly Action<Transition, object[]> _action;

            public EntryActionBehavior(Action<Transition, object[]> action, string actionDescription)
            {
                _action = action;
                _actionDescription = Enforce.ArgumentNotNull(actionDescription, nameof(actionDescription));
            }

            internal string ActionDescription { get { return _actionDescription; } }
            internal Action<Transition, object[]> Action  { get { return _action; } }
        }
    }
}
