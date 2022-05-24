using Stateless.Reflection;

namespace Stateless;

public partial class StateMachine<TState, TTrigger> {
    internal partial class StateRepresentation {
        public void AddActivateAction(Func<Task> action, InvocationInfo activateActionDescription) {
            ActivateActions.Add(new ActivationChangeActionBehaviour(action, activateActionDescription));
        }

        public void AddDeactivateAction(Func<Task> action, InvocationInfo deactivateActionDescription) {
            DeactivateActions.Add(new ActivationChangeActionBehaviour(action, deactivateActionDescription));
        }

        public void AddEntryAction(TTrigger       trigger, Func<Transition, object?[], Task> action,
                                   InvocationInfo entryActionDescription) {
            if (action == null) throw new ArgumentNullException(nameof(action));

            EntryActions.Add(new EntryActionBehavior.From(trigger, action, entryActionDescription));
        }

        public void AddEntryAction(Func<Transition, object?[], Task> action, InvocationInfo entryActionDescription) {
            EntryActions.Add(
                             new EntryActionBehavior(
                                                     action,
                                                     entryActionDescription));
        }

        public void AddExitAction(Func<Transition, Task> action, InvocationInfo exitActionDescription) {
            ExitActions.Add(new ExitActionBehavior(action, exitActionDescription));
        }

        public async Task ActivateAsync() {
            if (Superstate is { })
                await Superstate.ActivateAsync().ConfigureAwait(false);

            await ExecuteActivationActionsAsync().ConfigureAwait(false);
        }

        public async Task DeactivateAsync() {
            await ExecuteDeactivationActionsAsync().ConfigureAwait(false);

            if (Superstate is { })
                await Superstate.DeactivateAsync().ConfigureAwait(false);
        }

        private async Task ExecuteActivationActionsAsync() {
            foreach (var action in ActivateActions)
                await action.ExecuteAsync().ConfigureAwait(false);
        }

        private async Task ExecuteDeactivationActionsAsync() {
            foreach (var action in DeactivateActions)
                await action.ExecuteAsync().ConfigureAwait(false);
        }


        public async Task EnterAsync(Transition transition, params object?[] entryArgs) {
            if (transition.IsReentry) {
                await ExecuteEntryActionsAsync(transition, entryArgs).ConfigureAwait(false);
            } else if (!Includes(transition.Source)) {
                if (Superstate is { } && !transition.IsInitial)
                    await Superstate.EnterAsync(transition, entryArgs).ConfigureAwait(false);

                await ExecuteEntryActionsAsync(transition, entryArgs).ConfigureAwait(false);
            }
        }

        public async Task<Transition> ExitAsync(Transition transition) {
            if (transition.IsReentry) {
                await ExecuteExitActionsAsync(transition).ConfigureAwait(false);
            } else if (!Includes(transition.Destination)) {
                await ExecuteExitActionsAsync(transition).ConfigureAwait(false);

                // Must check if there is a superstate, and if we are leaving that superstate
                if (Superstate is null) return transition;
                // Check if destination is within the state list
                if (IsIncludedIn(transition.Destination)) {
                    // Destination state is within the list, exit first superstate only if it is NOT the first
                    if (!Superstate.UnderlyingState.Equals(transition.Destination))
                        return await Superstate.ExitAsync(transition).ConfigureAwait(false);
                } else {
                    // Exit the superstate as well
                    return await Superstate.ExitAsync(transition).ConfigureAwait(false);
                }
            }

            return transition;
        }

        private async Task ExecuteEntryActionsAsync(Transition transition, object?[] entryArgs) {
            foreach (var action in EntryActions)
                await action.ExecuteAsync(transition, entryArgs).ConfigureAwait(false);
        }

        private async Task ExecuteExitActionsAsync(Transition transition) {
            foreach (var action in ExitActions)
                await action.ExecuteAsync(transition).ConfigureAwait(false);
        }
    }
}