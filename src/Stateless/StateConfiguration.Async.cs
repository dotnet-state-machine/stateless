#if TASKS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
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
            /// <param name="entryAction"></param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionAsync(TTrigger trigger, Func<Transition, Task> entryAction)
            {
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour(trigger));
                _representation.AddInternalAction(trigger, (t, args) => entryAction(t));
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
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour(trigger));
                _representation.AddInternalAction(trigger, (t, args) => internalAction());
                return this;
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
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour(trigger));
                _representation.AddInternalAction(trigger, (t, args) => internalAction(t));
                return this;
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
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour(trigger.Trigger));
                _representation.AddInternalAction(trigger.Trigger, (t, args) => internalAction(ParameterConversion.Unpack<TArg0>(args, 0), t));
                return this;
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
                Enforce.ArgumentNotNull(activateAction, nameof(activateAction));
                _representation.AddActivateAction(
                    activateAction,
                    activateActionDescription ?? activateAction.TryGetMethodName());
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
                Enforce.ArgumentNotNull(deactivateAction, nameof(deactivateAction));
                _representation.AddDeactivateAction(
                    deactivateAction,
                    deactivateActionDescription ?? deactivateAction.TryGetMethodName());
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
                Enforce.ArgumentNotNull(entryAction, nameof(entryAction));
                return OnEntryAsync(
                    t => entryAction(),
                    entryActionDescription ?? entryAction.TryGetMethodName());
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
                Enforce.ArgumentNotNull(entryAction, nameof(entryAction));
                _representation.AddEntryAction(
                    (t, args) => entryAction(t),
                    entryActionDescription ?? entryAction.TryGetMethodName());
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
                Enforce.ArgumentNotNull(entryAction, nameof(entryAction));
                return OnEntryFromAsync(
                    trigger,
                    t => entryAction(),
                    entryActionDescription ?? entryAction.TryGetMethodName());
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
                Enforce.ArgumentNotNull(entryAction, nameof(entryAction));
                _representation.AddEntryAction(
                    trigger,
                    (t, args) => entryAction(t),
                    entryActionDescription ?? entryAction.TryGetMethodName());
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
                Enforce.ArgumentNotNull(entryAction, nameof(entryAction));
                return OnEntryFromAsync<TArg0>(
                    trigger,
                    (a0, t) => entryAction(a0),
                    entryActionDescription ?? entryAction.TryGetMethodName());
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
                Enforce.ArgumentNotNull(entryAction, nameof(entryAction));
                Enforce.ArgumentNotNull(trigger, nameof(trigger));
                _representation.AddEntryAction(
                    trigger.Trigger,
                    (t, args) => entryAction(
                        ParameterConversion.Unpack<TArg0>(args, 0), t),
                        entryActionDescription ?? entryAction.TryGetMethodName());
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
                Enforce.ArgumentNotNull(entryAction, nameof(entryAction));
                return OnEntryFromAsync<TArg0, TArg1>(
                    trigger, 
                    (a0, a1, t) => entryAction(a0, a1), entryActionDescription ?? entryAction.TryGetMethodName());
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
                Enforce.ArgumentNotNull(entryAction, nameof(entryAction));
                Enforce.ArgumentNotNull(trigger, nameof(trigger));
                _representation.AddEntryAction(trigger.Trigger, (t, args) => entryAction(
                    ParameterConversion.Unpack<TArg0>(args, 0),
                    ParameterConversion.Unpack<TArg1>(args, 1), t), entryActionDescription ?? entryAction.TryGetMethodName());
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
                Enforce.ArgumentNotNull(entryAction, nameof(entryAction));
                return OnEntryFromAsync<TArg0, TArg1, TArg2>(
                    trigger, 
                    (a0, a1, a2, t) => entryAction(a0, a1, a2), entryActionDescription ?? entryAction.TryGetMethodName());
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
                Enforce.ArgumentNotNull(entryAction, nameof(entryAction));
                Enforce.ArgumentNotNull(trigger, nameof(trigger));
                _representation.AddEntryAction(trigger.Trigger, (t, args) => entryAction(
                    ParameterConversion.Unpack<TArg0>(args, 0),
                    ParameterConversion.Unpack<TArg1>(args, 1),
                    ParameterConversion.Unpack<TArg2>(args, 2), t), entryActionDescription ?? entryAction.TryGetMethodName());
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
                Enforce.ArgumentNotNull(exitAction, nameof(exitAction));
                return OnExitAsync(
                    t => exitAction(),
                    exitActionDescription ?? exitAction.TryGetMethodName());
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
                Enforce.ArgumentNotNull(exitAction, nameof(exitAction));
                _representation.AddExitAction(
                    exitAction,
                    exitActionDescription ?? exitAction.TryGetMethodName());
                return this;
            }
        }
    }
}
#endif