using System;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class ExitActionBehavior
        {
            private readonly EventCallback<Transition> _callback;

            public ExitActionBehavior(EventCallback<Transition> action, Reflection.InvocationInfo actionDescription)
                : this(actionDescription)
            {
                _callback = action;
            }

            protected ExitActionBehavior(Reflection.InvocationInfo actionDescription)
            {
                Description = actionDescription ?? throw new ArgumentNullException(nameof(actionDescription));
            }

            internal Reflection.InvocationInfo Description { get; }

            public virtual Task Execute(Transition transition)
            {
                return _callback.InvokeAsync(transition);
            }
        }
    }
}
