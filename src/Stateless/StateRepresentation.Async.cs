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
            internal IDictionary<TTrigger, ICollection<TriggerBehaviourAsync>> TriggerBehavioursAsync { get; } = new Dictionary<TTrigger, ICollection<TriggerBehaviourAsync>>();
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
                    new EntryActionBehavior.AsyncFrom<TTrigger>(
                        trigger,
                        action,
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
                    await _superstate.ActivateAsync().ConfigureAwait(_retainSynchronizationContext);

                await ExecuteActivationActionsAsync().ConfigureAwait(_retainSynchronizationContext);
            }

            public async Task DeactivateAsync()
            {
                await ExecuteDeactivationActionsAsync().ConfigureAwait(_retainSynchronizationContext);

                if (_superstate != null)
                    await _superstate.DeactivateAsync().ConfigureAwait(_retainSynchronizationContext);
            }

            async Task ExecuteActivationActionsAsync()
            {
                foreach (var action in ActivateActions)
                    await action.ExecuteAsync().ConfigureAwait(_retainSynchronizationContext);
            }

            async Task ExecuteDeactivationActionsAsync()
            {
                foreach (var action in DeactivateActions)
                    await action.ExecuteAsync().ConfigureAwait(_retainSynchronizationContext);
            }


            public async Task EnterAsync(Transition transition, params object[] entryArgs)
            {
                if (transition.IsReentry)
                {
                    await ExecuteEntryActionsAsync(transition, entryArgs).ConfigureAwait(_retainSynchronizationContext);
                }
                else if (!Includes(transition.Source))
                {
                    if (_superstate != null && !(transition is InitialTransition))
                        await _superstate.EnterAsync(transition, entryArgs).ConfigureAwait(_retainSynchronizationContext);

                    await ExecuteEntryActionsAsync(transition, entryArgs).ConfigureAwait(_retainSynchronizationContext);
                }
            }
            
            public async Task<Transition> ExitAsync(Transition transition)
            {
                if (transition.IsReentry)
                {
                    await ExecuteExitActionsAsync(transition).ConfigureAwait(_retainSynchronizationContext);
                }
                else if (!Includes(transition.Destination))
                {
                    await ExecuteExitActionsAsync(transition).ConfigureAwait(_retainSynchronizationContext);

                    // Must check if there is a superstate, and if we are leaving that superstate
                    if (_superstate != null)
                    {
                        // Check if destination is within the state list
                        if (IsIncludedIn(transition.Destination))
                        {
                            // Destination state is within the list, exit first superstate only if it is NOT the first
                            if (!_superstate.UnderlyingState.Equals(transition.Destination))
                            {
                                return await _superstate.ExitAsync(transition).ConfigureAwait(_retainSynchronizationContext);
                            }
                        }
                        else
                        {
                            // Exit the superstate as well
                            return await _superstate.ExitAsync(transition).ConfigureAwait(_retainSynchronizationContext);
                        }
                    }
                }
                return transition;
            }

            async Task ExecuteEntryActionsAsync(Transition transition, object[] entryArgs)
            {
                foreach (var action in EntryActions)
                    await action.ExecuteAsync(transition, entryArgs).ConfigureAwait(_retainSynchronizationContext);
            }

            async Task ExecuteExitActionsAsync(Transition transition)
            {
                foreach (var action in ExitActions)
                    await action.ExecuteAsync(transition).ConfigureAwait(_retainSynchronizationContext);
            }

            public void AddTriggerBehaviourAsync(TriggerBehaviourAsync triggerBehaviour)
            {
                if (!TriggerBehavioursAsync.TryGetValue(triggerBehaviour.Trigger, out ICollection<TriggerBehaviourAsync> allowed))
                {
                    allowed = new List<TriggerBehaviourAsync>();
                    TriggerBehavioursAsync.Add(triggerBehaviour.Trigger, allowed);
                }
                allowed.Add(triggerBehaviour);
            }

            public async Task<List<TTrigger>> GetAsyncTriggers(params object[] args)
            {
                var syncResult = new List<TTrigger>();

                foreach (var triggerBehaviour in TriggerBehavioursAsync)
                {
                    foreach (var value in triggerBehaviour.Value)
                    {
                        if (!(await value.UnmetGuardConditions(args)).Any())
                        {
                            syncResult.Add(triggerBehaviour.Key);
                            break; // Exit inner loop once a match is found
                        }
                    }
                }

                return syncResult;
            }


            public async Task<List<TTrigger>> GetPermittedTriggersAsync(params object[] args)
            {
                var resultList = new List<TTrigger>();

                var syncResult = TriggerBehaviours
                    .Where(t => t.Value.Any(a => !a.UnmetGuardConditions(args).Any()))
                    .Select(t => t.Key);

                resultList.AddRange(syncResult);

                var asyncTriggers = await GetAsyncTriggers(args);
                resultList.AddRange(asyncTriggers);

                
                if (Superstate != null)
                {
                    var superStatePermittedTriggers = await Superstate.GetPermittedTriggersAsync(args);
                    resultList = resultList.Union(superStatePermittedTriggers).ToList();
                }

                return resultList;
            }

            public async Task<TriggerBehaviourResult> TryFindHandlerAsync(TTrigger trigger, object[] args)
            {
                var localHandlerFound = await TryFindLocalHandlerAsync(trigger, args);

                var superstateHandlerFound = Superstate != null
                    ? await Superstate.TryFindHandlerAsync(trigger, args)
                    : null;

                return superstateHandlerFound ?? localHandlerFound;
            }

            private async Task<TriggerBehaviourResult> TryFindLocalHandlerAsync(TTrigger trigger, object[] args)
            {
                // Get list of candidate trigger handlers
                var syncTriggerNotFound = !TriggerBehaviours.TryGetValue(trigger, out var syncTriggers);
                var asyncTriggerNotFound = !TriggerBehavioursAsync.TryGetValue(trigger, out var asyncTriggers);

                if (syncTriggerNotFound && asyncTriggerNotFound) return null;

                // Guard functions are executed here
                var actualSync = syncTriggers?
                    .Select(h => new TriggerBehaviourResult(h, h.UnmetGuardConditions(args)))
                    .ToArray();

                var handleResultSync = actualSync != null ? TryFindLocalHandlerResult(trigger, actualSync) ?? TryFindLocalHandlerResultWithUnmetGuardConditions(actualSync) : null;

                var asyncTriggerBehaviourResult = new List<TriggerBehaviourResult>();

                TriggerBehaviourResult handleResultAsync = null;

                if (asyncTriggers != null)
                {
                    foreach (var asyncTrigger in asyncTriggers)
                    {
                        var unmetGuardConditions = await asyncTrigger.UnmetGuardConditions(args);
                        asyncTriggerBehaviourResult.Add(new TriggerBehaviourResult(asyncTrigger, unmetGuardConditions));
                    }

                    handleResultAsync = TryFindLocalHandlerResult(trigger, asyncTriggerBehaviourResult) ?? TryFindLocalHandlerResultWithUnmetGuardConditions(asyncTriggerBehaviourResult);
                }
               

                return handleResultSync ?? handleResultAsync;
            }
        }
    }
}

#endif
