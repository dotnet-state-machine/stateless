namespace Stateless; 

public partial class StateMachine<TState, TTrigger>
{
    internal sealed class UnhandledTriggerAction
    {
        private readonly EventCallback<TState, TTrigger, ICollection<string>?> _callback;

        public UnhandledTriggerAction(Action<TState, TTrigger, ICollection<string>?> action) {
            _callback = EventCallbackFactory.Create(action);
        }

        public UnhandledTriggerAction(Func<TState, TTrigger, ICollection<string>?, Task> action)
        {
            _callback = EventCallbackFactory.Create(action);
        }

        public Task ExecuteAsync(TState state, TTrigger trigger, ICollection<string>? unmetGuards)
        {
            return _callback.InvokeAsync(state, trigger, unmetGuards);
        }
    }
}