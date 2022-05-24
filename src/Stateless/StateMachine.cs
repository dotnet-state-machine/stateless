using Stateless.Reflection;

namespace Stateless;

/// <summary>
///     Enum for the different modes used when Fire-ing a trigger
/// </summary>
public enum FiringMode {
    /// <summary>
    ///     Use immediate mode when the queuing of trigger events are not needed. Care must be taken when using this
    ///     mode, as there is no run-to-completion guaranteed.
    /// </summary>
    Immediate,

    /// <summary> Use the queued Fire-ing mode when run-to-completion is required. This is the recommended mode.</summary>
    Queued
}

/// <summary>
///     Models behaviour as transitions between a finite set of states.
/// </summary>
/// <typeparam name="TState">The type used to represent the states.</typeparam>
/// <typeparam name="TTrigger">The type used to represent the triggers that cause state transitions.</typeparam>
public partial class StateMachine<TState, TTrigger> where TState : notnull where TTrigger : notnull {
    private readonly Queue<QueuedTrigger> _eventQueue = new();
    private readonly FiringMode           _firingMode;
    private readonly OnTransitionedEvent  _onTransitionCompletedEvent;
    private readonly OnTransitionedEvent  _onTransitionedEvent;
    private readonly Func<TState>         _stateAccessor;

    private readonly IDictionary<TState, StateRepresentation> _stateConfiguration =
        new Dictionary<TState, StateRepresentation>();

    private readonly Action<TState> _stateMutator;

    private readonly IDictionary<TTrigger, TriggerWithParameters> _triggerConfiguration =
        new Dictionary<TTrigger, TriggerWithParameters>();

    private bool                   _firing;
    private UnhandledTriggerAction _unhandledTriggerAction;

    /// <summary>
    ///     The current state.
    /// </summary>
    public TState State {
        get => _stateAccessor();
        private set => _stateMutator(value);
    }

    /// <summary>
    ///     The currently-permissible trigger values.
    /// </summary>
    public IEnumerable<TTrigger> PermittedTriggers => GetPermittedTriggers();

    private StateRepresentation CurrentRepresentation => GetRepresentation(State);

    /// <summary>
    ///     Construct a state machine with external state storage.
    /// </summary>
    /// <param name="stateAccessor">A function that will be called to read the current state value.</param>
    /// <param name="stateMutator">An action that will be called to write new state values.</param>
    /// <param name="firingMode">Optional specification of firing mode.</param>
    public StateMachine(Func<TState> stateAccessor, Action<TState> stateMutator,
                        FiringMode   firingMode = FiringMode.Queued) : this() {
        _stateAccessor = stateAccessor ?? throw new ArgumentNullException(nameof(stateAccessor));
        _stateMutator  = stateMutator  ?? throw new ArgumentNullException(nameof(stateMutator));

        _firingMode = firingMode;
    }

    /// <summary>
    ///     Construct a state machine.
    /// </summary>
    /// <param name="initialState">The initial state.</param>
    /// <param name="firingMode">Optional specification of firing mode.</param>
    public StateMachine(TState initialState, FiringMode firingMode = FiringMode.Queued) : this() {
        var reference = new StateReference(initialState);
        _stateAccessor = () => reference.State;
        _stateMutator  = s => reference.State = s;

        _firingMode = firingMode;
    }


    /// <summary>
    ///     Default constructor
    /// </summary>
    private StateMachine() {
        _unhandledTriggerAction     = new UnhandledTriggerAction(DefaultUnhandledTriggerAction);
        _onTransitionedEvent        = new OnTransitionedEvent();
        _onTransitionCompletedEvent = new OnTransitionedEvent();
        _stateAccessor              = default!; // Set in other ctor
        _stateMutator               = default!; // Set in other ctor
    }

    /// <summary>
    ///     The currently-permissible trigger values.
    /// </summary>
    public IEnumerable<TTrigger> GetPermittedTriggers(params object[] args) =>
        CurrentRepresentation.GetPermittedTriggers(args);

