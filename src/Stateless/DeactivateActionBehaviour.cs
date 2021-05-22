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

            public DeactivateActionBehaviour(TState state, EventCallback action, Reflection.InvocationInfo actionDescription)
                : this(state, actionDescription)
            {
                _callback = action;
            }

            protected DeactivateActionBehaviour(TState state, Reflection.InvocationInfo actionDescription)
            {
                _state = state;
                Description = actionDescription ?? throw new ArgumentNullException(nameof(actionDescription));
            }

            internal Reflection.InvocationInfo Description { get; }

            public virtual Task Execute()
            {
                return _callback.InvokeAsync();
            }
        }
    }
}
