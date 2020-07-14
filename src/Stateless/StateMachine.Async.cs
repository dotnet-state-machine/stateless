#if TASKS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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
            return FireAsync(trigger, CancellationToken.None);
        }

        /// <summary>
        /// Transition from the current state via the specified trigger in async fashion.
        /// The target state is determined by the configuration of the current state.
        /// Actions associated with leaving the current state and entering the new one
        /// will be invoked.
        /// </summary>
        /// <param name="trigger">The trigger to fire.</param>
        /// <param name="ct">cancellation token</param>
        /// <exception cref="System.InvalidOperationException">The current state does
        /// not allow the trigger to be fired.</exception>
        public Task FireAsync(TTrigger trigger, CancellationToken ct)
        {
            return InternalFireAsync(trigger, ct, new object[0]);
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
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            return FireAsync(trigger, arg0, CancellationToken.None);
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
        /// <param name="ct">cancellation token</param>
        /// <exception cref="System.InvalidOperationException">The current state does
        /// not allow the trigger to be fired.</exception>
        public Task FireAsync<TArg0>(TriggerWithParameters<TArg0> trigger, TArg0 arg0, CancellationToken ct)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            return InternalFireAsync(trigger.Trigger, ct, arg0);
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
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            return FireAsync(trigger, arg0, arg1, CancellationToken.None);
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
        /// <param name="ct">cancellation token</param>
        /// <exception cref="System.InvalidOperationException">The current state does
        /// not allow the trigger to be fired.</exception>
        public Task FireAsync<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, TArg0 arg0, TArg1 arg1, CancellationToken ct)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            return InternalFireAsync(trigger.Trigger, ct, arg0, arg1);
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
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            return FireAsync(trigger, arg0, arg1, arg2, CancellationToken.None);
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
        /// <param name="ct">cancellation token</param>
        /// <exception cref="System.InvalidOperationException">The current state does
        /// not allow the trigger to be fired.</exception>
        public Task FireAsync<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, TArg0 arg0, TArg1 arg1, TArg2 arg2, CancellationToken ct)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            return InternalFireAsync(trigger.Trigger, ct, arg0, arg1, arg2);
        }

        /// <summary>
        /// Determine how to Fire the trigger
        /// </summary>
        /// <param name="trigger">The trigger. </param>
        /// <param name="ct">cancellation token</param>
        /// <param name="args">A variable-length parameters list containing arguments. </param>
        async Task InternalFireAsync(TTrigger trigger, CancellationToken ct, params object[] args)
        {
            switch (_firingMode)
            {
                case FiringMode.Immediate:
                    await InternalFireOneAsync(trigger, ct, args).ConfigureAwait(false);
                    break;
                case FiringMode.Queued:
                    await InternalFireQueuedAsync(trigger, ct, args).ConfigureAwait(false);
                    break;
                default:
                    // If something is completely messed up we let the user know ;-)
                    throw new InvalidOperationException("The firing mode has not been configured!");
            }
        }

        /// <summary>
        /// Queue events and then fire in order.
        /// If only one event is queued, this behaves identically to the non-queued version.
        /// </summary>
        /// <param name="trigger">  The trigger. </param>
        /// <param name="ct">cancellation token</param>
        /// <param name="args">     A variable-length parameters list containing arguments. </param>
        async Task InternalFireQueuedAsync(TTrigger trigger, CancellationToken ct, params object[] args)
        {
            if (_firing)
            {
                _eventQueue.Enqueue(new QueuedTrigger { Trigger = trigger, Args = args, CancellationToken = ct });
                return;
            }

            try
            {
                _firing = true;

                await InternalFireOneAsync(trigger, ct, args).ConfigureAwait(false);

                while (_eventQueue.Count != 0)
                {
                    var queuedEvent = _eventQueue.Dequeue();
                    await InternalFireOneAsync(queuedEvent.Trigger, queuedEvent.CancellationToken, queuedEvent.Args).ConfigureAwait(false);
                }
            }
            finally
            {
                _firing = false;
            }
        }

        async Task InternalFireOneAsync(TTrigger trigger, CancellationToken ct, params object[] args)
        {
            // If this is a trigger with parameters, we must validate the parameter(s)
            if (_triggerConfiguration.TryGetValue(trigger, out var configuration))
                configuration.ValidateParameters(args);

            var source = State;
            var representativeState = GetRepresentation(source);

            // Try to find a trigger handler, either in the current state or a super state.
            var searchResult = await representativeState.TryFindHandlerAsync(trigger, args).ConfigureAwait(false);
            if (!searchResult.Item1)
            {
                await _unhandledTriggerAction.ExecuteAsync(representativeState.UnderlyingState, trigger, searchResult.Item2?.UnmetGuardConditions).ConfigureAwait(false);
                return;
            }

            // Check if this trigger should be ignored
            if (searchResult.Item2.Handler is IgnoredTriggerBehaviour)
            {
                return;
            }

            // Handle special case, re-entry in superstate
            if (searchResult.Item2.Handler is ReentryTriggerBehaviour handler)
            {
                // Handle transition, and set new state
                var transition = new Transition(source, handler.Destination, trigger, args);
                transition = await representativeState.ExitAsync(transition);
                State = transition.Destination;
                var newRepresentation = GetRepresentation(transition.Destination);

                if (!source.Equals(transition.Destination))
                {
                    // Then Exit the final superstate
                    transition = new Transition(handler.Destination, handler.Destination, trigger, args);
                    await newRepresentation.ExitAsync(transition);
                }

                await _onTransitionedEvent.InvokeAsync(new Transition(source, handler.Destination, trigger, args));

                await newRepresentation.EnterAsync(transition, ct, args);
            }
            else
            {
                // Check if it is an internal transition, or a transition from one state to another.
                var transitionResult = await searchResult.Item2.Handler.ResultsInTransitionFromAsync(source, args, ct);
                var result = transitionResult.Item1;
                var destination = transitionResult.Item2;
                if (result)
                {
                    var transition = new Transition(source, destination, trigger);

                    transition = await representativeState.ExitAsync(transition).ConfigureAwait(false);

                    State = transition.Destination;
                    var newRepresentation = GetRepresentation(transition.Destination);

                    // Check if there is an intital transition configured
                    if (newRepresentation.HasInitialTransition)
                    {
                        // Verify that the target state is a substate
                        if (!newRepresentation.GetSubstates().Any(s => s.UnderlyingState.Equals(newRepresentation.InitialTransitionTarget)))
                        {
                            throw new InvalidOperationException($"The target ({newRepresentation.InitialTransitionTarget}) for the initial transition is not a substate.");
                        }

                        // Check if state has substate(s), and if an initial transition(s) has been set up.
                        while (newRepresentation.GetSubstates().Any() && newRepresentation.HasInitialTransition)
                        {
                            var initialTransition = new Transition(source, newRepresentation.InitialTransitionTarget, trigger);
                            newRepresentation = GetRepresentation(newRepresentation.InitialTransitionTarget);
                            await newRepresentation.EnterAsync(initialTransition, ct, args);
                            State = newRepresentation.UnderlyingState;
                        }
                        //Alert all listeners of state transition
                        await _onTransitionedEvent.InvokeAsync(new Transition(source, destination, trigger));
                    }
                    else
                    {
                        //Alert all listeners of state transition
                        await _onTransitionedEvent.InvokeAsync(new Transition(source, destination, trigger));

                        await newRepresentation.EnterAsync(transition, ct, args);
                    }
                }
                else
                {
                    var transition = new Transition(source, destination, trigger);

                    await CurrentRepresentation.InternalActionAsync(transition, args).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Override the default behaviour of throwing an exception when an unhandled trigger
        /// is fired.
        /// </summary>
        /// <param name="unhandledTriggerAction"></param>
        public void OnUnhandledTriggerAsync(Func<TState, TTrigger, Task> unhandledTriggerAction)
        {
            if (unhandledTriggerAction == null) throw new ArgumentNullException(nameof(unhandledTriggerAction));
            _unhandledTriggerAction = new UnhandledTriggerAction.Async((s, t, c) => unhandledTriggerAction(s, t));
        }

        /// <summary>
        /// Override the default behaviour of throwing an exception when an unhandled trigger
        /// is fired.
        /// </summary>
        /// <param name="unhandledTriggerAction">An asynchronous action to call when an unhandled trigger is fired.</param>
        public void OnUnhandledTriggerAsync(Func<TState, TTrigger, ICollection<string>, Task> unhandledTriggerAction)
        {
            if (unhandledTriggerAction == null) throw new ArgumentNullException(nameof(unhandledTriggerAction));
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
            if (onTransitionAction == null) throw new ArgumentNullException(nameof(onTransitionAction));
            _onTransitionedEvent.Register(onTransitionAction);
        }
    }
}

#endif
