using System;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class ActivateActionBehaviour
        {
            readonly TState _state;
            private readonly EventCallback _callback;

            public ActivateActionBehaviour(TState state, EventCallback action, Reflection.InvocationInfo actionDescription)
                : this(state, actionDescription)
            {
                _callback = action;
            }

            protected ActivateActionBehaviour(TState state, Reflection.InvocationInfo actionDescription)
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