    /// <summary>
    ///     Provides an info object which exposes the states, transitions, and actions of this machine.
    /// </summary>
    public StateMachineInfo GetInfo() {
        var initialState = StateInfo.CreateStateInfo(new StateRepresentation(State));

        var representations = _stateConfiguration.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var behaviours = _stateConfiguration
                        .SelectMany(kvp => kvp.Value.TriggerBehaviours.SelectMany(b => b.Value
                                       .OfType<TransitioningTriggerBehaviour>().Select(tb => tb.Destination)))
                        .ToList();
        behaviours.AddRange(_stateConfiguration
                           .SelectMany(kvp => kvp.Value.TriggerBehaviours.SelectMany(b => b.Value
                                          .OfType<ReentryTriggerBehaviour>().Select(tb => tb.Destination)))
                           .ToList());

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

    private StateRepresentation GetRepresentation(TState state) {
        if (!_stateConfiguration.TryGetValue(state, out var result)) {
            result = new StateRepresentation(state);
            _stateConfiguration.Add(state, result);
        }

        return result;
    }

    /// <summary>
    ///     Begin configuration of the entry/exit actions and allowed transitions
    ///     when the state machine is in a particular state.
    /// </summary>
    /// <param name="state">The state to configure.</param>
    /// <returns>A configuration object through which the state can be configured.</returns>
    public StateConfiguration Configure(TState state) =>
        new StateConfiguration(this, GetRepresentation(state), GetRepresentation);

    /// <summary>
    ///     Specify the arguments that must be supplied when a specific trigger is fired.
    /// </summary>
    /// <param name="trigger">The underlying trigger value.</param>
    /// <param name="argumentTypes">The argument types expected by the trigger.</param>
    /// <returns>
    ///     An object that can be passed to the Fire() method in order to
    ///     fire the parameterized trigger.
    /// </returns>
    public TriggerWithParameters SetTriggerParameters(TTrigger trigger, params Type[] argumentTypes) {
        var configuration = new TriggerWithParameters(trigger, argumentTypes);
        SaveTriggerConfiguration(configuration);
        return configuration;
    }

    /// <summary>
    ///     Override the default behaviour of throwing an exception when an unhandled trigger
    ///     is fired.
    /// </summary>
    /// <param name="unhandledTriggerAction">An action to call when an unhandled trigger is fired.</param>
    public void OnUnhandledTrigger(Action<TState, TTrigger> unhandledTriggerAction) {
        if (unhandledTriggerAction == null) throw new ArgumentNullException(nameof(unhandledTriggerAction));
        _unhandledTriggerAction = new UnhandledTriggerAction((s, t, _) => unhandledTriggerAction(s, t));
    }

    /// <summary>
    ///     Override the default behaviour of throwing an exception when an unhandled trigger
    ///     is fired.
    /// </summary>
    /// <param name="unhandledTriggerAction">An action to call when an unhandled trigger is fired.</param>
    public void OnUnhandledTrigger(Action<TState, TTrigger, ICollection<string>?> unhandledTriggerAction) {
        if (unhandledTriggerAction == null) throw new ArgumentNullException(nameof(unhandledTriggerAction));
        _unhandledTriggerAction = new UnhandledTriggerAction(unhandledTriggerAction);
    }

    /// <summary>
    ///     Determine if the state machine is in the supplied state.
    /// </summary>
    /// <param name="state">The state to test for.</param>
    /// <returns>
    ///     True if the current state is equal to, or a substate of,
    ///     the supplied state.
    /// </returns>
    public bool IsInState(TState state) => CurrentRepresentation.IsIncludedIn(state);

    /// <summary>
    ///     Returns true if <paramref name="trigger" /> can be fired
    ///     in the current state.
    /// </summary>
    /// <param name="trigger">Trigger to test.</param>
    /// <returns>True if the trigger can be fired, false otherwise.</returns>
    public bool CanFire(TTrigger trigger) => CurrentRepresentation.CanHandle(trigger);

    /// <summary>
    ///     Returns true if <paramref name="trigger" /> can be fired
    ///     in the current state.
    /// </summary>
    /// <param name="trigger">The trigger to fire.</param>
    /// <param name="args">A variable-length parameters list containing arguments. </param>
    /// <returns>True if the trigger can be fired, false otherwise.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool CanFire(TriggerWithParameters trigger, params object[] args) {
        if (trigger == null) throw new ArgumentNullException(nameof(trigger));
        return CurrentRepresentation.CanHandle(trigger.Trigger, args);
    }

