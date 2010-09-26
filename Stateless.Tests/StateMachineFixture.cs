using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Stateless.Tests
{
    [TestFixture]
    public class StateMachineFixture
    {
        const string
            StateA = "A", StateB = "B", StateC = "C",
            TriggerX = "X", TriggerY = "Y";

        [Test]
        public void CanUseReferenceTypeMarkers()
        {
            RunSimpleTest<string, string>(
                new[] { StateA, StateB, StateC },
                new[] { TriggerX, TriggerY });
        }

        [Test]
        public void CanUseValueTypeMarkers()
        {
            RunSimpleTest<State, Trigger>(
                Enum.GetValues(typeof(State)).Cast<State>(),
                Enum.GetValues(typeof(Trigger)).Cast<Trigger>());
        }

        void RunSimpleTest<TState, TTransition>(IEnumerable<TState> states, IEnumerable<TTransition> transitions)
        {
            var a = states.First();
            var b = states.Skip(1).First();
            var x = transitions.First();

            var sm = new StateMachine<TState, TTransition>(a);
            
            sm.Configure(a)
                .Permit(x, b);

            sm.Fire(x);

            Assert.AreEqual(b, sm.State);
        }

        [Test]
        public void InitialStateIsCurrent()
        {
            var initial = State.B;
            var sm = new StateMachine<State, Trigger>(initial);
            Assert.AreEqual(initial, sm.State);
        }

        [Test]
        public void StateCanBeStoredExternally()
        {
            var state = State.B;
            var sm = new StateMachine<State, Trigger>(() => state, s => state = s);
            sm.Configure(State.B).Permit(Trigger.X, State.C);
            Assert.AreEqual(State.B, sm.State);
            Assert.AreEqual(State.B, state);
            sm.Fire(Trigger.X);
            Assert.AreEqual(State.C, sm.State);
            Assert.AreEqual(State.C, state);
        }

        [Test]
        public void SubstateIsIncludedInCurrentState()
        {
            var sm = new StateMachine<State, Trigger>(State.B);
            sm.Configure(State.B).SubstateOf(State.C);

            Assert.AreEqual(State.B, sm.State);
            Assert.IsTrue(sm.IsInState(State.C));
        }

        [Test]
        public void WhenInSubstate_TriggerIgnoredInSuperstate_RemainsInSubstate()
        {
            var sm = new StateMachine<State, Trigger>(State.B);
            
            sm.Configure(State.B)
                .SubstateOf(State.C);

            sm.Configure(State.C)
                .Ignore(Trigger.X);

            sm.Fire(Trigger.X);

            Assert.AreEqual(State.B, sm.State);
        }

        [Test]
        public void PermittedTriggersIncludeSuperstatePermittedTriggers()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.A)
                .Permit(Trigger.Z, State.B);
            
            sm.Configure(State.B)
                .SubstateOf(State.C)
                .Permit(Trigger.X, State.A);

            sm.Configure(State.C)
                .Permit(Trigger.Y, State.A);

            var permitted = sm.PermittedTriggers;

            Assert.IsTrue(permitted.Contains(Trigger.X));
            Assert.IsTrue(permitted.Contains(Trigger.Y));
            Assert.IsFalse(permitted.Contains(Trigger.Z));
        }

        [Test]
        public void PermittedTriggersAreDistinctValues()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .SubstateOf(State.C)
                .Permit(Trigger.X, State.A);

            sm.Configure(State.C)
                .Permit(Trigger.X, State.B);

            var permitted = sm.PermittedTriggers;
            Assert.AreEqual(1, permitted.Count());
            Assert.AreEqual(Trigger.X, permitted.First());
        }

        [Test]
        public void AcceptedTriggersRespectGuards()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .PermitIf(Trigger.X, State.A, () => false);

            Assert.AreEqual(0, sm.PermittedTriggers.Count());
        }

        [Test]
        public void WhenDiscriminatedByGuard_ChoosesPermitedTransition()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .PermitIf(Trigger.X, State.A, () => false)
                .PermitIf(Trigger.X, State.C, () => true);

            sm.Fire(Trigger.X);

            Assert.AreEqual(State.C, sm.State);
        }

        [Test]
        public void WhenTriggerIsIgnored_ActionsNotExecuted()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            bool fired = false;

            sm.Configure(State.B)
                .OnEntry(t => fired = true)
                .Ignore(Trigger.X);

            sm.Fire(Trigger.X);

            Assert.IsFalse(fired);
        }

        [Test]
        public void IfSelfTransitionPermited_ActionsFire()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            bool fired = false;

            sm.Configure(State.B)
                .OnEntry(t => fired = true)
                .PermitReentry(Trigger.X);

            sm.Fire(Trigger.X);

            Assert.IsTrue(fired);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void ImplicitReentryIsDisallowed()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.B)
                .Permit(Trigger.X, State.B);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void TriggerParametersAreImmutableOnceSet()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.SetTriggerParameters<string, int>(Trigger.X);
            sm.SetTriggerParameters<string>(Trigger.X);
        }

        [Test]
        public void ParametersSuppliedToFireArePassedToEntryAction()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            var x = sm.SetTriggerParameters<string, int>(Trigger.X);

            sm.Configure(State.B)
                .Permit(Trigger.X, State.C);

            string entryArgS = null;
            int entryArgI = 0;

            sm.Configure(State.C)
                .OnEntryFrom(x, (s, i) =>
                {
                    entryArgS = s;
                    entryArgI = i;
                });

            var suppliedArgS = "something";
            var suppliedArgI = 42;

            sm.Fire(x, suppliedArgS, suppliedArgI);

            Assert.AreEqual(suppliedArgS, entryArgS);
            Assert.AreEqual(suppliedArgI, entryArgI);
        }

        [Test]
        public void WhenAnUnhandledTriggerIsFired_TheProvidedHandlerIsCalledWithStateAndTrigger()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            State? state = null;
            Trigger? trigger = null;
            sm.OnUnhandledTrigger((s, t) =>
                                      {
                                          state = s;
                                          trigger = t;
                                      });

            sm.Fire(Trigger.Z);

            Assert.AreEqual(State.B, state);
            Assert.AreEqual(Trigger.Z, trigger);
        }
    }
}
