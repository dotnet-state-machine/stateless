using System;
using Xunit;
using Stateless.Reflection;
using Xunit.Sdk;
using System.Linq;
using System.Threading.Tasks;

namespace Stateless.Tests
{
    public class ReflectionFixture
    {
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

        [Fact]
        public void ReflectionMethodNames()
        {
            string UserDescription = "UserDescription";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnActivateAsync(OnActivateAsync);
            sm.Configure(State.B)
                .OnActivateAsync(OnActivateAsync, UserDescription + "B");
            sm.Configure(State.C)
                .OnActivateAsync(() => OnActivateAsync());
            sm.Configure(State.D)
                .OnActivateAsync(() => OnActivateAsync(), UserDescription + "D");

            StateMachineInfo inf = sm.GetInfo();

            foreach (StateInfo stateInfo in inf.States)
            {
                var activateActions = stateInfo.ActivateActions;
                if ((State)stateInfo.UnderlyingState == State.A)
                {
                    Assert.Equal(1, activateActions.Count());
                    Assert.Equal("OnActivateAsync", activateActions.First().Description);
                    Assert.Equal(true, activateActions.First().IsAsync);
                }
                else if ((State)stateInfo.UnderlyingState == State.B)
                {
                    Assert.Equal(1, activateActions.Count());
                    Assert.Equal(UserDescription + "B", activateActions.First().Description);
                    Assert.Equal(true, activateActions.First().IsAsync);
                }
                else if ((State)stateInfo.UnderlyingState == State.C)
                {
                    Assert.Equal(1, activateActions.Count());
                    Assert.Equal(MethodInfo.DefaultFunctionName, activateActions.First().Description);
                    Assert.Equal(true, activateActions.First().IsAsync);
                }
                else if ((State)stateInfo.UnderlyingState == State.D)
                {
                    Assert.Equal(1, activateActions.Count());
                    Assert.Equal(UserDescription + "D", activateActions.First().Description);
                    Assert.Equal(true, activateActions.First().IsAsync);
                }
            }

            /*
            public StateConfiguration OnDeactivateAsync(Func<Task> deactivateAction, string deactivateActionDescription = null)
            public StateConfiguration OnEntryAsync(Func<Task> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryAsync(Func<Transition, Task> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFromAsync(TTrigger trigger, Func<Task> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFromAsync(TTrigger trigger, Func<Transition, Task> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFromAsync<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, Task> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFromAsync<TArg0>(TriggerWithParameters<TArg0> trigger, Func<TArg0, Transition, Task> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFromAsync<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, Task> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFromAsync<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Func<TArg0, TArg1, Transition, Task> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFromAsync<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, Task> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFromAsync<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Func<TArg0, TArg1, TArg2, Transition, Task> entryAction, string entryActionDescription = null)
            public StateConfiguration OnExitAsync(Func<Task> exitAction, string exitActionDescription = null)
            public StateConfiguration OnExitAsync(Func<Transition, Task> exitAction, string exitActionDescription = null

            public StateConfiguration OnActivate(Action activateAction, string activateActionDescription = null)
            public StateConfiguration OnDeactivate(Action deactivateAction, string deactivateActionDescription = null)
            public StateConfiguration OnEntry(Action entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntry(Action<Transition> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFrom(TTrigger trigger, Action entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFrom(TTrigger trigger, Action<Transition> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFrom<TArg0>(TriggerWithParameters<TArg0> trigger, Action<TArg0> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFrom<TArg0>(TriggerWithParameters<TArg0> trigger, Action<TArg0, Transition> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFrom<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Action<TArg0, TArg1> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFrom<TArg0, TArg1>(TriggerWithParameters<TArg0, TArg1> trigger, Action<TArg0, TArg1, Transition> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFrom<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Action<TArg0, TArg1, TArg2> entryAction, string entryActionDescription = null)
            public StateConfiguration OnEntryFrom<TArg0, TArg1, TArg2>(TriggerWithParameters<TArg0, TArg1, TArg2> trigger, Action<TArg0, TArg1, TArg2, Transition> entryAction, string entryActionDescription = null)
            public StateConfiguration OnExit(Action exitAction, string exitActionDescription = null)
            public StateConfiguration OnExit(Action<Transition> exitAction, string exitActionDescription = null)

            internal TransitionGuard(Tuple<Func<bool>, string>[] guards)
            internal TransitionGuard(Func<bool> guard, string description = null)
            */
        }
}
}
