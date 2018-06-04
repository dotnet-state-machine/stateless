using System;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal abstract class InternalTriggerBehaviour : TriggerBehaviour
        {
            protected InternalTriggerBehaviour(TTrigger trigger, TransitionGuard guard) : base(trigger, guard)
            {
            }

            public abstract void Execute(Transition transition, object[] args);
            public abstract Task ExecuteAsync(Transition transition, object[] args);

            public override bool ResultsInTransitionFrom(TState source, object[] args, out TState destination)
            {
                destination = source;
                return false;
            }


            public class Sync: InternalTriggerBehaviour
            {
                public Action<Transition, object[]> InternalAction { get; }

                public Sync(TTrigger trigger, Func<object[], bool> guard, Action<Transition, object[]> internalAction, string guardDescription = null) : base(trigger, new TransitionGuard(guard, guardDescription))
                {
                    InternalAction = internalAction;
                }
                public override void Execute(Transition transition, object[] args)
                {
                    InternalAction(transition, args);
                }

                public override Task ExecuteAsync(Transition transition, object[] args)
                {
                    Execute(transition, args);
                    return TaskResult.Done;
                }
            }

            public class Async : InternalTriggerBehaviour
            {
                readonly Func<Transition, object[], Task> InternalAction;

                public Async(TTrigger trigger, Func<bool> guard,Func<Transition, object[], Task> internalAction, string guardDescription = null) : base(trigger, new TransitionGuard(guard, guardDescription))
                {
                    InternalAction = internalAction;
                }

                public override void Execute(Transition transition, object[] args)
                {
                    throw new InvalidOperationException(
                        $"Cannot execute asynchronous action specified in OnEntry event for '{transition.Destination}' state. " +
                         "Use asynchronous version of Fire [FireAsync]");
                }

                public override Task ExecuteAsync(Transition transition, object[] args)
                {
                    return InternalAction(transition, args);
                }

            }


        }
    }
}