using System.Diagnostics.CodeAnalysis;
using Stateless.Reflection;

namespace Stateless;

public partial class StateMachine<TState, TTrigger> {
    internal partial class StateRepresentation {
        private readonly TState _state;

        private readonly ICollection<StateRepresentation> _substates = new List<StateRepresentation>();

        internal IDictionary<TTrigger, ICollection<TriggerBehaviour>> TriggerBehaviours { get; } =
            new Dictionary<TTrigger, ICollection<TriggerBehaviour>>();

        internal ICollection<EntryActionBehavior> EntryActions { get; } = new List<EntryActionBehavior>();
        internal ICollection<ExitActionBehavior>  ExitActions  { get; } = new List<ExitActionBehavior>();

        internal ICollection<ActivationChangeActionBehaviour> ActivateActions { get; } =
            new List<ActivationChangeActionBehaviour>();

        internal ICollection<ActivationChangeActionBehaviour> DeactivateActions { get; } =
            new List<ActivationChangeActionBehaviour>();

        public TState? InitialTransitionTarget { get; private set; }

        public StateRepresentation? Superstate { get; set; }

        public TState UnderlyingState => _state;
        
        public bool HasInitialTransition { get; private set; }

        public StateRepresentation(TState state) => _state = state;

        internal ICollection<StateRepresentation> GetSubstates() => _substates;

        public bool CanHandle(TTrigger trigger, params object[] args) => TryFindHandler(trigger, args, out _);

        public bool CanHandle(TTrigger                                     trigger, object[] args,
                              [NotNullWhen(true)] out ICollection<string>? unmetGuards) {
            var handlerFound = TryFindHandler(trigger, args, out var result);
            unmetGuards = result?.UnmetGuardConditions;
            return handlerFound;
        }

        public bool TryFindHandler(TTrigger                                        trigger, object?[] args,
                                   [NotNullWhen(true)] out TriggerBehaviourResult? handler) {
            TriggerBehaviourResult? superStateHandler = null;

            var handlerFound = TryFindLocalHandler(trigger, args, out var localHandler) ||
                               (Superstate is { } && Superstate.TryFindHandler(trigger, args, out superStateHandler));

            // If no handler for super state, replace by local handler (see issue #398)
            handler = superStateHandler ?? localHandler;

            return handlerFound;
        }

        private bool TryFindLocalHandler(TTrigger                                        trigger, object?[] args,
                                         [NotNullWhen(true)] out TriggerBehaviourResult? handlerResult) {
            // Get list of candidate trigger handlers
            if (!TriggerBehaviours.TryGetValue(trigger, out var possible)) {
                handlerResult = null;
                return false;
            }

            // Guard functions are executed here
            var actual = possible
                        .Select(h => new TriggerBehaviourResult(h, h.UnmetGuardConditions(args)))
                        .ToArray();

            // Find a handler for the trigger
            handlerResult = TryFindLocalHandlerResult(trigger, actual)
                         ?? TryFindLocalHandlerResultWithUnmetGuardConditions(actual);

            if (handlerResult == null)
                return false;

            return !handlerResult.UnmetGuardConditions.Any();
        }

        private TriggerBehaviourResult? TryFindLocalHandlerResult(TTrigger                            trigger,
                                                                  IEnumerable<TriggerBehaviourResult> results) {
            var actual = results
                        .Where(r => !r.UnmetGuardConditions.Any())
                        .ToList();

            if (actual.Count <= 1)
                return actual.FirstOrDefault();

            var message = string.Format(StateRepresentationResources.MultipleTransitionsPermitted, trigger, _state);
            throw new InvalidOperationException(message);
        }

        private static TriggerBehaviourResult? TryFindLocalHandlerResultWithUnmetGuardConditions(
            IEnumerable<TriggerBehaviourResult> results) {
            var triggerBehaviourResults = results as TriggerBehaviourResult[] ?? results.ToArray();
            var result = triggerBehaviourResults.FirstOrDefault(r => r.UnmetGuardConditions.Any());

            if (result is null) return result;
            var unmetConditions = triggerBehaviourResults.Where(r => r.UnmetGuardConditions.Any())
                                                         .SelectMany(r => r.UnmetGuardConditions);

            // Add other unmet conditions to first result
            foreach (var condition in unmetConditions)
                if (!result.UnmetGuardConditions.Contains(condition))
                    result.UnmetGuardConditions.Add(condition);

            return result;
        }

        public void AddActivateAction(Action action, InvocationInfo activateActionDescription) {
            ActivateActions.Add(new ActivationChangeActionBehaviour(action, activateActionDescription));
        }

        public void AddDeactivateAction(Action action, InvocationInfo deactivateActionDescription) {
            DeactivateActions.Add(new ActivationChangeActionBehaviour(action, deactivateActionDescription));
        }

        public void AddEntryAction(TTrigger       trigger, Action<Transition, object?[]> action,
                                   InvocationInfo entryActionDescription) {
            EntryActions.Add(new EntryActionBehavior.From(trigger, action, entryActionDescription));
        }

        public void AddEntryAction(Action<Transition, object?[]> action, InvocationInfo entryActionDescription) {
            EntryActions.Add(new EntryActionBehavior(action, entryActionDescription));
        }

        public void AddExitAction(Action<Transition> action, InvocationInfo exitActionDescription) {
            ExitActions.Add(new ExitActionBehavior(action, exitActionDescription));
        }

        public void AddTriggerBehaviour(TriggerBehaviour triggerBehaviour) {
            if (!TriggerBehaviours.TryGetValue(triggerBehaviour.Trigger, out var allowed)) {
                allowed = new List<TriggerBehaviour>();
                TriggerBehaviours.Add(triggerBehaviour.Trigger, allowed);
            }

            allowed.Add(triggerBehaviour);
        }

        public void AddSubstate(StateRepresentation substate) {
            _substates.Add(substate);
        }

        /// <summary>
        ///     Checks if the state is in the set of this state or its sub-states
        /// </summary>
        /// <param name="state">The state to check</param>
        /// <returns>True if included</returns>
        public bool Includes(TState state) {
            return _state.Equals(state) || _substates.Any(s => s.Includes(state));
        }

        /// <summary>
        ///     Checks if the state is in the set of this state or a super-state
        /// </summary>
        /// <param name="state">The state to check</param>
        /// <returns>True if included</returns>
        public bool IsIncludedIn(TState state) =>
            _state.Equals(state) ||
            (Superstate is { } && Superstate.IsIncludedIn(state));

        public IEnumerable<TTrigger> GetPermittedTriggers(params object[] args) {
            var result = new HashSet<TTrigger>();
            foreach (var triggerBehaviour in TriggerBehaviours)
                try {
                    if (triggerBehaviour.Value.Any(a => !a.UnmetGuardConditions(args).Any()))
                        result.Add(triggerBehaviour.Key);
                } catch (Exception) {
                    //Ignore
                    //There is no need to throw an exception when trying to get the Permitted Triggers. If it's not valid just don't return it.
                }

            if (Superstate is { })
                result.UnionWith(Superstate.GetPermittedTriggers(args));

            return result;
        }

        internal void SetInitialTransition(TState state) {
            InitialTransitionTarget = state;
            HasInitialTransition    = true;
        }
    }
}