    /// <summary>
    ///     Returns true if <paramref name="trigger" /> can be fired
    ///     in the current state.
    /// </summary>
    /// <param name="trigger">Trigger to test.</param>
    /// <param name="unmetGuards">
    ///     Guard descriptions of unmet guards. If given trigger is not configured for current state,
    ///     this will be null.
    /// </param>
    /// <returns>True if the trigger can be fired, false otherwise.</returns>
    public bool CanFire(TTrigger trigger, out ICollection<string>? unmetGuards) =>
        CurrentRepresentation.CanHandle(trigger, ArrayHelper.Empty<object>(), out unmetGuards);

    /// <summary>
    ///     A human-readable representation of the state machine.
    /// </summary>
    /// <returns>A description of the current state and permitted triggers.</returns>
    public override string ToString() {
        return string.Format(
                             "StateMachine {{ State = {0}, PermittedTriggers = {{ {1} }}}}",
                             State,
                             string.Join(", ", GetPermittedTriggers().Select(t => t.ToString()).ToArray()));
    }

    /// <summary>
    ///     Specify the arguments that must be supplied when a specific trigger is fired.
    /// </summary>
    /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
    /// <param name="trigger">The underlying trigger value.</param>
    /// <returns>
    ///     An object that can be passed to the Fire() method in order to
    ///     fire the parameterized trigger.
    /// </returns>
    public TriggerWithParameters<TArg0> SetTriggerParameters<TArg0>(TTrigger trigger) {
        var configuration = new TriggerWithParameters<TArg0>(trigger);
        SaveTriggerConfiguration(configuration);
        return configuration;
    }

    /// <summary>
    ///     Specify the arguments that must be supplied when a specific trigger is fired.
    /// </summary>
    /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
    /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
    /// <param name="trigger">The underlying trigger value.</param>
    /// <returns>
    ///     An object that can be passed to the Fire() method in order to
    ///     fire the parameterized trigger.
    /// </returns>
    public TriggerWithParameters<TArg0, TArg1> SetTriggerParameters<TArg0, TArg1>(TTrigger trigger) {
        var configuration = new TriggerWithParameters<TArg0, TArg1>(trigger);
        SaveTriggerConfiguration(configuration);
        return configuration;
    }

    /// <summary>
    ///     Specify the arguments that must be supplied when a specific trigger is fired.
    /// </summary>
    /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
    /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
    /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
    /// <param name="trigger">The underlying trigger value.</param>
    /// <returns>
    ///     An object that can be passed to the Fire() method in order to
    ///     fire the parameterized trigger.
    /// </returns>
    public TriggerWithParameters<TArg0, TArg1, TArg2> SetTriggerParameters<TArg0, TArg1, TArg2>(TTrigger trigger) {
        var configuration = new TriggerWithParameters<TArg0, TArg1, TArg2>(trigger);
        SaveTriggerConfiguration(configuration);
        return configuration;
    }

    private void SaveTriggerConfiguration(TriggerWithParameters trigger) {
        if (_triggerConfiguration.ContainsKey(trigger.Trigger))
            throw new InvalidOperationException(
                                                string.Format(StateMachineResources.CannotReconfigureParameters,
                                                              trigger));

        _triggerConfiguration.Add(trigger.Trigger, trigger);
    }

    private void DefaultUnhandledTriggerAction(TState               state, TTrigger trigger,
                                               ICollection<string>? unmetGuardConditions) {
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
    ///     Registers a callback that will be invoked every time the state machine
    ///     transitions from one state into another.
    /// </summary>
    /// <param name="onTransitionAction">
    ///     The action to execute, accepting the details
    ///     of the transition.
    /// </param>
    public void OnTransitioned(Action<Transition> onTransitionAction) {
        if (onTransitionAction == null) throw new ArgumentNullException(nameof(onTransitionAction));
        _onTransitionedEvent.Register(onTransitionAction);
    }

    /// <summary>
    ///     Registers a callback that will be invoked every time the StateMachine
    ///     transitions from one state into another and all the OnEntryFrom etc methods
    ///     have been invoked
    /// </summary>
    /// <param name="onTransitionAction">
    ///     The action to execute, accepting the details
    ///     of the transition.
    /// </param>
    public void OnTransitionCompleted(Action<Transition> onTransitionAction) {
        if (onTransitionAction == null) throw new ArgumentNullException(nameof(onTransitionAction));
        _onTransitionCompletedEvent.Register(onTransitionAction);
    }

    private class QueuedTrigger {
        public TTrigger  Trigger { get; }
        public object?[] Args    { get; }

        public QueuedTrigger(TTrigger trigger, object?[] args) {
            Trigger = trigger;
            Args    = args;
        }
    }
}