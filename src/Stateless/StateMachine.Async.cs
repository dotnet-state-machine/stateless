#if TASKS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        /// <summary>
        /// Activates current state in asynchronous fashion. Actions associated with activating the currrent state
        /// will be invoked. The activation is idempotent and subsequent activation of the same current state 
        /// will not lead to re-execution of activation callbacks.
        /// </summary>
        public Task ActivateAsync()
        {
            var representativeState = GetRepresentation(State);
            return representativeState.ActivateAsync();
        }

        /// <summary>
        /// Deactivates current state in asynchronous fashion. Actions associated with deactivating the currrent state
        /// will be invoked. The deactivation is idempotent and subsequent deactivation of the same current state 
        /// will not lead to re-execution of deactivation callbacks.
        /// </summary>
        public Task DeactivateAsync()
        {
            var representativeState = GetRepresentation(State);
            return representativeState.DeactivateAsync();
        }

        /// <summary>
        /// Transition from the current state via the specified trigger in async fashion.
        /// The target state is determined by the configuration of the current state.
        /// Actions associated with leaving the current state and entering the new one
        /// will be invoked.
        /// </summary>
        /// <param name="trigger">The trigger to fire.</param>
        /// <exception cref="System.InvalidOperationException">The current state does
        /// not allow the trigger to be fired.</exception>
        public Task FireAsync(TTrigger trigger)
        {
            return InternalFireAsync(trigger, new object[0]);
        }

        /// <summary>
        /// Transition from the current state via the specified trigger in async fashion.
        /// The target state is determined by the configuration of the current state.
        /// Actions associated with leaving the current state and entering the new one
        /// will be invoked.
        /// </summary>
        /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
        /// <param name="trigger">The trigger to fire.</param>
        /// <param name="arg0">The first argument.</param>
        /// <exception cref="System.InvalidOperationException">The current state does
        /// not allow the trigger to be fired.</exception>
        public Task FireAsync<TArg0>(TriggerWithParameters<TArg0> trigger, TArg0 arg0)
        {
            Enforce.ArgumentNotNull(trigger, "trigger");
            return InternalFireAsync(trigger.Trigger, arg0);
        }

        /// <summary>
        /// Transition from the current state via the specified trigger in async fashion.
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
        public Task FireAsync<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, TArg0 arg0, TArg1 arg1)
        {
            Enforce.ArgumentNotNull(trigger, "trigger");
            return InternalFireAsync(trigger.Trigger, arg0, arg1);
        }

        /// <summary>
        /// Transition from the current state via the specified trigger in async fashion.
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
        public Task FireAsync<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, TArg0 arg0, TArg1 arg1, TArg2 arg2)
        {
            Enforce.ArgumentNotNull(trigger, "trigger");
            return InternalFireAsync(trigger.Trigger, arg0, arg1, arg2);
        }

        async Task InternalFireAsync(TTrigger trigger, params object[] args)
        {
            TriggerWithParameters configuration;
            if (_triggerConfiguration.TryGetValue(trigger, out configuration))
                configuration.ValidateParameters(args);

            var source = State;
            var representativeState = GetRepresentation(source);

            TriggerBehaviour triggerBehaviour;
            if (!representativeState.TryFindHandler(trigger, out triggerBehaviour))
            {
                await _unhandledTriggerAction.ExecuteAsync(representativeState.UnderlyingState, trigger);
                return;
            }

            TState destination;
            if (triggerBehaviour.ResultsInTransitionFrom(source, args, out destination))
            {
                var transition = new Transition(source, destination, trigger);

                await representativeState.ExitAsync(transition);

                State = transition.Destination;
                var newRepresentation = GetRepresentation(transition.Destination);
                await _onTransitionedEvent.InvokeAsync(transition);

                await newRepresentation.EnterAsync(transition, args);
            }
            else
            {
                var transition = new Transition(source, destination, trigger);

                await CurrentRepresentation.InternalActionAsync(transition, args);
            }
        }

        /// <summary>
        /// Override the default behaviour of throwing an exception when an unhandled trigger
        /// is fired.
        /// </summary>
        /// <param name="unhandledTriggerAction">An asynchronous action to call when an unhandled trigger is fired.</param>
        public void OnUnhandledTriggerAsync(Func<TState, TTrigger, Task> unhandledTriggerAction)
        {
            if (unhandledTriggerAction == null) throw new ArgumentNullException("unhandledTriggerAction");
            _unhandledTriggerAction = new UnhandledTriggerAction.Async(unhandledTriggerAction);
        }

        /// <summary>
        /// Registers an asynchronous callback that will be invoked every time the statemachine
        /// transitions from one state into another.
        /// </summary>
        /// <param name="onTransitionAction">The asynchronous action to execute, accepting the details
        /// of the transition.</param>
        public void OnTransitionedAsync(Func<Transition, Task> onTransitionAction)
        {
            if (onTransitionAction == null) throw new ArgumentNullException("onTransitionAction");
            _onTransitionedEvent.Register(onTransitionAction);
        }
    }
}

#endif