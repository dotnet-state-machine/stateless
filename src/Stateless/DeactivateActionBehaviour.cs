using System;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class DeactivateActionBehaviour
        {
            readonly TState _state;
            private readonly EventCallback _callback;

            public DeactivateActionBehaviour(TState state, Action action, Reflection.InvocationInfo actionDescription)
                : this(state, actionDescription)
            {
                _callback = EventCallbackFactory.Create(action);
            }

            public DeactivateActionBehaviour(TState state, Func<Task> action, Reflection.InvocationInfo actionDescription)
                : this(state, actionDescription)
            {
                _callback = EventCallbackFactory.Create(action);
            }

            protected DeactivateActionBehaviour(TState state, Reflection.InvocationInfo actionDescription)
            {
                _state = state;
                Description = actionDescription ?? throw new ArgumentNullException(nameof(actionDescription));
            }

            internal Reflection.InvocationInfo Description { get; }

            public virtual Task ExecuteAsync()
            {
                return _callback.InvokeAsync();
            }
        }
    }
}
