using System;
using System.Collections.Generic;
using System.Linq;
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
            RunSimpleTest(
                new[] { StateA, StateB, StateC },
                new[] { TriggerX, TriggerY });
        }

        [Test]
        public void CanUseValueTypeMarkers()
        {
            RunSimpleTest(
                Enum.GetValues(typeof(State)).Cast<State>(),
                Enum.GetValues(typeof(Trigger)).Cast<Trigger>());
        }

        void RunSimpleTest<TState, TTransition>(IEnumerable<TState> states, IEnumerable<TTransition> transitions)
        {
            var a = states.First();
            var b = states.Skip(1).First();
            var x = transitions.First();

            var sm = StateMachine<TState, TTransition>.Create(a);

            sm.Configure(a)
                .Permit(x, b);

            var csm = sm.FinishConfiguration();

            csm.Fire(x);

            Assert.AreEqual(b, csm.State);
        }

        [Test]
        public void InitialStateIsCurrent()
        {
            var initial = State.B;
            var sm = StateMachine<State, Trigger>.Create(initial);

            var csm = sm.FinishConfiguration();
            
            Assert.AreEqual(initial, csm.State);
        }

        [Test]
        public void StateCanBeStoredExternally()
        {
            var state = State.B;
            var sm = StateMachine<State, Trigger>.Create(() => state, s => state = s);
            sm.Configure(State.B).Permit(Trigger.X, State.C);


            var csm = sm.FinishConfiguration();


            Assert.AreEqual(State.B, csm.State);
            Assert.AreEqual(State.B, state);
            csm.Fire(Trigger.X);
            Assert.AreEqual(State.C, csm.State);
            Assert.AreEqual(State.C, state);
        }

        [Test]
        public void SubstateIsIncludedInCurrentState()
        {
            var sm = StateMachine<State, Trigger>.Create(State.B);
            sm.Configure(State.B).SubstateOf(State.C);

            var csm = sm.FinishConfiguration();


            Assert.AreEqual(State.B, csm.State);
            Assert.IsTrue(csm.IsInState(State.C));
        }

        [Test]
        public void WhenInSubstate_TriggerIgnoredInSuperstate_RemainsInSubstate()
        {
            var sm = StateMachine<State, Trigger>.Create(State.B);

            sm.Configure(State.B)
                .SubstateOf(State.C);

            sm.Configure(State.C)
                .Ignore(Trigger.X);

            var csm = sm.FinishConfiguration();

            csm.Fire(Trigger.X);

            Assert.AreEqual(State.B, csm.State);
        }

        [Test]
        public void PermittedTriggersIncludeSuperstatePermittedTriggers()
        {
            var sm = StateMachine<State, Trigger>.Create(State.B);

            sm.Configure(State.A)
                .Permit(Trigger.Z, State.B);

            sm.Configure(State.B)
                .SubstateOf(State.C)
                .Permit(Trigger.X, State.A);

            sm.Configure(State.C)
                .Permit(Trigger.Y, State.A);

            var csm = sm.FinishConfiguration();

            var permitted = csm.PermittedTriggers;

            Assert.IsTrue(permitted.Contains(Trigger.X));
            Assert.IsTrue(permitted.Contains(Trigger.Y));
            Assert.IsFalse(permitted.Contains(Trigger.Z));
        }

        [Test]
        public void PermittedTriggersAreDistinctValues()
        {
            var sm = StateMachine<State, Trigger>.Create(State.B);

            sm.Configure(State.B)
                .SubstateOf(State.C)
                .Permit(Trigger.X, State.A);

            sm.Configure(State.C)
                .Permit(Trigger.X, State.B);

            var csm = sm.FinishConfiguration();

            var permitted = csm.PermittedTriggers;
            Assert.AreEqual(1, permitted.Count());
            Assert.AreEqual(Trigger.X, permitted.First());
        }

        [Test]
        public void AcceptedTriggersRespectGuards()
        {
            var sm = StateMachine<State, Trigger>.Create(State.B);

            sm.Configure(State.B)
                .PermitIf(Trigger.X, State.A, () => false);

            var csm = sm.FinishConfiguration();

            Assert.AreEqual(0, csm.PermittedTriggers.Count());
        }

        [Test]
        public void WhenDiscriminatedByGuard_ChoosesPermitedTransition()
        {
            var sm = StateMachine<State, Trigger>.Create(State.B);

            sm.Configure(State.B)
                .PermitIf(Trigger.X, State.A, () => false)
                .PermitIf(Trigger.X, State.C, () => true);

            var csm = sm.FinishConfiguration();

            csm.Fire(Trigger.X);

            Assert.AreEqual(State.C, csm.State);
        }

        [Test]
        public void WhenTriggerIsIgnored_ActionsNotExecuted()
        {
            var sm = StateMachine<State, Trigger>.Create(State.B);

            bool fired = false;

            sm.Configure(State.B)
                .OnEntry(t => fired = true)
                .Ignore(Trigger.X);

            var csm = sm.FinishConfiguration();

            csm.Fire(Trigger.X);

            Assert.IsFalse(fired);
        }

        [Test]
        public void IfSelfTransitionPermited_ActionsFire()
        {
            var sm = StateMachine<State, Trigger>.Create(State.B);

            bool fired = false;

            sm.Configure(State.B)
                .OnEntry(t => fired = true)
                .PermitReentry(Trigger.X);

            var csm = sm.FinishConfiguration();

            csm.Fire(Trigger.X);

            Assert.IsTrue(fired);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void ImplicitReentryIsDisallowed()
        {
            var sm = StateMachine<State, Trigger>.Create(State.B);

            sm.Configure(State.B)
                .Permit(Trigger.X, State.B);
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void TriggerParametersAreImmutableOnceSet()
        {
            var sm = StateMachine<State, Trigger>.Create(State.B);

            sm.SetTriggerParameters<string, int>(Trigger.X);
            sm.SetTriggerParameters<string>(Trigger.X);
        }

        [Test]
        public void ParametersSuppliedToFireArePassedToEntryAction()
        {
            var sm = StateMachine<State, Trigger>.Create(State.B);

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

            var csm = sm.FinishConfiguration();

            var suppliedArgS = "something";
            var suppliedArgI = 42;

            csm.Fire(x, suppliedArgS, suppliedArgI);

            Assert.AreEqual(suppliedArgS, entryArgS);
            Assert.AreEqual(suppliedArgI, entryArgI);
        }

        [Test]
        public void WhenAnUnhandledTriggerIsFired_TheProvidedHandlerIsCalledWithStateAndTrigger()
        {
            var sm = StateMachine<State, Trigger>.Create(State.B);

            State? state = null;
            Trigger? trigger = null;
            sm.OnUnhandledTrigger((s, t) =>
                                      {
                                          state = s;
                                          trigger = t;
                                      });

            var csm = sm.FinishConfiguration();

            csm.Fire(Trigger.Z);

            Assert.AreEqual(State.B, state);
            Assert.AreEqual(Trigger.Z, trigger);
        }

        [Test]
        public void WhenATransitionOccurs_TheOnTransitionEventFires()
        {
            var sm = StateMachine<State, Trigger>.Create(State.B);

            sm.Configure(State.B)
                .Permit(Trigger.X, State.A);

            StateMachine<State, Trigger>.Transition transition = null;
            sm.OnTransitioned(t => transition = t);

            var csm = sm.FinishConfiguration();

            csm.Fire(Trigger.X);

            Assert.IsNotNull(transition);
            Assert.AreEqual(Trigger.X, transition.Trigger);
            Assert.AreEqual(State.B, transition.Source);
            Assert.AreEqual(State.A, transition.Destination);
        }

        [Test]
        public void TheOnTransitionEventFiresBeforeTheOnEntryEvent()
        {
            var sm = StateMachine<State, Trigger>.Create(State.B);
            var expectedOrdering = new List<string> { "OnExit", "OnTransitioned", "OnEntry" };
            var actualOrdering = new List<string>();

            sm.Configure(State.B)
                .Permit(Trigger.X, State.A)
                .OnExit(() => actualOrdering.Add("OnExit"));

            sm.Configure(State.A)
                .OnEntry(() => actualOrdering.Add("OnEntry"));

            sm.OnTransitioned(t => actualOrdering.Add("OnTransitioned"));

            var csm = sm.FinishConfiguration();

            csm.Fire(Trigger.X);

            Assert.AreEqual(expectedOrdering.Count, actualOrdering.Count);
            for (int i = 0; i < expectedOrdering.Count; i++)
            {
                Assert.AreEqual(expectedOrdering[i], actualOrdering[i]);
            }
        }
    }
}
