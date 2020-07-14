using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        /// <summary>
        /// The configuration for a single state value.
        /// </summary>
        public partial class StateConfiguration
        {
            private readonly StateMachine<TState, TTrigger> _machine;
            readonly StateRepresentation _representation;
            readonly Func<TState, StateRepresentation> _lookup;

            internal StateConfiguration(StateMachine<TState, TTrigger> machine, StateRepresentation representation, Func<TState, StateRepresentation> lookup)
            {
                _machine = machine;
                _representation = representation;
                _lookup = lookup;
            }

            /// <summary>
            /// The state that is configured with this configuration.
            /// </summary>
            public TState State { get { return _representation.UnderlyingState; } }

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
            public StateConfiguration Permit(TTrigger trigger, TState destinationState)
            {
                EnforceNotIdentityTransition(destinationState);
                return InternalPermit(trigger, destinationState);
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <param name="trigger"></param>
            /// <param name="entryAction"></param>
            /// <returns></returns>
            public StateConfiguration InternalTransition(TTrigger trigger, Action<Transition> entryAction)
            {
                return InternalTransitionIf(trigger, t => true, entryAction);
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <param name="trigger"></param>
            /// <param name="guard">Function that must return true in order for the trigger to be accepted.</param>
            /// <param name="entryAction"></param>
            /// <param name="guardDescription">A description of the guard condition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionIf(TTrigger trigger, Func<object[], bool> guard, Action<Transition> entryAction, string guardDescription = null)
            {
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour.Sync(trigger, guard, (t, args) => entryAction(t), guardDescription));
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
                return InternalTransitionIf(trigger, t => true, internalAction);
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="guard">Function that must return true in order for the trigger to be accepted.</param>
            /// <param name="internalAction">The action performed by the internal transition</param>
            /// <param name="guardDescription">A description of the guard condition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionIf(TTrigger trigger, Func<object[], bool> guard, Action internalAction, string guardDescription = null)
            {
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour.Sync(trigger, guard, (t, args) => internalAction(), guardDescription));
                return this;
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="guard">Function that must return true in order for the trigger to be accepted.</param>
            /// <param name="internalAction">The action performed by the internal transition</param>
            /// <param name="guardDescription">A description of the guard condition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionIf<TArg0>(TTrigger trigger, Func<object[], bool> guard, Action<Transition> internalAction, string guardDescription = null)
            {
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour.Sync(trigger, guard, (t, args) => internalAction(t), guardDescription));
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
                return InternalTransitionIf(trigger, t => true, internalAction);
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
                return InternalTransitionIf(trigger, t => true, internalAction);
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="guard">Function that must return true in order for the trigger to be accepted.</param>
            /// <param name="internalAction">The action performed by the internal transition</param>
            /// <param name="guardDescription">A description of the guard condition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionIf<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, bool> guard, Action<TArg0, Transition> internalAction, string guardDescription = null)
            {
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour.Sync(trigger.Trigger, TransitionGuard.ToPackedGuard(guard), (t, args) => internalAction(ParameterConversion.Unpack<TArg0>(args, 0), t), guardDescription));
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
                return InternalTransitionIf(trigger, t => true, internalAction);
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <typeparam name="TArg1"></typeparam>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="guard">Function that must return true in order for the trigger to be accepted.</param>
            /// <param name="internalAction">The action performed by the internal transition</param>
            /// <param name="guardDescription">A description of the guard condition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionIf<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<object[], bool> guard, Action<TArg0, TArg1, Transition> internalAction, string guardDescription = null)
            {
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour.Sync(trigger.Trigger, guard, (t, args) => internalAction(
                     ParameterConversion.Unpack<TArg0>(args, 0),
                     ParameterConversion.Unpack<TArg1>(args, 1), t),
                     guardDescription));
                return this;
            }

            /// <summary>
            /// Add an internal transition to the state machine. An internal action does not cause the Exit and Entry actions to be triggered, and does not change the state of the state machine
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <typeparam name="TArg1"></typeparam>
            /// <param name="trigger">The accepted trigger</param>
            /// <param name="guard">Function that must return true in order for the trigger to be accepted.</param>
            /// <param name="internalAction">The action performed by the internal transition</param>
            /// <param name="guardDescription">A description of the guard condition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionIf<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, bool> guard, Action<TArg0, TArg1, Transition> internalAction, string guardDescription = null)
            {
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour.Sync(
                    trigger.Trigger,
                    TransitionGuard.ToPackedGuard(guard),
                    (t, args) => internalAction(
                    ParameterConversion.Unpack<TArg0>(args, 0),
                    ParameterConversion.Unpack<TArg1>(args, 1), t),
                    guardDescription
                    ));
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
            /// <param name="guardDescription">A description of the guard condition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionIf<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<object[], bool> guard, Action<TArg0, TArg1, TArg2, Transition> internalAction, string guardDescription = null)
            {
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour.Sync(trigger.Trigger, guard, (t, args) => internalAction(
                    ParameterConversion.Unpack<TArg0>(args, 0),
                    ParameterConversion.Unpack<TArg1>(args, 1),
                    ParameterConversion.Unpack<TArg2>(args, 2), t),
                    guardDescription));
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
            /// <param name="guardDescription">A description of the guard condition</param>
            /// <returns></returns>
            public StateConfiguration InternalTransitionIf<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, bool> guard, Action<TArg0, TArg1, TArg2, Transition> internalAction, string guardDescription = null)
            {
                if (internalAction == null) throw new ArgumentNullException(nameof(internalAction));

                _representation.AddTriggerBehaviour(new InternalTriggerBehaviour.Sync(trigger.Trigger, TransitionGuard.ToPackedGuard(guard), (t, args) => internalAction(
                    ParameterConversion.Unpack<TArg0>(args, 0),
                    ParameterConversion.Unpack<TArg1>(args, 1),
                    ParameterConversion.Unpack<TArg2>(args, 2), t),
                    guardDescription));
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
                return InternalTransitionIf(trigger, t => true, internalAction);
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
            public StateConfiguration PermitIf(TTrigger trigger, TState destinationState, Func<bool> guard, string guardDescription = null)
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
            ///  Accept the specified trigger, transition to the destination state, and guard condition. 
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationState">The state that the trigger will cause a
            /// transition to.</param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted. Takes a single argument of type TArg0</param>
            /// <param name="guardDescription">Guard description</param>
            /// <returns>The reciever.</returns>
            public StateConfiguration PermitIf<TArg0>(TriggerWithParameters<TArg0> trigger, TState destinationState, Func<TArg0, bool> guard, string guardDescription = null)
            {
                EnforceNotIdentityTransition(destinationState);

                return InternalPermitIf(
                    trigger.Trigger,
                    destinationState,
                    new TransitionGuard(TransitionGuard.ToPackedGuard(guard), guardDescription));
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
            public StateConfiguration PermitIf<TArg0>(TriggerWithParameters<TArg0> trigger, TState destinationState, params Tuple<Func<TArg0, bool>, string>[] guards)
            {
                EnforceNotIdentityTransition(destinationState);

                return InternalPermitIf(
                    trigger.Trigger,
                    destinationState,
                    new TransitionGuard(TransitionGuard.ToPackedGuards(guards))
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
            /// <returns>The reciever.</returns>
            public StateConfiguration PermitIf<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, TState destinationState, Func<TArg0, TArg1, bool> guard, string guardDescription = null)
            {
                EnforceNotIdentityTransition(destinationState);

                return InternalPermitIf(
                    trigger.Trigger,
                    destinationState,
                    new TransitionGuard(TransitionGuard.ToPackedGuard(guard), guardDescription));
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
            /// <returns>The reciever.</returns>
            public StateConfiguration PermitIf<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, TState destinationState, params Tuple<Func<TArg0, TArg1, bool>, string>[] guards)
            {
                EnforceNotIdentityTransition(destinationState);

                return InternalPermitIf(
                    trigger.Trigger,
                    destinationState,
                    new TransitionGuard(TransitionGuard.ToPackedGuards(guards))
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
            /// <returns>The reciever.</returns>
            public StateConfiguration PermitIf<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, TState destinationState, Func<TArg0, TArg1, TArg2, bool> guard, string guardDescription = null)
            {
                EnforceNotIdentityTransition(destinationState);

                return InternalPermitIf(
                    trigger.Trigger,
                    destinationState,
                    new TransitionGuard(TransitionGuard.ToPackedGuard(guard), guardDescription));
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
            /// <returns>The reciever.</returns>
            public StateConfiguration PermitIf<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, TState destinationState, params Tuple<Func<TArg0, TArg1, TArg2, bool>, string>[] guards)
            {
                EnforceNotIdentityTransition(destinationState);

                return InternalPermitIf(
                    trigger.Trigger,
                    destinationState,
                    new TransitionGuard(TransitionGuard.ToPackedGuards(guards))
                );
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
                return InternalPermitReentryIf(trigger, _representation.UnderlyingState, null);
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
                return InternalPermitReentryIf(
                    trigger,
                    _representation.UnderlyingState,
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
                return InternalPermitReentryIf(
                    trigger,
                    _representation.UnderlyingState,
                    new TransitionGuard(guards));
            }

            /// <summary>
            ///  Accept the specified trigger, transition to the destination state, and guard condition. 
            /// </summary>
            /// <typeparam name="TArg0"></typeparam>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted. Takes a single argument of type TArg0</param>
            /// <param name="guardDescription">Guard description</param>
            /// <returns>The reciever.</returns>
            public StateConfiguration PermitReentryIf<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, bool> guard, string guardDescription = null)
            {
                return InternalPermitReentryIf(
                    trigger.Trigger,
                    _representation.UnderlyingState,
                    new TransitionGuard(TransitionGuard.ToPackedGuard(guard), guardDescription)
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
            public StateConfiguration PermitReentryIf<TArg0>(TriggerWithParameters<TArg0> trigger, params Tuple<Func<TArg0, bool>, string>[] guards)
            {
                return InternalPermitReentryIf(
                    trigger.Trigger,
                    _representation.UnderlyingState,
                    new TransitionGuard(TransitionGuard.ToPackedGuards(guards))
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
            /// <returns>The reciever.</returns>
            public StateConfiguration PermitReentryIf<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, bool> guard, string guardDescription = null)
            {
                return InternalPermitReentryIf(
                    trigger.Trigger,
                    _representation.UnderlyingState,
                    new TransitionGuard(TransitionGuard.ToPackedGuard(guard), guardDescription)
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
            /// <returns>The reciever.</returns>
            public StateConfiguration PermitReentryIf<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, params Tuple<Func<TArg0, TArg1, bool>, string>[] guards)
            {
                return InternalPermitReentryIf(
                    trigger.Trigger,
                    _representation.UnderlyingState,
                    new TransitionGuard(TransitionGuard.ToPackedGuards(guards))
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
            /// <returns>The reciever.</returns>
            public StateConfiguration PermitReentryIf<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, bool> guard, string guardDescription = null)
            {
                return InternalPermitReentryIf(
                    trigger.Trigger,
                    _representation.UnderlyingState,
                    new TransitionGuard(TransitionGuard.ToPackedGuard(guard), guardDescription)
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
            /// <returns>The reciever.</returns>
            public StateConfiguration PermitReentryIf<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, params Tuple<Func<TArg0, TArg1, TArg2, bool>, string>[] guards)
            {
                return InternalPermitReentryIf(
                    trigger.Trigger,
                    _representation.UnderlyingState,
                    new TransitionGuard(TransitionGuard.ToPackedGuards(guards))
                );
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
                _representation.AddTriggerBehaviour(
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
                _representation.AddTriggerBehaviour(
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
                _representation.AddTriggerBehaviour(
                    new IgnoredTriggerBehaviour(
                        trigger,
                        new TransitionGuard(guards)));
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
            public StateConfiguration IgnoreIf<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, bool> guard, string guardDescription = null)
            {
                _representation.AddTriggerBehaviour(
                    new IgnoredTriggerBehaviour(
                        trigger.Trigger,
                        new TransitionGuard(TransitionGuard.ToPackedGuard(guard), guardDescription)
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
            public StateConfiguration IgnoreIf<TArg0>(TriggerWithParameters<TArg0> trigger, params Tuple<Func<TArg0, bool>, string>[] guards)
            {
                _representation.AddTriggerBehaviour(
                    new IgnoredTriggerBehaviour(
                        trigger.Trigger,
                        new TransitionGuard(TransitionGuard.ToPackedGuards(guards))));
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
            public StateConfiguration IgnoreIf<TArg0, TArgo1>(TriggerWithParameters<TArg0, TArgo1> trigger, Func<TArg0, TArgo1, bool> guard, string guardDescription = null)
            {
                _representation.AddTriggerBehaviour(
                    new IgnoredTriggerBehaviour(
                        trigger.Trigger,
                        new TransitionGuard(TransitionGuard.ToPackedGuard(guard), guardDescription)
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
            public StateConfiguration IgnoreIf<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, params Tuple<Func<TArg0, TArg1, bool>, string>[] guards)
            {
                _representation.AddTriggerBehaviour(
                    new IgnoredTriggerBehaviour(
                        trigger.Trigger,
                        new TransitionGuard(TransitionGuard.ToPackedGuards(guards))));
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
            public StateConfiguration IgnoreIf<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, bool> guard, string guardDescription = null)
            {
                _representation.AddTriggerBehaviour(
                    new IgnoredTriggerBehaviour(
                        trigger.Trigger,
                        new TransitionGuard(TransitionGuard.ToPackedGuard(guard), guardDescription)
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
            public StateConfiguration IgnoreIf<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, params Tuple<Func<TArg0, TArg1, TArg2, bool>, string>[] guards)
            {
                _representation.AddTriggerBehaviour(
                    new IgnoredTriggerBehaviour(
                        trigger.Trigger,
                        new TransitionGuard(TransitionGuard.ToPackedGuards(guards))));
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
                _representation.AddActivateAction(
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
                _representation.AddDeactivateAction(
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
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddEntryAction(
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
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddEntryAction(
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
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddEntryAction(
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
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddEntryAction(
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
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddEntryAction(
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
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddEntryAction(
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
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddEntryAction(trigger.Trigger,
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
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddEntryAction(trigger.Trigger, (t, args) => entryAction(
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
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddEntryAction(trigger.Trigger, (t, args) => entryAction(
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
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (entryAction == null) throw new ArgumentNullException(nameof(entryAction));

                _representation.AddEntryAction(trigger.Trigger, (t, args) => entryAction(
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
                if (exitAction == null) throw new ArgumentNullException(nameof(exitAction));

                _representation.AddExitAction(
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
                _representation.AddExitAction(
                    exitAction,
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
                var State = _representation.UnderlyingState;

                // Check for accidental identical cyclic configuration
                if (State.Equals(superstate))
                {
                    throw new ArgumentException($"Configuring {State} as a substate of {superstate} creates an illegal cyclic configuration.");
                }

                // Check for accidental identical nested cyclic configuration
                var superstates = new HashSet<TState> { State };

                // Build list of super states and check for
                var activeRepresentation = _lookup(superstate);
                while (activeRepresentation.Superstate != null)
                {
                    // Check if superstate is already added to hashset
                    if (superstates.Contains(activeRepresentation.Superstate.UnderlyingState))
                        throw new ArgumentException($"Configuring {State} as a substate of {superstate} creates an illegal nested cyclic configuration.");

                    superstates.Add(activeRepresentation.Superstate.UnderlyingState);
                    activeRepresentation = _lookup(activeRepresentation.Superstate.UnderlyingState);
                }

                // The check was OK, we can add this
                var superRepresentation = _lookup(superstate);
                _representation.Superstate = superRepresentation;
                superRepresentation.AddSubstate(_representation);
                return this;
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state
            /// that the trigger will cause a transition to.</param>
            /// <param name="destinationStateSelectorDescription">Optional description for the function to calculate the state </param>
            /// <param name="possibleDestinationStates">Optional array of possible destination states (used by output formatters) </param>
            /// <returns>The reciever.</returns>
            public StateConfiguration PermitDynamic(TTrigger trigger, Func<TState> destinationStateSelector,
                string destinationStateSelectorDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                _representation.AddTriggerBehaviour(
                    new DynamicTriggerBehaviour(trigger,
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
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state
            /// that the trigger will cause a transition to.</param>
            /// <param name="destinationStateSelectorDescription">Optional description of the function to calculate the state </param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The reciever.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            public StateConfiguration PermitDynamic<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, TState> destinationStateSelector,
                string destinationStateSelectorDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                if (trigger == null) throw new ArgumentNullException(nameof(trigger));

                _representation.AddTriggerBehaviour(
                    new DynamicTriggerBehaviour(trigger.Trigger,
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
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state
            /// that the trigger will cause a transition to.</param>
            /// <param name="destinationStateSelectorDescription">Optional description of the function to calculate the state </param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The reciever.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            public StateConfiguration PermitDynamic<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger,
                Func<TArg0, TArg1, TState> destinationStateSelector, string destinationStateSelectorDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                if (trigger == null) throw new ArgumentNullException(nameof(trigger));

                _representation.AddTriggerBehaviour(
                    new DynamicTriggerBehaviour(trigger.Trigger, args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0),
                        ParameterConversion.Unpack<TArg1>(args, 1)),
                    null,       // No transition guard
                    Reflection.DynamicTransitionInfo.Create(trigger.Trigger,
                        null,       // No guards
                        Reflection.InvocationInfo.Create(destinationStateSelector, destinationStateSelectorDescription),
                        possibleDestinationStates)
                ));
                return this;
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state
            /// that the trigger will cause a transition to.</param>
            /// <param name="destinationStateSelectorDescription">Optional description of the function to calculate the state </param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The reciever.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
            public StateConfiguration PermitDynamic<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger,
                Func<TArg0, TArg1, TArg2, TState> destinationStateSelector, string destinationStateSelectorDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                _representation.AddTriggerBehaviour(
                    new DynamicTriggerBehaviour(trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0),
                        ParameterConversion.Unpack<TArg1>(args, 1),
                        ParameterConversion.Unpack<TArg2>(args, 2)),
                    null,       // No transition guard
                    Reflection.DynamicTransitionInfo.Create(trigger.Trigger,
                        null,       // No guards
                        Reflection.InvocationInfo.Create(destinationStateSelector, destinationStateSelectorDescription),
                        possibleDestinationStates)
                    ));
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
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The reciever.</returns>
            public StateConfiguration PermitDynamicIf(TTrigger trigger, Func<TState> destinationStateSelector,
                Func<bool> guard, string guardDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                return PermitDynamicIf(trigger, destinationStateSelector, null, guard, guardDescription, possibleDestinationStates);
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state 
            /// that the trigger will cause a transition to.</param>
            /// <param name="destinationStateSelectorDescription">Description of the function to calculate the state </param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="guardDescription">Guard description</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The reciever.</returns>
            public StateConfiguration PermitDynamicIf(TTrigger trigger, Func<TState> destinationStateSelector,
                string destinationStateSelectorDescription, Func<bool> guard, string guardDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIf(
                    trigger,
                    args => destinationStateSelector(),
                    destinationStateSelectorDescription,
                    new TransitionGuard(guard, guardDescription),
                    possibleDestinationStates);
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
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The reciever.</returns>
            public StateConfiguration PermitDynamicIf(TTrigger trigger, Func<TState> destinationStateSelector, Reflection.DynamicStateInfos possibleDestinationStates = null, params Tuple<Func<bool>, string>[] guards)
            {
                return PermitDynamicIf(trigger, destinationStateSelector, null, possibleDestinationStates, guards);
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state 
            /// that the trigger will cause a transition to.</param>
            /// <param name="destinationStateSelectorDescription">Description of the function to calculate the state </param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The reciever.</returns>
            public StateConfiguration PermitDynamicIf(TTrigger trigger, Func<TState> destinationStateSelector,
                string destinationStateSelectorDescription, Reflection.DynamicStateInfos possibleDestinationStates = null, params Tuple<Func<bool>, string>[] guards)
            {
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIf(
                    trigger,
                    args => destinationStateSelector(),
                    destinationStateSelectorDescription,
                    new TransitionGuard(guards),
                    possibleDestinationStates);
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
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The reciever.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIf<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, TState> destinationStateSelector, Func<bool> guard, string guardDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIf(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0)),
                    null,    // destinationStateSelectorString
                    new TransitionGuard(guard, guardDescription),
                    possibleDestinationStates);
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state
            /// that the trigger will cause a transition to.</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be accepted.</param>
            /// <returns>The reciever.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIf<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, TState> destinationStateSelector, Reflection.DynamicStateInfos possibleDestinationStates = null, params Tuple<Func<bool>, string>[] guards)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIf(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0)),
                    null,    // destinationStateSelectorString
                    new TransitionGuard(guards),
                    possibleDestinationStates);
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
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The reciever.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIf<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, TState> destinationStateSelector, Func<bool> guard, string guardDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIf(
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
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state
            /// that the trigger will cause a transition to.</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be accepted.</param>
            /// <returns>The reciever.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIf<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, TState> destinationStateSelector, Reflection.DynamicStateInfos possibleDestinationStates = null, params Tuple<Func<bool>, string>[] guards)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIf(
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
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state
            /// that the trigger will cause a transition to.</param>
            /// <returns>The reciever.</returns>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="guardDescription">Guard description</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIf<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, TState> destinationStateSelector, Func<bool> guard, string guardDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIf(
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
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state
            /// that the trigger will cause a transition to.</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The reciever.</returns>
            /// <param name="guards">Functions ant their descriptions that must return true in order for the
            /// trigger to be accepted.</param>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            /// <typeparam name="TArg2">Type of the third trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIf<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, TState> destinationStateSelector, Reflection.DynamicStateInfos possibleDestinationStates = null, params Tuple<Func<bool>, string>[] guards)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIf(
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
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state
            /// that the trigger will cause a transition to.</param>
            /// <param name="guard">Parameterized Function that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="guardDescription">Guard description</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The reciever.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIf<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, TState> destinationStateSelector, Func<TArg0, bool> guard, string guardDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIf(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0)),
                    null,    // destinationStateSelectorString
                    new TransitionGuard(TransitionGuard.ToPackedGuard(guard), guardDescription),
                    possibleDestinationStates);
            }

            /// <summary>
            /// Accept the specified trigger and transition to the destination state, calculated
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state
            /// that the trigger will cause a transition to.</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <param name="guards">Functions and their descriptions that must return true in order for the
            /// trigger to be accepted.</param>
            /// <returns>The reciever.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIf<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, TState> destinationStateSelector, Reflection.DynamicStateInfos possibleDestinationStates = null, params Tuple<Func<TArg0, bool>, string>[] guards)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIf(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0)),
                    null,    // destinationStateSelectorString
                    new TransitionGuard(TransitionGuard.ToPackedGuards(guards)),
                    possibleDestinationStates);
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
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The reciever.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIf<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, TState> destinationStateSelector, Func<TArg0, TArg1, bool> guard, string guardDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIf(
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
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state
            /// that the trigger will cause a transition to.</param>
            /// <param name="guards">Functions that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The reciever.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            public StateConfiguration PermitDynamicIf<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, TState> destinationStateSelector, Tuple<Func<TArg0, TArg1, bool>, string>[] guards, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIf(
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
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state
            /// that the trigger will cause a transition to.</param>
            /// <param name="guard">Function that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="guardDescription">Guard description</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The reciever.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            /// <typeparam name="TArg2"></typeparam>
            public StateConfiguration PermitDynamicIf<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, TState> destinationStateSelector, Func<TArg0, TArg1, TArg2, bool> guard, string guardDescription = null, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIf(
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
            /// dynamically by the supplied function.
            /// </summary>
            /// <param name="trigger">The accepted trigger.</param>
            /// <param name="destinationStateSelector">Function to calculate the state
            /// that the trigger will cause a transition to.</param>
            /// <param name="guards">Functions that must return true in order for the
            /// trigger to be accepted.</param>
            /// <param name="possibleDestinationStates">Optional list of possible target states.</param>
            /// <returns>The reciever.</returns>
            /// <typeparam name="TArg0">Type of the first trigger argument.</typeparam>
            /// <typeparam name="TArg1">Type of the second trigger argument.</typeparam>
            /// <typeparam name="TArg2"></typeparam>
            public StateConfiguration PermitDynamicIf<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, TState> destinationStateSelector, Tuple<Func<TArg0, TArg1, TArg2, bool>, string>[] guards, Reflection.DynamicStateInfos possibleDestinationStates = null)
            {
                if (trigger == null) throw new ArgumentNullException(nameof(trigger));
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));

                return InternalPermitDynamicIf(
                    trigger.Trigger,
                    args => destinationStateSelector(
                        ParameterConversion.Unpack<TArg0>(args, 0),
                        ParameterConversion.Unpack<TArg1>(args, 1),
                        ParameterConversion.Unpack<TArg2>(args, 2)),
                    null,    // destinationStateSelectorString
                    new TransitionGuard(TransitionGuard.ToPackedGuards(guards)),
                    possibleDestinationStates);
            }

            void EnforceNotIdentityTransition(TState destination)
            {
                if (destination.Equals(_representation.UnderlyingState))
                {
                    throw new ArgumentException(StateConfigurationResources.SelfTransitionsEitherIgnoredOrReentrant);
                }
            }

            StateConfiguration InternalPermit(TTrigger trigger, TState destinationState)
            {
                _representation.AddTriggerBehaviour(new TransitioningTriggerBehaviour(trigger, destinationState, null));
                return this;
            }

            StateConfiguration InternalPermitIf(TTrigger trigger, TState destinationState, TransitionGuard transitionGuard)
            {
                _representation.AddTriggerBehaviour(new TransitioningTriggerBehaviour(trigger, destinationState, transitionGuard));
                return this;
            }

            StateConfiguration InternalPermitReentryIf(TTrigger trigger, TState destinationState, TransitionGuard transitionGuard)
            {
                _representation.AddTriggerBehaviour(new ReentryTriggerBehaviour(trigger, destinationState, transitionGuard));
                return this;
            }

            StateConfiguration InternalPermitDynamicIf(TTrigger trigger, Func<object[], TState> destinationStateSelector,
                string destinationStateSelectorDescription, TransitionGuard transitionGuard, Reflection.DynamicStateInfos possibleDestinationStates)
            {
                if (destinationStateSelector == null) throw new ArgumentNullException(nameof(destinationStateSelector));
                if (transitionGuard == null) throw new ArgumentNullException(nameof(transitionGuard));

                _representation.AddTriggerBehaviour(new DynamicTriggerBehaviour(trigger,
                    destinationStateSelector,
                    transitionGuard,
                    Reflection.DynamicTransitionInfo.Create(trigger,
                        transitionGuard.Conditions.Select(x => x.MethodDescription),
                        Reflection.InvocationInfo.Create(destinationStateSelector, destinationStateSelectorDescription),
                        possibleDestinationStates)
                    ));
                return this;
            }

            /// <summary>
            ///  Adds internal transition to this state. When entering the current state the state machine will look for an initial transition, and enter the target state.
            /// </summary>
            /// <param name="targetState">The target initial state</param>
            /// <returns>A stateConfiguration object</returns>
            public StateConfiguration InitialTransition(TState targetState)
            {
                if (_representation.HasInitialTransition) throw new InvalidOperationException($"This state has already been configured with an inital transition ({_representation.InitialTransitionTarget}).");
                if (targetState.Equals(State)) throw new ArgumentException("Setting the current state as the target destination state is not allowed.", nameof(targetState));

                _representation.SetInitialTransition(targetState);
                return this;
            }
        }
    }
}
