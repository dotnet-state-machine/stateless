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

            StateRepresentation _superstate; // null
            bool active;

            readonly ICollection<StateRepresentation> _substates = new List<StateRepresentation>();
            public TState InitialTransitionTarget { get; private set; } = default(TState);

            public StateRepresentation(TState state)
            {
                _state = state;
            }

            internal ICollection<StateRepresentation> GetSubstates()
            {
                return _substates;
            }

            public bool CanHandle(TTrigger trigger, params object[] args)
            {
                return TryFindHandler(trigger, args, out TriggerBehaviourResult unused);
            }

            public bool TryFindHandler(TTrigger trigger, object[] args, out TriggerBehaviourResult handler)
            {
                return (TryFindLocalHandler(trigger, args, out handler) ||
                    (Superstate != null && Superstate.TryFindHandler(trigger, args, out handler)));
            }

            bool TryFindLocalHandler(TTrigger trigger, object[] args, out TriggerBehaviourResult handlerResult)
            {
                // Get list of candidate trigger handlers
                if (!_triggerBehaviours.TryGetValue(trigger, out ICollection<TriggerBehaviour> possible))
                {
                    handlerResult = null;
                    return false;
                }

                // Remove those that have unmet guard conditions
                // Guard functions are executed here
                var actual = possible
                    .Select(h => new TriggerBehaviourResult(h, h.UnmetGuardConditions(args)))
                    .Where(g => g.UnmetGuardConditions.Count == 0)
                    .ToArray();

                // Find a handler for the trigger
                handlerResult = TryFindLocalHandlerResult(trigger, actual, r => !r.UnmetGuardConditions.Any())
                    ?? TryFindLocalHandlerResult(trigger, actual, r => r.UnmetGuardConditions.Any());

                if (handlerResult == null) return false;

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
                InternalTriggerBehaviour.Sync internalTransition = null;

                // Look for actions in superstate(s) recursivly until we hit the topmost superstate, or we actually find some trigger handlers.
                StateRepresentation aStateRep = this;
                while (aStateRep != null)
                {
                    if (aStateRep.TryFindLocalHandler(transition.Trigger, args, out TriggerBehaviourResult result))
                    {
                        // Trigger handler found in this state
                        if (result.Handler is InternalTriggerBehaviour.Async)
                            throw new InvalidOperationException("Running Async internal actions in synchronous mode is not allowed");

                        internalTransition = result.Handler as InternalTriggerBehaviour.Sync;
                        break;
                    }
                    // Try to look for trigger handlers in superstate (if it exists)
                    aStateRep = aStateRep._superstate;
                }

                // Execute internal transition event handler
                internalTransition?.InternalAction(transition, args);
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
					return GetPermittedTriggers();
				}
			}

            public IEnumerable<TTrigger> GetPermittedTriggers(params object[] args)
            {
                var result = _triggerBehaviours
                    .Where(t => t.Value.Any(a => !a.UnmetGuardConditions(args).Any()))
                    .Select(t => t.Key);

                if (Superstate != null)
                    result = result.Union(Superstate.GetPermittedTriggers(args));

                return result.ToArray();
            }

            internal void SetInitialTransition(TState state)
            {
                InitialTransitionTarget = state;
            }

            public bool HasInitialTransition()
            {
                if (InitialTransitionTarget == null) return false;

                return !InitialTransitionTarget.Equals(default(TState));
            }
        }
    }
}
