using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        /// <summary>
        /// Does everything StateConfiguration does, but only comes after a specific trigger has been
        /// defined, so we can add options to the trigger's behaviour
        /// </summary>
        public class TriggerConfiguration : StateConfiguration
        {
            TransitioningTriggerBehaviour Trigger;

            internal TriggerConfiguration(StateConfiguration config, TransitioningTriggerBehaviour trigger)
                : base(config.Machine, config.Representation, config.Lookup)
            {
                Trigger = trigger;
            }

            /// <summary>
            /// Specify a guard condition for this transition
            /// </summary>
            /// <param name="guard"></param>
            /// <param name="guardDescription"></param>
            /// <returns></returns>
            public TriggerConfiguration If(Func<bool> guard, string guardDescription = null)
            {
                Trigger.Guard = new TransitionGuard(guard, guardDescription);

                return this;
            }

            /// <summary>
            /// Specify an action to be executed when this transition is executed
            /// </summary>
            /// <param name="action"></param>
            /// <param name="actionDescription"></param>
            /// <returns></returns>
            public TriggerConfiguration Do(Action action, string actionDescription = null)
            {
                Trigger.SetAction(action, Reflection.InvocationInfo.Create(action, actionDescription));

                return this;
            }

            /// <summary>
            /// Specify a tag to be associated with this transition
            /// </summary>
            /// <param name="tag"></param>
            /// <returns></returns>
            public TriggerConfiguration Tag(object tag)
            {
                Trigger.Tag = tag;

                return this;
            }
        }

        /// <summary>
        /// The configuration for a single state value.
        /// </summary>
        public partial class StateConfiguration
        {
            readonly StateMachine<TState, TTrigger> _machine;
            internal readonly StateRepresentation Representation;
            internal readonly Func<TState, StateRepresentation> Lookup;

            internal StateConfiguration(StateMachine<TState, TTrigger> machine, StateRepresentation representation, Func<TState, StateRepresentation> lookup)
            {
                _machine = Enforce.ArgumentNotNull(machine, nameof(machine));
                Representation = Enforce.ArgumentNotNull(representation, nameof(representation));
                Lookup = Enforce.ArgumentNotNull(lookup, nameof(lookup));
            }

            /// <summary>
            /// The state that is configured with this configuration.
            /// </summary>
            public TState State { get { return Representation.UnderlyingState; } }

            /// <summary>
            /// The machine that is configured with this configuration.
            /// </summary>
            public StateMachine<TState, TTrigger> Machine { get { return _machine; } }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationState">The state that the trigger will cause a
            /// transition to.</param>
            /// <returns>The reciever.</returns>
            public TriggerConfiguration Permit(TTrigger trigger, TState destinationState)
            {
                EnforceNotIdentityTransition(destinationState);

                TransitioningTriggerBehaviour ttb = new TransitioningTriggerBehaviour(trigger, destinationState, null);

                Representation.AddTriggerBehaviour(ttb);

                return new TriggerConfiguration(this, ttb);
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <param name="trigger"></param>
            /// <param name="entryAction"></param>
            /// <returns></returns>
            public StateConfiguration InternalTransition(TTrigger trigger, Action<Transition> entryAction)
            {
                return InternalTransitionIf(trigger, () => true, entryAction);
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <param name="trigger"></param>
            /// <param name="guard">Function that must return true in order for the trigger to be accepted.</param>
            /// <param name="entryAction"></param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionIf(TTrigger trigger, Func<bool> guard, Action<Transition> entryAction)
            {
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                Representation.AddTriggerBehaviour(new InternalTriggerBehaviour(trigger, guard));
                Representation.AddInternalAction(trigger, (t, args) => entryAction(t));
                return this;
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="internalAction">The action performed by the internal transition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransition(TTrigger trigger, Action internalAction)
            {
                return InternalTransitionIf(trigger, () => true, internalAction);
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="guard">Function that must return true in order for the trigger to be accepted.</param>
            /// <param name="internalAction">The action performed by the internal transition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionIf(TTrigger trigger, Func<bool> guard, Action internalAction)
            {
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                Representation.AddTriggerBehaviour(new InternalTriggerBehaviour(trigger, guard));
                Representation.AddInternalAction(trigger, (t, args) => internalAction());
                return this;
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="guard">Function that must return true in order for the trigger to be accepted.</param>
            /// <param name="internalAction">The action performed by the internal transition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionIf<TArg0>(TTrigger trigger, Func<bool> guard, Action<Transition> internalAction)
            {
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                Representation.AddTriggerBehaviour(new InternalTriggerBehaviour(trigger, guard));
                Representation.AddInternalAction(trigger, (t, args) => internalAction(t));
                return this;
            }
            
            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="internalAction">The action performed by the internal transition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransition<TArg0>(TTrigger trigger, Action<Transition> internalAction)
            {
                return InternalTransitionIf(trigger, () => true, internalAction);
            }
            
            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="internalAction">The action performed by the internal transition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransition<TArg0>(TriggerWithParameters<TArg0> trigger, Action<TArg0, Transition> internalAction)
            {
                return InternalTransitionIf(trigger, () => true, internalAction);
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="guard">Function that must return true in order for the trigger to be accepted.</param>
            /// <param name="internalAction">The action performed by the internal transition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionIf<TArg0>(TriggerWithParameters<TArg0> trigger, Func<bool> guard, Action<TArg0, Transition> internalAction)
            {
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                Representation.AddTriggerBehaviour(new InternalTriggerBehaviour(trigger.Trigger, guard));
                Representation.AddInternalAction(trigger.Trigger, (t, args) => internalAction(ParameterConversion.Unpack<TArg0>(args, 0), t));
                return this;
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <typeparam name="TArg1"></typeparam>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="internalAction">The action performed by the internal transition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransition<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger,
                Action<TArg0, TArg1, Transition> internalAction)
            {
                return InternalTransitionIf(trigger, () => true, internalAction);
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <typeparam name="TArg1"></typeparam>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="guard">Function that must return true in order for the trigger to be accepted.</param>
            /// <param name="internalAction">The action performed by the internal transition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionIf<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<bool> guard, Action<TArg0, TArg1, Transition> internalAction)
            {
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                Representation.AddTriggerBehaviour(new InternalTriggerBehaviour(trigger.Trigger, guard));
                Representation.AddInternalAction(trigger.Trigger, (t, args) => internalAction(
                    ParameterConversion.Unpack<TArg0>(args, 0),
                    ParameterConversion.Unpack<TArg1>(args, 1), t));
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
            /// <param name="internalAction">The action performed by the internal transition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionIf<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<bool> guard, Action<TArg0, TArg1, TArg2, Transition> internalAction)
            {
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                Representation.AddTriggerBehaviour(new InternalTriggerBehaviour(trigger.Trigger, guard));
                Representation.AddInternalAction(trigger.Trigger, (t, args) => internalAction(
                    ParameterConversion.Unpack<TArg0>(args, 0),
                    ParameterConversion.Unpack<TArg1>(args, 1),
                    ParameterConversion.Unpack<TArg2>(args, 2), t));
                return this;
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <typeparam name="TArg1"></typeparam>
            /// <typeparam name="TArg2"></typeparam>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="internalAction">The action performed by the internal transition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransition<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Action<TArg0, TArg1, TArg2, Transition> internalAction)
            {
                return InternalTransitionIf(trigger, () => true, internalAction);
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
            /// <returns>The reciever.</returns>
            public StateConfiguration PermitIf(TTrigger trigger, TState destinationState,
                Func<bool> guard, string guardDescription = null)
            {
                EnforceNotIdentityTransition(destinationState);

                return InternalPermitIf(
                    trigger,
                    destinationState,
                    new TransitionGuard(guard, guardDescription));
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be accepted.</param>   
            /// <param name="destinationState">State of the destination.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration PermitIf(TTrigger trigger, TState destinationState, params Tuple<Func<bool>, string>[] guards)
            {
                EnforceNotIdentityTransition(destinationState);

                return InternalPermitIf(
                    trigger,
                    destinationState,
                    new TransitionGuard(guards));
            }

            /// <summary>
            /// Accept the specified trigger, execute exit actions and re-execute entry actions.
            /// Reentry behaves as though the configured state transitions to an identical sibling state.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <returns>The reciever.</returns>
            /// <remarks>
            /// Applies to the current state only. Will not re-execute superstate actions, or
            /// cause actions to execute transitioning between super- and sub-states.
            /// </remarks>
            public StateConfiguration PermitReentry(TTrigger trigger)
            {
                return InternalPermit(trigger, Representation.UnderlyingState);
            }

            /// <summary>
            /// Accept the specified trigger, execute exit actions and re-execute entry actions.
            /// Reentry behaves as though the configured state transitions to an identical sibling state.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="guardDescription">Guard description</param>
            /// <returns>The reciever.</returns>
            /// <remarks>
            /// Applies to the current state only. Will not re-execute superstate actions, or
            /// cause actions to execute transitioning between super- and sub-states.
            /// </remarks>
            public StateConfiguration PermitReentryIf(TTrigger trigger, Func<bool> guard, string guardDescription = null)
            {
                return InternalPermitIf(
                    trigger,
                    Representation.UnderlyingState,
                    new TransitionGuard(guard, guardDescription));
            }

            /// <summary>
            /// Accept the specified trigger, execute exit actions and re-execute entry actions.
            /// Reentry behaves as though the configured state transitions to an identical sibling state.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be accepted.</param>           
            /// <returns>The reciever.</returns>
            /// <remarks>
            /// Applies to the current state only. Will not re-execute superstate actions, or
            /// cause actions to execute transitioning between super- and sub-states.
            /// </remarks>
            public StateConfiguration PermitReentryIf(TTrigger trigger, params Tuple<Func<bool>, string>[] guards)
            {
                return InternalPermitIf(
                    trigger,
                    Representation.UnderlyingState,
                    new TransitionGuard(guards));
            }

            /// <summary>
            /// Ignore the specified trigger when in the configured state.
            /// </summary>
            /// <param name="trigger">The trigger to ignore.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration Ignore(TTrigger trigger)
            {
                // return IgnoreIf(trigger, NoGuard);
                // Enforce.ArgumentNotNull(guard, nameof(guard));
                Representation.AddTriggerBehaviour(
                    new IgnoredTriggerBehaviour(
                        trigger,
                        null));
                return this;
            }

            /// <summary>
            /// Ignore the specified trigger when in the configured state, if the guard
            /// returns true..
            /// </summary>
            /// <param name="trigger">The trigger to ignore.</param>
            /// <param name="guardDescription">Guard description</param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be ignored.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration IgnoreIf(TTrigger trigger, Func<bool> guard, string guardDescription = null)
            {
                Enforce.ArgumentNotNull(guard, nameof(guard));
                Representation.AddTriggerBehaviour(
                    new IgnoredTriggerBehaviour(
                        trigger,
                        new TransitionGuard(guard, guardDescription)
                        ));
                return this;
            }

            /// <summary>
            /// Ignore the specified trigger when in the configured state, if the guard
            /// returns true..
            /// </summary>
            /// <param name="trigger">The trigger to ignore.</param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be ignored.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration IgnoreIf(TTrigger trigger, params Tuple<Func<bool>, string>[] guards)
            {
                Enforce.ArgumentNotNull(guards, nameof(guards));
                Representation.AddTriggerBehaviour(
                    new IgnoredTriggerBehaviour(
                        trigger,
                        new TransitionGuard(guards)));
                return this;
            }

            /// <summary>
            /// Specify an action that will execute when activating
            /// the configured state.
            /// </summary>
            /// <param name="activateAction">Action to execute.</param>
            /// <param name="activateActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnActivate(Action activateAction, string activateActionDescription = null)
            {
                Enforce.ArgumentNotNull(activateAction, nameof(activateAction));
                Representation.AddActivateAction(
                    activateAction,
                    Reflection.InvocationInfo.Create(activateAction, activateActionDescription));
                return this;
            }

            /// <summary>
            /// Specify an action that will execute when deactivating
            /// the configured state.
            /// </summary>
            /// <param name="deactivateAction">Action to execute.</param>
            /// <param name="deactivateActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnDeactivate(Action deactivateAction, string deactivateActionDescription = null)
            {
                Enforce.ArgumentNotNull(deactivateAction, nameof(deactivateAction));
                Representation.AddDeactivateAction(
                    deactivateAction,
                    Reflection.InvocationInfo.Create(deactivateAction, deactivateActionDescription));
                return this;
            }

            /// <summary>
            /// Specify an action that will execute when transitioning into
            /// the configured state.
            /// </summary>
            /// <param name="entryAction">Action to execute.</param>
            /// <param name="entryActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnEntry(Action entryAction, string entryActionDescription = null)
            {
                Enforce.ArgumentNotNull(entryAction, nameof(entryAction));
                Representation.AddEntryAction(
                    (t, args) => entryAction(),
                    Reflection.InvocationInfo.Create(entryAction, entryActionDescription));
                return this;

            }

            /// <summary>
            /// Specify an action that will execute when transitioning into
            /// the configured state.
            /// </summary>
            /// <param name="entryAction">Action to execute, providing details of the transition.</param>
            /// <param name="entryActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnEntry(Action<Transition> entryAction, string entryActionDescription = null)
            {
                Enforce.ArgumentNotNull(entryAction, nameof(entryAction));
                Representation.AddEntryAction(
                    (t, args) => entryAction(t),
                    Reflection.InvocationInfo.Create(entryAction, entryActionDescription));
                return this;
            }

            /// <summary>
            /// Specify an action that will execute when transitioning into
            /// the configured state.
            /// </summary>
            /// <param name="entryAction">Action to execute.</param>
            /// <param name="trigger">The trigger by which the state must be entered in order for the action to execute.</param>
            /// <param name="entryActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnEntryFrom(TTrigger trigger, Action entryAction, string entryActionDescription = null)
            {
                Enforce.ArgumentNotNull(entryAction, nameof(entryAction));
                Representation.AddEntryAction(
                    trigger,
                    (t, args) => entryAction(),
                    Reflection.InvocationInfo.Create(entryAction, entryActionDescription));
                return this;

            }

            /// <summary>
            /// Specify an action that will execute when transitioning into
            /// the configured state.
            /// </summary>
            /// <param name="entryAction">Action to execute, providing details of the transition.</param>
            /// <param name="trigger">The trigger by which the state must be entered in order for the action to execute.</param>
            /// <param name="entryActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnEntryFrom(TTrigger trigger, Action<Transition> entryAction, string entryActionDescription = null)
            {
                Enforce.ArgumentNotNull(entryAction, nameof(entryAction));
                Representation.AddEntryAction(
                    trigger,
                    (t, args) => entryAction(t),
                    Reflection.InvocationInfo.Create(entryAction, entryActionDescription));
                return this;
            }

            /// <summary>
            /// Specify an action that will execute when transitioning into
            /// the configured state.
            /// </summary>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <param name="entryAction">Action to execute, providing details of the transition.</param>
            /// <param name="trigger">The trigger by which the state must be entered in order for the action to execute.</param>
            /// <param name="entryActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnEntryFrom<TArg0>(TriggerWithParameters<TArg0> trigger, Action<TArg0> entryAction, string entryActionDescription = null)
            {
                Enforce.ArgumentNotNull(entryAction, nameof(entryAction));
                Enforce.ArgumentNotNull(trigger, nameof(trigger));
                Representation.AddEntryAction(
                    trigger.Trigger,
                    (t, args) => entryAction(
                        ParameterConversion.Unpack<TArg0>(args, 0)),
                    Reflection.InvocationInfo.Create(entryAction, entryActionDescription));
                return this;

            }

            /// <summary>
            /// Specify an action that will execute when transitioning into
            /// the configured state.
            /// </summary>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <param name="entryAction">Action to execute, providing details of the transition.</param>
            /// <param name="trigger">The trigger by which the state must be entered in order for the action to execute.</param>
            /// <param name="entryActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnEntryFrom<TArg0>(TriggerWithParameters<TArg0> trigger, Action<TArg0, Transition> entryAction, string entryActionDescription = null)
            {
                Enforce.ArgumentNotNull(entryAction, nameof(entryAction));
                Enforce.ArgumentNotNull(trigger, nameof(trigger));
                Representation.AddEntryAction(
                    trigger.Trigger,
                    (t, args) => entryAction(
                        ParameterConversion.Unpack<TArg0>(args, 0), t),
                    Reflection.InvocationInfo.Create(entryAction, entryActionDescription));
                return this;
            }

            /// <summary>
            /// Specify an action that will execute when transitioning into
            /// the configured state.
            /// </summary>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            /// <param name="entryAction">Action to execute, providing details of the transition.</param>
            /// <param name="trigger">The trigger by which the state must be entered in order for the action to execute.</param>
            /// <param name="entryActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnEntryFrom<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Action<TArg0, TArg1> entryAction, string entryActionDescription = null)
            {
                Enforce.ArgumentNotNull(entryAction, nameof(entryAction));
                Enforce.ArgumentNotNull(trigger, nameof(trigger));

                Representation.AddEntryAction(trigger.Trigger,
                    (t, args) => entryAction(
                        ParameterConversion.Unpack<TArg0>(args, 0),
                        ParameterConversion.Unpack<TArg1>(args, 1)),
                    Reflection.InvocationInfo.Create(entryAction, entryActionDescription));
                return this;

            }

            /// <summary>
            /// Specify an action that will execute when transitioning into
            /// the configured state.
            /// </summary>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            /// <param name="entryAction">Action to execute, providing details of the transition.</param>
            /// <param name="trigger">The trigger by which the state must be entered in order for the action to execute.</param>
            /// <param name="entryActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnEntryFrom<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Action<TArg0, TArg1, Transition> entryAction, string entryActionDescription = null)
            {
                Enforce.ArgumentNotNull(entryAction, nameof(entryAction));
                Enforce.ArgumentNotNull(trigger, nameof(trigger));
                Representation.AddEntryAction(trigger.Trigger, (t, args) => entryAction(
                    ParameterConversion.Unpack<TArg0>(args, 0),
                    ParameterConversion.Unpack<TArg1>(args, 1), t),
                    Reflection.InvocationInfo.Create(entryAction, entryActionDescription));
                return this;
            }

            /// <summary>
            /// Specify an action that will execute when transitioning into
            /// the configured state.
            /// </summary>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
            /// <param name="entryAction">Action to execute, providing details of the transition.</param>
            /// <param name="trigger">The trigger by which the state must be entered in order for the action to execute.</param>
            /// <param name="entryActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnEntryFrom<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Action<TArg0, TArg1, TArg2> entryAction, string entryActionDescription = null)
            {
                Enforce.ArgumentNotNull(entryAction, nameof(entryAction));
                Enforce.ArgumentNotNull(trigger, nameof(trigger));
                Representation.AddEntryAction(trigger.Trigger, (t, args) => entryAction(
                    ParameterConversion.Unpack<TArg0>(args, 0),
                    ParameterConversion.Unpack<TArg1>(args, 1),
                    ParameterConversion.Unpack<TArg2>(args, 2)),
                    Reflection.InvocationInfo.Create(entryAction, entryActionDescription));
                return this;

            }

            /// <summary>
            /// Specify an action that will execute when transitioning into
            /// the configured state.
            /// </summary>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
            /// <param name="entryAction">Action to execute, providing details of the transition.</param>
            /// <param name="trigger">The trigger by which the state must be entered in order for the action to execute.</param>
            /// <param name="entryActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnEntryFrom<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Action<TArg0, TArg1, TArg2, Transition> entryAction, string entryActionDescription = null)
            {
                Enforce.ArgumentNotNull(entryAction, nameof(entryAction));
                Enforce.ArgumentNotNull(trigger, nameof(trigger));
                Representation.AddEntryAction(trigger.Trigger, (t, args) => entryAction(
                    ParameterConversion.Unpack<TArg0>(args, 0),
                    ParameterConversion.Unpack<TArg1>(args, 1),
                    ParameterConversion.Unpack<TArg2>(args, 2), t),
                    Reflection.InvocationInfo.Create(entryAction, entryActionDescription));
                return this;
            }

            /// <summary>
            /// Specify an action that will execute when transitioning from
            /// the configured state.
            /// </summary>
            /// <param name="exitAction">Action to execute.</param>
            /// <param name="exitActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnExit(Action exitAction, string exitActionDescription = null)
            {
                Enforce.ArgumentNotNull(exitAction, nameof(exitAction));
                Representation.AddExitAction(
                    t => exitAction(),
                    Reflection.InvocationInfo.Create(exitAction, exitActionDescription));
                return this;
            }

            /// <summary>
            /// Specify an action that will execute when transitioning from
            /// the configured state.
            /// </summary>
            /// <param name="exitAction">Action to execute, providing details of the transition.</param>
            /// <param name="exitActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnExit(Action<Transition> exitAction, string exitActionDescription = null)
            {
                Enforce.ArgumentNotNull(exitAction, nameof(exitAction));
                Representation.AddExitAction(
                    exitAction,
                    Reflection.InvocationInfo.Create(exitAction, exitActionDescription));
                return this;
            }

            /// <summary>
            /// Specify an action that will execute when transitioning from
            /// the configured state.
            /// </summary>
            /// <param name="trigger">The trigger by which the state must be exited in order for the action to execute.</param>
            /// <param name="exitAction">Action to execute.</param>
            /// <param name="exitActionDescription">Action description.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration OnExitBy(TTrigger trigger, Action exitAction, string exitActionDescription = null)
            {
                Enforce.ArgumentNotNull(exitAction, nameof(exitAction));

                // Downside of doing it this way: the reflection API won't be able to tell
                // that this action is only performed for the specific trigger

                Representation.AddExitAction(
                    (t) => { if (t.Trigger.Equals(trigger)) exitAction(); },
                    Reflection.InvocationInfo.Create(exitAction, exitActionDescription));
                return this;

            }


            /// <summary>
            /// Sets the superstate that the configured state is a substate of.
            /// </summary>
            /// <remarks>
            /// Substates inherit the allowed transitions of their superstate.
            /// When entering directly into a substate from outside of the superstate,
            /// entry actions for the superstate are executed.
            /// Likewise when leaving from the substate to outside the supserstate,
            /// exit actions for the superstate will execute.
            /// </remarks>
            /// <param name="superstate">The superstate.</param>
            /// <returns>The receiver.</returns>
            public StateConfiguration SubstateOf(TState superstate)
            {
                var State = Representation.UnderlyingState;

                // Check for accidental identical cyclic configuration 
                if (State.Equals(superstate))
                {
                    throw new ArgumentException($"Configuring {State} as a substate of {superstate} creates an illegal cyclic configuration.");
                }

                // Check for accidental identical nested cyclic configuration 
                var superstates = new HashSet<TState> { State };

                // Build list of super states and check for 
                var activeRepresentation = Lookup(superstate);
                while (activeRepresentation.Superstate != null)
                {
                    // Check if superstate is already added to hashset
                    if (superstates.Contains(activeRepresentation.Superstate.UnderlyingState))
                        throw new ArgumentException($"Configuring {State} as a substate of {superstate} creates an illegal nested cyclic configuration.");

                    superstates.Add(activeRepresentation.Superstate.UnderlyingState);
                    activeRepresentation = Lookup(activeRepresentation.Superstate.UnderlyingState);
                }

                // The check was OK, we can add this 
                var superRepresentation = Lookup(superstate);
                Representation.Superstate = superRepresentation;
                superRepresentation.AddSubstate(Representation);
                return this;
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state 
            /// that the trigger will cause a transition to.</param>
            /// <returns>The reciever.</returns>
            public StateConfiguration PermitDynamic(TTrigger trigger, Func<TState> destinationStateSelector)
            {
                Enforce.ArgumentNotNull(destinationStateSelector, nameof(destinationStateSelector));
                Representation.AddTriggerBehaviour(
                    new DynamicTriggerBehaviour(trigger, args => destinationStateSelector(), null));
                return this;
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state 
            /// that the trigger will cause a transition to.</param>
            /// <returns>The reciever.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            public StateConfiguration PermitDynamic<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, TState> destinationStateSelector)
            {
                Enforce.ArgumentNotNull(trigger, nameof(trigger));
                Enforce.ArgumentNotNull(destinationStateSelector, nameof(destinationStateSelector));
                Representation.AddTriggerBehaviour(
                    new DynamicTriggerBehaviour(trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0)),
                    null));
                return this;

            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state 
            /// that the trigger will cause a transition to.</param>
            /// <returns>The reciever.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            public StateConfiguration PermitDynamic<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, TState> destinationStateSelector)
            {
                Enforce.ArgumentNotNull(trigger, nameof(trigger));
                Enforce.ArgumentNotNull(destinationStateSelector, nameof(destinationStateSelector));
                Representation.AddTriggerBehaviour(
                    new DynamicTriggerBehaviour(trigger.Trigger, args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0),
                        ParameterConversion.Unpack<TArg1>(args, 1)),
                        null));
                return this;

            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state 
            /// that the trigger will cause a transition to.</param>
            /// <returns>The reciever.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
            public StateConfiguration PermitDynamic<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, TState> destinationStateSelector)
            {
                Enforce.ArgumentNotNull(trigger, nameof(trigger));
                Enforce.ArgumentNotNull(destinationStateSelector, nameof(destinationStateSelector));
                Representation.AddTriggerBehaviour(
                    new DynamicTriggerBehaviour(trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0),
                        ParameterConversion.Unpack<TArg1>(args, 1),
                        ParameterConversion.Unpack<TArg2>(args, 2)),
                    null));
                return this;
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state 
            /// that the trigger will cause a transition to.</param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="guardDescription">Guard description</param>
            /// <returns>The reciever.</returns>
            public StateConfiguration PermitDynamicIf(TTrigger trigger, Func<TState> destinationStateSelector, Func<bool> guard, string guardDescription = null)
            {
                Enforce.ArgumentNotNull(destinationStateSelector, nameof(destinationStateSelector));

                return InternalPermitDynamicIf(
                    trigger,
                    args => destinationStateSelector(),
                    new TransitionGuard(guard, guardDescription));
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state 
            /// that the trigger will cause a transition to.</param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be accepted.</param>
            /// <returns>The reciever.</returns>
            public StateConfiguration PermitDynamicIf(TTrigger trigger, Func<TState> destinationStateSelector, params Tuple<Func<bool>, string>[] guards)
            {
                Enforce.ArgumentNotNull(destinationStateSelector, nameof(destinationStateSelector));

                return InternalPermitDynamicIf(
                    trigger,
                    args => destinationStateSelector(),
                    new TransitionGuard(guards));
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state 
            /// that the trigger will cause a transition to.</param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="guardDescription">Guard description</param>
            /// <returns>The reciever.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIf<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, TState> destinationStateSelector, Func<bool> guard, string guardDescription = null)
            {
                Enforce.ArgumentNotNull(trigger, nameof(trigger));
                Enforce.ArgumentNotNull(destinationStateSelector, nameof(destinationStateSelector));

                return InternalPermitDynamicIf(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0)),
                    new TransitionGuard(guard, guardDescription));
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state 
            /// that the trigger will cause a transition to.</param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be accepted.</param>
            /// <returns>The reciever.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIf<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, TState> destinationStateSelector, params Tuple<Func<bool>, string>[] guards)
            {
                Enforce.ArgumentNotNull(trigger, nameof(trigger));
                Enforce.ArgumentNotNull(destinationStateSelector, nameof(destinationStateSelector));

                return InternalPermitDynamicIf(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0)),
                    new TransitionGuard(guards));
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state 
            /// that the trigger will cause a transition to.</param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="guardDescription">Guard description</param>
            /// <returns>The reciever.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIf<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, TState> destinationStateSelector, Func<bool> guard, string guardDescription = null)
            {
                Enforce.ArgumentNotNull(trigger, nameof(trigger));
                Enforce.ArgumentNotNull(destinationStateSelector, nameof(destinationStateSelector));

                return InternalPermitDynamicIf(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0),
                        ParameterConversion.Unpack<TArg1>(args, 1)),
                    new TransitionGuard(guard, guardDescription));
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state 
            /// that the trigger will cause a transition to.</param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be accepted.</param>
            /// <returns>The reciever.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIf<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, TState> destinationStateSelector, params Tuple<Func<bool>, string>[] guards)
            {
                Enforce.ArgumentNotNull(trigger, nameof(trigger));
                Enforce.ArgumentNotNull(destinationStateSelector, nameof(destinationStateSelector));

                return InternalPermitDynamicIf(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0),
                        ParameterConversion.Unpack<TArg1>(args, 1)),
                    new TransitionGuard(guards));
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state 
            /// that the trigger will cause a transition to.</param>
            /// <returns>The reciever.</returns>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="guardDescription">Guard description</param>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIf<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, TState> destinationStateSelector, Func<bool> guard, string guardDescription = null)
            {
                Enforce.ArgumentNotNull(trigger, nameof(trigger));
                Enforce.ArgumentNotNull(destinationStateSelector, nameof(destinationStateSelector));

                return InternalPermitDynamicIf(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0),
                        ParameterConversion.Unpack<TArg1>(args, 1),
                        ParameterConversion.Unpack<TArg2>(args, 2)),
                    new TransitionGuard(guard, guardDescription));
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state 
            /// that the trigger will cause a transition to.</param>
            /// <returns>The reciever.</returns>
            /// <param name="guards">Functions ant their descriptions that must return true in order for the
            /// trigger to be accepted.</param>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIf<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, TState> destinationStateSelector, params Tuple<Func<bool>, string>[] guards)
            {
                Enforce.ArgumentNotNull(trigger, nameof(trigger));
                Enforce.ArgumentNotNull(destinationStateSelector, nameof(destinationStateSelector));

                return InternalPermitDynamicIf(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0),
                        ParameterConversion.Unpack<TArg1>(args, 1),
                        ParameterConversion.Unpack<TArg2>(args, 2)),
                    new TransitionGuard(guards));
            }

            void EnforceNotIdentityTransition(TState destination)
            {
                if (destination.Equals(Representation.UnderlyingState))
                {
                    throw new ArgumentException(StateConfigurationResources.SelfTransitionsEitherIgnoredOrReentrant);
                }
            }

            StateConfiguration InternalPermit(TTrigger trigger, TState destinationState)
            {
                Representation.AddTriggerBehaviour(new TransitioningTriggerBehaviour(trigger, destinationState, null));
                return this;
            }

            StateConfiguration InternalPermitIf(TTrigger trigger, TState destinationState, TransitionGuard transitionGuard)
            {
                Enforce.ArgumentNotNull(transitionGuard, nameof(transitionGuard));
                Representation.AddTriggerBehaviour(new TransitioningTriggerBehaviour(trigger, destinationState, transitionGuard));
                return this;
            }

            StateConfiguration InternalPermitDynamic(TTrigger trigger, Func<object[], TState> destinationStateSelector)
            {
                Enforce.ArgumentNotNull(destinationStateSelector, nameof(destinationStateSelector));
                Representation.AddTriggerBehaviour(
                    new DynamicTriggerBehaviour(trigger, destinationStateSelector, null));
                return this;

            }

            StateConfiguration InternalPermitDynamicIf(TTrigger trigger, Func<object[], TState> destinationStateSelector, TransitionGuard transitionGuard)
            {
                Enforce.ArgumentNotNull(destinationStateSelector, nameof(destinationStateSelector));
                Enforce.ArgumentNotNull(transitionGuard, nameof(transitionGuard));
                Representation.AddTriggerBehaviour(new DynamicTriggerBehaviour(trigger, destinationStateSelector, transitionGuard));
                return this;
            }
        }
    }
}
