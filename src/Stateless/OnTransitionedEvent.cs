namespace Stateless; 

public partial class StateMachine<TState, TTrigger>
{
    private sealed class OnTransitionedEvent
    {
        private readonly ICollection<EventCallback<Transition>> _callbacks;

        public OnTransitionedEvent()
        {
            _callbacks = new List<EventCallback<Transition>>();
        }

        public async Task InvokeAsync(Transition transition)
        {
            foreach (var callback in _callbacks)
                await callback.InvokeAsync(transition).ConfigureAwait(false);
        }

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