namespace Stateless; 

public partial class StateMachine<TState, TTrigger>
{
    internal class UnhandledTriggerAction
    {
        private readonly EventCallback<TState, TTrigger, ICollection<string>?> _callback;

        public UnhandledTriggerAction(Action<TState, TTrigger, ICollection<string>?>? action = null) {
            if (action is null)
                _callback = EventCallbackFactory.Create(new Action<TState, TTrigger, ICollection<string>?>(delegate { }));
            else
                _callback = EventCallbackFactory.Create(action);
        }

        public UnhandledTriggerAction(Func<TState, TTrigger, ICollection<string>?, Task>? action = null)
        {
            if (action is null)
                _callback = EventCallbackFactory.Create(new Func<TState, TTrigger, ICollection<string>?, Task>(delegate { return TaskResult.Done; }));
            else
                _callback = EventCallbackFactory.Create(action);
        }

        public virtual Task ExecuteAsync(TState state, TTrigger trigger, ICollection<string>? unmetGuards)
        {
            return _callback.InvokeAsync(state, trigger, unmetGuards);
        }
    }
}