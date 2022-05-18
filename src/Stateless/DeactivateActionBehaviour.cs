using System;
using System.Threading.Tasks;
using Stateless.Reflection;

namespace Stateless; 

public partial class StateMachine<TState, TTrigger>
{
    internal abstract class DeactivateActionBehaviour
    {
        private readonly TState _state;

        protected DeactivateActionBehaviour(TState state, InvocationInfo actionDescription)
        {
            _state      = state;
            Description = actionDescription ?? throw new ArgumentNullException(nameof(actionDescription));
        }

        internal InvocationInfo Description { get; }

        public abstract void Execute();
        public abstract Task ExecuteAsync();

        public class Sync : DeactivateActionBehaviour
        {
            private readonly Action _action;

            public Sync(TState state, Action action, InvocationInfo actionDescription)
                : base(state, actionDescription)
            {
                _action = action;
            }

            public override void Execute()
            {
                _action();
            }

            public override Task ExecuteAsync()
            {
                Execute();
                return TaskResult.Done;
            }
        }

        public class Async : DeactivateActionBehaviour
        {
            private readonly Func<Task> _action;

            public Async(TState state, Func<Task> action, InvocationInfo actionDescription)
                : base(state, actionDescription)
            {
                _action = action;
            }

            public override void Execute()
            {
                throw new InvalidOperationException(
                                                    $"Cannot execute asynchronous action specified in OnDeactivateAsync for '{_state}' state. Use asynchronous version of Deactivate [DeactivateAsync]");
            }

            public override Task ExecuteAsync()
            {
                return _action();
            }
        }
    }
}