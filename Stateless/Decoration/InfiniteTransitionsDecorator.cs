using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless.Decoration
{
    /// <summary>
    /// By using this decorator, the StateMachine supports an endless number of transistions.
    /// This logically results in the following limitations: after a trigger is fired, the current method firing the trigger may not fire a trigger again and must return immediately.
    /// Code that comes after the trigger is fired, in the same scope is executed while the trigger isn't actually fired yet.
    /// This results in a callstack that is only deepened till a certain number of calls.
    /// </summary>
    public class InfiniteTransitionsDecorator<TState, TTrigger> : StateMachineDecoratorBase<TState, TTrigger>
    {
        private Action _nextTrigger = null;
        private bool _inFireLoop = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public InfiniteTransitionsDecorator(IStateMachine<TState, TTrigger> stateMachine)
            : base(stateMachine)
        {
        }

        /// <summary>
        /// Transition from the current state via the specified trigger.
        /// The target state is determined by the configuration of the current state.
        /// Actions associated with leaving the current state and entering the new one
        /// will be invoked.
        /// </summary>
        /// <param name="trigger">The trigger to fire.</param>
        /// <exception cref="System.InvalidOperationException">The current state does not allow the trigger to be fired.</exception>
        /// <exception cref="System.InvalidOperationException">A trigger has already been set in the same method scope.</exception>
        public override void Fire(TTrigger trigger)
        {
            SetNextTriggerToFire(() =>
            {
                base.Fire(trigger);
            });
        }

        /// <summary>
        /// Transition from the current state via the specified trigger.
        /// The target state is determined by the configuration of the current state.
        /// Actions associated with leaving the current state and entering the new one
        /// will be invoked.
        /// </summary>
        /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
        /// <param name="trigger">The trigger to fire.</param>
        /// <param name="arg0">The first argument.</param>
        /// <exception cref="System.InvalidOperationException">The current state does not allow the trigger to be fired.</exception>
        /// <exception cref="System.InvalidOperationException">A trigger has already been set in the same method scope.</exception>
        public override void Fire<TArg0>(StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0> trigger, TArg0 arg0)
        {
            SetNextTriggerToFire(() =>
            {
                base.Fire(trigger, arg0);
            });
        }

        /// <summary>
        /// Transition from the current state via the specified trigger.
        /// The target state is determined by the configuration of the current state.
        /// Actions associated with leaving the current state and entering the new one
        /// will be invoked.
        /// </summary>
        /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
        /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
        /// <param name="arg0">The first argument.</param>
        /// <param name="arg1">The second argument.</param>
        /// <param name="trigger">The trigger to fire.</param>
        /// <exception cref="System.InvalidOperationException">The current state does not allow the trigger to be fired.</exception>
        /// <exception cref="System.InvalidOperationException">A trigger has already been set in the same method scope.</exception>
        public override void Fire<TArg0, TArg1>(StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0, TArg1> trigger, TArg0 arg0, TArg1 arg1)
        {
            SetNextTriggerToFire(() =>
            {
                base.Fire(trigger, arg0, arg1);
            });
        }

        /// <summary>
        /// Transition from the current state via the specified trigger.
        /// The target state is determined by the configuration of the current state.
        /// Actions associated with leaving the current state and entering the new one
        /// will be invoked.
        /// </summary>
        /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
        /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
        /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
        /// <param name="arg0">The first argument.</param>
        /// <param name="arg1">The second argument.</param>
        /// <param name="arg2">The third argument.</param>
        /// <param name="trigger">The trigger to fire.</param>
        /// <exception cref="System.InvalidOperationException">The current state does  not allow the trigger to be fired.</exception>
        /// <exception cref="System.InvalidOperationException">A trigger has already been set in the same method scope.</exception>
        public override void Fire<TArg0, TArg1, TArg2>(StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0, TArg1, TArg2> trigger, TArg0 arg0, TArg1 arg1, TArg2 arg2)
        {
            SetNextTriggerToFire(() =>
            {
                base.Fire(trigger, arg0, arg1, arg2);
            });
        }

        private void SetNextTriggerToFire(Action action)
        {
            if (_nextTrigger != null)
            {
                throw new InvalidOperationException("A trigger is already set to execute. No trigger may follow a trigger in the same method scope when using an infinite state machine.");
            }

            _nextTrigger = action;

            if (!_inFireLoop)
            {
                FireWhileNextTriggerSet();
            }
        }

        private void FireWhileNextTriggerSet()
        {
            _inFireLoop = true;
            do
            {
                var action = _nextTrigger;
                _nextTrigger = null;
                action();
            } while (_nextTrigger != null);
            _inFireLoop = false;
        }
    }
}
