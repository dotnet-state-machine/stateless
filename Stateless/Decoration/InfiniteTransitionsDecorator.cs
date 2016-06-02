using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless.Decoration
{
    /// <summary>
    /// By using this decorator, the StateMachine supports an endless number of transistions.
    /// This logicall results in the following limitations: after a trigger is fired, the current method firing the trigger may not fire a trigger again and must return immediately.
    /// Code that comes after the trigger is fired, in the same scope is executed while the trigger isn't actually fired yet.
    /// This results in a callstack that is only deepened till a certain number of calls.
    /// </summary>
    public class InfiniteTransitionsDecorator<TState, TTrigger> : StateMachineDecoratorBase<TState, TTrigger>
    {
        private TTrigger _nextTrigger = default(TTrigger);
        private bool _nextTriggerSet = false;
        private bool _inFireLoop = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public InfiniteTransitionsDecorator(IStateMachine<TState, TTrigger> stateMachine)
            : base(stateMachine)
        {
        }

        /// <summary>
        /// Does not actually fire a trigger, but can be seen more as setting the next trigger for when the current invoked method returns.
        /// </summary>
        public override void Fire(TTrigger trigger)
        {
            if (_nextTriggerSet)
            {
                throw new InvalidOperationException("The trigger '" + _nextTrigger + "' is already set to execute. No trigger may follow a trigger in the same scope an infinite state machine.");
            }

            _nextTrigger = trigger;
            _nextTriggerSet = true;

            if (!_inFireLoop)
            {
                FireInternal();
            }
        }

        /// <summary>
        /// Not supported yet.
        /// </summary>
        public override void Fire<TArg0, TArg1, TArg2>(StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0, TArg1, TArg2> trigger, TArg0 arg0, TArg1 arg1, TArg2 arg2)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not supported yet.
        /// </summary>
        public override void Fire<TArg0, TArg1>(StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0, TArg1> trigger, TArg0 arg0, TArg1 arg1)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Not supported yet.
        /// </summary>
        public override void Fire<TArg0>(StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0> trigger, TArg0 arg0)
        {
            throw new NotSupportedException();
        }

        private void FireInternal()
        {
            _inFireLoop = true;
            do
            {
                _nextTriggerSet = false;
                base.Fire(_nextTrigger);
            } while (_nextTriggerSet);
            _inFireLoop = false;
        }
    }
}
