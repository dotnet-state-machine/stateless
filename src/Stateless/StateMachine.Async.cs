using System.Diagnostics;

namespace Stateless; 

public partial class StateMachine<TState, TTrigger> {
    /// <summary>
    ///     Activates current state in asynchronous fashion. Actions associated with activating the current state
    ///     will be invoked. The activation is idempotent and subsequent activation of the same current state
    ///     will not lead to re-execution of activation callbacks.
    /// </summary>
    public Task ActivateAsync() {
        var representativeState = GetRepresentation(State);
        return representativeState.ActivateAsync();
    }

    /// <summary>
    ///     Deactivates current state in asynchronous fashion. Actions associated with deactivating the current state
    ///     will be invoked. The deactivation is idempotent and subsequent deactivation of the same current state
    ///     will not lead to re-execution of deactivation callbacks.
    /// </summary>
    public Task DeactivateAsync() {
        var representativeState = GetRepresentation(State);
        return representativeState.DeactivateAsync();
    }

    /// <summary>
    ///     Transition from the current state via the specified trigger in async fashion.
    ///     The target state is determined by the configuration of the current state.
    ///     Actions associated with leaving the current state and entering the new one
    ///     will be invoked.
    /// </summary>
    /// <param name="trigger">The trigger to fire.</param>
    /// <exception cref="System.InvalidOperationException">
    ///     The current state does
    ///     not allow the trigger to be fired.
    /// </exception>
    public Task FireAsync(TTrigger trigger) => InternalFireAsync(trigger, ArrayHelper.Empty<object>());

    /// <summary>
    ///     Transition from the current state via the specified trigger.
    ///     The target state is determined by the configuration of the current state.
    ///     Actions associated with leaving the current state and entering the new one
    ///     will be invoked.
    /// </summary>
    /// <param name="trigger">The trigger to fire.</param>
    /// <param name="args">A variable-length parameters list containing arguments. </param>
    /// <exception cref="System.InvalidOperationException">
    ///     The current state does
    ///     not allow the trigger to be fired.
    /// </exception>
    public Task FireAsync(Stateless.TriggerWithParameters<TTrigger> trigger, params object?[] args) {
        if (trigger == null) throw new ArgumentNullException(nameof(trigger));

        return InternalFireAsync(trigger.Trigger, args);
    }

    /// <summary>
    ///     Transition from the current state via the specified trigger in async fashion.
    ///     The target state is determined by the configuration of the current state.
    ///     Actions associated with leaving the current state and entering the new one
    ///     will be invoked.
    /// </summary>
    /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
    /// <param name="trigger">The trigger to fire.</param>
    /// <param name="arg0">The first argument.</param>
    /// <exception cref="System.InvalidOperationException">
    ///     The current state does
    ///     not allow the trigger to be fired.
    /// </exception>
    public Task FireAsync<TArg0>(Stateless.TriggerWithParameters<TTrigger, TArg0> trigger, TArg0? arg0) {
        if (trigger == null) throw new ArgumentNullException(nameof(trigger));

        return InternalFireAsync(trigger.Trigger, arg0);
    }

    /// <summary>
    ///     Transition from the current state via the specified trigger in async fashion.
    ///     The target state is determined by the configuration of the current state.
    ///     Actions associated with leaving the current state and entering the new one
    ///     will be invoked.
    /// </summary>
    /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
    /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
    /// <param name="arg0">The first argument.</param>
    /// <param name="arg1">The second argument.</param>
    /// <param name="trigger">The trigger to fire.</param>
    /// <exception cref="System.InvalidOperationException">
    ///     The current state does
    ///     not allow the trigger to be fired.
    /// </exception>
    public Task FireAsync<TArg0, TArg1>(TriggerWithParameters<TTrigger, TArg0, TArg1> trigger, TArg0? arg0, TArg1? arg1) {
        if (trigger == null) throw new ArgumentNullException(nameof(trigger));

        return InternalFireAsync(trigger.Trigger, arg0, arg1);
    }

    /// <summary>
    ///     Transition from the current state via the specified trigger in async fashion.
    ///     The target state is determined by the configuration of the current state.
    ///     Actions associated with leaving the current state and entering the new one
    ///     will be invoked.
    /// </summary>
    /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
    /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
    /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
    /// <param name="arg0">The first argument.</param>
    /// <param name="arg1">The second argument.</param>
    /// <param name="arg2">The third argument.</param>
    /// <param name="trigger">The trigger to fire.</param>
    /// <exception cref="System.InvalidOperationException">
    ///     The current state does
    ///     not allow the trigger to be fired.
    /// </exception>
    public Task FireAsync<TArg0, TArg1, TArg2>(TriggerWithParameters<TTrigger, TArg0, TArg1, TArg2> trigger, TArg0? arg0,
                                               TArg1?                                               arg1,    TArg2? arg2) {
        if (trigger == null) throw new ArgumentNullException(nameof(trigger));

        return InternalFireAsync(trigger.Trigger, arg0, arg1, arg2);
    }

