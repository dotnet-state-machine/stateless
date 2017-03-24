using System;
using Xunit;
using Stateless.Reflection;
using Xunit.Sdk;

namespace Stateless.Tests
{
    public class BindingFixture
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

        [Fact]
        public void SimpleTransition_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.Equal(inf.StateBindings.Count, 1);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.True(binding.State.Value is State);
                Assert.True(binding.State.StateType == typeof(State));
                Assert.Equal(State.A, (State)binding.State.Value);
                //
                Assert.Equal(0, binding.Substates.Count);
                Assert.Equal(null, binding.Superstate);
                Assert.Equal(0, binding.EntryActions.Count);
                Assert.Equal(0, binding.ExitActions.Count);
                //
                Assert.Equal(1, binding.Transitions.Count);
                foreach (TransitionInfo trans in binding.Transitions)
                {
                    Assert.True(trans.Trigger.Value is Trigger);
                    Assert.Equal(trans.Trigger.TriggerType, typeof(Trigger));
                    Assert.Equal(Trigger.X, (Trigger)trans.Trigger.Value);
                    //
                    Assert.True(trans.DestinationState.Value is State);
                    Assert.Equal(State.B, (State)trans.DestinationState.Value);
                    Assert.Equal(null, trans.GuardDescription);
                }
                Assert.Equal(0, binding.IgnoredTriggers.Count);
                Assert.Equal(0, binding.InternalTransitions.Count);
                Assert.Equal(0, binding.DynamicTransitions.Count);
            }
        }

        [Fact]
        public void TwoSimpleTransitions_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B)
                .Permit(Trigger.Y, State.C);

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.Equal(inf.StateBindings.Count, 1); // Binding count mismatch"
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.True(binding.State.Value is State);
                Assert.True(binding.State.StateType == typeof(State));
                Assert.Equal(State.A, (State)binding.State.Value); // Binding state value mismatch
                //
                Assert.Equal(0, binding.Substates.Count); //  Binding substate count mismatch"
                Assert.Equal(null, binding.Superstate);
                Assert.Equal(0, binding.EntryActions.Count); //  Binding entry actions count mismatch
                Assert.Equal(0, binding.ExitActions.Count);
                //
                Assert.Equal(2, binding.Transitions.Count); // Transition count mismatch
                //
                bool haveXB = false;
                bool haveYC = false;
                foreach (TransitionInfo trans in binding.Transitions)
                {
                    Assert.True(trans.Trigger.Value is Trigger);
                    Assert.True(trans.Trigger.TriggerType == typeof(Trigger));
                    //
                    Assert.True(trans.DestinationState.Value is State);
                    Assert.Equal(null, trans.GuardDescription);
                    //
                    // Can't make assumptions about which trigger/destination comes first in the list
                    if ((Trigger)trans.Trigger.Value == Trigger.X)
                    {
                        Assert.Equal(State.B, (State)trans.DestinationState.Value);
                        Assert.False(haveXB);
                        haveXB = true;
                    }
                    else if ((Trigger)trans.Trigger.Value == Trigger.Y)
                    {
                        Assert.Equal(State.C, (State)trans.DestinationState.Value);
                        Assert.False(haveYC);
                        haveYC = true;
                    }
                    else
                        throw new XunitException("Failed.");
                }
                Assert.True(haveXB && haveYC);
                //
                Assert.Equal(0, binding.IgnoredTriggers.Count);
                Assert.Equal(0, binding.InternalTransitions.Count);
                Assert.Equal(0, binding.DynamicTransitions.Count);
            }
        }

        [Fact]
        public void WhenDiscriminatedByAnonymousGuard_Binding()
        {
            Func<bool> anonymousGuard = () => true;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, anonymousGuard);

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.Equal(1, inf.StateBindings.Count);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.True(binding.State.Value is State);
                Assert.Equal(typeof(State), binding.State.StateType);
                Assert.Equal(State.A, (State)binding.State.Value);
                //
                Assert.Equal(0, binding.Substates.Count);
                Assert.Equal(null, binding.Superstate);
                Assert.Equal(0, binding.EntryActions.Count);
                Assert.Equal(0, binding.ExitActions.Count);
                //
                Assert.Equal(1, binding.Transitions.Count);
                foreach (TransitionInfo trans in binding.Transitions)
                {
                    Assert.True(trans.Trigger.Value is Trigger);
                    Assert.Equal(typeof(Trigger), trans.Trigger.TriggerType);
                    Assert.Equal(Trigger.X, (Trigger)trans.Trigger.Value);
                    //
                    Assert.True(trans.DestinationState.Value is State);
                    Assert.Equal(State.B, (State)trans.DestinationState.Value);
                    //
                    Assert.NotEqual(null, trans.GuardDescription);
                }
                Assert.Equal(0, binding.IgnoredTriggers.Count);
                Assert.Equal(0, binding.InternalTransitions.Count);
                Assert.Equal(0, binding.DynamicTransitions.Count);
            }
        }

        [Fact]
        public void WhenDiscriminatedByAnonymousGuardWithDescription_Binding()
        {
            Func<bool> anonymousGuard = () => true;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, anonymousGuard, "description");

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.Equal(1, inf.StateBindings.Count);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.True(binding.State.Value is State);
                Assert.Equal(typeof(State), binding.State.StateType);
                Assert.Equal(State.A, (State)binding.State.Value);
                //
                Assert.Equal(0, binding.Substates.Count);
                Assert.Equal(null, binding.Superstate);
                Assert.Equal(0, binding.EntryActions.Count);
                Assert.Equal(0, binding.ExitActions.Count);
                //
                Assert.Equal(1, binding.Transitions.Count);
                foreach (TransitionInfo trans in binding.Transitions)
                {
                    Assert.True(trans.Trigger.Value is Trigger);
                    Assert.Equal(typeof(Trigger), trans.Trigger.TriggerType);
                    Assert.Equal(Trigger.X, (Trigger)trans.Trigger.Value);
                    //
                    Assert.True(trans.DestinationState.Value is State);
                    Assert.Equal(State.B, (State)trans.DestinationState.Value);
                    //
                    Assert.NotEqual(null, trans.GuardDescription);
                    Assert.Equal("description", trans.GuardDescription);
                }
                Assert.Equal(0, binding.IgnoredTriggers.Count);
                Assert.Equal(0, binding.InternalTransitions.Count);
                Assert.Equal(0, binding.DynamicTransitions.Count);
            }
        }

        [Fact]
        public void WhenDiscriminatedByNamedDelegate_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue);

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.Equal(1, inf.StateBindings.Count);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.True(binding.State.Value is State);
                Assert.Equal(typeof(State), binding.State.StateType);
                Assert.Equal(State.A, (State)binding.State.Value);
                //
                Assert.Equal(0, binding.Substates.Count);
                Assert.Equal(null, binding.Superstate);
                Assert.Equal(0, binding.EntryActions.Count);
                Assert.Equal(0, binding.ExitActions.Count);
                //
                Assert.Equal(1, binding.Transitions.Count);
                foreach (TransitionInfo trans in binding.Transitions)
                {
                    Assert.True(trans.Trigger.Value is Trigger);
                    Assert.Equal(typeof(Trigger), trans.Trigger.TriggerType);
                    Assert.Equal(Trigger.X, (Trigger)trans.Trigger.Value);
                    //
                    Assert.True(trans.DestinationState.Value is State);
                    Assert.Equal(State.B, (State)trans.DestinationState.Value);
                    //
                    Assert.NotEqual(null, trans.GuardDescription);
                    Assert.Equal("IsTrue", trans.GuardDescription);
                }
                Assert.Equal(0, binding.IgnoredTriggers.Count);
                Assert.Equal(0, binding.InternalTransitions.Count);
                Assert.Equal(0, binding.DynamicTransitions.Count);
            }
        }

        [Fact]
        public void WhenDiscriminatedByNamedDelegateWithDescription_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue, "description");

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.Equal(1, inf.StateBindings.Count);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.True(binding.State.Value is State);
                Assert.Equal(typeof(State), binding.State.StateType);
                Assert.Equal(State.A, (State)binding.State.Value);
                //
                Assert.Equal(0, binding.Substates.Count);
                Assert.Equal(null, binding.Superstate);
                Assert.Equal(0, binding.EntryActions.Count);
                Assert.Equal(0, binding.ExitActions.Count);
                //
                Assert.Equal(1, binding.Transitions.Count);
                foreach (TransitionInfo trans in binding.Transitions)
                {
                    Assert.True(trans.Trigger.Value is Trigger);
                    Assert.Equal(typeof(Trigger), trans.Trigger.TriggerType);
                    Assert.Equal(Trigger.X, (Trigger)trans.Trigger.Value);
                    //
                    Assert.True(trans.DestinationState.Value is State);
                    Assert.Equal(State.B, (State)trans.DestinationState.Value);
                    //
                    Assert.NotEqual(null, trans.GuardDescription);
                    Assert.Equal("description", trans.GuardDescription);
                }
                Assert.Equal(0, binding.IgnoredTriggers.Count);
                Assert.Equal(0, binding.InternalTransitions.Count);
                Assert.Equal(0, binding.DynamicTransitions.Count);
            }
        }

        [Fact]
        public void DestinationStateIsDynamic_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .PermitDynamic(Trigger.X, () => State.B);

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.Equal(1, inf.StateBindings.Count);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.True(binding.State.Value is State);
                Assert.Equal(typeof(State), binding.State.StateType);
                Assert.Equal(State.A, (State)binding.State.Value);
                //
                Assert.Equal(0, binding.Substates.Count);
                Assert.Equal(null, binding.Superstate);
                Assert.Equal(0, binding.EntryActions.Count);
                Assert.Equal(0, binding.ExitActions.Count);
                //
                Assert.Equal(0, binding.Transitions.Count); // Binding transition count mismatch
                Assert.Equal(0, binding.IgnoredTriggers.Count);
                Assert.Equal(0, binding.InternalTransitions.Count); // Internal transition count mismatch
                Assert.Equal(1, binding.DynamicTransitions.Count); // Dynamic transition count mismatch
                foreach (DynamicTransitionInfo trans in binding.DynamicTransitions)
                {
                    Assert.True(trans.Trigger.Value is Trigger);
                    Assert.Equal(typeof(Trigger), trans.Trigger.TriggerType);
                    Assert.Equal(Trigger.X, (Trigger)trans.Trigger.Value);
                    Assert.NotEqual(null, trans.Destination);
                    Assert.Equal(null, trans.GuardDescription);
                }
            }
        }

        [Fact]
        public void DestinationStateIsCalculatedBasedOnTriggerParameters_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitDynamic(trigger, i => i == 1 ? State.B : State.C);

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.Equal(1, inf.StateBindings.Count);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.True(binding.State.Value is State);
                Assert.Equal(typeof(State), binding.State.StateType);
                Assert.Equal(State.A, (State)binding.State.Value);
                //
                Assert.Equal(0, binding.Substates.Count);
                Assert.Equal(null, binding.Superstate);
                Assert.Equal(0, binding.EntryActions.Count);
                Assert.Equal(0, binding.ExitActions.Count);
                //
                Assert.Equal(0, binding.Transitions.Count); // Binding transition count mismatch"
                Assert.Equal(0, binding.IgnoredTriggers.Count);
                Assert.Equal(0, binding.InternalTransitions.Count); // Internal transition count mismatch
                Assert.Equal(1, binding.DynamicTransitions.Count); // Dynamic transition count mismatch
                foreach (DynamicTransitionInfo trans in binding.DynamicTransitions)
                {
                    Assert.True(trans.Trigger.Value is Trigger);
                    Assert.Equal(typeof(Trigger), trans.Trigger.TriggerType);
                    Assert.Equal(Trigger.X, (Trigger)trans.Trigger.Value);
                    Assert.NotEqual(null, trans.Destination);
                    Assert.Equal(null, trans.GuardDescription);
                }
            }
        }

        [Fact]
        public void OnEntryWithAnonymousActionAndDescription_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => { }, "enteredA");

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.Equal(1, inf.StateBindings.Count);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.True(binding.State.Value is State);
                Assert.Equal(typeof(State), binding.State.StateType);
                Assert.Equal(State.A, (State)binding.State.Value);
                //
                Assert.Equal(0, binding.Substates.Count);
                Assert.Equal(null, binding.Superstate);
                Assert.Equal(1, binding.EntryActions.Count);
                foreach (string entryAction in binding.EntryActions)
                {
                    Assert.Equal("enteredA", entryAction);
                }
                Assert.Equal(0, binding.ExitActions.Count);
                //
                Assert.Equal(0, binding.Transitions.Count); // Binding count mismatch
                Assert.Equal(0, binding.IgnoredTriggers.Count);
                Assert.Equal(0, binding.InternalTransitions.Count); // Internal transition count mismatch
                Assert.Equal(0, binding.DynamicTransitions.Count); // Dynamic transition count mismatch
            }
        }

        [Fact]
        public void OnEntryWithNamedDelegateActionAndDescription_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(OnEntry, "enteredA");

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.Equal(1, inf.StateBindings.Count);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.True(binding.State.Value is State);
                Assert.Equal(typeof(State), binding.State.StateType);
                Assert.Equal(State.A, (State)binding.State.Value);
                //
                Assert.Equal(0, binding.Substates.Count);
                Assert.Equal(null, binding.Superstate);
                //
                Assert.Equal(1, binding.EntryActions.Count);
                foreach (string entryAction in binding.EntryActions)
                    Assert.Equal("enteredA", entryAction);
                Assert.Equal(0, binding.ExitActions.Count);
                //
                Assert.Equal(0, binding.Transitions.Count); // Binding count mismatch
                Assert.Equal(0, binding.IgnoredTriggers.Count);
                Assert.Equal(0, binding.InternalTransitions.Count); // Internal transition count mismatch
                Assert.Equal(0, binding.DynamicTransitions.Count); // Dynamic transition count mismatch
            }
        }

        [Fact]
        public void OnExitWithAnonymousActionAndDescription_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(() => { }, "exitA");

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.Equal(1, inf.StateBindings.Count);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.True(binding.State.Value is State);
                Assert.Equal(typeof(State), binding.State.StateType);
                Assert.Equal(State.A, (State)binding.State.Value);
                //
                Assert.Equal(0, binding.Substates.Count);
                Assert.Equal(null, binding.Superstate);
                //
                Assert.Equal(0, binding.EntryActions.Count);
                Assert.Equal(1, binding.ExitActions.Count);
                foreach (string entryAction in binding.ExitActions)
                    Assert.Equal("exitA", entryAction);
                //
                Assert.Equal(0, binding.Transitions.Count); // Binding count mismatch
                Assert.Equal(0, binding.IgnoredTriggers.Count);
                Assert.Equal(0, binding.InternalTransitions.Count); // Internal transition count mismatch
                Assert.Equal(0, binding.DynamicTransitions.Count); // Dynamic transition count mismatch
            }
        }

        [Fact]
        public void OnExitWithNamedDelegateActionAndDescription_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(OnExit, "exitA");

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.Equal(1, inf.StateBindings.Count);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.True(binding.State.Value is State);
                Assert.Equal(typeof(State), binding.State.StateType);
                Assert.Equal(State.A, (State)binding.State.Value);
                //
                Assert.Equal(0, binding.Substates.Count);
                Assert.Equal(null, binding.Superstate);
                //
                Assert.Equal(0, binding.EntryActions.Count);
                Assert.Equal(1, binding.ExitActions.Count);
                foreach (string entryAction in binding.ExitActions)
                    Assert.Equal("exitA", entryAction);
                //
                Assert.Equal(0, binding.Transitions.Count); // Binding count mismatch
                Assert.Equal(0, binding.IgnoredTriggers.Count);
                Assert.Equal(0, binding.InternalTransitions.Count); // Internal transition count mismatch
                Assert.Equal(0, binding.DynamicTransitions.Count); // Dynamic transition count mismatch
            }
        }

        [Fact]
        public void TransitionWithIgnore_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Ignore(Trigger.Y)
                .Permit(Trigger.X, State.B);

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.Equal(1, inf.StateBindings.Count);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.True(binding.State.Value is State);
                Assert.Equal(typeof(State), binding.State.StateType);
                Assert.Equal(State.A, (State)binding.State.Value);
                //
                Assert.Equal(1, binding.Transitions.Count); // Transition count mismatch"
                foreach (TransitionInfo trans in binding.Transitions)
                {
                    Assert.True(trans.Trigger.Value is Trigger);
                    Assert.Equal(trans.Trigger.TriggerType, typeof(Trigger));
                    Assert.Equal(Trigger.X, (Trigger)trans.Trigger.Value);
                    //
                    Assert.True(trans.DestinationState.Value is State);
                    Assert.Equal(State.B, (State)trans.DestinationState.Value);
                    Assert.Equal(null, trans.GuardDescription);
                }
                //
                Assert.Equal(1, binding.IgnoredTriggers.Count); //  Ignored triggers count mismatch
                foreach (TriggerInfo ignore in binding.IgnoredTriggers)
                {
                    Assert.True(ignore.Value is Trigger);
                    Assert.Equal(typeof(Trigger), ignore.TriggerType);
                    Assert.Equal(Trigger.Y, (Trigger)ignore.Value); // Ignored trigger value mismatch
                }
                //
                Assert.Equal(0, binding.Substates.Count);
                Assert.Equal(null, binding.Superstate);
                Assert.Equal(0, binding.EntryActions.Count);
                Assert.Equal(0, binding.ExitActions.Count);
                Assert.Equal(0, binding.InternalTransitions.Count); // Internal transition count mismatch
                Assert.Equal(0, binding.DynamicTransitions.Count); // Dynamic transition count mismatch
            }
        }
    }
}
