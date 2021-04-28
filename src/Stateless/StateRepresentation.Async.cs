#if TASKS

using System;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal partial class StateRepresentation
        {
            public void AddActivateAction(Func<Task> action, Reflection.InvocationInfo activateActionDescription)
            {
                ActivateActions.Add(new ActivateActionBehaviour.Async(_state, action, activateActionDescription));
            }

            public void AddDeactivateAction(Func<Task> action, Reflection.InvocationInfo deactivateActionDescription)
            {
                DeactivateActions.Add(new DeactivateActionBehaviour.Async(_state, action, deactivateActionDescription));
            }

            public void AddEntryAction(TTrigger trigger, Func<Transition, object[], Task> action, Reflection.InvocationInfo entryActionDescription)
            {
                if (action == null) throw new ArgumentNullException(nameof(action));

                EntryActions.Add(
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
                EntryActions.Add(
                    new EntryActionBehavior.Async(
                        action,
                        entryActionDescription));
            }

            public void AddExitAction(Func<Transition, Task> action, Reflection.InvocationInfo exitActionDescription)
            {
                ExitActions.Add(new ExitActionBehavior.Async(action, exitActionDescription));
            }

            public async Task ActivateAsync()
            {
                if (_superstate != null)
                    await _superstate.ActivateAsync().ConfigureAwait(false);

                await ExecuteActivationActionsAsync().ConfigureAwait(false);
            }

            public async Task DeactivateAsync()
            {
                await ExecuteDeactivationActionsAsync().ConfigureAwait(false);

                if (_superstate != null)
                    await _superstate.DeactivateAsync().ConfigureAwait(false);
            }

            async Task ExecuteActivationActionsAsync()
            {
                foreach (var action in ActivateActions)
                    await action.ExecuteAsync().ConfigureAwait(false);
            }

            async Task ExecuteDeactivationActionsAsync()
            {
                foreach (var action in DeactivateActions)
                    await action.ExecuteAsync().ConfigureAwait(false);
            }


            public async Task EnterAsync(Transition transition, params object[] entryArgs)
            {
                if (transition.IsReentry)
                {
                    await ExecuteEntryActionsAsync(transition, entryArgs).ConfigureAwait(false);
                }
                else if (!Includes(transition.Source))
                {
                    if (_superstate != null && !(transition is InitialTransition))
                        await _superstate.EnterAsync(transition, entryArgs).ConfigureAwait(false);

                    await ExecuteEntryActionsAsync(transition, entryArgs).ConfigureAwait(false);
                }
            }
            
            public async Task<Transition> ExitAsync(Transition transition)
            {
                if (transition.IsReentry)
                {
                    await ExecuteExitActionsAsync(transition).ConfigureAwait(false);
                }
                else if (!Includes(transition.Destination))
                {
                    await ExecuteExitActionsAsync(transition).ConfigureAwait(false);

                    // Must check if there is a superstate, and if we are leaving that superstate
                    if (_superstate != null)
                    {
                        // Check if destination is within the state list
                        if (IsIncludedIn(transition.Destination))
                        {
                            // Destination state is within the list, exit first superstate only if it is NOT the first
                            if (!_superstate.UnderlyingState.Equals(transition.Destination))
                            {
                                return await _superstate.ExitAsync(transition).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            // Exit the superstate as well
                            return await _superstate.ExitAsync(transition).ConfigureAwait(false);
                        }
                    }
                }
                return transition;
            }

            async Task ExecuteEntryActionsAsync(Transition transition, object[] entryArgs)
            {
                foreach (var action in EntryActions)
                    await action.ExecuteAsync(transition, entryArgs).ConfigureAwait(false);
            }

            async Task ExecuteExitActionsAsync(Transition transition)
            {
                foreach (var action in ExitActions)
                    await action.ExecuteAsync(transition).ConfigureAwait(false);
            }
        }
    }
}

#endif
