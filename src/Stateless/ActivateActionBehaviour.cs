using System;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal abstract class ActivateActionBehaviour
        {
            readonly TState _state;
            readonly string _actionDescription;

            protected ActivateActionBehaviour(TState state, string actionDescription)
            {
                _state = state;
                _actionDescription = Enforce.ArgumentNotNull(actionDescription, nameof(actionDescription));
            }

            internal string ActionDescription { get { return _actionDescription; } }

            public abstract void Execute();
            public abstract Task ExecuteAsync();

            public class Sync : ActivateActionBehaviour
            {
                readonly Action _action;

                public Sync(TState state, Action action, string actionDescription)
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

            public class Async : ActivateActionBehaviour
            {
                readonly Func<Task> _action;

                public Async(TState state, Func<Task> action, string actionDescription)
                    : base(state, actionDescription)
                {
                    _action = action;
                }

                public override void Execute()
                {
                    throw new InvalidOperationException(
                        $"Cannot execute asynchronous action specified in OnActivateAsync for '{_state}' state. " +
                         "Use asynchronous version of Activate [ActivateAsync]");
                }

                public override Task ExecuteAsync()
                {
                    return _action();
                }
            }
        }
    }
}