    /// <summary>
    ///     Determine how to Fire the trigger
    /// </summary>
    /// <param name="trigger">The trigger. </param>
    /// <param name="args">A variable-length parameters list containing arguments. </param>
    private async Task InternalFireAsync(TTrigger trigger, params object?[] args) {
        switch (_firingMode) {
            case FiringMode.Immediate:
                await InternalFireOneAsync(trigger, args).ConfigureAwait(false);
                break;
            case FiringMode.Queued:
                await InternalFireQueuedAsync(trigger, args).ConfigureAwait(false);
                break;
            default:
                // If something is completely messed up we let the user know ;-)
                throw new InvalidOperationException("The firing mode has not been configured!");
        }
    }

    /// <summary>
    ///     Queue events and then fire in order.
    ///     If only one event is queued, this behaves identically to the non-queued version.
    /// </summary>
    /// <param name="trigger">  The trigger. </param>
    /// <param name="args">     A variable-length parameters list containing arguments. </param>
    private async Task InternalFireQueuedAsync(TTrigger trigger, params object?[] args) {
        // Add trigger to queue
        _eventQueue.Enqueue(new QueuedTrigger(trigger, args));

        // If a trigger is already being handled then the trigger will be queued (FIFO) and processed later.
        if (_firing) return;

        try {
            _firing = true;

            while (_eventQueue.Any()) {
                var queuedEvent = _eventQueue.Dequeue();
                await InternalFireOneAsync(queuedEvent.Trigger, queuedEvent.Args).ConfigureAwait(false);
            }
        } finally {
            _firing = false;
        }
    }

    /// <summary>
    ///     This method handles the execution of a trigger handler. It finds a
    ///     handle, then updates the current state information.
    /// </summary>
    /// <param name="trigger"></param>
    /// <param name="args"></param>
    private async Task InternalFireOneAsync(TTrigger trigger, params object?[] args) {
        // If this is a trigger with parameters, we must validate the parameter(s)
        if (_triggerConfiguration.TryGetValue(trigger, out var configuration))
            configuration.ValidateParameters(args);

        var source = State;
        var representativeState = GetRepresentation(source);

        // Try to find a trigger handler, either in the current state or a super state.
        if (!representativeState.TryFindHandler(trigger, args, out var result)) {
            await _unhandledTriggerAction
                 .ExecuteAsync(representativeState.UnderlyingState, trigger, result?.UnmetGuardConditions)
                 .ConfigureAwait(false);
            return;
        }

        switch (result.Handler) {
            // Check if this trigger should be ignored
            case IgnoredTriggerBehaviour:
                return;
            // Handle special case, re-entry in superstate
            // Check if it is an internal transition, or a transition from one state to another.
            case ReentryTriggerBehaviour handler: {
                // Handle transition, and set new state
                var transition = new Transition(source, handler.Destination, trigger, args);
                await HandleReentryTriggerAsync(args, representativeState, transition).ConfigureAwait(false);
                break;
            }
            case DynamicTriggerBehaviour when result.Handler.ResultsInTransitionFrom(source, args, out var destination):
            case TransitioningTriggerBehaviour
                when result.Handler.ResultsInTransitionFrom(source, args, out destination): {
                // Handle transition, and set new state
                var transition = new Transition(source, destination, trigger, args);
                await HandleTransitioningTriggerAsync(args, representativeState, transition).ConfigureAwait(false);

                break;
            }
            case InternalTriggerBehaviour itb: {
                // Internal transitions does not update the current state, but must execute the associated action.
                var transition = new Transition(source, source, trigger, args);

                await itb.ExecuteAsync(transition, args).ConfigureAwait(false);
                break;
            }
            default:
                throw new InvalidOperationException("State machine configuration incorrect, no handler for trigger.");
        }
    }

    private async Task HandleReentryTriggerAsync(object?[]  args, StateRepresentation representativeState,
                                                 Transition transition) {
        StateRepresentation representation;
        transition = await representativeState.ExitAsync(transition).ConfigureAwait(false);
        var newRepresentation = GetRepresentation(transition.Destination);

        if (!transition.Source.Equals(transition.Destination)) {
            // Then Exit the final superstate
            transition = new Transition(transition.Destination, transition.Destination, transition.Trigger, args);
            await newRepresentation.ExitAsync(transition).ConfigureAwait(false);

            await _onTransitionedEvent.InvokeAsync(transition).ConfigureAwait(false);
            representation = await EnterStateAsync(newRepresentation, transition, args).ConfigureAwait(false);
            await _onTransitionCompletedEvent.InvokeAsync(transition).ConfigureAwait(false);
        } else {
            await _onTransitionedEvent.InvokeAsync(transition).ConfigureAwait(false);
            representation = await EnterStateAsync(newRepresentation, transition, args).ConfigureAwait(false);
            await _onTransitionCompletedEvent.InvokeAsync(transition).ConfigureAwait(false);
        }

        State = representation.UnderlyingState;
    }

