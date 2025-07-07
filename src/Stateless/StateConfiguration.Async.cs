#if TASKS

using System;
using System.Linq;
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
            /// <typeparam name="TArg0"></typeparam>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="guard">Function that must return true in order for the trigger to be accepted.</param>
            /// <param name="internalAction">The asynchronous action performed by the internal transition</param>
            /// <returns></returns>
            [Obsolete("Use InternalTransitionAsyncIf(TTrigger, Func<bool>, Func<Task>) instead.")]
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
            [Obsolete("Use InternalTransitionAsyncIf<TArg0>(TriggerWithParameters<TArg0>, Func<TArg0, bool>, Func<TArg0, Transition, Task>) instead.")]
            public StateConfiguration InternalTransitionAsyncIf<TArg0>(TriggerWithParameters<TArg0> trigger, Func<bool> guard, Func<TArg0, Transition, Task> internalAction)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
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
            [Obsolete("Use InternalTransitionAsyncIf<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1>, Func<TArg0, TArg1, bool>, Func<TArg0, TArg1, Transition, Task>) instead.")]
            public StateConfiguration InternalTransitionAsyncIf<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<bool> guard, Func<TArg0, TArg1, Transition, Task> internalAction)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
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
            [Obsolete("Use InternalTransitionAsyncIf<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2>, Func<TArg0, TArg1, TArg2, bool>, Func<TArg0, TArg1, TArg2, Transition, Task>) instead.")]
            public StateConfiguration InternalTransitionAsyncIf<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<bool> guard, Func<TArg0, TArg1, TArg2, Transition, Task> internalAction)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
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
            /// <typeparam name="TArg0"></typeparam>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="internalAction">The asynchronous action performed by the internal transition</param>
            /// <returns></returns>
            [Obsolete("Use InternalTransitionAsync(TTrigger, Func<Transition, Task>) instead.")]
            public StateConfiguration InternalTransitionAsync<TArg0>(TTrigger trigger, Func<Transition, Task> internalAction)
            {
                return InternalTransitionAsyncIf(trigger, () => true, internalAction);
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

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour.Async(trigger, t => guard(), (t, args) => internalAction()));
                return this;
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="guard">Function that must return true in order for the trigger to be accepted.</param>
            /// <param name="internalAction">The asynchronous action performed by the internal transition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionAsyncIf(TTrigger trigger, Func<bool> guard, Func<Transition, Task> internalAction)
            {
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour.Async(trigger, t => guard(), (t, args) => internalAction(t)));
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
            public StateConfiguration InternalTransitionAsyncIf<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, bool> guard, Func<TArg0, Transition, Task> internalAction)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour.Async(trigger.Trigger, TransitionGuard.ToPackedGuard(guard), (t, args) => internalAction(ParameterConversion.Unpack<TArg0>(args, 0), t)));
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
            public StateConfiguration InternalTransitionAsyncIf<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, bool> guard, Func<TArg0, TArg1, Transition, Task> internalAction)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour.Async(trigger.Trigger, TransitionGuard.ToPackedGuard(guard), (t, args) => internalAction(
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
            public StateConfiguration InternalTransitionAsyncIf<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, bool> guard, Func<TArg0, TArg1, TArg2, Transition, Task> internalAction)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour.Async(trigger.Trigger, TransitionGuard.ToPackedGuard(guard), (t, args) => internalAction(
                    ParameterConversion.Unpack<TArg0>(args, 0),
                    ParameterConversion.Unpack<TArg1>(args, 1),
                    ParameterConversion.Unpack<TArg2>(args, 2), t)));
                return this;
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
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="internalAction">The asynchronous action performed by the internal transition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionAsync(TTrigger trigger, Func<Transition, Task> internalAction)
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
                return InternalTransitionAsyncIf(trigger, t => true, internalAction);
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
                return InternalTransitionAsyncIf(trigger, (t1, t2) => true, internalAction);
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
                return InternalTransitionAsyncIf(trigger, (t1, t2, t3) => true, internalAction);
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

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied async function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">
            /// Async function to calculate the destination state; if the source and destination states are the same, it will be reentered and 
            /// any exit or entry logic will be invoked.
            /// </param>
            /// <param name="destinationStateSelectorDescription">Optional description for the async function to calculate the state </param>
            /// <param name="possibleDestinationStates">Optional array of possible destination states (used by output formatters) </param>
            /// <returns>The receiver.</returns>
            public StateConfiguration PermitDynamicAsync(TTrigger trigger, Func<Task<TState>> destinationStateSelector,
                string destinationStateSelectorDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                _representation.AddTriggerBehaviour(
                    new DynamicTriggerBehaviourAsync(trigger,
                        args => destinationStateSelector(),
                        null,           // No transition guard
                        Reflection.DynamicTransitionInfo.Create(trigger,
                            null,       // No guards
                            Reflection.InvocationInfo.Create(destinationStateSelector, destinationStateSelectorDescription),
                            possibleDestinationStates
                        )
                    ));
                return this;
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied async function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">
            /// Function to calculate the destination state; if the source and destination states are the same, it will be reentered and 
            /// any exit or entry logic will be invoked.
            /// </param>
            /// <param name="destinationStateSelectorDescription">Optional description of the function to calculate the state </param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The receiver.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            public StateConfiguration PermitDynamicAsync<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, Task<TState>> destinationStateSelector,
                string destinationStateSelectorDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                if (trigger == null) throw new ArgumentNullException(nameof(trigger));

                _representation.AddTriggerBehaviour(
                    new DynamicTriggerBehaviourAsync(trigger.Trigger,
                        args => destinationStateSelector(
                            ParameterConversion.Unpack<TArg0>(args, 0)),
                        null,       // No transition guards
                        Reflection.DynamicTransitionInfo.Create(trigger.Trigger,
                            null,    // No guards
                            Reflection.InvocationInfo.Create(destinationStateSelector, destinationStateSelectorDescription),
                            possibleDestinationStates)
                    ));
                return this;

            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied async function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">
            /// Function to calculate the destination state; if the source and destination states are the same, it will be reentered and 
            /// any exit or entry logic will be invoked.
            /// </param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="guardDescription">Guard description</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration PermitDynamicIfAsync(TTrigger trigger, Func<Task<TState>> destinationStateSelector,
                Func<bool> guard, string guardDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                return PermitDynamicIfAsync(trigger, destinationStateSelector, null, guard, guardDescription, possibleDestinationStates);
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied async function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">
            /// Function to calculate the destination state; if the source and destination states are the same, it will be reentered and 
            /// any exit or entry logic will be invoked.
            /// </param>
            /// <param name="destinationStateSelectorDescription">Description of the function to calculate the state </param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="guardDescription">Guard description</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration PermitDynamicIfAsync(TTrigger trigger, Func<Task<TState>> destinationStateSelector,
                string destinationStateSelectorDescription, Func<bool> guard, string guardDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIfAsync(
                    trigger,
                    args => destinationStateSelector(),
                    destinationStateSelectorDescription,
                    new TransitionGuard(guard, guardDescription),
                    possibleDestinationStates);
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied async function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">
            /// Function to calculate the destination state; if the source and destination states are the same, it will be reentered and 
            /// any exit or entry logic will be invoked.
            /// </param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration PermitDynamicIfAsync(TTrigger trigger, Func<Task<TState>> destinationStateSelector, Reflection.DynamicStateInfos possibleDestinationStates = null, params Tuple<Func<bool>, string>[] guards)
            {
                return PermitDynamicIfAsync(trigger, destinationStateSelector, null, possibleDestinationStates, guards);
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied async function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">
            /// Function to calculate the destination state; if the source and destination states are the same, it will be reentered and 
            /// any exit or entry logic will be invoked.
            /// </param>
            /// <param name="destinationStateSelectorDescription">Description of the function to calculate the state </param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration PermitDynamicIfAsync(TTrigger trigger, Func<Task<TState>> destinationStateSelector,
                string destinationStateSelectorDescription, Reflection.DynamicStateInfos possibleDestinationStates = null, params Tuple<Func<bool>, string>[] guards)
            {
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIfAsync(
                    trigger,
                    args => destinationStateSelector(),
                    destinationStateSelectorDescription,
                    new TransitionGuard(guards),
                    possibleDestinationStates);
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied async function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">
            /// Function to calculate the destination state; if the source and destination states are the same, it will be reentered and 
            /// any exit or entry logic will be invoked.
            /// </param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="guardDescription">Guard description</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The receiver.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIfAsync<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, Task<TState>> destinationStateSelector, Func<bool> guard, string guardDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIfAsync(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0)),
                    null,    // destinationStateSelectorString
                    new TransitionGuard(guard, guardDescription),
                    possibleDestinationStates);
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied async function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">
            /// Function to calculate the destination state; if the source and destination states are the same, it will be reentered and 
            /// any exit or entry logic will be invoked.
            /// </param>
            /// <returns>The receiver.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIfAsync<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, Task<TState>> destinationStateSelector)
            {
                return PermitDynamicIfAsync<TArg0>(trigger, destinationStateSelector, null, new Tuple<Func<bool>, string>[0]);
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied async function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">
            /// Function to calculate the destination state; if the source and destination states are the same, it will be reentered and 
            /// any exit or entry logic will be invoked.
            /// </param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be accepted.</param>
            /// <returns>The receiver.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIfAsync<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, Task<TState>> destinationStateSelector, Reflection.DynamicStateInfos possibleDestinationStates = null, params Tuple<Func<bool>, string>[] guards)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIfAsync(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0)),
                    null,    // destinationStateSelectorString
                    new TransitionGuard(guards),
                    possibleDestinationStates);
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied async function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">
            /// Function to calculate the destination state; if the source and destination states are the same, it will be reentered and 
            /// any exit or entry logic will be invoked.
            /// </param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="guardDescription">Guard description</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The receiver.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIfAsync<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, Task<TState>> destinationStateSelector, Func<bool> guard, string guardDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIfAsync(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0),
                        ParameterConversion.Unpack<TArg1>(args, 1)),
                    null,    // destinationStateSelectorString
                    new TransitionGuard(guard, guardDescription),
                    possibleDestinationStates);
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied async function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">
            /// Function to calculate the destination state; if the source and destination states are the same, it will be reentered and 
            /// any exit or entry logic will be invoked.
            /// </param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be accepted.</param>
            /// <returns>The receiver.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIfAsync<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, Task<TState>> destinationStateSelector, Reflection.DynamicStateInfos possibleDestinationStates = null, params Tuple<Func<bool>, string>[] guards)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIfAsync(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0),
                        ParameterConversion.Unpack<TArg1>(args, 1)),
                    null,    // destinationStateSelectorString
                    new TransitionGuard(guards),
                    possibleDestinationStates);
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied async function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">
            /// Function to calculate the destination state; if the source and destination states are the same, it will be reentered and 
            /// any exit or entry logic will be invoked.
            /// </param>
            /// <returns>The receiver.</returns>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="guardDescription">Guard description</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIfAsync<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, Task<TState>> destinationStateSelector, Func<bool> guard, string guardDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIfAsync(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0),
                        ParameterConversion.Unpack<TArg1>(args, 1),
                        ParameterConversion.Unpack<TArg2>(args, 2)),
                    null,    // destinationStateSelectorString
                    new TransitionGuard(guard, guardDescription),
                    possibleDestinationStates);
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied async function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">
            /// Function to calculate the destination state; if the source and destination states are the same, it will be reentered and 
            /// any exit or entry logic will be invoked.
            /// </param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The receiver.</returns>
            /// <param name="guards">Functions ant their descriptions that must return true in order for the
            /// trigger to be accepted.</param>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIfAsync<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, Task<TState>> destinationStateSelector, Reflection.DynamicStateInfos possibleDestinationStates = null, params Tuple<Func<bool>, string>[] guards)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIfAsync(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0),
                        ParameterConversion.Unpack<TArg1>(args, 1),
                        ParameterConversion.Unpack<TArg2>(args, 2)),
                    null,    // destinationStateSelectorString
                    new TransitionGuard(guards),
                    possibleDestinationStates);
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied async function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">
            /// Function to calculate the destination state; if the source and destination states are the same, it will be reentered and 
            /// any exit or entry logic will be invoked.
            /// </param>
            /// <param name="guard">Parameterized Function that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="guardDescription">Guard description</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The receiver.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIfAsync<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, Task<TState>> destinationStateSelector, Func<TArg0, bool> guard, string guardDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIfAsync(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0)),
                    null,    // destinationStateSelectorString
                    new TransitionGuard(TransitionGuard.ToPackedGuard(guard), guardDescription),
                    possibleDestinationStates);
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied async function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">
            /// Function to calculate the destination state; if the source and destination states are the same, it will be reentered and 
            /// any exit or entry logic will be invoked.
            /// </param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be accepted.</param>
            /// <returns>The receiver.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIfAsync<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, Task<TState>> destinationStateSelector, Reflection.DynamicStateInfos possibleDestinationStates = null, params Tuple<Func<TArg0, bool>, string>[] guards)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIfAsync(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0)),
                    null,    // destinationStateSelectorString
                    new TransitionGuard(TransitionGuard.ToPackedGuards(guards)),
                    possibleDestinationStates);
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied async function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">
            /// Function to calculate the destination state; if the source and destination states are the same, it will be reentered and 
            /// any exit or entry logic will be invoked.
            /// </param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="guardDescription">Guard description</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The receiver.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIfAsync<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, Task<TState>> destinationStateSelector, Func<TArg0, TArg1, bool> guard, string guardDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIfAsync(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0),
                        ParameterConversion.Unpack<TArg1>(args, 1)),
                    null,    // destinationStateSelectorString
                    new TransitionGuard(TransitionGuard.ToPackedGuard(guard), guardDescription),
                    possibleDestinationStates);
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied async function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">
            /// Function to calculate the destination state; if the source and destination states are the same, it will be reentered and 
            /// any exit or entry logic will be invoked.
            /// </param>
            /// <param name="guards">Functions that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The receiver.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIfAsync<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, Task<TState>> destinationStateSelector, Tuple<Func<TArg0, TArg1, bool>, string>[] guards, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIfAsync(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0),
                        ParameterConversion.Unpack<TArg1>(args, 1)),
                    null,    // destinationStateSelectorString
                    new TransitionGuard(TransitionGuard.ToPackedGuards(guards)),
                    possibleDestinationStates);
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied async function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">
            /// Function to calculate the destination state; if the source and destination states are the same, it will be reentered and 
            /// any exit or entry logic will be invoked.
            /// </param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="guardDescription">Guard description</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The receiver.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIfAsync<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, Task<TState>> destinationStateSelector, Func<TArg0, TArg1, TArg2, bool> guard, string guardDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIfAsync(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0),
                        ParameterConversion.Unpack<TArg1>(args, 1),
                        ParameterConversion.Unpack<TArg2>(args, 2)),
                    null,    // destinationStateSelectorString
                    new TransitionGuard(TransitionGuard.ToPackedGuard(guard), guardDescription),
                    possibleDestinationStates);
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied async function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">
            /// Function to calculate the destination state; if the source and destination states are the same, it will be reentered and 
            /// any exit or entry logic will be invoked.
            /// </param>
            /// <param name="guards">Functions that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The receiver.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIfAsync<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, Task<TState>> destinationStateSelector, Tuple<Func<TArg0, TArg1, TArg2, bool>, string>[] guards, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIfAsync(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0),
                        ParameterConversion.Unpack<TArg1>(args, 1),
                        ParameterConversion.Unpack<TArg2>(args, 2)),
                    null,    // destinationStateSelectorString
                    new TransitionGuard(TransitionGuard.ToPackedGuards(guards)),
                    possibleDestinationStates);
            }
            /// <summary>
            /// Accept the specified trigger and transition to the destination state.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationState">The state that the trigger will cause a
            /// transition to.</param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="guardDescription">Guard description</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration PermitIfAsync(TTrigger trigger, TState destinationState, Func<Task<bool>> guard, string guardDescription = null)
            {
                EnforceNotIdentityTransition(destinationState);

                return InternalPermitIfAsync(
                    trigger,
                    destinationState,
                    new TransitionGuardAsync(guard, guardDescription));
            }
            /// <summary>
            /// Accept the specified trigger and transition to the destination state.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="destinationState">State of the destination.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration PermitIfAsync(TTrigger trigger, TState destinationState, params Tuple<Func<Task<bool>>, string>[] guards)
            {
                EnforceNotIdentityTransition(destinationState);

                return InternalPermitIfAsync(
                    trigger,
                    destinationState,
                    new TransitionGuardAsync(guards));
            }

            /// <summary>
            ///  Accept the specified trigger, transition to the destination state, and guard condition. 
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationState">The state that the trigger will cause a
            /// transition to.</param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted. Takes a single argument of type TArg0</param>
            /// <param name="guardDescription">Guard description</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration PermitIfAsync<TArg0>(TriggerWithParameters<TArg0> trigger, TState destinationState, Func<TArg0, Task<bool>> guard, string guardDescription = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                EnforceNotIdentityTransition(destinationState);

                return InternalPermitIfAsync(
                    trigger.Trigger,
                    destinationState,
                    new TransitionGuardAsync(TransitionGuardAsync.ToPackedGuard(guard), guardDescription));
            }

            /// <summary>
            /// Accept the specified trigger, transition to the destination state, and guard conditions.
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be accepted. Functions take a single argument of type TArg0.</param>
            /// <param name="destinationState">State of the destination.</param>
            /// <returns>The receiver.</returns>
            /// <returns></returns>
            public StateConfiguration PermitIfAsync<TArg0>(TriggerWithParameters<TArg0> trigger, TState destinationState, params Tuple<Func<TArg0, Task<bool>>, string>[] guards)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                EnforceNotIdentityTransition(destinationState);

                return InternalPermitIfAsync(
                    trigger.Trigger,
                    destinationState,
                    new TransitionGuardAsync(TransitionGuardAsync.ToPackedGuards(guards))
                );
            }

            /// <summary>
            ///  Accept the specified trigger, transition to the destination state, and guard condition. 
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <typeparam name="TArg1"></typeparam>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationState">The state that the trigger will cause a
            /// transition to.</param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted. Takes a single argument of type TArg0</param>
            /// <param name="guardDescription">Guard description</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration PermitIfAsync<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, TState destinationState, Func<TArg0, TArg1, Task<bool>> guard, string guardDescription = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                EnforceNotIdentityTransition(destinationState);

                return InternalPermitIfAsync(
                    trigger.Trigger,
                    destinationState,
                    new TransitionGuardAsync(TransitionGuardAsync.ToPackedGuard(guard), guardDescription));
            }

            /// <summary>
            ///  Accept the specified trigger, transition to the destination state, and guard condition. 
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <typeparam name="TArg1"></typeparam>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be accepted. Functions take a single argument of type TArg0.</param>
            /// <param name="destinationState">State of the destination.</param>
            /// <returns>The receiver.</returns>
            /// <returns></returns>
            /// <returns>The receiver.</returns>
            public StateConfiguration PermitIfAsync<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, TState destinationState, params Tuple<Func<TArg0, TArg1, Task<bool>>, string>[] guards)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                EnforceNotIdentityTransition(destinationState);

                return InternalPermitIfAsync(
                    trigger.Trigger,
                    destinationState,
                    new TransitionGuardAsync(TransitionGuardAsync.ToPackedGuards(guards))
                );
            }

            /// <summary>
            ///  Accept the specified trigger, transition to the destination state, and guard condition. 
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <typeparam name="TArg1"></typeparam>
            /// <typeparam name="TArg2"></typeparam>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationState">The state that the trigger will cause a
            /// transition to.</param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted. Takes a single argument of type TArg0</param>
            /// <param name="guardDescription">Guard description</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration PermitIfAsync<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, TState destinationState, Func<TArg0, TArg1, TArg2, Task<bool>> guard, string guardDescription = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                EnforceNotIdentityTransition(destinationState);

                return InternalPermitIfAsync(
                    trigger.Trigger,
                    destinationState,
                    new TransitionGuardAsync(TransitionGuardAsync.ToPackedGuard(guard), guardDescription));
            }

            /// <summary>
            ///  Accept the specified trigger, transition to the destination state, and guard condition. 
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <typeparam name="TArg1"></typeparam>
            /// <typeparam name="TArg2"></typeparam>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be accepted. Functions take a single argument of type TArg0.</param>
            /// <param name="destinationState">State of the destination.</param>
            /// <returns>The receiver.</returns>
            /// <returns></returns>
            /// <returns>The receiver.</returns>
            public StateConfiguration PermitIfAsync<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, TState destinationState, params Tuple<Func<TArg0, TArg1, TArg2, Task<bool>>, string>[] guards)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                EnforceNotIdentityTransition(destinationState);

                return InternalPermitIfAsync(
                    trigger.Trigger,
                    destinationState,
                    new TransitionGuardAsync(TransitionGuardAsync.ToPackedGuards(guards))
                );
            }

            /// <summary>
            /// Accept the specified trigger, execute exit actions and re-execute entry actions.
            /// Reentry behaves as though the configured state transitions to an identical sibling state.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="guardDescription">Guard description</param>
            /// <returns>The receiver.</returns>
            /// <remarks>
            /// Applies to the current state only. Will not re-execute superstate actions, or
            /// cause actions to execute transitioning between super- and sub-states.
            /// </remarks>
            public StateConfiguration PermitReentryIfAsync(TTrigger trigger, Func<Task<bool>> guard, string guardDescription = null)
            {
                return InternalPermitReentryIfAsync(
                    trigger,
                    _representation.UnderlyingState,
                    new TransitionGuardAsync(guard, guardDescription));
            }

            /// <summary>
            /// Accept the specified trigger, execute exit actions and re-execute entry actions.
            /// Reentry behaves as though the configured state transitions to an identical sibling state.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be accepted.</param>
            /// <returns>The receiver.</returns>
            /// <remarks>
            /// Applies to the current state only. Will not re-execute superstate actions, or
            /// cause actions to execute transitioning between super- and sub-states.
            /// </remarks>
            public StateConfiguration PermitReentryIfAsync(TTrigger trigger, params Tuple<Func<Task<bool>>, string>[] guards)
            {
                return InternalPermitReentryIfAsync(
                    trigger,
                    _representation.UnderlyingState,
                    new TransitionGuardAsync(guards));
            }

            /// <summary>
            ///  Accept the specified trigger, transition to the destination state, and guard condition. 
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted. Takes a single argument of type TArg0</param>
            /// <param name="guardDescription">Guard description</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration PermitReentryIfAsync<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, Task<bool>> guard, string guardDescription = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));

                return InternalPermitReentryIfAsync(
                    trigger.Trigger,
                    _representation.UnderlyingState,
                    new TransitionGuardAsync(TransitionGuardAsync.ToPackedGuard(guard), guardDescription)
                );
            }

            /// <summary>
            /// Accept the specified trigger, transition to the destination state, and guard conditions.
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be accepted. Functions take a single argument of type TArg0.</param>
            /// <returns>The receiver.</returns>
            /// <returns></returns>
            public StateConfiguration PermitReentryIfAsync<TArg0>(TriggerWithParameters<TArg0> trigger, params Tuple<Func<TArg0, Task<bool>>, string>[] guards)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));

                return InternalPermitReentryIfAsync(
                    trigger.Trigger,
                    _representation.UnderlyingState,
                    new TransitionGuardAsync(TransitionGuardAsync.ToPackedGuards(guards))
                );
            }

            /// <summary>
            ///  Accept the specified trigger, transition to the destination state, and guard condition. 
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <typeparam name="TArg1"></typeparam>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted. Takes a single argument of type TArg0</param>
            /// <param name="guardDescription">Guard description</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration PermitReentryIfAsync<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, Task<bool>> guard, string guardDescription = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));

                return InternalPermitReentryIfAsync(
                    trigger.Trigger,
                    _representation.UnderlyingState,
                    new TransitionGuardAsync(TransitionGuardAsync.ToPackedGuard(guard), guardDescription)
                );
            }

            /// <summary>
            ///  Accept the specified trigger, transition to the destination state, and guard condition. 
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <typeparam name="TArg1"></typeparam>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be accepted. Functions take a single argument of type TArg0.</param>
            /// <returns>The receiver.</returns>
            /// <returns></returns>
            /// <returns>The receiver.</returns>
            public StateConfiguration PermitReentryIfAsync<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, params Tuple<Func<TArg0, TArg1, Task<bool>>, string>[] guards)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));

                return InternalPermitReentryIfAsync(
                    trigger.Trigger,
                    _representation.UnderlyingState,
                    new TransitionGuardAsync(TransitionGuardAsync.ToPackedGuards(guards))
                );
            }

            /// <summary>
            ///  Accept the specified trigger, transition to the destination state, and guard condition. 
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <typeparam name="TArg1"></typeparam>
            /// <typeparam name="TArg2"></typeparam>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted. Takes a single argument of type TArg0</param>
            /// <param name="guardDescription">Guard description</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration PermitReentryIfAsync<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, Task<bool>> guard, string guardDescription = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));

                return InternalPermitReentryIfAsync(
                    trigger.Trigger,
                    _representation.UnderlyingState,
                    new TransitionGuardAsync(TransitionGuardAsync.ToPackedGuard(guard), guardDescription)
                );
            }

            /// <summary>
            ///  Accept the specified trigger, transition to the destination state, and guard condition. 
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <typeparam name="TArg1"></typeparam>
            /// <typeparam name="TArg2"></typeparam>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be accepted. Functions take a single argument of type TArg0.</param>
            /// <returns>The receiver.</returns>
            /// <returns></returns>
            /// <returns>The receiver.</returns>
            public StateConfiguration PermitReentryIfAsync<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, params Tuple<Func<TArg0, TArg1, TArg2, Task<bool>>, string>[] guards)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));

                return InternalPermitReentryIfAsync(
                    trigger.Trigger,
                    _representation.UnderlyingState,
                    new TransitionGuardAsync(TransitionGuardAsync.ToPackedGuards(guards))
                );
            }

            StateConfiguration InternalPermitIfAsync(TTrigger trigger, TState destinationState, TransitionGuardAsync transitionGuard)
            {
                _representation.AddTriggerBehaviourAsync(new TransitioningTriggerBehaviourAsync(trigger, destinationState, transitionGuard));
                return this;
            }
            StateConfiguration InternalPermitReentryIfAsync(TTrigger trigger, TState destinationState, TransitionGuardAsync transitionGuard)
            {
                _representation.AddTriggerBehaviourAsync(new ReentryTriggerBehaviourAsync(trigger, destinationState, transitionGuard));
                return this;
            }
            StateConfiguration InternalPermitDynamicIfAsync(TTrigger trigger, Func<object[],Task<TState>> destinationStateSelector,
                string destinationStateSelectorDescription, TransitionGuard transitionGuard, Reflection.DynamicStateInfos possibleDestinationStates)
            {
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));
                if (transitionGuard == null) throw new ArgumentNullException(nameof(transitionGuard));

                _representation.AddTriggerBehaviour(new DynamicTriggerBehaviourAsync(trigger,
                    destinationStateSelector,
                    transitionGuard,
                    Reflection.DynamicTransitionInfo.Create(trigger,
                        transitionGuard.Conditions.Select(x => x.MethodDescription),
                        Reflection.InvocationInfo.Create(destinationStateSelector, destinationStateSelectorDescription),
                        possibleDestinationStates)
                    ));
                return this;
            }
        }
    }
}
#endif
