using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal abstract class ExitActionBehavior
        {
            readonly string _actionDescription;

            public abstract void Execute(Transition transition);
            public abstract Task ExecuteAsync(Transition transition);

            protected ExitActionBehavior(string actionDescription)
            {
                _actionDescription = Enforce.ArgumentNotNull(actionDescription, nameof(actionDescription));
            }

            internal string ActionDescription { get { return _actionDescription; } }

            public class Sync : ExitActionBehavior
            {
                readonly Action<Transition> _action;

                public Sync(Action<Transition> action, string actionDescription) : base(actionDescription)
                {
                    _action = action;
                }

                public override void Execute(Transition transition)
                {
                    _action(transition);
                }

                public override Task ExecuteAsync(Transition transition)
                {
                    Execute(transition);
                    return TaskResult.Done;
                }
            }

            public class Async : ExitActionBehavior
            {
                readonly Func<Transition, Task> _action;

                public Async(Func<Transition, Task> action, string actionDescription) : base(actionDescription)
                {
                    _action = action;
                }

                public override void Execute(Transition transition)
                {
                    throw new InvalidOperationException(
                        $"Cannot execute asynchronous action specified in OnExit event for '{transition.Source}' state. " +
                         "Use asynchronous version of Fire [FireAsync]");
                }

                public override Task ExecuteAsync(Transition transition)
                {
                    return _action(transition);
                }
            }
        }
    }
}
