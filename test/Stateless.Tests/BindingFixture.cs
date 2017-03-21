using System;
using NUnit.Framework;
using Stateless.Reflection;

namespace Stateless.Tests
{
    [TestFixture]
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

        [Test]
        public void SimpleTransition_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.AreEqual(inf.StateBindings.Count, 1);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.IsTrue(binding.State.Value is State);
                Assert.IsTrue(binding.State.StateType == typeof(State));
                Assert.AreEqual(State.A, (State)binding.State.Value);
                //
                Assert.AreEqual(0, binding.Substates.Count);
                Assert.AreEqual(null, binding.Superstate);
                Assert.AreEqual(0, binding.EntryActions.Count);
                Assert.AreEqual(0, binding.ExitActions.Count);
                //
                Assert.AreEqual(1, binding.Transitions.Count);
                foreach (TransitionInfo trans in binding.Transitions)
                {
                    Assert.IsTrue(trans.Trigger.Value is Trigger);
                    Assert.AreEqual(trans.Trigger.TriggerType, typeof(Trigger));
                    Assert.AreEqual(Trigger.X, (Trigger)trans.Trigger.Value);
                    //
                    Assert.IsTrue(trans.DestinationState.Value is State);
                    Assert.AreEqual(State.B, (State)trans.DestinationState.Value);
                    Assert.AreEqual(null, trans.GuardDescription);
                }
                Assert.AreEqual(0, binding.IgnoredTriggers.Count);
                Assert.AreEqual(0, binding.InternalTransitions.Count);
                Assert.AreEqual(0, binding.DynamicTransitions.Count);
            }
        }

        [Test]
        public void TwoSimpleTransitions_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B)
                .Permit(Trigger.Y, State.C);

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.AreEqual(inf.StateBindings.Count, 1, "Binding count mismatch");
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.IsTrue(binding.State.Value is State);
                Assert.IsTrue(binding.State.StateType == typeof(State));
                Assert.AreEqual(State.A, (State)binding.State.Value, "Binding state value mismatch");
                //
                Assert.AreEqual(0, binding.Substates.Count, "Binding substate count mismatch");
                Assert.AreEqual(null, binding.Superstate);
                Assert.AreEqual(0, binding.EntryActions.Count, "Binding entry actions count mismatch");
                Assert.AreEqual(0, binding.ExitActions.Count);
                //
                Assert.AreEqual(2, binding.Transitions.Count, "Transition count mismatch");
                //
                bool haveXB = false;
                bool haveYC = false;
                foreach (TransitionInfo trans in binding.Transitions)
                {
                    Assert.IsTrue(trans.Trigger.Value is Trigger);
                    Assert.IsTrue(trans.Trigger.TriggerType == typeof(Trigger));
                    //
                    Assert.IsTrue(trans.DestinationState.Value is State);
                    Assert.AreEqual(null, trans.GuardDescription);
                    //
                    // Can't make assumptions about which trigger/destination comes first in the list
                    if ((Trigger)trans.Trigger.Value == Trigger.X)
                    {
                        Assert.AreEqual(State.B, (State)trans.DestinationState.Value);
                        Assert.IsFalse(haveXB);
                        haveXB = true;
                    }
                    else if ((Trigger)trans.Trigger.Value == Trigger.Y)
                    {
                        Assert.AreEqual(State.C, (State)trans.DestinationState.Value);
                        Assert.IsFalse(haveYC);
                        haveYC = true;
                    }
                    else
                        Assert.Fail();
                }
                Assert.IsTrue(haveXB && haveYC);
                //
                Assert.AreEqual(0, binding.IgnoredTriggers.Count);
                Assert.AreEqual(0, binding.InternalTransitions.Count);
                Assert.AreEqual(0, binding.DynamicTransitions.Count);
            }
        }

        [Test]
        public void WhenDiscriminatedByAnonymousGuard_Binding()
        {
            Func<bool> anonymousGuard = () => true;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, anonymousGuard);

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.AreEqual(1, inf.StateBindings.Count);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.IsTrue(binding.State.Value is State);
                Assert.AreEqual(typeof(State), binding.State.StateType);
                Assert.AreEqual(State.A, (State)binding.State.Value);
                //
                Assert.AreEqual(0, binding.Substates.Count);
                Assert.AreEqual(null, binding.Superstate);
                Assert.AreEqual(0, binding.EntryActions.Count);
                Assert.AreEqual(0, binding.ExitActions.Count);
                //
                Assert.AreEqual(1, binding.Transitions.Count);
                foreach (TransitionInfo trans in binding.Transitions)
                {
                    Assert.IsTrue(trans.Trigger.Value is Trigger);
                    Assert.AreEqual(typeof(Trigger), trans.Trigger.TriggerType);
                    Assert.AreEqual(Trigger.X, (Trigger)trans.Trigger.Value);
                    //
                    Assert.IsTrue(trans.DestinationState.Value is State);
                    Assert.AreEqual(State.B, (State)trans.DestinationState.Value);
                    //
                    Assert.AreNotEqual(null, trans.GuardDescription);
                }
                Assert.AreEqual(0, binding.IgnoredTriggers.Count);
                Assert.AreEqual(0, binding.InternalTransitions.Count);
                Assert.AreEqual(0, binding.DynamicTransitions.Count);
            }
        }

        [Test]
        public void WhenDiscriminatedByAnonymousGuardWithDescription_Binding()
        {
            Func<bool> anonymousGuard = () => true;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, anonymousGuard, "description");

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.AreEqual(1, inf.StateBindings.Count);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.IsTrue(binding.State.Value is State);
                Assert.AreEqual(typeof(State), binding.State.StateType);
                Assert.AreEqual(State.A, (State)binding.State.Value);
                //
                Assert.AreEqual(0, binding.Substates.Count);
                Assert.AreEqual(null, binding.Superstate);
                Assert.AreEqual(0, binding.EntryActions.Count);
                Assert.AreEqual(0, binding.ExitActions.Count);
                //
                Assert.AreEqual(1, binding.Transitions.Count);
                foreach (TransitionInfo trans in binding.Transitions)
                {
                    Assert.IsTrue(trans.Trigger.Value is Trigger);
                    Assert.AreEqual(typeof(Trigger), trans.Trigger.TriggerType);
                    Assert.AreEqual(Trigger.X, (Trigger)trans.Trigger.Value);
                    //
                    Assert.IsTrue(trans.DestinationState.Value is State);
                    Assert.AreEqual(State.B, (State)trans.DestinationState.Value);
                    //
                    Assert.AreNotEqual(null, trans.GuardDescription);
                    Assert.AreEqual("description", trans.GuardDescription);
                }
                Assert.AreEqual(0, binding.IgnoredTriggers.Count);
                Assert.AreEqual(0, binding.InternalTransitions.Count);
                Assert.AreEqual(0, binding.DynamicTransitions.Count);
            }
        }

        [Test]
        public void WhenDiscriminatedByNamedDelegate_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue);

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.AreEqual(1, inf.StateBindings.Count);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.IsTrue(binding.State.Value is State);
                Assert.AreEqual(typeof(State), binding.State.StateType);
                Assert.AreEqual(State.A, (State)binding.State.Value);
                //
                Assert.AreEqual(0, binding.Substates.Count);
                Assert.AreEqual(null, binding.Superstate);
                Assert.AreEqual(0, binding.EntryActions.Count);
                Assert.AreEqual(0, binding.ExitActions.Count);
                //
                Assert.AreEqual(1, binding.Transitions.Count);
                foreach (TransitionInfo trans in binding.Transitions)
                {
                    Assert.IsTrue(trans.Trigger.Value is Trigger);
                    Assert.AreEqual(typeof(Trigger), trans.Trigger.TriggerType);
                    Assert.AreEqual(Trigger.X, (Trigger)trans.Trigger.Value);
                    //
                    Assert.IsTrue(trans.DestinationState.Value is State);
                    Assert.AreEqual(State.B, (State)trans.DestinationState.Value);
                    //
                    Assert.AreNotEqual(null, trans.GuardDescription);
                    Assert.AreEqual("IsTrue", trans.GuardDescription);
                }
                Assert.AreEqual(0, binding.IgnoredTriggers.Count);
                Assert.AreEqual(0, binding.InternalTransitions.Count);
                Assert.AreEqual(0, binding.DynamicTransitions.Count);
            }
        }

        [Test]
        public void WhenDiscriminatedByNamedDelegateWithDescription_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue, "description");

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.AreEqual(1, inf.StateBindings.Count);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.IsTrue(binding.State.Value is State);
                Assert.AreEqual(typeof(State), binding.State.StateType);
                Assert.AreEqual(State.A, (State)binding.State.Value);
                //
                Assert.AreEqual(0, binding.Substates.Count);
                Assert.AreEqual(null, binding.Superstate);
                Assert.AreEqual(0, binding.EntryActions.Count);
                Assert.AreEqual(0, binding.ExitActions.Count);
                //
                Assert.AreEqual(1, binding.Transitions.Count);
                foreach (TransitionInfo trans in binding.Transitions)
                {
                    Assert.IsTrue(trans.Trigger.Value is Trigger);
                    Assert.AreEqual(typeof(Trigger), trans.Trigger.TriggerType);
                    Assert.AreEqual(Trigger.X, (Trigger)trans.Trigger.Value);
                    //
                    Assert.IsTrue(trans.DestinationState.Value is State);
                    Assert.AreEqual(State.B, (State)trans.DestinationState.Value);
                    //
                    Assert.AreNotEqual(null, trans.GuardDescription);
                    Assert.AreEqual("description", trans.GuardDescription);
                }
                Assert.AreEqual(0, binding.IgnoredTriggers.Count);
                Assert.AreEqual(0, binding.InternalTransitions.Count);
                Assert.AreEqual(0, binding.DynamicTransitions.Count);
            }
        }

        [Test]
        public void DestinationStateIsDynamic_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .PermitDynamic(Trigger.X, () => State.B);

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.AreEqual(1, inf.StateBindings.Count);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.IsTrue(binding.State.Value is State);
                Assert.AreEqual(typeof(State), binding.State.StateType);
                Assert.AreEqual(State.A, (State)binding.State.Value);
                //
                Assert.AreEqual(0, binding.Substates.Count);
                Assert.AreEqual(null, binding.Superstate);
                Assert.AreEqual(0, binding.EntryActions.Count);
                Assert.AreEqual(0, binding.ExitActions.Count);
                //
                Assert.AreEqual(0, binding.Transitions.Count, "Binding transition count mismatch");
                Assert.AreEqual(0, binding.IgnoredTriggers.Count);
                Assert.AreEqual(0, binding.InternalTransitions.Count, "Internal transition count mismatch");
                Assert.AreEqual(1, binding.DynamicTransitions.Count, "Dynamic transition count mismatch");
                foreach (DynamicTransitionInfo trans in binding.DynamicTransitions)
                {
                    Assert.IsTrue(trans.Trigger.Value is Trigger);
                    Assert.AreEqual(typeof(Trigger), trans.Trigger.TriggerType);
                    Assert.AreEqual(Trigger.X, (Trigger)trans.Trigger.Value);
                    Assert.AreNotEqual(null, trans.Destination);
                    Assert.AreEqual(null, trans.GuardDescription);
                }
            }
        }

        [Test]
        public void DestinationStateIsCalculatedBasedOnTriggerParameters_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitDynamic(trigger, i => i == 1 ? State.B : State.C);

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.AreEqual(1, inf.StateBindings.Count);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.IsTrue(binding.State.Value is State);
                Assert.AreEqual(typeof(State), binding.State.StateType);
                Assert.AreEqual(State.A, (State)binding.State.Value);
                //
                Assert.AreEqual(0, binding.Substates.Count);
                Assert.AreEqual(null, binding.Superstate);
                Assert.AreEqual(0, binding.EntryActions.Count);
                Assert.AreEqual(0, binding.ExitActions.Count);
                //
                Assert.AreEqual(0, binding.Transitions.Count, "Binding transition count mismatch");
                Assert.AreEqual(0, binding.IgnoredTriggers.Count);
                Assert.AreEqual(0, binding.InternalTransitions.Count, "Internal transition count mismatch");
                Assert.AreEqual(1, binding.DynamicTransitions.Count, "Dynamic transition count mismatch");
                foreach (DynamicTransitionInfo trans in binding.DynamicTransitions)
                {
                    Assert.IsTrue(trans.Trigger.Value is Trigger);
                    Assert.AreEqual(typeof(Trigger), trans.Trigger.TriggerType);
                    Assert.AreEqual(Trigger.X, (Trigger)trans.Trigger.Value);
                    Assert.AreNotEqual(null, trans.Destination);
                    Assert.AreEqual(null, trans.GuardDescription);
                }
            }
        }

        [Test]
        public void OnEntryWithAnonymousActionAndDescription_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => { }, "enteredA");

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.AreEqual(1, inf.StateBindings.Count);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.IsTrue(binding.State.Value is State);
                Assert.AreEqual(typeof(State), binding.State.StateType);
                Assert.AreEqual(State.A, (State)binding.State.Value);
                //
                Assert.AreEqual(0, binding.Substates.Count);
                Assert.AreEqual(null, binding.Superstate);
                Assert.AreEqual(1, binding.EntryActions.Count);
                foreach (string entryAction in binding.EntryActions)
                {
                    Assert.AreEqual("enteredA", entryAction);
                }
                Assert.AreEqual(0, binding.ExitActions.Count);
                //
                Assert.AreEqual(0, binding.Transitions.Count, "Binding transition count mismatch");
                Assert.AreEqual(0, binding.IgnoredTriggers.Count);
                Assert.AreEqual(0, binding.InternalTransitions.Count, "Internal transition count mismatch");
                Assert.AreEqual(0, binding.DynamicTransitions.Count, "Dynamic transition count mismatch");
            }
        }

        [Test]
        public void OnEntryWithNamedDelegateActionAndDescription_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(OnEntry, "enteredA");

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.AreEqual(1, inf.StateBindings.Count);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.IsTrue(binding.State.Value is State);
                Assert.AreEqual(typeof(State), binding.State.StateType);
                Assert.AreEqual(State.A, (State)binding.State.Value);
                //
                Assert.AreEqual(0, binding.Substates.Count);
                Assert.AreEqual(null, binding.Superstate);
                //
                Assert.AreEqual(1, binding.EntryActions.Count);
                foreach (string entryAction in binding.EntryActions)
                    Assert.AreEqual("enteredA", entryAction);
                Assert.AreEqual(0, binding.ExitActions.Count);
                //
                Assert.AreEqual(0, binding.Transitions.Count, "Binding transition count mismatch");
                Assert.AreEqual(0, binding.IgnoredTriggers.Count);
                Assert.AreEqual(0, binding.InternalTransitions.Count, "Internal transition count mismatch");
                Assert.AreEqual(0, binding.DynamicTransitions.Count, "Dynamic transition count mismatch");
            }
        }

        [Test]
        public void OnExitWithAnonymousActionAndDescription_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(() => { }, "exitA");

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.AreEqual(1, inf.StateBindings.Count);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.IsTrue(binding.State.Value is State);
                Assert.AreEqual(typeof(State), binding.State.StateType);
                Assert.AreEqual(State.A, (State)binding.State.Value);
                //
                Assert.AreEqual(0, binding.Substates.Count);
                Assert.AreEqual(null, binding.Superstate);
                //
                Assert.AreEqual(0, binding.EntryActions.Count);
                Assert.AreEqual(1, binding.ExitActions.Count);
                foreach (string entryAction in binding.ExitActions)
                    Assert.AreEqual("exitA", entryAction);
                //
                Assert.AreEqual(0, binding.Transitions.Count, "Binding transition count mismatch");
                Assert.AreEqual(0, binding.IgnoredTriggers.Count);
                Assert.AreEqual(0, binding.InternalTransitions.Count, "Internal transition count mismatch");
                Assert.AreEqual(0, binding.DynamicTransitions.Count, "Dynamic transition count mismatch");
            }
        }

        [Test]
        public void OnExitWithNamedDelegateActionAndDescription_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(OnExit, "exitA");

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.AreEqual(1, inf.StateBindings.Count);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.IsTrue(binding.State.Value is State);
                Assert.AreEqual(typeof(State), binding.State.StateType);
                Assert.AreEqual(State.A, (State)binding.State.Value);
                //
                Assert.AreEqual(0, binding.Substates.Count);
                Assert.AreEqual(null, binding.Superstate);
                //
                Assert.AreEqual(0, binding.EntryActions.Count);
                Assert.AreEqual(1, binding.ExitActions.Count);
                foreach (string entryAction in binding.ExitActions)
                    Assert.AreEqual("exitA", entryAction);
                //
                Assert.AreEqual(0, binding.Transitions.Count, "Binding transition count mismatch");
                Assert.AreEqual(0, binding.IgnoredTriggers.Count);
                Assert.AreEqual(0, binding.InternalTransitions.Count, "Internal transition count mismatch");
                Assert.AreEqual(0, binding.DynamicTransitions.Count, "Dynamic transition count mismatch");
            }
        }

        [Test]
        public void TransitionWithIgnore_Binding()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Ignore(Trigger.Y)
                .Permit(Trigger.X, State.B);

            StateMachineInfo inf = sm.GetStateMachineInfo();

            // Since only state A is configured, it's the only one that
            // shows up in StateBindings.
            Assert.AreEqual(1, inf.StateBindings.Count);
            foreach (StateBindingInfo binding in inf.StateBindings)
            {
                Assert.IsTrue(binding.State.Value is State);
                Assert.AreEqual(typeof(State), binding.State.StateType);
                Assert.AreEqual(State.A, (State)binding.State.Value);
                //
                Assert.AreEqual(1, binding.Transitions.Count, "Transition count mismatch");
                foreach (TransitionInfo trans in binding.Transitions)
                {
                    Assert.IsTrue(trans.Trigger.Value is Trigger);
                    Assert.AreEqual(trans.Trigger.TriggerType, typeof(Trigger));
                    Assert.AreEqual(Trigger.X, (Trigger)trans.Trigger.Value);
                    //
                    Assert.IsTrue(trans.DestinationState.Value is State);
                    Assert.AreEqual(State.B, (State)trans.DestinationState.Value);
                    Assert.AreEqual(null, trans.GuardDescription);
                }
                //
                Assert.AreEqual(1, binding.IgnoredTriggers.Count, "Ignored triggers count mismatch");
                foreach (TriggerInfo ignore in binding.IgnoredTriggers)
                {
                    Assert.IsTrue(ignore.Value is Trigger);
                    Assert.AreEqual(typeof(Trigger), ignore.TriggerType);
                    Assert.AreEqual(Trigger.Y, (Trigger)ignore.Value, "Ignored trigger value mismatch");
                }
                //
                Assert.AreEqual(0, binding.Substates.Count);
                Assert.AreEqual(null, binding.Superstate);
                Assert.AreEqual(0, binding.EntryActions.Count);
                Assert.AreEqual(0, binding.ExitActions.Count);
                Assert.AreEqual(0, binding.InternalTransitions.Count, "Internal transition count mismatch");
                Assert.AreEqual(0, binding.DynamicTransitions.Count, "Dynamic transition count mismatch");
            }
        }
    }
}
