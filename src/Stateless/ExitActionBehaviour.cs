using Stateless.Reflection;

namespace Stateless;

public partial class StateMachine<TState, TTrigger> {
    internal sealed class ExitActionBehavior {
        private readonly EventCallback<Transition> _callback =
            EventCallbackFactory.Create(new Action<Transition>(delegate { }));

        internal InvocationInfo Description { get; }

        public ExitActionBehavior(Action<Transition> action, InvocationInfo actionDescription)
            : this(actionDescription) =>
            _callback = EventCallbackFactory.Create(action);

        public ExitActionBehavior(Func<Transition, Task> action, InvocationInfo actionDescription)
            : this(actionDescription) =>
            _callback = EventCallbackFactory.Create(action);

        private ExitActionBehavior(InvocationInfo actionDescription) =>
            Description = actionDescription ?? throw new ArgumentNullException(nameof(actionDescription));

        public Task ExecuteAsync(Transition transition) => _callback.InvokeAsync(transition);
    }
}