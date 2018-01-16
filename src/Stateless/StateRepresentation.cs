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
            internal ICollection<ActivateActionBehaviour> ActivateActions { get { return _activateActions; } }

            readonly ICollection<DeactivateActionBehaviour> _deactivateActions = new List<DeactivateActionBehaviour>();
            internal ICollection<DeactivateActionBehaviour> DeactivateActions { get { return _deactivateActions; } }

            readonly ICollection<InternalActionBehaviour> _internalActions = new List<InternalActionBehaviour>();

            StateRepresentation _superstate; // null
            bool active;

            readonly ICollection<StateRepresentation> _substates = new List<StateRepresentation>();

            public StateRepresentation(TState state)
            {
                _state = state;
            }

            internal ICollection<StateRepresentation> GetSubstates()
            {
                return _substates;
            }

            public bool CanHandle(TTrigger trigger)
            {
                return TryFindHandler(trigger, out TriggerBehaviourResult unused);
            }

            public bool TryFindHandler(TTrigger trigger, out TriggerBehaviourResult handler)
            {
                return (TryFindLocalHandler(trigger, out handler) ||
                    (Superstate != null && Superstate.TryFindHandler(trigger, out handler)));
            }

            bool TryFindLocalHandler(TTrigger trigger, out TriggerBehaviourResult handlerResult)
            {
                if (!_triggerBehaviours.TryGetValue(trigger, out ICollection<TriggerBehaviour> possible))
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
            public void AddActivateAction(Action action, Reflection.InvocationInfo activateActionDescription)
            {
                _activateActions.Add(new ActivateActionBehaviour.Sync(_state, action, activateActionDescription));
            }

            public void AddDeactivateAction(Action action, Reflection.InvocationInfo deactivateActionDescription)
            {
                _deactivateActions.Add(new DeactivateActionBehaviour.Sync(_state, action, deactivateActionDescription));
            }

            public void AddEntryAction(TTrigger trigger, Action<Transition, object[]> action, Reflection.InvocationInfo entryActionDescription)
            {
                _entryActions.Add(new EntryActionBehavior.SyncFrom<TTrigger>(trigger, action, entryActionDescription));
            }

            public void AddEntryAction(Action<Transition, object[]> action, Reflection.InvocationInfo entryActionDescription)
            {
                _entryActions.Add(new EntryActionBehavior.Sync(action, entryActionDescription));
            }

            public void AddExitAction(Action<Transition> action, Reflection.InvocationInfo exitActionDescription)
            {
                _exitActions.Add(new ExitActionBehavior.Sync(action, exitActionDescription));
            }

            internal void AddInternalAction(TTrigger trigger, Action<Transition, object[]> action)
            {
                if (action == null) throw new ArgumentNullException(nameof(action));

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

            public Transition Exit(Transition transition)
            {
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
                    {
                        transition = new Transition(_superstate.UnderlyingState, transition.Destination, transition.Trigger);
                         return _superstate.Exit(transition);
                    }
                }
                return transition;
            }

            void ExecuteEntryActions(Transition transition, object[] entryArgs)
            {
                foreach (var action in _entryActions)
                    action.Execute(transition, entryArgs);
            }

            void ExecuteExitActions(Transition transition)
            {
                foreach (var action in _exitActions)
                    action.Execute(transition);
            }
            internal void InternalAction(Transition transition, object[] args)
            {
                var possibleActions = new List<InternalActionBehaviour>();

                // Look for actions in superstate(s) recursivly until we hit the topmost superstate, or we actually find some trigger handlers.
                StateRepresentation aStateRep = this;
                while (aStateRep != null)
                {
                    if (aStateRep.TryFindLocalHandler(transition.Trigger, out TriggerBehaviourResult result))
                    {
                        // Trigger handler(s) found in this state
                        possibleActions.AddRange(aStateRep._internalActions);
                        break;
                    }
                    // Try to look for trigger handlers in superstate (if it exists)
                    aStateRep = aStateRep._superstate;
                }

                // Execute internal transition event handler
                foreach (var action in possibleActions)
                {
                    action.Execute(transition, args);
                }
            }
            public void AddTriggerBehaviour(TriggerBehaviour triggerBehaviour)
            {
                if (!_triggerBehaviours.TryGetValue(triggerBehaviour.Trigger, out ICollection<TriggerBehaviour> allowed))
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
        }
    }
}
