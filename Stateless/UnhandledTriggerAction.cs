using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        abstract class UnhandledTriggerAction
        {
            public abstract void Execute(TState state, TTrigger trigger);
            public abstract Task ExecuteAsync(TState state, TTrigger trigger);

            internal class Sync : UnhandledTriggerAction
            {
                readonly Action<TState, TTrigger> _action;

                internal Sync(Action<TState, TTrigger> action = null)
                {
                    _action = action;
                }

                public override void Execute(TState state, TTrigger trigger)
                {
                    _action(state, trigger);
                }

                public override Task ExecuteAsync(TState state, TTrigger trigger)
                {
                    Execute(state, trigger);
                    return TaskResult.Done;
                }
            }

            internal class Async : UnhandledTriggerAction
            {
                readonly Func<TState, TTrigger, Task> _action;

                internal Async(Func<TState, TTrigger, Task> action)
                {
                    _action = action;
                }

                public override void Execute(TState state, TTrigger trigger)
                {
                    throw new InvalidOperationException(
                        "Cannot execute asynchronous action specified in OnUnhandledTrigger. " +
                        "Use asynchronous version of Fire [FireAsync]");
                }

                public override Task ExecuteAsync(TState state, TTrigger trigger)
                {
                    return _action(state, trigger);
                }
            }
        }
    }
}
