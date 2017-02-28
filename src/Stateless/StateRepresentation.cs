using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal partial class StateRepresentation
        {
            readonly TState _state;

            readonly IDictionary<TTrigger, ICollection<TriggerBehaviour>> _triggerBehaviours =
                new Dictionary<TTrigger, ICollection<TriggerBehaviour>>();

            internal IDictionary<TTrigger, ICollection<TriggerBehaviour>> TriggerBehaviours { get { return _triggerBehaviours; } }

            readonly ICollection<EntryActionBehavior> _entryActions = new List<EntryActionBehavior>();
            internal ICollection<EntryActionBehavior> EntryActions { get { return _entryActions; } }
            readonly ICollection<ExitActionBehavior> _exitActions = new List<ExitActionBehavior>();
            internal ICollection<ExitActionBehavior> ExitActions { get { return _exitActions; } }

            readonly ICollection<ActivateActionBehaviour> _activateActions = new List<ActivateActionBehaviour>();
            readonly ICollection<DeactivateActionBehaviour> _deactivateActions = new List<DeactivateActionBehaviour>();

            readonly ICollection<InternalActionBehaviour> _internalActions = new List<InternalActionBehaviour>();

            StateRepresentation _superstate; // null
            bool active;

            readonly ICollection<StateRepresentation> _substates = new List<StateRepresentation>();

            public StateRepresentation(TState state)
            {
                _state = state;
            }

            public bool CanHandle(TTrigger trigger)
            {
                TriggerBehaviourResult unused;
                return TryFindHandler(trigger, out unused);
            }

            public bool TryFindHandler(TTrigger trigger, out TriggerBehaviourResult handler)
            {
                return (TryFindLocalHandler(trigger, out handler) ||
                    (Superstate != null && Superstate.TryFindHandler(trigger, out handler)));
            }

            bool TryFindLocalHandler(TTrigger trigger, out TriggerBehaviourResult handlerResult)
            {
                ICollection<TriggerBehaviour> possible;
                if (!_triggerBehaviours.TryGetValue(trigger, out possible))
                {
                    handlerResult = null;
                    return false;
                }

                // Guard functions executed
                var actual = possible
                    .Select(h => new TriggerBehaviourResult(h, h.UnmetGuardConditions));
        
                handlerResult = TryFindLocalHandlerResult(trigger, actual, r => !r.UnmetGuardConditions.Any())
                    ?? TryFindLocalHandlerResult(trigger, actual, r => r.UnmetGuardConditions.Any());

                return !handlerResult.UnmetGuardConditions.Any();
            }

            TriggerBehaviourResult TryFindLocalHandlerResult(TTrigger trigger, IEnumerable<TriggerBehaviourResult> results, Func<TriggerBehaviourResult, bool> filter)
            {
                var actual = results
                    .Where(filter);

                if (actual.Count() > 1)
                    throw new InvalidOperationException(
                        string.Format(StateRepresentationResources.MultipleTransitionsPermitted,
                        trigger, _state));

                return actual
                    .FirstOrDefault();
            }
            public void AddActivateAction(Action action, string activateActionDescription)
            {
                _activateActions.Add(
                    new ActivateActionBehaviour.Sync(
                        _state,
                        Enforce.ArgumentNotNull(action, nameof(action)),
                        Enforce.ArgumentNotNull(activateActionDescription, nameof(activateActionDescription))));
            }

            public void AddDeactivateAction(Action action, string deactivateActionDescription)
            {
                _deactivateActions.Add(
                    new DeactivateActionBehaviour.Sync(
                        _state,
                        Enforce.ArgumentNotNull(action, nameof(action)),
                        Enforce.ArgumentNotNull(deactivateActionDescription, nameof(deactivateActionDescription))));
            }

            public void AddEntryAction(TTrigger trigger, Action<Transition, object[]> action, string entryActionDescription)
            {
                Enforce.ArgumentNotNull(action, nameof(action));
                _entryActions.Add(
                    new EntryActionBehavior.Sync((t, args) =>
                    {
                        if (t.Trigger.Equals(trigger))
                            action(t, args);
                    },
                    Enforce.ArgumentNotNull(entryActionDescription, nameof(entryActionDescription))));
            }

            public void AddEntryAction(Action<Transition, object[]> action, string entryActionDescription)
            {
                _entryActions.Add(
                    new EntryActionBehavior.Sync(
                        Enforce.ArgumentNotNull(action, nameof(action)),
                        Enforce.ArgumentNotNull(entryActionDescription, nameof(entryActionDescription))));
            }

            public void AddExitAction(Action<Transition> action, string exitActionDescription)
            {
                _exitActions.Add(
                    new ExitActionBehavior.Sync(
                        Enforce.ArgumentNotNull(action, nameof(action)),
                        Enforce.ArgumentNotNull(exitActionDescription, nameof(exitActionDescription))));
            }

            internal void AddInternalAction(TTrigger trigger, Action<Transition, object[]> action)
            {
                Enforce.ArgumentNotNull(action, nameof(action));

                _internalActions.Add(new InternalActionBehaviour.Sync((t, args) =>
                {
                    if (t.Trigger.Equals(trigger))
                        action(t, args);
                }));
            }

            public void Activate()
            {
                if (_superstate != null)
                    _superstate.Activate();

                if (active)
                    return;

                ExecuteActivationActions();
                active = true;
            }

            public void Deactivate()
            {
                if (!active)
                    return;

                ExecuteDeactivationActions();
                active = false;

                if (_superstate != null)
                    _superstate.Deactivate();
            }

            void ExecuteActivationActions()
            {
                foreach (var action in _activateActions)
                    action.Execute();
            }

            void ExecuteDeactivationActions()
            {
                foreach (var action in _deactivateActions)
                    action.Execute();
            }

            public void Enter(Transition transition, params object[] entryArgs)
            {
                Enforce.ArgumentNotNull(transition, nameof(transition));

                if (transition.IsReentry)
                {
                    ExecuteEntryActions(transition, entryArgs);
                    ExecuteActivationActions();
                }
                else if (!Includes(transition.Source))
                {
                    if (_superstate != null)
                        _superstate.Enter(transition, entryArgs);

                    ExecuteEntryActions(transition, entryArgs);
                    ExecuteActivationActions();
                }
            }

            public void Exit(Transition transition)
            {
                Enforce.ArgumentNotNull(transition, nameof(transition));

                if (transition.IsReentry)
                {
                    ExecuteDeactivationActions();
                    ExecuteExitActions(transition);
                }
                else if (!Includes(transition.Destination))
                {
                    ExecuteDeactivationActions();
                    ExecuteExitActions(transition);

                    if (_superstate != null)
                        _superstate.Exit(transition);
                }
            }

            void ExecuteEntryActions(Transition transition, object[] entryArgs)
            {
                Enforce.ArgumentNotNull(transition, nameof(transition));
                Enforce.ArgumentNotNull(entryArgs, nameof(entryArgs));
                foreach (var action in _entryActions)
                    action.Execute(transition, entryArgs);
            }

            void ExecuteExitActions(Transition transition)
            {
                Enforce.ArgumentNotNull(transition, nameof(transition));
                foreach (var action in _exitActions)
                    action.Execute(transition);
            }

            void ExecuteInternalActions(Transition transition, object[] args)
            {
                var possibleActions = new List<InternalActionBehaviour>();

                // Look for actions in superstate(s) recursivly until we hit the topmost superstate
                StateRepresentation aStateRep = this;
                do
                {
                    possibleActions.AddRange(aStateRep._internalActions);
                    aStateRep = aStateRep._superstate;
                } while (aStateRep != null);

                // Execute internal transition event handler
                foreach (var action in possibleActions)
                {
                    action.Execute(transition, args);
                }
            }

            public void AddTriggerBehaviour(TriggerBehaviour triggerBehaviour)
            {
                ICollection<TriggerBehaviour> allowed;
                if (!_triggerBehaviours.TryGetValue(triggerBehaviour.Trigger, out allowed))
                {
                    allowed = new List<TriggerBehaviour>();
                    _triggerBehaviours.Add(triggerBehaviour.Trigger, allowed);
                }
                allowed.Add(triggerBehaviour);
            }

            public StateRepresentation Superstate
            {
                get
                {
                    return _superstate;
                }
                set
                {
                    _superstate = value;
                }
            }

            public TState UnderlyingState
            {
                get
                {
                    return _state;
                }
            }

            public void AddSubstate(StateRepresentation substate)
            {
                Enforce.ArgumentNotNull(substate, nameof(substate));
                _substates.Add(substate);
            }

            public bool Includes(TState state)
            {
                return _state.Equals(state) || _substates.Any(s => s.Includes(state));
            }

            public bool IsIncludedIn(TState state)
            {
                return
                    _state.Equals(state) ||
                    (_superstate != null && _superstate.IsIncludedIn(state));
            }

            public IEnumerable<TTrigger> PermittedTriggers
            {
                get
                {
                    var result = _triggerBehaviours
                        .Where(t => t.Value.Any(a => !a.UnmetGuardConditions.Any()))
                        .Select(t => t.Key);

                    if (Superstate != null)
                        result = result.Union(Superstate.PermittedTriggers);

                    return result.ToArray();
                }
            }

            internal void InternalAction(Transition transition, object[] args)
            {
                Enforce.ArgumentNotNull(transition, nameof(transition));
                ExecuteInternalActions(transition, args);
            }
        }
    }
}
