using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stateless.Decoration
{
    /// <summary>
    /// Base class for decorating a StateMachine.
    /// See decoration pattern for more details.
    /// </summary>
    /// <typeparam name="TState">The type used to represent the states.</typeparam>
    /// <typeparam name="TTrigger">The type used to represent the triggers that cause state transitions.</typeparam>
    public abstract class StateMachineDecoratorBase<TState, TTrigger> : IStateMachine<TState, TTrigger>
    {
        private IStateMachine<TState, TTrigger> _stateMachine;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stateMachine">The StateMachine to decorate.</param>
        public StateMachineDecoratorBase(IStateMachine<TState, TTrigger> stateMachine)
        {
            _stateMachine = stateMachine;
        }

        /// <summary>
        /// TODO Devoney
        /// </summary>
        public virtual TState State
        {
            get { return _stateMachine.State; }
        }

        /// <summary>
        /// TODO Devoney
        /// </summary>
        public virtual IEnumerable<TTrigger> PermittedTriggers
        {
            get { return _stateMachine.PermittedTriggers; }
        }

        /// <summary>
        /// TODO Devoney
        /// </summary>
        public virtual StateMachine<TState, TTrigger>.StateConfiguration Configure(TState state)
        {
            return _stateMachine.Configure(state);
        }

        /// <summary>
        /// TODO Devoney
        /// </summary>
        public virtual void Fire(TTrigger trigger)
        {
            _stateMachine.Fire(trigger);
        }

        /// <summary>
        /// TODO Devoney
        /// </summary>
        public virtual void Fire<TArg0>(StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0> trigger, TArg0 arg0)
        {
            _stateMachine.Fire(trigger, arg0);
        }

        /// <summary>
        /// TODO Devoney
        /// </summary>
        public virtual void Fire<TArg0, TArg1>(StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0, TArg1> trigger, TArg0 arg0, TArg1 arg1)
        {
            _stateMachine.Fire(trigger, arg0, arg1);
        }

        /// <summary>
        /// TODO Devoney
        /// </summary>
        public virtual void Fire<TArg0, TArg1, TArg2>(StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0, TArg1, TArg2> trigger, TArg0 arg0, TArg1 arg1, TArg2 arg2)
        {
            _stateMachine.Fire(trigger, arg0, arg1, arg2);
        }

        /// <summary>
        /// TODO Devoney
        /// </summary>
        public virtual void OnUnhandledTrigger(Action<TState, TTrigger> unhandledTriggerAction)
        {
            _stateMachine.OnUnhandledTrigger(unhandledTriggerAction);
        }

        /// <summary>
        /// TODO Devoney
        /// </summary>
        public virtual bool IsInState(TState state)
        {
            return _stateMachine.IsInState(state);
        }

        /// <summary>
        /// TODO Devoney
        /// </summary>
        public virtual bool CanFire(TTrigger trigger)
        {
            return _stateMachine.CanFire(trigger);
        }

        /// <summary>
        /// TODO Devoney
        /// </summary>
        public virtual StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0> SetTriggerParameters<TArg0>(TTrigger trigger)
        {
            return _stateMachine.SetTriggerParameters<TArg0>(trigger);
        }

        /// <summary>
        /// TODO Devoney
        /// </summary>
        public virtual StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0, TArg1> SetTriggerParameters<TArg0, TArg1>(TTrigger trigger)
        {
            return _stateMachine.SetTriggerParameters<TArg0, TArg1>(trigger);
        }

        /// <summary>
        /// TODO Devoney
        /// </summary>
        public virtual StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0, TArg1, TArg2> SetTriggerParameters<TArg0, TArg1, TArg2>(TTrigger trigger)
        {
            return _stateMachine.SetTriggerParameters<TArg0, TArg1, TArg2>(trigger);
        }

        /// <summary>
        /// TODO Devoney
        /// </summary>
        public virtual void OnTransitioned(Action<StateMachine<TState, TTrigger>.Transition> onTransistionHandler)
        {
            _stateMachine.OnTransitioned(onTransistionHandler);
        }

        /// <summary>
        /// TODO Devoney
        /// </summary>
        public virtual string ToDotGraph()
        {
            return _stateMachine.ToDotGraph();
        }
    }
}
