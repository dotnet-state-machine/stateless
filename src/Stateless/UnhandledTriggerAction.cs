using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class UnhandledTriggerAction
        {
            private readonly EventCallback<TState, TTrigger, ICollection<string>> _callback;

            public UnhandledTriggerAction(EventCallback<TState, TTrigger, ICollection<string>> action = null)
            {
                _callback = action;
            }

            public virtual Task Execute(TState state, TTrigger trigger, ICollection<string> unmetGuards)
            {
                return _callback.InvokeAsync(state, trigger, unmetGuards);
            }
        }
    }
}
