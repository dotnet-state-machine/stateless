using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateless; 

public partial class StateMachine<TState, TTrigger>
{
    private class OnTransitionedEvent
    {
        private event Action<Transition>?             OnTransitioned;
        private readonly List<Func<Transition, Task>> _onTransitionedAsync = new();

        public void Invoke(Transition transition)
        {
            if (_onTransitionedAsync.Count != 0)
                throw new InvalidOperationException(
                                                    "Cannot execute asynchronous action specified as OnTransitioned callback. " +
                                                    "Use asynchronous version of Fire [FireAsync]");

            OnTransitioned?.Invoke(transition);
        }

#if TASKS
        public async Task InvokeAsync(Transition transition)
        {
            OnTransitioned?.Invoke(transition);

            foreach (var callback in _onTransitionedAsync)
                await callback(transition).ConfigureAwait(false);
        }
#endif

        public void Register(Action<Transition> action)
        {
            OnTransitioned += action;
        }

        public void Register(Func<Transition, Task> action)
        {
            _onTransitionedAsync.Add(action);
        }
    }
}