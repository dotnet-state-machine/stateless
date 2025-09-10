using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class OnTransitionedEvent
        {
            event Action<Transition> _onTransitioned;
            readonly List<Func<Transition, Task>> _onTransitionedAsync = new List<Func<Transition, Task>>();

            public void Invoke(Transition transition)
            {
                if (_onTransitionedAsync.Count != 0)
                    throw new InvalidOperationException(
                        "Cannot execute asynchronous action specified as OnTransitioned callback. " +
                        "Use asynchronous version of Fire [FireAsync]");

                _onTransitioned?.Invoke(transition);
            }

            public async Task InvokeAsync(Transition transition, bool retainSynchronizationContext)
            {
                _onTransitioned?.Invoke(transition);

                foreach (var callback in _onTransitionedAsync)
                    await callback(transition).ConfigureAwait(retainSynchronizationContext);
            }

            public void Register(Action<Transition> action)
            {
                _onTransitioned -= action;
                _onTransitioned += action;
            }

            public void Register(Func<Transition, Task> action)
            {
                _onTransitionedAsync.Remove(action);
                _onTransitionedAsync.Add(action);
            }

            public void Unregister(Action<Transition> action)
            {
                if (_onTransitioned != null)
                {
                    _onTransitioned -= action;
                }
            }

            public void Unregister(Func<Transition, Task> action)
            {
                _onTransitionedAsync.Remove(action);
            }

            public void UnregisterAll()
            {
                if (_onTransitioned != null)
                {
                    foreach (Delegate eventHandler in _onTransitioned.GetInvocationList())
                    {
                        _onTransitioned -= (Action<Transition>)eventHandler;
                    }
                }

                _onTransitionedAsync.Clear();
            }
        }
    }
}
