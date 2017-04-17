using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal abstract class EntryActionBehavior
        {
            MethodDescription _description;

            protected EntryActionBehavior(MethodDescription description)
            {
                _description = description;
            }

            // Returns a description of the entry action.  If the caller specified a description, it
            // is returned, else the name of the method itself.
            public string ActionDescription => _description.Description;

            public MethodDescription MethodDescription => _description;

            public abstract void Execute(Transition transition, object[] args);
            public abstract Task ExecuteAsync(Transition transition, object[] args);

            public class Sync : EntryActionBehavior
            {
                readonly Action<Transition, object[]> _action;

                public Sync(Action<Transition, object[]> action, MethodDescription description) : base(description)
                {
                    _action = action;
                }

                public override void Execute(Transition transition, object[] args)
                {
                    _action(transition, args);
                }

                public override Task ExecuteAsync(Transition transition, object[] args)
                {
                    Execute(transition, args);
                    return TaskResult.Done;
                }
            }

            public class Async : EntryActionBehavior
            {
                readonly Func<Transition, object[], Task> _action;

                public Async(Func<Transition, object[], Task> action, MethodDescription description) : base(description)
                {
                    _action = action;
                }

                public override void Execute(Transition transition, object[] args)
                {
                    throw new InvalidOperationException(
                        $"Cannot execute asynchronous action specified in OnEntry event for '{transition.Destination}' state. " +
                         "Use asynchronous version of Fire [FireAsync]");
                }

                public override Task ExecuteAsync(Transition transition, object[] args)
                {
                    return _action(transition, args);
                }
            }
        }
    }
}
