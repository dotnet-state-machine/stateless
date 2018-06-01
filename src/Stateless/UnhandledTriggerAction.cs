using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        abstract class UnhandledTriggerAction
        {
            public abstract void Execute(TState state, TTrigger trigger, ICollection<string> unmetGuards);
            public abstract Task ExecuteAsync(TState state, TTrigger trigger, ICollection<string> unmetGuards);

            internal class Sync : UnhandledTriggerAction
            {
                readonly Action<TState, TTrigger, ICollection<string>> _action;

                internal Sync(Action<TState, TTrigger, ICollection<string>> action = null)
                {
                    _action = action;
                }

                public override void Execute(TState state, TTrigger trigger, ICollection<string> unmetGuards)
                {
                    _action(state, trigger, unmetGuards);
                }

                public override Task ExecuteAsync(TState state, TTrigger trigger, ICollection<string> unmetGuards)
                {
                    Execute(state, trigger, unmetGuards);
                    return TaskResult.Done;
                }
            }

            internal class Async : UnhandledTriggerAction
            {
                readonly Func<TState, TTrigger, ICollection<string>, Task> _action;

                internal Async(Func<TState, TTrigger, ICollection<string>, Task> action)
                {
                    _action = action;
                }

                public override void Execute(TState state, TTrigger trigger, ICollection<string> unmetGuards)
                {
                    throw new InvalidOperationException(
                        "Cannot execute asynchronous action specified in OnUnhandledTrigger. " +
                        "Use asynchronous version of Fire [FireAsync]");
                }

                public override Task ExecuteAsync(TState state, TTrigger trigger, ICollection<string> unmetGuards)
                {
                    return _action(state, trigger, unmetGuards);
                }
            }
        }
    }
}
