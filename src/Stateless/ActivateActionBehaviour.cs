using System;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal abstract class ActivateActionBehaviour
        {
            protected ActivateActionBehaviour(Reflection.InvocationInfo actionDescription)
            {
                Description = actionDescription ?? throw new ArgumentNullException(nameof(actionDescription));
            }
            internal Reflection.InvocationInfo Description { get; }

            public abstract void Execute();
            public abstract Task ExecuteAsync();

            public class Sync : ActivateActionBehaviour
            {
                readonly Action _action;

                public Sync(Action action, Reflection.InvocationInfo actionDescription)
                    : base(actionDescription)
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

                public Async(Func<Task> action, Reflection.InvocationInfo actionDescription)
                    : base(actionDescription)
                {
                    _action = action;
                }

                public override void Execute()
                {
                    _action().GetAwaiter().GetResult();
                }

                public override Task ExecuteAsync()
                {
                    return _action();
                }
            }
        }
    }
}
