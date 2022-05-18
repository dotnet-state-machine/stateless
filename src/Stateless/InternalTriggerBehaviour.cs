using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Stateless; 

public partial class StateMachine<TState, TTrigger>
{
    internal abstract class InternalTriggerBehaviour : TriggerBehaviour
    {
        private InternalTriggerBehaviour(TTrigger trigger, TransitionGuard? guard) : base(trigger, guard)
        {
        }

        public abstract void Execute(Transition      transition, object[] args);
        public abstract Task ExecuteAsync(Transition transition, object[] args);

        public override bool ResultsInTransitionFrom(TState source, object[] args, [NotNullWhen(true)]out TState? destination)
        {
            destination = source;
            return false;
        }


        public class Sync: InternalTriggerBehaviour
        {
            public Action<Transition, object[]> InternalAction { get; }

            public Sync(TTrigger trigger, TransitionGuard? guard, Action<Transition, object[]> internalAction) : base(trigger, guard)
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
            private readonly Func<Transition, object[], Task> _internalAction;

            public Async(TTrigger trigger, Func<bool> guard, Func<Transition, object[], Task> internalAction, string? guardDescription = null) : base(trigger, new TransitionGuard(guard, guardDescription))
            {
                _internalAction = internalAction;
            }

            public override void Execute(Transition transition, object[] args)
            {
                throw new InvalidOperationException(
                                                    $"Cannot execute asynchronous action specified in OnEntry event for '{transition.Destination}' state. Use asynchronous version of Fire [FireAsync]");
            }

            public override Task ExecuteAsync(Transition transition, object[] args)
            {
                return _internalAction(transition, args);
            }

        }


    }
}