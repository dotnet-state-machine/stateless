#if TASKS

using System;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        public partial class StateConfiguration
        {
            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <param name="trigger"></param>
            /// <param name="guard">Function that must return true in order for the trigger to be accepted.</param>
            /// <param name="entryAction"></param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionAsyncIf(TTrigger trigger, Func<bool> guard, Func<Transition, Task> entryAction)
            {
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour.Async(trigger, guard, (t, args) => entryAction(t)));
                return this;
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="guard">Function that must return true in order for the\r\n            /// trigger to be accepted.</param>
            /// <param name="internalAction">The asynchronous action performed by the internal transition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionAsyncIf(TTrigger trigger, Func<bool> guard, Func<Task> internalAction)
            {
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour.Async(trigger, guard, (t, args) => internalAction()));
                return this;
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="guard">Function that must return true in order for the trigger to be accepted.</param>
            /// <param name="internalAction">The asynchronous action performed by the internal transition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionAsyncIf<TArg0>(TTrigger trigger, Func<bool> guard, Func<Transition, Task> internalAction)
            {
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour.Async(trigger, guard, (t, args) => internalAction(t)));
                return this;
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="guard">Function that must return true in order for the trigger to be accepted.</param>
            /// <param name="internalAction">The asynchronous action performed by the internal transition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionAsyncIf<TArg0>(TriggerWithParameters<TArg0> trigger, Func<bool> guard, Func<TArg0, Transition, Task> internalAction)
            {
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour.Async(trigger.Trigger, guard, (t, args) => internalAction(ParameterConversion.Unpack<TArg0>(args, 0), t)));
                return this;
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <typeparam name="TArg1"></typeparam>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="guard">Function that must return true in order for the trigger to be accepted.</param>
            /// <param name="internalAction">The asynchronous action performed by the internal transition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionAsyncIf<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<bool> guard, Func<TArg0, TArg1, Transition, Task> internalAction)
            {
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour.Async(trigger.Trigger, guard, (t, args) => internalAction(
                    ParameterConversion.Unpack<TArg0>(args, 0),
                    ParameterConversion.Unpack<TArg1>(args, 1), t)));
                return this;
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <typeparam name="TArg1"></typeparam>
            /// <typeparam name="TArg2"></typeparam>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="guard">Function that must return true in order for the trigger to be accepted.</param>
            /// <param name="internalAction">The asynchronous action performed by the internal transition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionAsyncIf<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<bool> guard, Func<TArg0, TArg1, TArg2, Transition, Task> internalAction)
            {
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour.Async(trigger.Trigger, guard, (t, args) => internalAction(
                    ParameterConversion.Unpack<TArg0>(args, 0),
                    ParameterConversion.Unpack<TArg1>(args, 1),
                    ParameterConversion.Unpack<TArg2>(args, 2), t)));
                return this;
            }


            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <param name="trigger"></param>
            /// <param name="entryAction"></param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionAsync(TTrigger trigger, Func<Transition, Task> entryAction)
            {
                return InternalTransitionAsyncIf(trigger, () => true, entryAction);
            }
            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="internalAction">The asynchronous action performed by the internal transition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionAsync(TTrigger trigger, Func<Task> internalAction)
            {
                return InternalTransitionAsyncIf(trigger, () => true, internalAction);
            }
            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="internalAction">The asynchronous action performed by the internal transition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionAsync<TArg0>(TTrigger trigger, Func<Transition, Task> internalAction)
            {
                return InternalTransitionAsyncIf(trigger, () => true, internalAction);
            }
            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="internalAction">The asynchronous action performed by the internal transition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionAsync<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, Transition, Task> internalAction)
            {
                return InternalTransitionAsyncIf(trigger, () => true, internalAction);
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <typeparam name="TArg1"></typeparam>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="internalAction">The asynchronous action performed by the internal transition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionAsync<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, Transition, Task> internalAction)
            {
                return InternalTransitionAsyncIf(trigger, () => true, internalAction);
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <typeparam name="TArg1"></typeparam>
            /// <typeparam name="TArg2"></typeparam>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="internalAction">The asynchronous action performed by the internal transition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionAsync<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, Transition, Task> internalAction)
            {
                return InternalTransitionAsyncIf(trigger, () => true, internalAction);
            }

            /// <summary>
            /// Specify an asynchronous action that will execute when activating
            /// the configured state.
            /// </summary>
            /// <param name="activateAction">Action to execute.</param>
            /// <param name="activateActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnActivateAsync(Func<Task> activateAction, string activateActionDescription = null)
            {
                _representation.AddActivateAction(
                    activateAction,
                    Reflection.InvocationInfo.Create(activateAction, activateActionDescription, Reflection.InvocationInfo.Timing.Asynchronous));
                return this;
            }

            /// <summary>
            /// Specify an asynchronous action that will execute when deactivating
            /// the configured state.
            /// </summary>
            /// <param name="deactivateAction">Action to execute.</param>
            /// <param name="deactivateActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnDeactivateAsync(Func<Task> deactivateAction, string deactivateActionDescription = null)
            {
                _representation.AddDeactivateAction(
                    deactivateAction,
                    Reflection.InvocationInfo.Create(deactivateAction, deactivateActionDescription, Reflection.InvocationInfo.Timing.Asynchronous));
                return this;
            }

            /// <summary>
            /// Specify an asynchronous action that will execute when transitioning into
            /// the configured state.
            /// </summary>
            /// <param name="entryAction">Action to execute.</param>
            /// <param name="entryActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnEntryAsync(Func<Task> entryAction, string entryActionDescription = null)
            {
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddEntryAction(
                    (t, args) => entryAction(),
                    Reflection.InvocationInfo.Create(entryAction, entryActionDescription, Reflection.InvocationInfo.Timing.Asynchronous));
                return this;

            }

            /// <summary>
            /// Specify an asynchronous action that will execute when transitioning into
            /// the configured state.
            /// </summary>
            /// <param name="entryAction">Action to execute, providing details of the transition.</param>
            /// <param name="entryActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnEntryAsync(Func<Transition, Task> entryAction, string entryActionDescription = null)
            {
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddEntryAction(
                    (t, args) => entryAction(t),
                    Reflection.InvocationInfo.Create(entryAction, entryActionDescription, Reflection.InvocationInfo.Timing.Asynchronous));
                return this;
            }

            /// <summary>
            /// Specify an asynchronous action that will execute when transitioning into
            /// the configured state.
            /// </summary>
            /// <param name="entryAction">Action to execute.</param>
            /// <param name="trigger">The trigger by which the state must be entered in order for the action to execute.</param>
            /// <param name="entryActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnEntryFromAsync(TTrigger trigger, Func<Task> entryAction, string entryActionDescription = null)
            {
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddEntryAction(
                    trigger,
                    (t, args) => entryAction(),
                    Reflection.InvocationInfo.Create(entryAction, entryActionDescription, Reflection.InvocationInfo.Timing.Asynchronous));
                return this;
            }

            /// <summary>
            /// Specify an asynchronous action that will execute when transitioning into
            /// the configured state.
            /// </summary>
            /// <param name="entryAction">Action to execute, providing details of the transition.</param>
            /// <param name="trigger">The trigger by which the state must be entered in order for the action to execute.</param>
            /// <param name="entryActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnEntryFromAsync(TTrigger trigger, Func<Transition, Task> entryAction, string entryActionDescription = null)
            {
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddEntryAction(
                    trigger,
                    (t, args) => entryAction(t),
                    Reflection.InvocationInfo.Create(entryAction, entryActionDescription, Reflection.InvocationInfo.Timing.Asynchronous));
                return this;
            }

            /// <summary>
            /// Specify an asynchronous action that will execute when transitioning into
            /// the configured state.
            /// </summary>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <param name="entryAction">Action to execute, providing details of the transition.</param>
            /// <param name="trigger">The trigger by which the state must be entered in order for the action to execute.</param>
            /// <param name="entryActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnEntryFromAsync<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, Task> entryAction, string entryActionDescription = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddEntryAction(
                    trigger.Trigger,
                    (t, args) => entryAction(
                        ParameterConversion.Unpack<TArg0>(args, 0)),
                    Reflection.InvocationInfo.Create(entryAction, entryActionDescription, Reflection.InvocationInfo.Timing.Asynchronous));
                return this;
            }

            /// <summary>
            /// Specify an asynchronous action that will execute when transitioning into
            /// the configured state.
            /// </summary>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <param name="entryAction">Action to execute, providing details of the transition.</param>
            /// <param name="trigger">The trigger by which the state must be entered in order for the action to execute.</param>
            /// <param name="entryActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnEntryFromAsync<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, Transition, Task> entryAction, string entryActionDescription = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddEntryAction(
                    trigger.Trigger,
                    (t, args) => entryAction(
                        ParameterConversion.Unpack<TArg0>(args, 0), t),
                    Reflection.InvocationInfo.Create(entryAction, entryActionDescription, Reflection.InvocationInfo.Timing.Asynchronous));
                return this;
            }

            /// <summary>
            /// Specify an asynchronous action that will execute when transitioning into
            /// the configured state.
            /// </summary>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            /// <param name="entryAction">Action to execute, providing details of the transition.</param>
            /// <param name="trigger">The trigger by which the state must be entered in order for the action to execute.</param>
            /// <param name="entryActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnEntryFromAsync<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, Task> entryAction, string entryActionDescription = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddEntryAction(trigger.Trigger,
                    (t, args) => entryAction(
                        ParameterConversion.Unpack<TArg0>(args, 0),
                        ParameterConversion.Unpack<TArg1>(args, 1)),
                    Reflection.InvocationInfo.Create(entryAction, entryActionDescription, Reflection.InvocationInfo.Timing.Asynchronous));
                return this;
            }

            /// <summary>
            /// Specify an asynchronous action that will execute when transitioning into
            /// the configured state.
            /// </summary>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            /// <param name="entryAction">Action to execute, providing details of the transition.</param>
            /// <param name="trigger">The trigger by which the state must be entered in order for the action to execute.</param>
            /// <param name="entryActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnEntryFromAsync<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, Transition, Task> entryAction, string entryActionDescription = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddEntryAction(trigger.Trigger,
                    (t, args) => entryAction(
                        ParameterConversion.Unpack<TArg0>(args, 0),
                        ParameterConversion.Unpack<TArg1>(args, 1), t),
                    Reflection.InvocationInfo.Create(entryAction, entryActionDescription, Reflection.InvocationInfo.Timing.Asynchronous));
                return this;
            }

            /// <summary>
            /// Specify an asynchronous action that will execute when transitioning into
            /// the configured state.
            /// </summary>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
            /// <param name="entryAction">Action to execute, providing details of the transition.</param>
            /// <param name="trigger">The trigger by which the state must be entered in order for the action to execute.</param>
            /// <param name="entryActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnEntryFromAsync<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, Task> entryAction, string entryActionDescription = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddEntryAction(trigger.Trigger,
                    (t, args) => entryAction(
                        ParameterConversion.Unpack<TArg0>(args, 0),
                        ParameterConversion.Unpack<TArg1>(args, 1),
                        ParameterConversion.Unpack<TArg2>(args, 2)),
                    Reflection.InvocationInfo.Create(entryAction, entryActionDescription, Reflection.InvocationInfo.Timing.Asynchronous));
                return this;
            }

            /// <summary>
            /// Specify an asynchronous action that will execute when transitioning into
            /// the configured state.
            /// </summary>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
            /// <param name="entryAction">Action to execute, providing details of the transition.</param>
            /// <param name="trigger">The trigger by which the state must be entered in order for the action to execute.</param>
            /// <param name="entryActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnEntryFromAsync<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, Transition, Task> entryAction, string entryActionDescription = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddEntryAction(trigger.Trigger,
                    (t, args) => entryAction(
                        ParameterConversion.Unpack<TArg0>(args, 0),
                        ParameterConversion.Unpack<TArg1>(args, 1),
                        ParameterConversion.Unpack<TArg2>(args, 2), t),
                    Reflection.InvocationInfo.Create(entryAction, entryActionDescription, Reflection.InvocationInfo.Timing.Asynchronous));
                return this;
            }

            /// <summary>
            /// Specify an asynchronous action that will execute when transitioning from
            /// the configured state.
            /// </summary>
            /// <param name="exitAction">Action to execute.</param>
            /// <param name="exitActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnExitAsync(Func<Task> exitAction, string exitActionDescription = null)
            {
                if (exitAction == null) throw new ArgumentNullException(nameof(exitAction));

                _representation.AddExitAction(
                    t => exitAction(),
                    Reflection.InvocationInfo.Create(exitAction, exitActionDescription, Reflection.InvocationInfo.Timing.Asynchronous));
                return this;
            }

            /// <summary>
            /// Specify an asynchronous action that will execute when transitioning from
            /// the configured state.
            /// </summary>
            /// <param name="exitAction">Action to execute, providing details of the transition.</param>
            /// <param name="exitActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnExitAsync(Func<Transition, Task> exitAction, string exitActionDescription = null)
            {
                _representation.AddExitAction(
                    exitAction,
                    Reflection.InvocationInfo.Create(exitAction, exitActionDescription, Reflection.InvocationInfo.Timing.Asynchronous));
                return this;
            }
        }
    }
}
#endif
