using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        class OnTransitionedEvent
        {
            private readonly ICollection<EventCallback<Transition>> _callbacks;

            public OnTransitionedEvent()
            {
                _callbacks = new List<EventCallback<Transition>>();
            }

            public void Invoke(Transition transition)
            {
                foreach (var callback in _callbacks)
                    callback.InvokeAsync(transition).GetAwaiter().GetResult();
            }

#if TASKS
            public async Task InvokeAsync(Transition transition)
            {
                foreach (var callback in _callbacks)
                    await callback.InvokeAsync(transition).ConfigureAwait(false);
            }
#endif

            public void Register(Action<Transition> action)
            {
                _callbacks.Add(EventCallbackFactory.Create(action));
            }

            public void Register(Func<Transition, Task> action)
            {
                _callbacks.Add(EventCallbackFactory.Create(action));
            }
        }
    }
}
