using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        class OnTransitionedEvent
        {
            private event Action<Transition> OnTransitioned;
            readonly List<Func<Transition, Task>> _onTransitionedAsync = new List<Func<Transition, Task>>();

            public void Invoke(Transition transition)
            {
                if (_onTransitionedAsync.Count != 0)
                    InvokeAsync(transition).GetAwaiter().GetResult();

                OnTransitioned?.Invoke(transition);
            }

            public async Task InvokeAsync(Transition transition)
            {
                OnTransitioned?.Invoke(transition);

                foreach (var callback in _onTransitionedAsync)
                    await callback(transition).ConfigureAwait(false);
            }

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
}
