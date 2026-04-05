using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger> : IDisposable
    {

        private bool _disposing;

        private class QueuedSerialTrigger : QueuedTrigger {
            public TaskCompletionSource<bool> TaskCompletionSource { get; set; }
        }

        private readonly Queue<QueuedSerialTrigger> _serialEventQueue = new Queue<QueuedSerialTrigger>();

        private Task _serialEventQueueProcessingTask;
        private CancellationTokenSource _serialEventQueueCancellationToken;

        /// <summary>
        /// Activates current state in asynchronous fashion. Actions associated with activating the current state
        /// will be invoked. The activation is idempotent and subsequent activation of the same current state 
        /// will not lead to re-execution of activation callbacks.
        /// </summary>
        public Task ActivateAsync()
        {
            var representativeState = GetRepresentation(State);
            return representativeState.ActivateAsync();
        }

        /// <summary>
        /// Deactivates current state in asynchronous fashion. Actions associated with deactivating the current state
        /// will be invoked. The deactivation is idempotent and subsequent deactivation of the same current state 
        /// will not lead to re-execution of deactivation callbacks.
        /// </summary>
        public Task DeactivateAsync()
        {
            var representativeState = GetRepresentation(State);
            return representativeState.DeactivateAsync();
        }

        /// <summary>
        /// The currently-permissible trigger values.
        /// </summary>
        public Task<IEnumerable<TTrigger>> PermittedTriggersAsync => GetPermittedTriggersAsync();

        /// <summary>
        /// The currently-permissible trigger values.
        /// </summary>
        public async Task<IEnumerable<TTrigger>> GetPermittedTriggersAsync(params object[] args)
        {
            return await CurrentRepresentation.GetPermittedTriggersAsync(args);
        }

        /// <summary> Obtains the curent serial events worker task. </summary>
        public Task GetSerialEventsWorkerTask() {
            return _serialEventQueueProcessingTask ?? Task.CompletedTask;
        }

        /// <summary> Resumes the execution of the event processing queue if not empty. </summary>
        public Task FireAsync() {
            return InternalFireAsync();
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
        /// <param name="trigger">The trigger to fire.</param>
        /// <param name="args">A variable-length parameters list containing arguments. </param>
        public Task FireAsync(TTrigger trigger, params object[] args)
        {
            return InternalFireAsync(trigger, args);
        }

        /// <summary>
        /// Transition from the current state via the specified trigger in async fashion.
        /// The target state is determined by the configuration of the current state.
        /// Actions associated with leaving the current state and entering the new one
        /// will be invoked.
        /// </summary>
        /// <param name="trigger">The trigger to fire.</param>
        /// <param name="args">A variable-length parameters list containing arguments. </param>
        /// <exception cref="System.InvalidOperationException">The current state does
        /// not allow the trigger to be fired.</exception>
        public Task FireAsync(TriggerWithParameters trigger, params object[] args)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            return InternalFireAsync(trigger.Trigger, args);
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
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

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
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));

            return InternalFireAsync(trigger.Trigger, arg0, arg1, arg2);
        }


        /// <summary>
        /// Fires the trigger and waits for the trigger to be processed.
        /// Relevant for FiringMode.Serial.
        /// </summary>
        /// <param name="trigger">The trigger to fire.</param>
        /// <param name="args">A variable-length parameters list containing arguments. </param>
        public Task FireAndWaitAsync(TTrigger trigger, params object[] args) {
            switch (_firingMode) {
                case FiringMode.Immediate:
                    return InternalFireOneAsync(trigger, args);
                case FiringMode.Queued:
                    return InternalFireQueuedAsync(trigger, args);
                case FiringMode.Serial:
                    return InternalFireSerialAsync(trigger, getTriggerCompletionTask: true, args);
                default:
                    // If something is completely messed up we let the user know ;-)
                    throw new InvalidOperationException("The firing mode has not been configured!");
            }
        }

        /// <summary>
        /// Determine how to Fire the trigger
        /// </summary>
        /// <param name="trigger">The trigger. </param>
        /// <param name="args">A variable-length parameters list containing arguments. </param>
        Task InternalFireAsync(TTrigger trigger, params object[] args)
        {
            switch (_firingMode)
            {
                case FiringMode.Immediate:
                    return InternalFireOneAsync(trigger, args);
                case FiringMode.Queued:
                    return InternalFireQueuedAsync(trigger, args);
                case FiringMode.Serial:
                    return InternalFireSerialAsync(trigger, getTriggerCompletionTask: false, args);
                default:
                    // If something is completely messed up we let the user know ;-)
                    throw new InvalidOperationException("The firing mode has not been configured!");
            }
        }


        /// <summary> Resumes the execution of the event processing queue if not empty. </summary>
        Task InternalFireAsync() {
            switch (_firingMode) {
                case FiringMode.Immediate:
                    return Task.CompletedTask;
                case FiringMode.Queued:
                    return InternalFireQueuedAsync();
                case FiringMode.Serial:
                    return InternalFireSerialAsync();
                default:
                    // If something is completely messed up we let the user know ;-)
                    throw new InvalidOperationException("The firing mode has not been configured!");
            }
        }

        /// <summary>
        /// Queue events and then fire in order on a separate worker thread.
        /// This returns immediately after queueing the trigger.
        /// This method is thread-safe.
        /// </summary>
        /// <param name="trigger">  The trigger. </param>
        /// <param name="getTriggerCompletionTask">  If true, returns the task associated with the processing of the trigger. </param>
        /// <param name="args">     A variable-length parameters list containing arguments. </param>
        Task InternalFireSerialAsync(TTrigger trigger, bool getTriggerCompletionTask, params object[] args)
        {

            if (_disposing)
                throw new ObjectDisposedException("State machine");

            var taskCompletionSource = new TaskCompletionSource<bool>();

            lock (_serialModeLock) 
            {
                _serialEventQueue.Enqueue(new QueuedSerialTrigger { Trigger = trigger, Args = args, TaskCompletionSource = taskCompletionSource });

                if (_firing) 
                    return getTriggerCompletionTask ? taskCompletionSource.Task : Task.CompletedTask;

                _firing = true;
            }

            _serialEventQueueProcessingTask = InternalFireSerialAsync_RunProcessingQueue();

            return getTriggerCompletionTask ? taskCompletionSource.Task : Task.CompletedTask;
        }

        /// <summary> Resumes the execution if the event queue is not empty </summary>
        Task InternalFireSerialAsync()
        {
            lock (_serialModeLock)
            {
                if (_firing || _serialEventQueue.Count == 0)
                    return Task.CompletedTask;

                _firing = true;
            }

            _serialEventQueueProcessingTask = InternalFireSerialAsync_RunProcessingQueue();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Starts a serial event processing thread.
        /// Only call if you aquired "_firing = true" inside a lock.
        /// </summary>
        Task InternalFireSerialAsync_RunProcessingQueue()
        {

            _serialEventQueueCancellationToken = new CancellationTokenSource();

            return Task.Run(
                async () => 
                {

                    QueuedSerialTrigger queuedEvent = null;
                    Task currentTask = null;

                    try 
                    {
                        if (_disposing)
                            throw new ObjectDisposedException("State machine");

                        while (true) 
                        {

                            lock (_serialModeLock)
                            {

                                if (_serialEventQueue.Count == 0)
                                {
                                    _firing = false;
                                    break;
                                }

                                queuedEvent = _serialEventQueue.Dequeue();
                            }

                            currentTask = InternalFireOneAsync(queuedEvent.Trigger, queuedEvent.Args);

                            await currentTask;

                            queuedEvent.TaskCompletionSource.SetResult(true);

                            _serialEventQueueCancellationToken.Token.ThrowIfCancellationRequested();
                        }
                    } 
                    catch
                    {

                        lock (_serialModeLock)
                        {
                            _firing = false;
                        }

                        if (currentTask?.IsCanceled == true)
                        {
                            queuedEvent?.TaskCompletionSource.SetCanceled();
                        } 
                        else if (currentTask?.IsFaulted == true) 
                        {
                            queuedEvent?.TaskCompletionSource.SetException(currentTask.Exception);
                        }

                        throw;
                    }
                }
            );
        }

        /// <summary>
        /// Queue events and then fire in order.
        /// If only one event is queued, this behaves identically to the non-queued version.
        /// </summary>
        /// <param name="trigger">  The trigger. </param>
        /// <param name="args">     A variable-length parameters list containing arguments. </param>
        Task InternalFireQueuedAsync(TTrigger trigger, params object[] args)
        {

            _eventQueue.Enqueue(new QueuedTrigger { Trigger = trigger, Args = args });

            return InternalFireQueuedAsync();
        }

        /// <summary> Processes the event queue </summary>
        async Task InternalFireQueuedAsync() {

            if (_firing) {
                return;
            }

            try {
                _firing = true;

                while (_eventQueue.Count != 0) {
                    var queuedEvent = _eventQueue.Dequeue();
                    await InternalFireOneAsync(queuedEvent.Trigger, queuedEvent.Args).ConfigureAwait(RetainSynchronizationContext);
                }
            } finally {
                _firing = false;
            }
        }

        private async Task InternalFireOneAsync(TTrigger trigger, params object[] args)
        {
            // If this is a trigger with parameters, we must validate the parameter(s)
            if (_triggerConfiguration.TryGetValue(trigger, out TriggerWithParameters configuration))
            {
                configuration.ValidateParameters(args);
            }

            var source = State;
            var representativeState = GetRepresentation(source);

            // Try to find a trigger handler, either in the current state or a super state.
            var foundHandler = await representativeState.TryFindHandlerAsync(trigger, args);

            if (foundHandler == null || foundHandler.UnmetGuardConditions.Any())
            {
                await _unhandledTriggerAction.ExecuteAsync(representativeState.UnderlyingState, trigger, null);
                return;
            }

            if (foundHandler.Handler != null)
            {
                await ProcessHandler(foundHandler.Handler, trigger, args);
            }
            else if (foundHandler.HandlerAsync != null)
            {
                await ProcessHandler(foundHandler.HandlerAsync, trigger, args);
            }
            else
            {
                throw new InvalidOperationException("TriggerBehaviourResult found, but no suitable Handler");
            }
        }

        private async Task ProcessHandler<TTriggerBehaviour>(TTriggerBehaviour triggerBehaviour, TTrigger trigger, object[] args) where TTriggerBehaviour : TriggerBehaviourBase
        {
            var source = State;
            var representativeState = GetRepresentation(source);

            switch (triggerBehaviour)
            {
                // Check if this trigger should be ignored
                case IgnoredTriggerBehaviour _:
                    return;
                // Handle special case, re-entry in superstate
                // Check if it is an internal transition, or a transition from one state to another.
                case ReentryTriggerBehaviour handler:
                    {
                        // Handle transition, and set new state
                        var transition = new Transition(source, handler.Destination, trigger, args);
                        await HandleReentryTriggerAsync(args, representativeState, transition);
                        break;
                    }
                case ReentryTriggerBehaviourAsync handler:
                {
                    // Handle transition, and set new state
                    var transition = new Transition(source, handler.Destination, trigger, args);
                    await HandleReentryTriggerAsync(args, representativeState, transition);
                    break;
                }
                case DynamicTriggerBehaviourAsync asyncHandler:
                    {
                        var destination = await asyncHandler.GetDestinationState(source, args);
                        // Handle transition, and set new state; reentry is permitted from dynamic trigger behaviours.
                        var transition = new Transition(source, destination, trigger, args);
                        await HandleTransitioningTriggerAsync(args, representativeState, transition);

                        break;
                    }
                case DynamicTriggerBehaviour handler:
                    {
                        handler.GetDestinationState(source, args, out var destination);
                        // Handle transition, and set new state; reentry is permitted from dynamic trigger behaviours.
                        var transition = new Transition(source, destination, trigger, args);
                        await HandleTransitioningTriggerAsync(args, representativeState, transition);

                        break;
                    }
                case TransitioningTriggerBehaviour handler:
                    {
                        // If a trigger was found on a superstate that would cause unintended reentry, don't trigger.
                        if (source.Equals(handler.Destination))
                            break;

                        // Handle transition, and set new state
                        var transition = new Transition(source, handler.Destination, trigger, args);
                        await HandleTransitioningTriggerAsync(args, representativeState, transition);

                        break;
                    }
                case TransitioningTriggerBehaviourAsync handler:
                {
                    // If a trigger was found on a superstate that would cause unintended reentry, don't trigger.
                    if (source.Equals(handler.Destination))
                        break;

                    // Handle transition, and set new state
                    var transition = new Transition(source, handler.Destination, trigger, args);
                    await HandleTransitioningTriggerAsync(args, representativeState, transition);

                    break;
                }
                case InternalTriggerBehaviour itb:
                    {
                        // Internal transitions does not update the current state, but must execute the associated action.
                        var transition = new Transition(source, source, trigger, args);

                        if (itb is InternalTriggerBehaviour.Async ita)
                            await ita.ExecuteAsync(transition, args);
                        else
                            if (RetainSynchronizationContext)
                            await Task.Factory.StartNew(() => itb.Execute(transition, args),
                                CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.FromCurrentSynchronizationContext());
                        else
                            await Task.Run(() => itb.Execute(transition, args));
                        break;
                    }
                default:
                    throw new InvalidOperationException("State machine configuration incorrect, no handler for trigger.");
            }
        }

        private async Task HandleReentryTriggerAsync(object[] args, StateRepresentation representativeState, Transition transition)
        {
            StateRepresentation representation;
            transition = await representativeState.ExitAsync(transition);
            var newRepresentation = GetRepresentation(transition.Destination);

            if (!transition.Source.Equals(transition.Destination))
            {
                // Then Exit the final superstate
                transition = new Transition(transition.Destination, transition.Destination, transition.Trigger, args);
                await newRepresentation.ExitAsync(transition);

                await _onTransitionedEvent.InvokeAsync(transition, RetainSynchronizationContext);
                representation = await EnterStateAsync(newRepresentation, transition, args);
                await _onTransitionCompletedEvent.InvokeAsync(transition, RetainSynchronizationContext);
            }
            else
            {
                await _onTransitionedEvent.InvokeAsync(transition, RetainSynchronizationContext);
                representation = await EnterStateAsync(newRepresentation, transition, args);
                await _onTransitionCompletedEvent.InvokeAsync(transition, RetainSynchronizationContext);
            }
            State = representation.UnderlyingState;
        }

        private async Task HandleTransitioningTriggerAsync(object[] args, StateRepresentation representativeState, Transition transition)
        {
            transition = await representativeState.ExitAsync(transition);

            State = transition.Destination;
            var newRepresentation = GetRepresentation(transition.Destination);

            //Alert all listeners of state transition
            await _onTransitionedEvent.InvokeAsync(transition, RetainSynchronizationContext);
            var representation = await EnterStateAsync(newRepresentation, transition, args);

            // Check if state has changed by entering new state (by firing triggers in OnEntry or such)
            if (!representation.UnderlyingState.Equals(State))
            {
                // The state has been changed after entering the state, must update current state to new one
                State = representation.UnderlyingState;
            }

            await _onTransitionCompletedEvent.InvokeAsync(new Transition(transition.Source, State, transition.Trigger, transition.Parameters), RetainSynchronizationContext);
        }


        private async Task<StateRepresentation> EnterStateAsync(StateRepresentation representation, Transition transition, object[] args)
        {
            // Enter the new state
            await representation.EnterAsync(transition, args);

            if (FiringMode.Immediate.Equals(_firingMode) && !State.Equals(transition.Destination))
            {
                // This can happen if triggers are fired in OnEntry
                // Must update current representation with updated State
                representation = GetRepresentation(State);
                transition = new Transition(transition.Source, State, transition.Trigger, args);
            }

            // Recursively enter substates that have an initial transition
            if (representation.HasInitialTransition)
            {
                // Verify that the target state is a substate
                // Check if state has substate(s), and if an initial transition(s) has been set up.
                if (!representation.GetSubstates().Any(s => s.UnderlyingState.Equals(representation.InitialTransitionTarget)))
                {
                    throw new InvalidOperationException($"The target ({representation.InitialTransitionTarget}) for the initial transition is not a substate.");
                }

                var initialTransition = new InitialTransition(transition.Source, representation.InitialTransitionTarget, transition.Trigger, args);
                representation = GetRepresentation(representation.InitialTransitionTarget);

                // Alert all listeners of initial state transition
                await _onTransitionedEvent.InvokeAsync(new Transition(transition.Destination, initialTransition.Destination, transition.Trigger, transition.Parameters), RetainSynchronizationContext);
                representation = await EnterStateAsync(representation, initialTransition, args);
            }

            return representation;
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

        /// <summary>
        /// Registers a callback that will be invoked every time the statemachine
        /// transitions from one state into another and all the OnEntryFrom etc methods
        /// have been invoked
        /// </summary>
        /// <param name="onTransitionAction">The asynchronous action to execute, accepting the details
        /// of the transition.</param>
        public void OnTransitionCompletedAsync(Func<Transition, Task> onTransitionAction)
        {
            if (onTransitionAction == null) throw new ArgumentNullException(nameof(onTransitionAction));
            _onTransitionCompletedEvent.Register(onTransitionAction);
        }

        /// <summary>
        /// Unregisters a previously registered callback to prevent further events from
        /// being raised when the state machine transitions from one state into another.
        /// </summary>
        /// <param name="onTransitionAction"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void OnTransitionedAsyncUnregister(Func<Transition, Task> onTransitionAction)
        {
            if (onTransitionAction == null) throw new ArgumentNullException(nameof(onTransitionAction));
            _onTransitionedEvent.Unregister(onTransitionAction);
        }

        /// <summary>
        /// Unregisters a previously registered callback to prevent further events from
        /// being raised when the state machine has completed its state transition.
        /// </summary>
        /// <param name="onTransitionAction"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void OnTransitionCompletedAsyncUnregister(Func<Transition, Task> onTransitionAction)
        {
            if (onTransitionAction == null) throw new ArgumentNullException(nameof(onTransitionAction));
            _onTransitionCompletedEvent.Unregister(onTransitionAction);
        }

        /// <summary>
        /// Dispose the state machine
        /// </summary>
        public void Dispose()
        {
            _disposing = true;

            if (_firingMode == FiringMode.Serial)
            {
                _serialEventQueueCancellationToken?.Cancel();

                if (!_serialEventQueueProcessingTask?.IsCompleted == true)
                {
                    try {
                        _serialEventQueueProcessingTask.Wait();
                    } catch {
                        //Since we are disposing, ignore any errors
                    }
                }
            }
        }
    }
}
