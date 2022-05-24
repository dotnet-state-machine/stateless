using Stateless.Reflection;

namespace Stateless;

public partial class StateMachine<TState, TTrigger> {
    internal class EntryActionBehavior {
        private readonly EventCallback<Transition, object?[]> _callback =
            EventCallbackFactory.Create<Transition, object?[]>(delegate { });

        public InvocationInfo Description { get; }

        public EntryActionBehavior(Action<Transition, object?[]> action, InvocationInfo description)
            : this(description) =>
            _callback = EventCallbackFactory.Create(action);

        public EntryActionBehavior(Func<Transition, object?[], Task> action, InvocationInfo description)
            : this(description) =>
            _callback = EventCallbackFactory.Create(action);

        private EntryActionBehavior(InvocationInfo description) => Description = description;

        public virtual Task ExecuteAsync(Transition transition, object?[] args) =>
            _callback.InvokeAsync(transition, args);

        public sealed class From : EntryActionBehavior {
            internal TTrigger Trigger { get; }

            public From(TTrigger trigger, Action<Transition, object?[]> action, InvocationInfo description)
                : base(action, description) =>
                Trigger = trigger;

            public From(TTrigger trigger, Func<Transition, object?[], Task> action, InvocationInfo description)
                : base(action, description) =>
                Trigger = trigger;

            public override Task ExecuteAsync(Transition transition, object?[] args) {
                return transition.Trigger.Equals(Trigger) ? base.ExecuteAsync(transition, args) : TaskResult.Done;
            }
        }
    }
}