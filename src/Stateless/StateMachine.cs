﻿using Stateless.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless
{
    /// <summary>
    /// Enum for the different modes used when Fire-ing a trigger
    /// </summary>
    public enum FiringMode
    {
        /// <summary> Use immediate mode when the queuing of trigger events are not needed. Care must be taken when using this mode, as there is no run-to-completion guaranteed.</summary>
        Immediate,
        /// <summary> Use the queued Fire-ing mode when run-to-completion is required. This is the recommended mode.</summary>
        Queued
    }

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
        private readonly FiringMode _firingMode;

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
        public StateMachine(Func<TState> stateAccessor, Action<TState> stateMutator) : this(stateAccessor, stateMutator, FiringMode.Queued)
        {
        }

        /// <summary>
        /// Construct a state machine.
        /// </summary>
        /// <param name="initialState">The initial state.</param>
        public StateMachine(TState initialState) : this(initialState, FiringMode.Queued)
        {
        }

        /// <summary>
        /// Construct a state machine with external state storage.
        /// </summary>
        /// <param name="stateAccessor">A function that will be called to read the current state value.</param>
        /// <param name="stateMutator">An action that will be called to write new state values.</param>
        /// <param name="firingMode">Optional specification of fireing mode.</param>
        public StateMachine(Func<TState> stateAccessor, Action<TState> stateMutator, FiringMode firingMode) : this()
        {
            _stateAccessor = stateAccessor ?? throw new ArgumentNullException(nameof(stateAccessor));
            _stateMutator = stateMutator ?? throw new ArgumentNullException(nameof(stateMutator));

            _firingMode = firingMode;
        }

        /// <summary>
        /// Construct a state machine.
        /// </summary>
        /// <param name="initialState">The initial state.</param>
        /// <param name="firingMode">Optional specification of fireing mode.</param>
        public StateMachine(TState initialState, FiringMode firingMode) : this()
        {
            var reference = new StateReference { State = initialState };
            _stateAccessor = () => reference.State;
            _stateMutator = s => reference.State = s;

            _firingMode = firingMode;
        }


        /// <summary>
        /// Default constructor
        /// </summary>
        StateMachine()
        {
            _unhandledTriggerAction = new UnhandledTriggerAction(EventCallbackFactory.Create<TState, TTrigger, ICollection<string>>(DefaultUnhandledTriggerAction));
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

        StateRepresentation CurrentRepresentation
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

        StateRepresentation GetRepresentation(TState state)
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
            FireAsync(trigger).GetAwaiter().GetResult();
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
            FireAsync(trigger, args).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Specify the arguments that must be supplied when a specific trigger is fired.
        /// </summary>
        /// <param name="trigger">The underlying trigger value.</param>
        /// <param name="argumentTypes">The argument types expected by the trigger.</param>
        /// <returns>An object that can be passed to the Fire() method in order to
        /// fire the parameterised trigger.</returns>
        public TriggerWithParameters SetTriggerParameters(TTrigger trigger, params Type[] argumentTypes)
        {
            var configuration = new TriggerWithParameters(trigger, argumentTypes);
            SaveTriggerConfiguration(configuration);
            return configuration;
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
            FireAsync(trigger, arg0).GetAwaiter().GetResult();
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
            FireAsync(trigger, arg0, arg1).GetAwaiter().GetResult();
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
            FireAsync(trigger, arg0, arg1, arg2).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Activates current state. Actions associated with activating the current state
        /// will be invoked. The activation is idempotent and subsequent activation of the same current state
        /// will not lead to re-execution of activation callbacks.
        /// </summary>
        public void Activate()
        {
            ActivateAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Deactivates current state. Actions associated with deactivating the current state
        /// will be invoked. The deactivation is idempotent and subsequent deactivation of the same current state
        /// will not lead to re-execution of deactivation callbacks.
        /// </summary>
        public void Deactivate()
        {
            DeactivateAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Override the default behaviour of throwing an exception when an unhandled trigger
        /// is fired.
        /// </summary>
        /// <param name="unhandledTriggerAction">An action to call when an unhandled trigger is fired.</param>
        public void OnUnhandledTrigger(Action<TState, TTrigger> unhandledTriggerAction)
        {
            if (unhandledTriggerAction == null) throw new ArgumentNullException(nameof(unhandledTriggerAction));
            _unhandledTriggerAction = new UnhandledTriggerAction(EventCallbackFactory.Create<TState, TTrigger, ICollection<string>>((s, t, c) => unhandledTriggerAction(s, t)));
        }

        /// <summary>
        /// Override the default behaviour of throwing an exception when an unhandled trigger
        /// is fired.
        /// </summary>
        /// <param name="unhandledTriggerAction">An action to call when an unhandled trigger is fired.</param>
        public void OnUnhandledTrigger(Action<TState, TTrigger, ICollection<string>> unhandledTriggerAction)
        {
            if (unhandledTriggerAction == null) throw new ArgumentNullException(nameof(unhandledTriggerAction));
            _unhandledTriggerAction = new UnhandledTriggerAction(EventCallbackFactory.Create(unhandledTriggerAction));
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
        /// Returns true if <paramref name="trigger"/> can be fired
        /// in the current state.
        /// </summary>
        /// <param name="trigger">Trigger to test.</param>
        /// <param name="unmetGuards">Guard descriptions of unmet guards. If given trigger is not configured for current state, this will be null.</param>
        /// <returns>True if the trigger can be fired, false otherwise.</returns>
        public bool CanFire(TTrigger trigger, out ICollection<string> unmetGuards)
        {
            return CurrentRepresentation.CanHandle(trigger, new object[] { }, out unmetGuards);
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
    }
}
