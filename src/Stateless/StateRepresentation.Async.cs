#if TASKS

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
            public void AddActivateAction(Func<Task> action, string activateActionDescription)
            {
                _activateActions.Add(
                    new ActivateActionBehaviour.Async(
                        _state,
                        Enforce.ArgumentNotNull(action, nameof(action)),
                        Enforce.ArgumentNotNull(activateActionDescription, nameof(activateActionDescription))));
            }

            public void AddDeactivateAction(Func<Task> action, string deactivateActionDescription)
            {
                _deactivateActions.Add(
                    new DeactivateActionBehaviour.Async(
                        _state,
                        Enforce.ArgumentNotNull(action, nameof(action)),
                        Enforce.ArgumentNotNull(deactivateActionDescription, nameof(deactivateActionDescription))));
            }

            public void AddEntryAction(TTrigger trigger, Func<Transition, object[], Task> action, string entryActionDescription)
            {
                Enforce.ArgumentNotNull(action, nameof(action));
                _entryActions.Add(
                    new EntryActionBehavior.Async((t, args) =>
                    {
                        if (t.Trigger.Equals(trigger))
                            return action(t, args);

                        return TaskResult.Done;
                    },
                    Enforce.ArgumentNotNull(entryActionDescription, nameof(entryActionDescription))));
            }

            public void AddEntryAction(Func<Transition, object[], Task> action, string entryActionDescription)
            {
                _entryActions.Add(
                    new EntryActionBehavior.Async(
                        Enforce.ArgumentNotNull(action, nameof(action)),
                        Enforce.ArgumentNotNull(entryActionDescription, nameof(entryActionDescription))));
            }

            public void AddExitAction(Func<Transition, Task> action, string exitActionDescription)
            {
                _exitActions.Add(
                    new ExitActionBehavior.Async(
                        Enforce.ArgumentNotNull(action, nameof(action)),
                        Enforce.ArgumentNotNull(exitActionDescription, nameof(exitActionDescription))));
            }

            internal void AddInternalAction(TTrigger trigger, Func<Transition, object[], Task> action)
            {
                Enforce.ArgumentNotNull(action, "action");

                _internalActions.Add(new InternalActionBehaviour.Async((t, args) =>
                {
                    if (t.Trigger.Equals(trigger))
                        return action(t, args);

                    return TaskResult.Done;
                }));
            }

            public async Task ActivateAsync()
            {
                if (_superstate != null)
                    await _superstate.ActivateAsync();

                if (active)
                    return;

                await ExecuteActivationActionsAsync();
                active = true;
            }

            public async Task DeactivateAsync()
            {
                if (!active)
                    return;

                await ExecuteDeactivationActionsAsync();
                active = false;

                if (_superstate != null)
                    await _superstate.DeactivateAsync();
            }

            async Task ExecuteActivationActionsAsync()
            {
                foreach (var action in _activateActions)
                    await action.ExecuteAsync();
            }

            async Task ExecuteDeactivationActionsAsync()
            {
                foreach (var action in _deactivateActions)
                    await action.ExecuteAsync();
            }

            public async Task EnterAsync(Transition transition, params object[] entryArgs)
            {
                Enforce.ArgumentNotNull(transition, nameof(transition));

                if (transition.IsReentry)
                {
                    await ExecuteEntryActionsAsync(transition, entryArgs);
                    await ExecuteActivationActionsAsync();
                }
                else if (!Includes(transition.Source))
                {
                    if (_superstate != null)
                        await _superstate.EnterAsync(transition, entryArgs);

                    await ExecuteEntryActionsAsync(transition, entryArgs);
                    await ExecuteActivationActionsAsync();
                }
            }

            public async Task ExitAsync(Transition transition)
            {
                Enforce.ArgumentNotNull(transition, nameof(transition));

                if (transition.IsReentry)
                {
                    await ExecuteDeactivationActionsAsync();
                    await ExecuteExitActionsAsync(transition);
                }
                else if (!Includes(transition.Destination))
                {
                    await ExecuteDeactivationActionsAsync();
                    await ExecuteExitActionsAsync(transition);

                    if (_superstate != null)
                        await _superstate.ExitAsync(transition);
                }
            }

            async Task ExecuteEntryActionsAsync(Transition transition, object[] entryArgs)
            {
                Enforce.ArgumentNotNull(transition, nameof(transition));
                Enforce.ArgumentNotNull(entryArgs, nameof(entryArgs));
                foreach (var action in _entryActions)
                    await action.ExecuteAsync(transition, entryArgs);
            }

            async Task ExecuteExitActionsAsync(Transition transition)
            {
                Enforce.ArgumentNotNull(transition, nameof(transition));
                foreach (var action in _exitActions)
                    await action.ExecuteAsync(transition);
            }

            async Task ExecuteInternalActionsAsync(Transition transition, object[] args)
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
                    await action.ExecuteAsync(transition, args);
                }
            }

            internal Task InternalActionAsync(Transition transition, object[] args)
            {
                Enforce.ArgumentNotNull(transition, "transition");
                return ExecuteInternalActionsAsync(transition, args);
            }
        }
    }
}

#endif