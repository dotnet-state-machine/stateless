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
            public void AddActivateAction(Func<Task> action, Reflection.InvocationInfo activateActionDescription)
            {
                _activateActions.Add(new ActivateActionBehaviour.Async(_state, action, activateActionDescription));
            }

            public void AddDeactivateAction(Func<Task> action, Reflection.InvocationInfo deactivateActionDescription)
            {
                _deactivateActions.Add(new DeactivateActionBehaviour.Async(_state, action, deactivateActionDescription));
            }

            public void AddEntryAction(TTrigger trigger, Func<Transition, object[], Task> action, Reflection.InvocationInfo entryActionDescription)
            {
                if (action == null) throw new ArgumentNullException(nameof(action));

                _entryActions.Add(
                    new EntryActionBehavior.Async((t, args) =>
                    {
                        if (t.Trigger.Equals(trigger))
                            return action(t, args);

                        return TaskResult.Done;
                    },
                    entryActionDescription));
            }

            public void AddEntryAction(Func<Transition, object[], Task> action, Reflection.InvocationInfo entryActionDescription)
            {
                _entryActions.Add(
                    new EntryActionBehavior.Async(
                        action,
                        entryActionDescription));
            }

            public void AddExitAction(Func<Transition, Task> action, Reflection.InvocationInfo exitActionDescription)
            {
                _exitActions.Add(new ExitActionBehavior.Async(action, exitActionDescription));
            }

            internal void AddInternalAction(TTrigger trigger, Func<Transition, object[], Task> action)
            {
                if (action == null) throw new ArgumentNullException(nameof(action));

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
                foreach (var action in _entryActions)
                    await action.ExecuteAsync(transition, entryArgs);
            }

            async Task ExecuteExitActionsAsync(Transition transition)
            {
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
                return ExecuteInternalActionsAsync(transition, args);
            }
        }
    }
}

#endif