using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless
{

    /// <summary>
    /// Represents a yet unconfigured/partly configured statemachine
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TTrigger"></typeparam>
    public interface IUnconfiguredStatemachine<TState, TTrigger>
    {

        /// <summary>
        /// Begin configuration of the entry/exit actions and allowed transitions
        /// when the state machine is in a particular state.
        /// </summary>
        /// <param name="state">The state to configure.</param>
        /// <returns>A configuration object through which the state can be configured.</returns>
        StateMachine<TState, TTrigger>.StateConfiguration Configure(TState state);

        /// <summary>
        /// Specify the arguments that must be supplied when a specific trigger is fired.
        /// </summary>
        /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
        /// <param name="trigger">The underlying trigger value.</param>
        /// <returns>An object that can be passed to the Fire() method in order to 
        /// fire the parameterised trigger.</returns>
        StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0> SetTriggerParameters<TArg0>(TTrigger trigger);


        /// <summary>
        /// finished configuration and returns a configured statemachine
        /// </summary>
        /// <returns></returns>
        IConfiguredStatemachine<TState, TTrigger> FinishConfiguration();

        /// <summary>
        /// Specify the arguments that must be supplied when a specific trigger is fired.
        /// </summary>
        /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
        /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
        /// <param name="trigger">The underlying trigger value.</param>
        /// <returns>An object that can be passed to the Fire() method in order to 
        /// fire the parameterised trigger.</returns>
        StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0, TArg1> SetTriggerParameters<TArg0, TArg1>(
            TTrigger trigger);

        /// <summary>
        /// Specify the arguments that must be supplied when a specific trigger is fired.
        /// </summary>
        /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
        /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
        /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
        /// <param name="trigger">The underlying trigger value.</param>
        /// <returns>An object that can be passed to the Fire() method in order to 
        /// fire the parameterised trigger.</returns>
        StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0, TArg1, TArg2> SetTriggerParameters
            <TArg0, TArg1, TArg2>(TTrigger trigger);



        /// <summary>
        /// Registers a callback that will be invoked every time the statemachine
        /// transitions from one state into another.
        /// </summary>
        /// <param name="onTransitionAction">The action to execute, accepting the details
        /// of the transition.</param>
        void OnTransitioned(Action<StateMachine<TState, TTrigger>.Transition> onTransitionAction);


        /// <summary>
        /// Override the default behaviour of throwing an exception when an unhandled trigger
        /// is fired.
        /// </summary>
        /// <param name="unhandledTriggerAction">An action to call when an unhandled trigger is fired.</param>
        void OnUnhandledTrigger(Action<TState, TTrigger> unhandledTriggerAction);

    }

    /// <summary>
    /// represents a configures statemachine
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TTrigger"></typeparam>
    public interface IConfiguredStatemachine<TState, TTrigger>
    {
        /// <summary>
        /// Transition from the current state via the specified trigger.
        /// The target state is determined by the configuration of the current state.
        /// Actions associated with leaving the current state and entering the new one
        /// will be invoked.
        /// </summary>
        /// <param name="trigger">The trigger to fire.</param>
        /// <exception cref="System.InvalidOperationException">The current state does
        /// not allow the trigger to be fired.</exception>
        void Fire(TTrigger trigger);

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
        void Fire<TArg0>(StateMachine<TState,TTrigger>.TriggerWithParameters<TArg0> trigger, TArg0 arg0);


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
        void Fire<TArg0, TArg1>(StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0, TArg1> trigger, TArg0 arg0, TArg1 arg1);

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
        void Fire<TArg0, TArg1, TArg2>(StateMachine<TState, TTrigger>.TriggerWithParameters<TArg0, TArg1, TArg2> trigger, TArg0 arg0, TArg1 arg1,
            TArg2 arg2);


        /// <summary>
        /// The current state.
        /// </summary>
        TState State { get; }

        /// <summary>
        /// Determine if the state machine is in the supplied state.
        /// </summary>
        /// <param name="state">The state to test for.</param>
        /// <returns>True if the current state is equal to, or a substate of,
        /// the supplied state.</returns>
        bool IsInState(TState state);

        /// <summary>
        /// Returns true if <paramref name="trigger"/> can be fired
        /// in the current state.
        /// </summary>
        /// <param name="trigger">Trigger to test.</param>
        /// <returns>True if the trigger can be fired, false otherwise.</returns>
        bool CanFire(TTrigger trigger);



        /// <summary>
        /// The currently-permissible trigger values.
        /// </summary>
        IEnumerable<TTrigger> PermittedTriggers { get; }
        
    }


    /// <summary>
    /// Models behaviour as transitions between a finite set of states.
    /// </summary>
    /// <typeparam name="TState">The type used to represent the states.</typeparam>
    /// <typeparam name="TTrigger">The type used to represent the triggers that cause state transitions.</typeparam>
    public partial class StateMachine<TState, TTrigger> : IUnconfiguredStatemachine<TState, TTrigger>, IConfiguredStatemachine<TState, TTrigger>
    {
        readonly IDictionary<TState, StateRepresentation> _stateConfiguration = new Dictionary<TState, StateRepresentation>();
        readonly IDictionary<TTrigger, TriggerWithParameters> _triggerConfiguration = new Dictionary<TTrigger, TriggerWithParameters>();
        readonly Func<TState> _stateAccessor;
        readonly Action<TState> _stateMutator;
        Action<TState, TTrigger> _unhandledTriggerAction = DefaultUnhandledTriggerAction;
        event Action<Transition> _onTransitioned;

        /// <summary>
        /// Construct a state machine with external state storage.
        /// </summary>
        /// <param name="stateAccessor">A function that will be called to read the current state value.</param>
        /// <param name="stateMutator">An action that will be called to write new state values.</param>
        private StateMachine(Func<TState> stateAccessor, Action<TState> stateMutator)
        {
            _stateAccessor = Enforce.ArgumentNotNull(stateAccessor, "stateAccessor");
            _stateMutator = Enforce.ArgumentNotNull(stateMutator, "stateMutator");
        }

        /// <summary>
        /// Construct a state machine.
        /// </summary>
        /// <param name="initialState">The initial state.</param>
        private StateMachine(TState initialState)
        {
            var reference = new StateReference { State = initialState };
            _stateAccessor = () => reference.State;
            _stateMutator = s => reference.State = s;
        }

        /// <summary>
        /// Construct a state machine.
        /// </summary>
        /// <param name="initialState">The initial state.</param>
        public static IUnconfiguredStatemachine<TState, TTrigger> Create(TState initialState)
        {
            return new StateMachine<TState, TTrigger>(initialState);            
        }

        /// <summary>
        /// Construct a state machine with external state storage.
        /// </summary>
        /// <param name="stateAccessor">A function that will be called to read the current state value.</param>
        /// <param name="stateMutator">An action that will be called to write new state values.</param>
        public static IUnconfiguredStatemachine<TState, TTrigger> Create(Func<TState> stateAccessor, Action<TState> stateMutator)
        {
            return new StateMachine<TState, TTrigger>(stateAccessor, stateMutator);
        }

        /// <summary>
        /// The current state.
        /// </summary>
        private TState State
        {
            get
            {
                return _stateAccessor();
            }
            set
            {
                _stateMutator(value);
            }
        }

        /// <summary>
        /// The current state.
        /// </summary>
        TState IConfiguredStatemachine<TState, TTrigger>.State
        {
            get { return this.State; }
        }

        /// <summary>
        /// The currently-permissible trigger values.
        /// </summary>
        IEnumerable<TTrigger> IConfiguredStatemachine<TState, TTrigger>.PermittedTriggers
        {
            get { return this.PermittedTriggers; }
        }
        
        private IEnumerable<TTrigger> PermittedTriggers
        
        {
            get
            {
                return CurrentRepresentation.PermittedTriggers;
            }
        }

        StateRepresentation CurrentRepresentation
        {
            get
            {
                return GetRepresentation(State);
            }
        }

        StateRepresentation GetRepresentation(TState state)
        {
            StateRepresentation result;

            if (!_stateConfiguration.TryGetValue(state, out result))
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
        StateConfiguration IUnconfiguredStatemachine<TState, TTrigger>.Configure(TState state)
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
        void IConfiguredStatemachine<TState, TTrigger>.Fire(TTrigger trigger)
        {
            InternalFire(trigger, new object[0]);
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
        void IConfiguredStatemachine<TState, TTrigger>.Fire<TArg0>(TriggerWithParameters<TArg0> trigger, TArg0 arg0)
        {
            Enforce.ArgumentNotNull(trigger, "trigger");
            InternalFire(trigger.Trigger, arg0);
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
        void IConfiguredStatemachine<TState, TTrigger>.Fire<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, TArg0 arg0, TArg1 arg1)
        {
            Enforce.ArgumentNotNull(trigger, "trigger");
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
        /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
        /// <param name="arg0">The first argument.</param>
        /// <param name="arg1">The second argument.</param>
        /// <param name="arg2">The third argument.</param>
        /// <param name="trigger">The trigger to fire.</param>
        /// <exception cref="System.InvalidOperationException">The current state does
        /// not allow the trigger to be fired.</exception>
        void IConfiguredStatemachine<TState, TTrigger>.Fire<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, TArg0 arg0, TArg1 arg1, TArg2 arg2)
        {
            Enforce.ArgumentNotNull(trigger, "trigger");
            InternalFire(trigger.Trigger, arg0, arg1, arg2);
        }

        void InternalFire(TTrigger trigger, params object[] args)
        {
            TriggerWithParameters configuration;
            if (_triggerConfiguration.TryGetValue(trigger, out configuration))
                configuration.ValidateParameters(args);

            var source = State;
            var representativeState = GetRepresentation(source);

            TriggerBehaviour triggerBehaviour;
            if (!representativeState.TryFindHandler(trigger, out triggerBehaviour))
            {
                _unhandledTriggerAction(representativeState.UnderlyingState, trigger);
                return;
            }

            TState destination;
            if (triggerBehaviour.ResultsInTransitionFrom(source, args, out destination))
            {
                var transition = new Transition(source, destination, trigger);

                representativeState.Exit(transition);

                State = transition.Destination;
                var newRepresentation = GetRepresentation(transition.Destination);
                var onTransitioned = _onTransitioned;
                if (onTransitioned != null)
                    onTransitioned(transition);

                newRepresentation.Enter(transition, args);
            }
        }

        /// <summary>
        /// Override the default behaviour of throwing an exception when an unhandled trigger
        /// is fired.
        /// </summary>
        /// <param name="unhandledTriggerAction">An action to call when an unhandled trigger is fired.</param>
        void IUnconfiguredStatemachine<TState, TTrigger>.OnUnhandledTrigger(Action<TState, TTrigger> unhandledTriggerAction)
        {
            if (unhandledTriggerAction == null) throw new ArgumentNullException("unhandledTriggerAction");
            _unhandledTriggerAction = unhandledTriggerAction;
        }

        /// <summary>
        /// Determine if the state machine is in the supplied state.
        /// </summary>
        /// <param name="state">The state to test for.</param>
        /// <returns>True if the current state is equal to, or a substate of,
        /// the supplied state.</returns>
        bool IConfiguredStatemachine<TState, TTrigger>.IsInState(TState state)
        {
            return CurrentRepresentation.IsIncludedIn(state);
        }

        /// <summary>
        /// Returns true if <paramref name="trigger"/> can be fired
        /// in the current state.
        /// </summary>
        /// <param name="trigger">Trigger to test.</param>
        /// <returns>True if the trigger can be fired, false otherwise.</returns>
        bool IConfiguredStatemachine<TState, TTrigger>.CanFire(TTrigger trigger)
        {
            return CurrentRepresentation.CanHandle(trigger);
        }

        /// <summary>
        /// finished configuration and returns a configured statemachine
        /// </summary>
        /// <returns></returns>
        IConfiguredStatemachine<TState, TTrigger> IUnconfiguredStatemachine<TState, TTrigger>.FinishConfiguration()
        {
            return this;
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
                string.Join(", ", PermittedTriggers.Select(t => t.ToString()).ToArray()));
        }

        /// <summary>
        /// Specify the arguments that must be supplied when a specific trigger is fired.
        /// </summary>
        /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
        /// <param name="trigger">The underlying trigger value.</param>
        /// <returns>An object that can be passed to the Fire() method in order to
        /// fire the parameterised trigger.</returns>
        TriggerWithParameters<TArg0> IUnconfiguredStatemachine<TState, TTrigger>.SetTriggerParameters<TArg0>(TTrigger trigger)
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
        TriggerWithParameters<TArg0, TArg1> IUnconfiguredStatemachine<TState, TTrigger>.SetTriggerParameters<TArg0, TArg1>(TTrigger trigger)
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
        TriggerWithParameters<TArg0, TArg1, TArg2> IUnconfiguredStatemachine<TState, TTrigger>.SetTriggerParameters<TArg0, TArg1, TArg2>(TTrigger trigger)
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

        static void DefaultUnhandledTriggerAction(TState state, TTrigger trigger)
        {
            throw new InvalidOperationException(
                string.Format(
                    StateMachineResources.NoTransitionsPermitted,
                    trigger, state));
        }

        /// <summary>
        /// Registers a callback that will be invoked every time the statemachine
        /// transitions from one state into another.
        /// </summary>
        /// <param name="onTransitionAction">The action to execute, accepting the details
        /// of the transition.</param>
        void IUnconfiguredStatemachine<TState, TTrigger>.OnTransitioned(Action<Transition> onTransitionAction)
        {
            if (onTransitionAction == null) throw new ArgumentNullException("onTransitionAction");
            _onTransitioned += onTransitionAction;
        }
    }
}
