using Stateless.Reflection;

namespace Stateless; 

public partial class StateMachine<TState, TTrigger>
{
    internal class ActivateActionBehaviour
    {
        private readonly TState        _state;

        private readonly EventCallback _callback = EventCallbackFactory.Empty;

        internal InvocationInfo Description { get; }

        public ActivateActionBehaviour(TState state, Action action, InvocationInfo actionDescription)
            : this(state, actionDescription) {
            _callback   = EventCallbackFactory.Create(action);
        }

        public ActivateActionBehaviour(TState state, Func<Task> action, InvocationInfo actionDescription)
            : this(state, actionDescription)
        {
            _callback = EventCallbackFactory.Create(action);
        }

        protected ActivateActionBehaviour(TState state, InvocationInfo actionDescription)
        {
            _state      = state;
            Description = actionDescription ?? throw new ArgumentNullException(nameof(actionDescription));
        }

        public virtual Task ExecuteAsync()
        {
            return _callback.InvokeAsync();
        }
    }
}