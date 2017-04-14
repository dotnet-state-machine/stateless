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

            // Rename base class 'Description' for backward compatibility
            public string ActionDescription => _description.Description;

            public abstract void Execute(Transition transition, object[] args);
            public abstract Task ExecuteAsync(Transition transition, object[] args);

            public class Sync : EntryActionBehavior
            {
                readonly Action<Transition, object[]> _action;

                public Sync(Action<Transition, object[]> action, MethodDescription description) : base(description)
                {
                    _action = action;
                }

                //internal override string MethodName {  get { return _action.TryGetMethodName(); } }
                //internal override bool IsAsync {  get { return false;  } }

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

                //internal override string MethodName { get { return _action.TryGetMethodName(); } }
                //internal override bool IsAsync { get { return true; } }

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
