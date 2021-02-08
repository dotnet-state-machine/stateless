﻿using Stateless.Reflection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Stateless
{
    /// <summary>
    /// Models behaviour as transitions between a finite set of states.
    /// </summary>
    /// <typeparam name="TState">The type used to represent the states.</typeparam>
    /// <typeparam name="TTrigger">The type used to represent the triggers that cause state transitions.</typeparam>
    public partial class StateMachine<TState, TTrigger>
    {
        private readonly IDictionary<TState, StateRepresentation> _stateConfiguration = new Dictionary<TState, StateRepresentation>();
        private readonly IDictionary<TTrigger, TriggerWithParameters> _triggerConfiguration = new Dictionary<TTrigger, TriggerWithParameters>();
        private readonly Func<TState> _stateAccessor;
        private readonly Action<TState> _stateMutator;
        private UnhandledTriggerAction _unhandledTriggerAction;
        private readonly OnTransitionedEvent _onTransitionedEvent;
        private readonly OnTransitionedEvent _onTransitionCompletedEvent;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);
        private class QueuedTrigger
        {
            public TTrigger Trigger { get; set; }
            public object[] Args { get; set; }
        }

        private readonly Queue<QueuedTrigger> _eventQueue = new Queue<QueuedTrigger>();
        private bool _firing;

        /// <summary>
        /// Construct a state machine with external state storage.
        /// </summary>
        /// <param name="stateAccessor">A function that will be called to read the current state value.</param>
        /// <param name="stateMutator">An action that will be called to write new state values.</param>
        public StateMachine(Func<TState> stateAccessor, Action<TState> stateMutator) : this()
        {
            _stateAccessor = stateAccessor;
            _stateMutator = stateMutator;
        }

        /// <summary>
        /// Construct a state machine.
        /// </summary>
        /// <param name="initialState">The initial state.</param>
        public StateMachine(TState initialState) : this()
        {
            var reference = new StateReference { State = initialState };
            _stateAccessor = () => reference.State;
            _stateMutator = s => reference.State = s;
        }


        /// <summary>
        /// Default constructor
        /// </summary>
        StateMachine()
        {
            _unhandledTriggerAction = new UnhandledTriggerAction.Sync(DefaultUnhandledTriggerAction);
            _onTransitionedEvent = new OnTransitionedEvent();
            _onTransitionCompletedEvent = new OnTransitionedEvent();
        }

        /// <summary>
        /// The current state.
        /// </summary>
        public TState State
        {
            get
            {
                return _stateAccessor();
            }
            private set
            {
                _stateMutator(value);
            }
        }

        /// <summary>
        /// The currently-permissible trigger values.
        /// </summary>
        public IEnumerable<TTrigger> PermittedTriggers
        {
            get
            {
                return GetPermittedTriggers();
            }
        }

        /// <summary>
        /// The currently-permissible trigger values.
        /// </summary>
        public IEnumerable<TTrigger> GetPermittedTriggers(params object[] args)
        {
            return CurrentRepresentation.GetPermittedTriggers(args);
        }

        private StateRepresentation CurrentRepresentation
        {
            get
            {
                return GetRepresentation(State);
            }
        }

        /// <summary>
        /// Provides an info object which exposes the states, transitions, and actions of this machine.
        /// </summary>
        public StateMachineInfo GetInfo()
        {
            var initialState = StateInfo.CreateStateInfo(new StateRepresentation(State));

            var representations = _stateConfiguration.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var behaviours = _stateConfiguration.SelectMany(kvp => kvp.Value.TriggerBehaviours.SelectMany(b => b.Value.OfType<TransitioningTriggerBehaviour>().Select(tb => tb.Destination))).ToList();
            behaviours.AddRange(_stateConfiguration.SelectMany(kvp => kvp.Value.TriggerBehaviours.SelectMany(b => b.Value.OfType<ReentryTriggerBehaviour>().Select(tb => tb.Destination))).ToList());

            var reachable = behaviours
                .Distinct()
                .Except(representations.Keys)
                .Select(underlying => new StateRepresentation(underlying))
                .ToArray();

            foreach (var representation in reachable)
                representations.Add(representation.UnderlyingState, representation);

            var info = representations.ToDictionary(kvp => kvp.Key, kvp => StateInfo.CreateStateInfo(kvp.Value));

            foreach (var state in info)
                StateInfo.AddRelationships(state.Value, representations[state.Key], k => info[k]);

            return new StateMachineInfo(info.Values, typeof(TState), typeof(TTrigger), initialState);
        }

        private StateRepresentation GetRepresentation(TState state)
        {
            if (!_stateConfiguration.TryGetValue(state, out StateRepresentation result))
            {
                result = new StateRepresentation(state);
                _stateConfiguration.Add(state, result);
            }

            return result;
        }

        /// <summary>
        /// Begin configuration of the entry/exit actions and allowed transitions
        /// when the state machine is in a particular state.
        /// </summary>
        /// <param name="state">The state to configure.</param>
        /// <returns>A configuration object through which the state can be configured.</returns>
        public StateConfiguration Configure(TState state)
        {
            return new StateConfiguration(this, GetRepresentation(state), GetRepresentation);
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
        public void Fire(TTrigger trigger)
        {
            InternalFire(trigger, Array.Empty<object>());
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
        public async Task FireAsync(TTrigger trigger)
        {
            await Task.Run(() => InternalFire(trigger, Array.Empty<object>()));
        }

        /// <summary>
        /// Transition from the current state via the specified trigger.
        /// The target state is determined by the configuration of the current state.
        /// Actions associated with leaving the current state and entering the new one
        /// will be invoked.
        /// </summary>
        /// <param name="trigger">The trigger to fire.</param>
        /// <param name="args">A variable-length parameters list containing arguments. </param>
        /// <exception cref="System.InvalidOperationException">The current state does
        /// not allow the trigger to be fired.</exception>
        public void Fire(TriggerWithParameters trigger, params object[] args)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));
            InternalFire(trigger.Trigger, args);
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
        public void Fire<TArg0>(TriggerWithParameters<TArg0> trigger, TArg0 arg0)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));
            InternalFire(trigger.Trigger, arg0);
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
        public async Task FireAsync<TArg0>(TriggerWithParameters<TArg0> trigger, TArg0 arg0)
        {
            await Task.Run(() => Fire(trigger, arg0));
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
        public void Fire<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, TArg0 arg0, TArg1 arg1)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));
            InternalFire(trigger.Trigger, arg0, arg1);
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
        public async Task FireAsync<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, TArg0 arg0, TArg1 arg1)
        {
            await Task.Run(() => Fire(trigger, arg0, arg1));
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
        public void Fire<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, TArg0 arg0, TArg1 arg1, TArg2 arg2)
        {
            if (trigger == null) throw new ArgumentNullException(nameof(trigger));
            InternalFire(trigger.Trigger, arg0, arg1, arg2);
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
        public async Task FireAsync<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, TArg0 arg0, TArg1 arg1, TArg2 arg2)
        {
            await Task.Run(() => Fire(trigger, arg0, arg1, arg2));
        }

        /// <summary>
        /// Activates current state. Actions associated with activating the current state
        /// will be invoked. The activation is idempotent and subsequent activation of the same current state
        /// will not lead to re-execution of activation callbacks.
        /// </summary>
        public void Activate()
        {
            var representativeState = GetRepresentation(State);
            representativeState.Activate();
        }

        /// <summary>
        /// Activates current state. Actions associated with activating the current state
        /// will be invoked. The activation is idempotent and subsequent activation of the same current state
        /// will not lead to re-execution of activation callbacks.
        /// </summary>
        public async Task ActivateAsync()
        {
            await Task.Run(() => Activate());
        }

        /// <summary>
        /// Deactivates current state. Actions associated with deactivating the current state
        /// will be invoked. The deactivation is idempotent and subsequent deactivation of the same current state
        /// will not lead to re-execution of deactivation callbacks.
        /// </summary>
        public void Deactivate()
        {
            var representativeState = GetRepresentation(State);
            representativeState.Deactivate();
        }

        /// <summary>
        /// Deactivates current state. Actions associated with deactivating the current state
        /// will be invoked. The deactivation is idempotent and subsequent deactivation of the same current state
        /// will not lead to re-execution of deactivation callbacks.
        /// </summary>
        public async Task DeactivateAsync()
        {
            await Task.Run(() => Deactivate());
        }

        /// <summary>
        /// Queue events and then fire in order.
        /// If only one event is queued, this behaves identically to the non-queued version.
        /// </summary>
        /// <param name="trigger">  The trigger. </param>
        /// <param name="args">     A variable-length parameters list containing arguments. </param>
        private void InternalFire(TTrigger trigger, params object[] args)
        {
            // Add trigger to queue
            _eventQueue.Enqueue(new QueuedTrigger { Trigger = trigger, Args = args });

            _semaphore.Wait();
            // If a trigger is already being handled then the trigger will be queued (FIFO) and processed later.
            if (_firing)
            {
                _semaphore.Release();
                return;
            }
            try
            {
                _firing = true;
                _semaphore.Release();

                // Empty queue for triggers
                while (_eventQueue.Any())
                {
                    var queuedEvent = _eventQueue.Dequeue();
                    InternalFireOne(queuedEvent.Trigger, queuedEvent.Args);
                }
            }
            finally
            {
                _semaphore.Wait();
                _firing = false;
                _semaphore.Release();
            }
        }

        /// <summary>
        /// This method handles the execution of a trigger handler. It finds a
        /// handle, then updates the current state information.
        /// </summary>
        /// <param name="trigger"></param>
        /// <param name="args"></param>
        void InternalFireOne(TTrigger trigger, params object[] args)
        {
            // If this is a trigger with parameters, we must validate the parameter(s)
            if (_triggerConfiguration.TryGetValue(trigger, out TriggerWithParameters configuration))
                configuration.ValidateParameters(args);

            var source = State;
            var representativeState = GetRepresentation(source);

            // Try to find a trigger handler, either in the current state or a super state.
            if (!representativeState.TryFindHandler(trigger, args, out TriggerBehaviourResult result))
            {
                _unhandledTriggerAction.Execute(representativeState.UnderlyingState, trigger, result?.UnmetGuardConditions);
                return;
            }

            switch (result.Handler)
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

                        // If trigger handler has action, execute it
                        if (result.Handler.HasAction())
                        {
                            result.Handler.ExecuteAction(transition, args);
                        }
                        HandleReentryTrigger(args, representativeState, transition);
                        break;
                    }
                case DynamicTriggerBehaviour _ when (result.Handler.ResultsInTransitionFrom(source, args, out var destination)):
                case TransitioningTriggerBehaviour _ when (result.Handler.ResultsInTransitionFrom(source, args, out destination)):
                    {
                        // Handle transition, and set new state
                        var transition = new Transition(source, destination, trigger, args);
                        // If trigger handler has action, execute it
                        if (result.Handler.HasAction())
                        {
                            result.Handler.ExecuteAction(transition, args);
                        }
                        HandleTransitioningTrigger(args, representativeState, transition);

                        break;
                    }
                case InternalTriggerBehaviour _:
                    {
                        // Internal transitions does not update the current state, but must execute the associated action.
                        var transition = new Transition(source, source, trigger, args);
                        // If trigger handler has action, execute it
                        if (result.Handler.HasAction())
                        {
                            result.Handler.ExecuteAction(transition, args);
                        }
                        //CurrentRepresentation.InternalAction(transition, args);
                        break;
                    }
                default:
                    throw new InvalidOperationException("State machine configuration incorrect, no handler for trigger.");
            }
        }

        private void HandleReentryTrigger(object[] args, StateRepresentation representativeState, Transition transition)
        {
            StateRepresentation representation;
            transition = representativeState.Exit(transition);
            var newRepresentation = GetRepresentation(transition.Destination);

            if (!transition.Source.Equals(transition.Destination))
            {
                // Then Exit the final superstate
                transition = new Transition(transition.Destination, transition.Destination, transition.Trigger, args);
                newRepresentation.Exit(transition);

                _onTransitionedEvent.Invoke(transition);
                representation = EnterState(newRepresentation, transition, args);
                _onTransitionCompletedEvent.Invoke(transition);

            }
            else
            {
                _onTransitionedEvent.Invoke(transition);
                representation = EnterState(newRepresentation, transition, args);
                _onTransitionCompletedEvent.Invoke(transition);
            }
            State = representation.UnderlyingState;
        }

        private void HandleTransitioningTrigger(object[] args, StateRepresentation representativeState, Transition transition)
        {
            transition = representativeState.Exit(transition);

            State = transition.Destination;
            var newRepresentation = GetRepresentation(transition.Destination);

            //Alert all listeners of state transition
            _onTransitionedEvent.Invoke(transition);
            var representation = EnterState(newRepresentation, transition, args);

            // Check if state has changed by entering new state (by fireing triggers in OnEntry or such)
            if (!representation.UnderlyingState.Equals(State))
            {
                // The state has been changed after entering the state, must update current state to new one
                State = representation.UnderlyingState;
            }

            _onTransitionCompletedEvent.Invoke(new Transition(transition.Source, State, transition.Trigger, transition.Parameters));
        }

        private StateRepresentation EnterState(StateRepresentation representation, Transition transition, object[] args)
        {
            // Enter the new state
            representation.Enter(transition, args);

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
                _onTransitionedEvent.Invoke(new Transition(transition.Destination, initialTransition.Destination, transition.Trigger, transition.Parameters));
                representation = EnterState(representation, initialTransition, args);
            }

            return representation;
        }

        /// <summary>
        /// Override the default behaviour of throwing an exception when an unhandled trigger
        /// is fired.
        /// </summary>
        /// <param name="unhandledTriggerAction">An action to call when an unhandled trigger is fired.</param>
        public void OnUnhandledTrigger(Action<TState, TTrigger> unhandledTriggerAction)
        {
            if (unhandledTriggerAction == null) throw new ArgumentNullException(nameof(unhandledTriggerAction));
            _unhandledTriggerAction = new UnhandledTriggerAction.Sync((s, t, c) => unhandledTriggerAction(s, t));
        }

        /// <summary>
        /// Override the default behaviour of throwing an exception when an unhandled trigger
        /// is fired.
        /// </summary>
        /// <param name="unhandledTriggerAction">An action to call when an unhandled trigger is fired.</param>
        public void OnUnhandledTrigger(Action<TState, TTrigger, ICollection<string>> unhandledTriggerAction)
        {
            if (unhandledTriggerAction == null) throw new ArgumentNullException(nameof(unhandledTriggerAction));
            _unhandledTriggerAction = new UnhandledTriggerAction.Sync(unhandledTriggerAction);
        }

        /// <summary>
        /// Override the default behaviour of throwing an exception when an unhandled trigger
        /// is fired.
        /// </summary>
        /// <param name="unhandledTriggerAction"></param>
        public void OnUnhandledTrigger(Func<TState, TTrigger, Task> unhandledTriggerAction)
        {
            if (unhandledTriggerAction == null) throw new ArgumentNullException(nameof(unhandledTriggerAction));
            _unhandledTriggerAction = new UnhandledTriggerAction.Async((s, t, c) => unhandledTriggerAction(s, t));
        }

        /// <summary>
        /// Override the default behaviour of throwing an exception when an unhandled trigger
        /// is fired.
        /// </summary>
        /// <param name="unhandledTriggerAction">An asynchronous action to call when an unhandled trigger is fired.</param>
        public void OnUnhandledTrigger(Func<TState, TTrigger, ICollection<string>, Task> unhandledTriggerAction)
        {
            if (unhandledTriggerAction == null) throw new ArgumentNullException(nameof(unhandledTriggerAction));
            _unhandledTriggerAction = new UnhandledTriggerAction.Async(unhandledTriggerAction);
        }
        /// <summary>
        /// Determine if the state machine is in the supplied state.
        /// </summary>
        /// <param name="state">The state to test for.</param>
        /// <returns>True if the current state is equal to, or a substate of,
        /// the supplied state.</returns>
        public bool IsInState(TState state)
        {
            return CurrentRepresentation.IsIncludedIn(state);
        }

        /// <summary>
        /// Returns true if <paramref name="trigger"/> can be fired
        /// in the current state.
        /// </summary>
        /// <param name="trigger">Trigger to test.</param>
        /// <returns>True if the trigger can be fired, false otherwise.</returns>
        public bool CanFire(TTrigger trigger)
        {
            return CurrentRepresentation.CanHandle(trigger);
        }

        /// <summary>
        /// A human-readable representation of the state machine.
        /// </summary>
        /// <returns>A description of the current state and permitted triggers.</returns>
        public override string ToString()
        {
            return string.Format(
                "StateMachine {{ State = {0}, PermittedTriggers = {{ {1} }}}}",
                State,
                string.Join(", ", GetPermittedTriggers().Select(t => t.ToString()).ToArray()));
        }

        /// <summary>
        /// Specify the arguments that must be supplied when a specific trigger is fired.
        /// </summary>
        /// <param name="trigger">The underlying trigger value.</param>
        /// <param name="argumentTypes">The argument types expected by the trigger.</param>
        /// <returns>An object that can be passed to the Fire() method in order to
        /// fire the parametrised trigger.</returns>
        public TriggerWithParameters SetTriggerParameters(TTrigger trigger, params Type[] argumentTypes)
        {
            var configuration = new TriggerWithParameters(trigger, argumentTypes);
            SaveTriggerConfiguration(configuration);
            return configuration;
        }

        /// <summary>
        /// Specify the arguments that must be supplied when a specific trigger is fired.
        /// </summary>
        /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
        /// <param name="trigger">The underlying trigger value.</param>
        /// <returns>An object that can be passed to the Fire() method in order to
        /// fire the parameterised trigger.</returns>
        public TriggerWithParameters<TArg0> SetTriggerParameters<TArg0>(TTrigger trigger)
        {
            var configuration = new TriggerWithParameters<TArg0>(trigger);
            SaveTriggerConfiguration(configuration);
            return configuration;
        }

        /// <summary>
        /// Specify the arguments that must be supplied when a specific trigger is fired.
        /// </summary>
        /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
        /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
        /// <param name="trigger">The underlying trigger value.</param>
        /// <returns>An object that can be passed to the Fire() method in order to
        /// fire the parameterised trigger.</returns>
        public TriggerWithParameters<TArg0, TArg1> SetTriggerParameters<TArg0, TArg1>(TTrigger trigger)
        {
            var configuration = new TriggerWithParameters<TArg0, TArg1>(trigger);
            SaveTriggerConfiguration(configuration);
            return configuration;
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
        public TriggerWithParameters<TArg0, TArg1, TArg2> SetTriggerParameters<TArg0, TArg1, TArg2>(TTrigger trigger)
        {
            var configuration = new TriggerWithParameters<TArg0, TArg1, TArg2>(trigger);
            SaveTriggerConfiguration(configuration);
            return configuration;
        }

        void SaveTriggerConfiguration(TriggerWithParameters trigger)
        {
            if (_triggerConfiguration.ContainsKey(trigger.Trigger))
                throw new InvalidOperationException(
                    string.Format(StateMachineResources.CannotReconfigureParameters, trigger));

            _triggerConfiguration.Add(trigger.Trigger, trigger);
        }

        void DefaultUnhandledTriggerAction(TState state, TTrigger trigger, ICollection<string> unmetGuardConditions)
        {
            if (unmetGuardConditions?.Any() ?? false)
                throw new InvalidOperationException(
                    string.Format(
                        StateMachineResources.NoTransitionsUnmetGuardConditions,
                        trigger, state, string.Join(", ", unmetGuardConditions)));

            throw new InvalidOperationException(
                string.Format(
                    StateMachineResources.NoTransitionsPermitted,
                    trigger, state));
        }

        /// <summary>
        /// Registers a callback that will be invoked every time the state machine
        /// transitions from one state into another.
        /// </summary>
        /// <param name="onTransitionAction">The action to execute, accepting the details
        /// of the transition.</param>
        public void OnTransitioned(Action<Transition> onTransitionAction)
        {
            if (onTransitionAction == null) throw new ArgumentNullException(nameof(onTransitionAction));
            _onTransitionedEvent.Register(onTransitionAction);
        }

        /// <summary>
        /// Registers an asynchronous callback that will be invoked every time the statemachine
        /// transitions from one state into another.
        /// </summary>
        /// <param name="onTransitionAction">The asynchronous action to execute, accepting the details
        /// of the transition.</param>
        public void OnTransitioned(Func<Transition, Task> onTransitionAction)
        {
            if (onTransitionAction == null) throw new ArgumentNullException(nameof(onTransitionAction));
            _onTransitionedEvent.Register(onTransitionAction);
        }

        /// <summary>
        /// Registers a callback that will be invoked every time the statemachine
        /// transitions from one state into another and all the OnEntryFrom etc methods
        /// have been invoked
        /// </summary>
        /// <param name="onTransitionAction">The action to execute, accepting the details
        /// of the transition.</param>
        public void OnTransitionCompleted(Action<Transition> onTransitionAction)
        {
            if (onTransitionAction == null) throw new ArgumentNullException(nameof(onTransitionAction));
            _onTransitionCompletedEvent.Register(onTransitionAction);
        }

        /// <summary>
        /// Registers a callback that will be invoked every time the statemachine
        /// transitions from one state into another and all the OnEntryFrom etc methods
        /// have been invoked
        /// </summary>
        /// <param name="onTransitionAction">The asynchronous action to execute, accepting the details
        /// of the transition.</param>
        public void OnTransitionCompleted(Func<Transition, Task> onTransitionAction)
        {
            if (onTransitionAction == null) throw new ArgumentNullException(nameof(onTransitionAction));
            _onTransitionCompletedEvent.Register(onTransitionAction);
        }
    }
}
