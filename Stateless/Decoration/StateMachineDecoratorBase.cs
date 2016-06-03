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
        /// The current state.
        /// </summary>
        public virtual TState State
        {
            get { return _stateMachine.State; }
        }

        /// <summary>
        /// The currently-permissible trigger values.
        /// </summary>
        public virtual IEnumerable<TTrigger> PermittedTriggers
        {
            get { return _stateMachine.PermittedTriggers; }
        }

        /// <summary>
        /// Begin configuration of the entry/exit actions and allowed transitions
        /// when the state machine is in a particular state.
        /// </summary>
        /// <param name="state">The state to configure.</param>
        /// <returns>A configuration object through which the state can be configured.</returns>
        public virtual StateMachine<TState, TTrigger>.StateConfiguration Configure(TState state)
        {
            return _stateMachine.Configure(state);
        }

        /// <summary>
        /// Transition from the current state via the specified trigger.
        /// The target state is determined by the configuration of the current state.
        /// Actions associated with leaving the current state and entering the new one
        /// will be invoked.
        /// </summary>
        /// <param name="trigger">The trigger to fire.</param>
        /// <exception cref="System.InvalidOperationException">The current state does
        /// not allow the trigger to be fired.</exception>
        public virtual void Fire(TTrigger trigger)
        {
            _stateMachine.Fire(trigger);
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
        /// <exception cref="System.InvalidOperationException">The current state does
        /// not allow the trigger to be fired.</exception>
        public virtual void Fire<TArg0>(StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0> trigger, TArg0 arg0)
        {
            _stateMachine.Fire(trigger, arg0);
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
        /// <exception cref="System.InvalidOperationException">The current state does
        /// not allow the trigger to be fired.</exception>
        public virtual void Fire<TArg0, TArg1>(StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0, TArg1> trigger, TArg0 arg0, TArg1 arg1)
        {
            _stateMachine.Fire(trigger, arg0, arg1);
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
        /// <exception cref="System.InvalidOperationException">The current state does
        /// not allow the trigger to be fired.</exception>
        public virtual void Fire<TArg0, TArg1, TArg2>(StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0, TArg1, TArg2> trigger, TArg0 arg0, TArg1 arg1, TArg2 arg2)
        {
            _stateMachine.Fire(trigger, arg0, arg1, arg2);
        }

        /// <summary>
        /// Override the default behaviour of throwing an exception when an unhandled trigger
        /// is fired.
        /// </summary>
        /// <param name="unhandledTriggerAction">An action to call when an unhandled trigger is fired.</param>
        public virtual void OnUnhandledTrigger(Action<TState, TTrigger> unhandledTriggerAction)
        {
            _stateMachine.OnUnhandledTrigger(unhandledTriggerAction);
        }

        /// <summary>
        /// Determine if the state machine is in the supplied state.
        /// </summary>
        /// <param name="state">The state to test for.</param>
        /// <returns>True if the current state is equal to, or a substate of,
        /// the supplied state.</returns>
        public virtual bool IsInState(TState state)
        {
            return _stateMachine.IsInState(state);
        }

        /// <summary>
        /// Returns true if <paramref name="trigger"/> can be fired
        /// in the current state.
        /// </summary>
        /// <param name="trigger">Trigger to test.</param>
        /// <returns>True if the trigger can be fired, false otherwise.</returns>
        public virtual bool CanFire(TTrigger trigger)
        {
            return _stateMachine.CanFire(trigger);
        }

        /// <summary>
        /// Specify the arguments that must be supplied when a specific trigger is fired.
        /// </summary>
        /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
        /// <param name="trigger">The underlying trigger value.</param>
        /// <returns>An object that can be passed to the Fire() method in order to
        /// fire the parameterised trigger.</returns>
        public virtual StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0> SetTriggerParameters<TArg0>(TTrigger trigger)
        {
            return _stateMachine.SetTriggerParameters<TArg0>(trigger);
        }

        /// <summary>
        /// Specify the arguments that must be supplied when a specific trigger is fired.
        /// </summary>
        /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
        /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
        /// <param name="trigger">The underlying trigger value.</param>
        /// <returns>An object that can be passed to the Fire() method in order to
        /// fire the parameterised trigger.</returns>
        public virtual StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0, TArg1> SetTriggerParameters<TArg0, TArg1>(TTrigger trigger)
        {
            return _stateMachine.SetTriggerParameters<TArg0, TArg1>(trigger);
        }

        /// <summary>
        /// Specify the arguments that must be supplied when a specific trigger is fired.
        /// </summary>
        /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
        /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
        /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
        /// <param name="trigger">The underlying trigger value.</param>
        /// <returns>An object that can be passed to the Fire() method in order to
        /// fire the parameterised trigger.</returns>
        public virtual StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0, TArg1, TArg2> SetTriggerParameters<TArg0, TArg1, TArg2>(TTrigger trigger)
        {
            return _stateMachine.SetTriggerParameters<TArg0, TArg1, TArg2>(trigger);
        }

        /// <summary>
        /// Registers a callback that will be invoked every time the statemachine
        /// transitions from one state into another.
        /// </summary>
        /// <param name="onTransistionHandler">The action to execute, accepting the details
        /// of the transition.</param>
        public virtual void OnTransitioned(Action<StateMachine<TState, TTrigger>.Transition> onTransistionHandler)
        {
            _stateMachine.OnTransitioned(onTransistionHandler);
        }

        /// <summary>
        /// A string representation of the state machine in the DOT graph language.
        /// </summary>
        /// <param name="includeIgnoredTriggers">Triggers that are ignored by states are added to the graph when set to true, they are left out when set to false.</param>
        /// <returns>A description of all simple source states, triggers and destination states.</returns>
        public virtual string ToDotGraph(bool includeIgnoredTriggers = true)
        {
            return _stateMachine.ToDotGraph(includeIgnoredTriggers);
        }
    }
}
