﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class UnhandledTriggerAction
        {
            private readonly EventCallback<TState, TTrigger, ICollection<string>> _callback;

            public UnhandledTriggerAction(Action<TState, TTrigger, ICollection<string>> action = null)
            {
                _callback = EventCallbackFactory.Create(action);
            }

            public UnhandledTriggerAction(Func<TState, TTrigger, ICollection<string>, Task> action = null)
            {
                _callback = EventCallbackFactory.Create(action);
            }

            public virtual Task ExecuteAsync(TState state, TTrigger trigger, ICollection<string> unmetGuards)
            {
                return _callback.InvokeAsync(state, trigger, unmetGuards);
            }
        }
    }
}
