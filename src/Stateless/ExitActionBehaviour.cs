using Stateless.Reflection;

namespace Stateless; 

public partial class StateMachine<TState, TTrigger>
{
    internal class ExitActionBehavior
    {
        private readonly EventCallback<Transition> _callback = EventCallbackFactory.Create(new Action<Transition>(delegate { }));

        internal InvocationInfo Description { get; }

        public ExitActionBehavior(Action<Transition> action, InvocationInfo actionDescription)
            : this(actionDescription)
        {
            _callback = EventCallbackFactory.Create(action);
        }

        public ExitActionBehavior(Func<Transition, Task> action, InvocationInfo actionDescription)
            : this(actionDescription) 
        {
            _callback = EventCallbackFactory.Create(action);
        }

        protected ExitActionBehavior(InvocationInfo actionDescription)
        {
            Description = actionDescription ?? throw new ArgumentNullException(nameof(actionDescription));
        }

        public virtual Task ExecuteAsync(Transition transition)
        {
            return _callback.InvokeAsync(transition);
        }
    }
}