    private async Task HandleTransitioningTriggerAsync(object?[]  args, StateRepresentation representativeState,
                                                       Transition transition) {
        transition = await representativeState.ExitAsync(transition).ConfigureAwait(false);

        State = transition.Destination;
        var newRepresentation = GetRepresentation(transition.Destination);

        //Alert all listeners of state transition
        await _onTransitionedEvent.InvokeAsync(transition).ConfigureAwait(false);
        var representation = await EnterStateAsync(newRepresentation, transition, args).ConfigureAwait(false);

        // Check if state has changed by entering new state (by firing triggers in OnEntry or such)
        if (!representation.UnderlyingState.Equals(State))
            // The state has been changed after entering the state, must update current state to new one
            State = representation.UnderlyingState;

        await _onTransitionCompletedEvent
             .InvokeAsync(new Transition(transition.Source, State, transition.Trigger, transition.Parameters))
             .ConfigureAwait(false);
    }


    private async Task<StateRepresentation> EnterStateAsync(StateRepresentation representation, Transition transition,
                                                            object?[]           args) {
        // Enter the new state
        await representation.EnterAsync(transition, args).ConfigureAwait(false);

        if (FiringMode.Immediate.Equals(_firingMode) && !State.Equals(transition.Destination)) {
            // This can happen if triggers are fired in OnEntry
            // Must update current representation with updated State
            representation = GetRepresentation(State);
            transition     = new Transition(transition.Source, State, transition.Trigger, args);
        }

        // Recursively enter substates that have an initial transition
        if (representation.HasInitialTransition) {
            // Verify that the target state is a substate
            // Check if state has substate(s), and if an initial transition(s) has been set up.
            if (!representation.GetSubstates()
                               .Any(s => s.UnderlyingState.Equals(representation.InitialTransitionTarget)))
                throw new
                    InvalidOperationException($"The target ({representation.InitialTransitionTarget}) for the initial transition is not a substate.");

            Debug.Assert(representation.InitialTransitionTarget != null);
            var initialTransition = new InitialTransition(transition.Source, representation.InitialTransitionTarget!,
                                                          transition.Trigger, args);
            representation = GetRepresentation(representation.InitialTransitionTarget!);

            // Alert all listeners of initial state transition
            await _onTransitionedEvent
                 .InvokeAsync(new Transition(transition.Destination, initialTransition.Destination, transition.Trigger,
                                             transition.Parameters)).ConfigureAwait(false);
            representation = await EnterStateAsync(representation, initialTransition, args).ConfigureAwait(false);
        }

        return representation;
    }

    /// <summary>
    ///     Override the default behaviour of throwing an exception when an unhandled trigger
    ///     is fired.
    /// </summary>
    /// <param name="unhandledTriggerAction"></param>
    public void OnUnhandledTriggerAsync(Func<TState, TTrigger, Task> unhandledTriggerAction) {
        if (unhandledTriggerAction == null) throw new ArgumentNullException(nameof(unhandledTriggerAction));
        _unhandledTriggerAction = new UnhandledTriggerAction((s, t, _) => unhandledTriggerAction(s, t));
    }

    /// <summary>
    ///     Override the default behaviour of throwing an exception when an unhandled trigger
    ///     is fired.
    /// </summary>
    /// <param name="unhandledTriggerAction">An asynchronous action to call when an unhandled trigger is fired.</param>
    public void OnUnhandledTriggerAsync(Func<TState, TTrigger, ICollection<string>?, Task> unhandledTriggerAction) {
        if (unhandledTriggerAction == null) throw new ArgumentNullException(nameof(unhandledTriggerAction));
        _unhandledTriggerAction = new UnhandledTriggerAction(unhandledTriggerAction);
    }

    /// <summary>
    ///     Registers an asynchronous callback that will be invoked every time the StateMachine
    ///     transitions from one state into another.
    /// </summary>
    /// <param name="onTransitionAction">
    ///     The asynchronous action to execute, accepting the details
    ///     of the transition.
    /// </param>
    public void OnTransitionedAsync(Func<Transition, Task> onTransitionAction) {
        if (onTransitionAction == null) throw new ArgumentNullException(nameof(onTransitionAction));
        _onTransitionedEvent.Register(onTransitionAction);
    }

    /// <summary>
    ///     Registers a callback that will be invoked every time the StateMachine
    ///     transitions from one state into another and all the OnEntryFrom etc methods
    ///     have been invoked
    /// </summary>
    /// <param name="onTransitionAction">
    ///     The asynchronous action to execute, accepting the details
    ///     of the transition.
    /// </param>
    public void OnTransitionCompletedAsync(Func<Transition, Task> onTransitionAction) {
        if (onTransitionAction == null) throw new ArgumentNullException(nameof(onTransitionAction));
        _onTransitionCompletedEvent.Register(onTransitionAction);
    }
}