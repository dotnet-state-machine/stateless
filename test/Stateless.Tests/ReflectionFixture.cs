using System;
using Xunit;
using Stateless.Reflection;
using Xunit.Sdk;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Stateless.Tests
{
    public class ReflectionFixture
    {
        static readonly string UserDescription = "UserDescription";

        bool IsTrue()
        {
            return true;
        }

        void OnEntry()
        {

        }

        void OnExit()
        {

        }

        Task OnActivateAsync()
        {
            return TaskResult.Done;
        }

        void OnActivate()
        {
        }

        void OnEntryTrans(StateMachine<State, Trigger>.Transition trans)
        {
        }

        Task OnEntryTransAsync(StateMachine<State, Trigger>.Transition trans)
        {
            return TaskResult.Done;
        }

        Task OnEntryAsync()
        {
            return TaskResult.Done;
        }

        Task OnDeactivateAsync()
        {
            return TaskResult.Done;
        }

        void OnDeactivate()
        {
        }

        void OnExitTrans(StateMachine<State, Trigger>.Transition trans)
        {
        }

        Task OnExitAsync()
        {
            return TaskResult.Done;
        }

        Task OnExitTransAsync(StateMachine<State, Trigger>.Transition trans)
        {
            return TaskResult.Done;
        }

        [Fact]
        public void SimpleTransition_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            StateMachineInfo inf = sm.GetInfo();

            Assert.Equal(inf.TriggerType, typeof(Trigger));
            Assert.Equal(inf.States.Count(), 2);
            var binding = inf.States.Single(s => (State)s.UnderlyingState == State.A);

            Assert.True(binding.UnderlyingState is State);
            Assert.Equal(State.A, (State)binding.UnderlyingState);
            //
            Assert.Equal(0, binding.Substates.Count());
            Assert.Equal(null, binding.Superstate);
            Assert.Equal(0, binding.EntryActions.Count());
            Assert.Equal(0, binding.ExitActions.Count());
            //
            Assert.Equal(1, binding.FixedTransitions.Count());
            foreach (FixedTransitionInfo trans in binding.FixedTransitions)
            {
                Assert.True(trans.Trigger.UnderlyingTrigger is Trigger);
                Assert.Equal(Trigger.X, (Trigger)trans.Trigger.UnderlyingTrigger);
                //
                Assert.True(trans.DestinationState.UnderlyingState is State);
                Assert.Equal(State.B, (State)trans.DestinationState.UnderlyingState);
                Assert.Equal(null, trans.GuardDescription);
            }
            Assert.Equal(0, binding.IgnoredTriggers.Count());
            Assert.Equal(0, binding.DynamicTransitions.Count());
        }

        [Fact]
        public void TwoSimpleTransitions_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B)
                .Permit(Trigger.Y, State.C);

            StateMachineInfo inf = sm.GetInfo();

            Assert.True(inf.StateType == typeof(State));
            Assert.Equal(inf.TriggerType, typeof(Trigger));
            Assert.Equal(inf.States.Count(), 3);
            var binding = inf.States.Single(s => (State)s.UnderlyingState == State.A);

            Assert.True(binding.UnderlyingState is State);
            Assert.Equal(State.A, (State)binding.UnderlyingState); // Binding state value mismatch
            //
            Assert.Equal(0, binding.Substates.Count()); //  Binding substate count mismatch"
            Assert.Equal(null, binding.Superstate);
            Assert.Equal(0, binding.EntryActions.Count()); //  Binding entry actions count mismatch
            Assert.Equal(0, binding.ExitActions.Count());
            //
            Assert.Equal(2, binding.FixedTransitions.Count()); // Transition count mismatch
            //
            bool haveXB = false;
            bool haveYC = false;
            foreach (FixedTransitionInfo trans in binding.FixedTransitions)
            {
                Assert.True(trans.Trigger.UnderlyingTrigger is Trigger);
                //
                Assert.True(trans.DestinationState.UnderlyingState is State);
                Assert.Equal(null, trans.GuardDescription);
                //
                // Can't make assumptions about which trigger/destination comes first in the list
                if ((Trigger)trans.Trigger.UnderlyingTrigger == Trigger.X)
                {
                    Assert.Equal(State.B, (State)trans.DestinationState.UnderlyingState);
                    Assert.False(haveXB);
                    haveXB = true;
                }
                else if ((Trigger)trans.Trigger.UnderlyingTrigger == Trigger.Y)
                {
                    Assert.Equal(State.C, (State)trans.DestinationState.UnderlyingState);
                    Assert.False(haveYC);
                    haveYC = true;
                }
                else
                    throw new XunitException("Failed.");
            }
            Assert.True(haveXB && haveYC);
            //
            Assert.Equal(0, binding.IgnoredTriggers.Count());
            Assert.Equal(0, binding.DynamicTransitions.Count());
        }

        [Fact]
        public void WhenDiscriminatedByAnonymousGuard_Binding()
        {
            Func<bool> anonymousGuard = () => true;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, anonymousGuard);

            StateMachineInfo inf = sm.GetInfo();

            Assert.True(inf.StateType == typeof(State));
            Assert.Equal(inf.TriggerType, typeof(Trigger));
            Assert.Equal(inf.States.Count(), 2);
            var binding = inf.States.Single(s => (State)s.UnderlyingState == State.A);

            Assert.True(binding.UnderlyingState is State);
            Assert.Equal(State.A, (State)binding.UnderlyingState);
            //
            Assert.Equal(0, binding.Substates.Count());
            Assert.Equal(null, binding.Superstate);
            Assert.Equal(0, binding.EntryActions.Count());
            Assert.Equal(0, binding.ExitActions.Count());
            //
            Assert.Equal(1, binding.FixedTransitions.Count());
            foreach (FixedTransitionInfo trans in binding.FixedTransitions)
            {
                Assert.True(trans.Trigger.UnderlyingTrigger is Trigger);
                Assert.Equal(Trigger.X, (Trigger)trans.Trigger.UnderlyingTrigger);
                //
                Assert.True(trans.DestinationState.UnderlyingState is State);
                Assert.Equal(State.B, (State)trans.DestinationState.UnderlyingState);
                //
                Assert.NotEqual(null, trans.GuardDescription);
            }
            Assert.Equal(0, binding.IgnoredTriggers.Count());
            Assert.Equal(0, binding.DynamicTransitions.Count());
        }

        [Fact]
        public void WhenDiscriminatedByAnonymousGuardWithDescription_Binding()
        {
            Func<bool> anonymousGuard = () => true;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, anonymousGuard, "description");

            StateMachineInfo inf = sm.GetInfo();

            Assert.True(inf.StateType == typeof(State));
            Assert.Equal(inf.TriggerType, typeof(Trigger));
            Assert.Equal(inf.States.Count(), 2);
            var binding = inf.States.Single(s => (State)s.UnderlyingState == State.A);

            Assert.True(binding.UnderlyingState is State);
            Assert.Equal(State.A, (State)binding.UnderlyingState);
            //
            Assert.Equal(0, binding.Substates.Count());
            Assert.Equal(null, binding.Superstate);
            Assert.Equal(0, binding.EntryActions.Count());
            Assert.Equal(0, binding.ExitActions.Count());
            //
            Assert.Equal(1, binding.FixedTransitions.Count());
            foreach (FixedTransitionInfo trans in binding.FixedTransitions)
            {
                Assert.True(trans.Trigger.UnderlyingTrigger is Trigger);
                Assert.Equal(Trigger.X, (Trigger)trans.Trigger.UnderlyingTrigger);
                //
                Assert.True(trans.DestinationState.UnderlyingState is State);
                Assert.Equal(State.B, (State)trans.DestinationState.UnderlyingState);
                //
                Assert.NotEqual(null, trans.GuardDescription);
                Assert.Equal("description", trans.GuardDescription);
            }
            Assert.Equal(0, binding.IgnoredTriggers.Count());
            Assert.Equal(0, binding.DynamicTransitions.Count());
        }

        [Fact]
        public void WhenDiscriminatedByNamedDelegate_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue);

            StateMachineInfo inf = sm.GetInfo();

            Assert.True(inf.StateType == typeof(State));
            Assert.Equal(inf.TriggerType, typeof(Trigger));
            Assert.Equal(inf.States.Count(), 2);
            var binding = inf.States.Single(s => (State)s.UnderlyingState == State.A);

            Assert.True(binding.UnderlyingState is State);
            Assert.Equal(State.A, (State)binding.UnderlyingState);
            //
            Assert.Equal(0, binding.Substates.Count());
            Assert.Equal(null, binding.Superstate);
            Assert.Equal(0, binding.EntryActions.Count());
            Assert.Equal(0, binding.ExitActions.Count());
            //
            Assert.Equal(1, binding.FixedTransitions.Count());
            foreach (FixedTransitionInfo trans in binding.FixedTransitions)
            {
                Assert.True(trans.Trigger.UnderlyingTrigger is Trigger);
                Assert.Equal(Trigger.X, (Trigger)trans.Trigger.UnderlyingTrigger);
                //
                Assert.True(trans.DestinationState.UnderlyingState is State);
                Assert.Equal(State.B, (State)trans.DestinationState.UnderlyingState);
                //
                Assert.NotEqual(null, trans.GuardDescription);
                Assert.Equal("IsTrue", trans.GuardDescription);
            }
            Assert.Equal(0, binding.IgnoredTriggers.Count());
            Assert.Equal(0, binding.DynamicTransitions.Count());
        }

        [Fact]
        public void WhenDiscriminatedByNamedDelegateWithDescription_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue, "description");

            StateMachineInfo inf = sm.GetInfo();

            Assert.True(inf.StateType == typeof(State));
            Assert.Equal(inf.TriggerType, typeof(Trigger));
            Assert.Equal(inf.States.Count(), 2);
            var binding = inf.States.Single(s => (State)s.UnderlyingState == State.A);

            Assert.True(binding.UnderlyingState is State);
            Assert.Equal(State.A, (State)binding.UnderlyingState);
            //
            Assert.Equal(0, binding.Substates.Count());
            Assert.Equal(null, binding.Superstate);
            Assert.Equal(0, binding.EntryActions.Count());
            Assert.Equal(0, binding.ExitActions.Count());
            //
            Assert.Equal(1, binding.FixedTransitions.Count());
            foreach (FixedTransitionInfo trans in binding.FixedTransitions)
            {
                Assert.True(trans.Trigger.UnderlyingTrigger is Trigger);
                Assert.Equal(Trigger.X, (Trigger)trans.Trigger.UnderlyingTrigger);
                //
                Assert.True(trans.DestinationState.UnderlyingState is State);
                Assert.Equal(State.B, (State)trans.DestinationState.UnderlyingState);
                //
                Assert.NotEqual(null, trans.GuardDescription);
                Assert.Equal("description", trans.GuardDescription);
            }
            Assert.Equal(0, binding.IgnoredTriggers.Count());
            Assert.Equal(0, binding.DynamicTransitions.Count());
        }

        [Fact]
        public void DestinationStateIsDynamic_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .PermitDynamic(Trigger.X, () => State.B);

            StateMachineInfo inf = sm.GetInfo();

            Assert.True(inf.StateType == typeof(State));
            Assert.Equal(inf.TriggerType, typeof(Trigger));
            Assert.Equal(inf.States.Count(), 1);
            var binding = inf.States.Single(s => (State)s.UnderlyingState == State.A);

            Assert.True(binding.UnderlyingState is State);
            Assert.Equal(State.A, (State)binding.UnderlyingState);
            //
            Assert.Equal(0, binding.Substates.Count());
            Assert.Equal(null, binding.Superstate);
            Assert.Equal(0, binding.EntryActions.Count());
            Assert.Equal(0, binding.ExitActions.Count());
            //
            Assert.Equal(0, binding.FixedTransitions.Count()); // Binding transition count mismatch
            Assert.Equal(0, binding.IgnoredTriggers.Count());
            Assert.Equal(1, binding.DynamicTransitions.Count()); // Dynamic transition count mismatch
            foreach (DynamicTransitionInfo trans in binding.DynamicTransitions)
            {
                Assert.True(trans.Trigger.UnderlyingTrigger is Trigger);
                Assert.Equal(Trigger.X, (Trigger)trans.Trigger.UnderlyingTrigger);
                Assert.NotEqual(null, trans.Destination);
                Assert.Equal(null, trans.GuardDescription);
            }
        }

        [Fact]
        public void DestinationStateIsCalculatedBasedOnTriggerParameters_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitDynamic(trigger, i => i == 1 ? State.B : State.C);

            StateMachineInfo inf = sm.GetInfo();

            Assert.True(inf.StateType == typeof(State));
            Assert.Equal(inf.TriggerType, typeof(Trigger));
            Assert.Equal(inf.States.Count(), 1);
            var binding = inf.States.Single(s => (State)s.UnderlyingState == State.A);

            Assert.True(binding.UnderlyingState is State);
            Assert.Equal(State.A, (State)binding.UnderlyingState);
            //
            Assert.Equal(0, binding.Substates.Count());
            Assert.Equal(null, binding.Superstate);
            Assert.Equal(0, binding.EntryActions.Count());
            Assert.Equal(0, binding.ExitActions.Count());
            //
            Assert.Equal(0, binding.FixedTransitions.Count()); // Binding transition count mismatch"
            Assert.Equal(0, binding.IgnoredTriggers.Count());
            Assert.Equal(1, binding.DynamicTransitions.Count()); // Dynamic transition count mismatch
            foreach (DynamicTransitionInfo trans in binding.DynamicTransitions)
            {
                Assert.True(trans.Trigger.UnderlyingTrigger is Trigger);
                Assert.Equal(Trigger.X, (Trigger)trans.Trigger.UnderlyingTrigger);
                Assert.NotEqual(null, trans.Destination);
                Assert.Equal(null, trans.GuardDescription);
            }
        }

        [Fact]
        public void OnEntryWithAnonymousActionAndDescription_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => { }, "enteredA");

            StateMachineInfo inf = sm.GetInfo();

            Assert.True(inf.StateType == typeof(State));
            Assert.Equal(inf.States.Count(), 1);
            var binding = inf.States.Single(s => (State)s.UnderlyingState == State.A);

            Assert.True(binding.UnderlyingState is State);
            Assert.Equal(State.A, (State)binding.UnderlyingState);
            //
            Assert.Equal(0, binding.Substates.Count());
            Assert.Equal(null, binding.Superstate);
            Assert.Equal(1, binding.EntryActions.Count());
            foreach (MethodInfo entryAction in binding.EntryActions)
            {
                Assert.Equal("enteredA", entryAction.Description);
            }
            Assert.Equal(0, binding.ExitActions.Count());
            //
            Assert.Equal(0, binding.FixedTransitions.Count()); // Binding count mismatch
            Assert.Equal(0, binding.IgnoredTriggers.Count());
            Assert.Equal(0, binding.DynamicTransitions.Count()); // Dynamic transition count mismatch
        }

        [Fact]
        public void OnEntryWithNamedDelegateActionAndDescription_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(OnEntry, "enteredA");

            StateMachineInfo inf = sm.GetInfo();

            Assert.True(inf.StateType == typeof(State));
            Assert.Equal(inf.States.Count(), 1);
            var binding = inf.States.Single(s => (State)s.UnderlyingState == State.A);

            Assert.True(binding.UnderlyingState is State);
            Assert.Equal(State.A, (State)binding.UnderlyingState);
            //
            Assert.Equal(0, binding.Substates.Count());
            Assert.Equal(null, binding.Superstate);
            //
            Assert.Equal(1, binding.EntryActions.Count());
            foreach (MethodInfo entryAction in binding.EntryActions)
                Assert.Equal("enteredA", entryAction.Description);
            Assert.Equal(0, binding.ExitActions.Count());
            //
            Assert.Equal(0, binding.FixedTransitions.Count()); // Binding count mismatch
            Assert.Equal(0, binding.IgnoredTriggers.Count());
            Assert.Equal(0, binding.DynamicTransitions.Count()); // Dynamic transition count mismatch
        }

        [Fact]
        public void OnExitWithAnonymousActionAndDescription_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(() => { }, "exitA");

            StateMachineInfo inf = sm.GetInfo();

            Assert.True(inf.StateType == typeof(State));
            Assert.Equal(inf.States.Count(), 1);
            var binding = inf.States.Single(s => (State)s.UnderlyingState == State.A);

            Assert.True(binding.UnderlyingState is State);
            Assert.Equal(State.A, (State)binding.UnderlyingState);
            //
            Assert.Equal(0, binding.Substates.Count());
            Assert.Equal(null, binding.Superstate);
            //
            Assert.Equal(0, binding.EntryActions.Count());
            Assert.Equal(1, binding.ExitActions.Count());
            foreach (MethodInfo exitAction in binding.ExitActions)
                Assert.Equal("exitA", exitAction.Description);
            //
            Assert.Equal(0, binding.FixedTransitions.Count()); // Binding count mismatch
            Assert.Equal(0, binding.IgnoredTriggers.Count());
            Assert.Equal(0, binding.DynamicTransitions.Count()); // Dynamic transition count mismatch
        }

        [Fact]
        public void OnExitWithNamedDelegateActionAndDescription_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(OnExit, "exitA");

            StateMachineInfo inf = sm.GetInfo();

            Assert.True(inf.StateType == typeof(State));
            Assert.Equal(inf.States.Count(), 1);
            var binding = inf.States.Single(s => (State)s.UnderlyingState == State.A);

            Assert.True(binding.UnderlyingState is State);
            Assert.Equal(State.A, (State)binding.UnderlyingState);
            //
            Assert.Equal(0, binding.Substates.Count());
            Assert.Equal(null, binding.Superstate);
            //
            Assert.Equal(0, binding.EntryActions.Count());
            Assert.Equal(1, binding.ExitActions.Count());
            foreach (MethodInfo entryAction in binding.ExitActions)
                Assert.Equal("exitA", entryAction.Description);
            //
            Assert.Equal(0, binding.FixedTransitions.Count()); // Binding count mismatch
            Assert.Equal(0, binding.IgnoredTriggers.Count());
            Assert.Equal(0, binding.DynamicTransitions.Count()); // Dynamic transition count mismatch
        }

        [Fact]
        public void TransitionWithIgnore_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Ignore(Trigger.Y)
                .Permit(Trigger.X, State.B);

            StateMachineInfo inf = sm.GetInfo();

            Assert.True(inf.StateType == typeof(State));
            Assert.Equal(inf.TriggerType, typeof(Trigger));
            Assert.Equal(inf.States.Count(), 2);
            var binding = inf.States.Single(s => (State)s.UnderlyingState == State.A);

            Assert.True(binding.UnderlyingState is State);
            Assert.Equal(State.A, (State)binding.UnderlyingState);
            //
            Assert.Equal(1, binding.FixedTransitions.Count()); // Transition count mismatch"
            foreach (FixedTransitionInfo trans in binding.FixedTransitions)
            {
                Assert.True(trans.Trigger.UnderlyingTrigger is Trigger);
                Assert.Equal(Trigger.X, (Trigger)trans.Trigger.UnderlyingTrigger);
                //
                Assert.True(trans.DestinationState.UnderlyingState is State);
                Assert.Equal(State.B, (State)trans.DestinationState.UnderlyingState);
                Assert.Equal(null, trans.GuardDescription);
            }
            //
            Assert.Equal(1, binding.IgnoredTriggers.Count()); //  Ignored triggers count mismatch
            foreach (TriggerInfo ignore in binding.IgnoredTriggers)
            {
                Assert.True(ignore.UnderlyingTrigger is Trigger);
                Assert.Equal(Trigger.Y, (Trigger)ignore.UnderlyingTrigger); // Ignored trigger value mismatch
            }
            //
            Assert.Equal(0, binding.Substates.Count());
            Assert.Equal(null, binding.Superstate);
            Assert.Equal(0, binding.EntryActions.Count());
            Assert.Equal(0, binding.ExitActions.Count());
            Assert.Equal(0, binding.DynamicTransitions.Count()); // Dynamic transition count mismatch
        }

        void VerifyMethodNames(IEnumerable<MethodInfo> methods, string prefix, State state, MethodDescription.Timing timing)
        {
            Assert.Equal(1, methods.Count());

            if (state == State.A)
                Assert.Equal("On" + prefix + ((timing == MethodDescription.Timing.Asynchronous) ? "Async" : ""), methods.First().Description);
            else if (state == State.B)
                Assert.Equal(UserDescription + "B-" + prefix, methods.First().Description);
            else if (state == State.C)
                Assert.Equal(MethodInfo.DefaultFunctionName, methods.First().Description);
            else if (state == State.D)
                Assert.Equal(UserDescription + "D-" + prefix, methods.First().Description);

            Assert.Equal(true, methods.First().IsAsync);
        }

        [Fact]
        public void ReflectionMethodNames()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnActivate(OnActivate)
                .OnEntry(OnEntry)
                .OnExit(OnExit)
                .OnDeactivate(OnDeactivate);
            sm.Configure(State.B)
                .OnActivate(OnActivate, UserDescription + "B-Activate")
                .OnEntry(OnEntry, UserDescription + "B-Entry")
                .OnExit(OnExit, UserDescription + "B-Exit")
                .OnDeactivate(OnDeactivate, UserDescription + "B-Deactivate");
            sm.Configure(State.C)
                .OnActivate(() => OnActivate())
                .OnEntry(() => OnEntry())
                .OnExit(() => OnExit())
                .OnDeactivate(() => OnDeactivate());
            sm.Configure(State.D)
                .OnActivate(() => OnActivate(), UserDescription + "D-Activate")
                .OnEntry(() => OnEntry(), UserDescription + "D-Entry")
                .OnExit(() => OnExit(), UserDescription + "D-Exit")
                .OnDeactivate(() => OnDeactivate(), UserDescription + "D-Deactivate");

            StateMachineInfo inf = sm.GetInfo();

            foreach (StateInfo stateInfo in inf.States)
            {
                VerifyMethodNames(stateInfo.ActivateActions, "Activate", (State)stateInfo.UnderlyingState, MethodDescription.Timing.Synchronous);
                VerifyMethodNames(stateInfo.EntryActions, "Entry", (State)stateInfo.UnderlyingState, MethodDescription.Timing.Synchronous);
                VerifyMethodNames(stateInfo.ExitActions, "Exit", (State)stateInfo.UnderlyingState, MethodDescription.Timing.Synchronous);
                VerifyMethodNames(stateInfo.DeactivateActions, "Deactivate", (State)stateInfo.UnderlyingState, MethodDescription.Timing.Synchronous);
            }

            // New StateMachine, new tests: entry and exit, functions that take the transition as an argument
            sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(OnEntryTrans)
                .OnExit(OnExitTrans);
            sm.Configure(State.B)
                .OnEntry(OnEntryTrans, UserDescription + "B-EntryTrans")
                .OnExit(OnExitTrans, UserDescription + "B-ExitTrans");
            sm.Configure(State.C)
                .OnEntry((t) => OnEntryTrans(t))
                .OnExit((t) => OnExitTrans(t));
            sm.Configure(State.D)
                .OnEntry((t) => OnEntryTrans(t), UserDescription + "D-EntryTrans")
                .OnExit((t) => OnExitTrans(t), UserDescription + "D-ExitTrans");

            inf = sm.GetInfo();

            foreach (StateInfo stateInfo in inf.States)
            {
                VerifyMethodNames(stateInfo.EntryActions, "EntryTrans", (State)stateInfo.UnderlyingState, MethodDescription.Timing.Synchronous);
                VerifyMethodNames(stateInfo.ExitActions, "ExitTrans", (State)stateInfo.UnderlyingState, MethodDescription.Timing.Synchronous);
            }

            /*
            public StateConfiguration OnEntryFrom(TTrigger trigger, Action entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFrom(TTrigger trigger, Action<Transition> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFrom<TArg0>(TriggerWithParameters<TArg0> trigger, Action<TArg0> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFrom<TArg0>(TriggerWithParameters<TArg0> trigger, Action<TArg0, Transition> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFrom<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Action<TArg0, TArg1> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFrom<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Action<TArg0, TArg1, Transition> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFrom<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Action<TArg0, TArg1, TArg2> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFrom<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Action<TArg0, TArg1, TArg2, Transition> entryAction, string entryActionDescription = null)
             */
        }

        [Fact]
        public void ReflectionMethodNamesAsync()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnActivateAsync(OnActivateAsync)
                .OnEntryAsync(OnEntryAsync)
                .OnExitAsync(OnExitAsync)
                .OnDeactivateAsync(OnDeactivateAsync);
            sm.Configure(State.B)
                .OnActivateAsync(OnActivateAsync, UserDescription + "B-Activate")
                .OnEntryAsync(OnEntryAsync, UserDescription + "B-Entry")
                .OnExitAsync(OnExitAsync, UserDescription + "B-Exit")
                .OnDeactivateAsync(OnDeactivateAsync, UserDescription + "B-Deactivate");
            sm.Configure(State.C)
                .OnActivateAsync(() => OnActivateAsync())
                .OnEntryAsync(() => OnEntryAsync())
                .OnExitAsync(() => OnExitAsync())
                .OnDeactivateAsync(() => OnDeactivateAsync());
            sm.Configure(State.D)
                .OnActivateAsync(() => OnActivateAsync(), UserDescription + "D-Activate")
                .OnEntryAsync(() => OnEntryAsync(), UserDescription + "D-Entry")
                .OnExitAsync(() => OnExitAsync(), UserDescription + "D-Exit")
                .OnDeactivateAsync(() => OnDeactivateAsync(), UserDescription + "D-Deactivate");

            StateMachineInfo inf = sm.GetInfo();

            foreach (StateInfo stateInfo in inf.States)
            {
                VerifyMethodNames(stateInfo.ActivateActions, "Activate", (State)stateInfo.UnderlyingState, MethodDescription.Timing.Asynchronous);
                VerifyMethodNames(stateInfo.EntryActions, "Entry", (State)stateInfo.UnderlyingState, MethodDescription.Timing.Asynchronous);
                VerifyMethodNames(stateInfo.ExitActions, "Exit", (State)stateInfo.UnderlyingState, MethodDescription.Timing.Asynchronous);
                VerifyMethodNames(stateInfo.DeactivateActions, "Deactivate", (State)stateInfo.UnderlyingState, MethodDescription.Timing.Asynchronous);
            }

            // New StateMachine, new tests: entry and exit, functions that take the transition as an argument
            sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntryAsync(OnEntryTransAsync)
                .OnExitAsync(OnExitTransAsync);
            sm.Configure(State.B)
                .OnEntryAsync(OnEntryTransAsync, UserDescription + "B-EntryTrans")
                .OnExitAsync(OnExitTransAsync, UserDescription + "B-ExitTrans");
            sm.Configure(State.C)
                .OnEntryAsync((t) => OnEntryTransAsync(t))
                .OnExitAsync((t) => OnExitTransAsync(t));
            sm.Configure(State.D)
                .OnEntryAsync((t) => OnEntryTransAsync(t), UserDescription + "D-EntryTrans")
                .OnExitAsync((t) => OnExitTransAsync(t), UserDescription + "D-ExitTrans");

            inf = sm.GetInfo();

            foreach (StateInfo stateInfo in inf.States)
            {
                VerifyMethodNames(stateInfo.EntryActions, "EntryTrans", (State)stateInfo.UnderlyingState, MethodDescription.Timing.Asynchronous);
                VerifyMethodNames(stateInfo.ExitActions, "ExitTrans", (State)stateInfo.UnderlyingState, MethodDescription.Timing.Asynchronous);
            }
            /*
            public StateConfiguration OnEntryFromAsync(TTrigger trigger, Func<Task> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFromAsync(TTrigger trigger, Func<Transition, Task> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFromAsync<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, Task> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFromAsync<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, Transition, Task> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFromAsync<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, Task> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFromAsync<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, Transition, Task> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFromAsync<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, Task> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFromAsync<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, Transition, Task> entryAction, string entryActionDescription = null)
            */
        }

        [Fact]
        public void TransitionGuardNames()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A);

            /*
            internal TransitionGuard(Tuple<Func<bool>, string>[] guards)
            internal TransitionGuard(Func<bool> guard, string description = null)
             */
            /*
           public IgnoredTriggerBehaviour(TTrigger trigger, Func<bool> guard, string description = null)
               : base(trigger, new TransitionGuard(guard, description))
            public InternalTriggerBehaviour(TTrigger trigger, Func<bool> guard)
                : base(trigger, new TransitionGuard(guard, "Internal Transition"))
            public TransitioningTriggerBehaviour(TTrigger trigger, TState destination, Func<bool> guard = null, string guardDescription = null)
                : base(trigger, new TransitionGuard(guard, guardDescription))
            public StateConfiguration PermitIf(TTrigger trigger, TState destinationState, Func<bool> guard, string guardDescription = null)
            public StateConfiguration PermitReentryIf(TTrigger trigger, Func<bool> guard, string guardDescription = null)
            public StateConfiguration PermitDynamicIf(TTrigger trigger, Func<TState> destinationStateSelector, Func<bool> guard, string guardDescription = null)
            public StateConfiguration PermitDynamicIf<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, TState> destinationStateSelector, Func<bool> guard, string guardDescription = null)
            public StateConfiguration PermitDynamicIf<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, TState> destinationStateSelector, Func<bool> guard, string guardDescription = null)
            public StateConfiguration PermitDynamicIf<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, TState> destinationStateSelector, Func<bool> guard, string guardDescription = null)
            StateConfiguration InternalPermit(TTrigger trigger, TState destinationState, string guardDescription)
            StateConfiguration InternalPermitDynamic(TTrigger trigger, Func<object[], TState> destinationStateSelector, string guardDescription)
             */
        }
    }
}

