using System.Linq;
using NUnit.Framework;

namespace Stateless.Tests
{
    [TestFixture]
    class InternalTransitionFixture
    {

        /// <summary>
        /// The expected behaviour of the internal transistion is that the state does not change.
        /// This will fail if the state changes after the trigger has fired.
        /// </summary>
        [Test]
        public void StayInSameStateOneState_Transition()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .InternalTransition(Trigger.X, (t) => { });

            Assert.AreEqual(State.A, sm.State);
            sm.Fire(Trigger.X);
            Assert.AreEqual(State.A, sm.State);
        }

        [Test]
        public void StayInSameStateTwoStates_Transition()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .InternalTransition(Trigger.X, (t) => { })
                .Permit(Trigger.Y, State.B);

            sm.Configure(State.B)
                    .InternalTransition(Trigger.X, (t) => { })
                    .Permit(Trigger.Y, State.A);

            // This should not cause any state changes
            Assert.AreEqual(State.A, sm.State);
            sm.Fire(Trigger.X);
            Assert.AreEqual(State.A, sm.State);

            // Change state to B
            sm.Fire(Trigger.Y);

            // This should also not cause any state changes
            Assert.AreEqual(State.B, sm.State);
            sm.Fire(Trigger.X);
            Assert.AreEqual(State.B, sm.State);
        }
        [Test]
        public void StayInSameSubStateTransitionInSuperstate_Transition()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.A)
                    .InternalTransition(Trigger.X, (t) => { });

            sm.Configure(State.B)
                    .SubstateOf(State.A);

            // This should not cause any state changes
            Assert.AreEqual(State.B, sm.State);
            sm.Fire(Trigger.X);
            Assert.AreEqual(State.B, sm.State);
        }
        [Test]
        public void StayInSameSubStateTransitionInSubstate_Transition()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.A);

            sm.Configure(State.B)
                    .SubstateOf(State.A)
                    .InternalTransition(Trigger.X, (t) => { });

            // This should not cause any state changes
            Assert.AreEqual(State.B, sm.State);
            sm.Fire(Trigger.X);
            Assert.AreEqual(State.B, sm.State);
        }

        [Test]
        public void StayInSameStateOneState_Action()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .InternalTransition(Trigger.X, () => { });

            Assert.AreEqual(State.A, sm.State);
            sm.Fire(Trigger.X);
            Assert.AreEqual(State.A, sm.State);
        }

        [Test]
        public void StayInSameStateTwoStates_Action()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .InternalTransition(Trigger.X, () => { })
                .Permit(Trigger.Y, State.B);

            sm.Configure(State.B)
                    .InternalTransition(Trigger.X, () => { })
                    .Permit(Trigger.Y, State.A);

            // This should not cause any state changes
            Assert.AreEqual(State.A, sm.State);
            sm.Fire(Trigger.X);
            Assert.AreEqual(State.A, sm.State);

            // Change state to B
            sm.Fire(Trigger.Y);

            // This should also not cause any state changes
            Assert.AreEqual(State.B, sm.State);
            sm.Fire(Trigger.X);
            Assert.AreEqual(State.B, sm.State);
        }
        [Test]
        public void StayInSameSubStateTransitionInSuperstate_Action()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.A)
                    .InternalTransition(Trigger.X, () => { })
                    .InternalTransition(Trigger.Y, () => { });

            sm.Configure(State.B)
                    .SubstateOf(State.A);

            // This should not cause any state changes
            Assert.AreEqual(State.B, sm.State);
            sm.Fire(Trigger.X);
            Assert.AreEqual(State.B, sm.State);
            sm.Fire(Trigger.Y);
            Assert.AreEqual(State.B, sm.State);
        }
        [Test]
        public void StayInSameSubStateTransitionInSubstate_Action()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.A);

            sm.Configure(State.B)
                    .SubstateOf(State.A)
                    .InternalTransition(Trigger.X, () => { })
                    .InternalTransition(Trigger.Y, () => { });

            // This should not cause any state changes
            Assert.AreEqual(State.B, sm.State);
            sm.Fire(Trigger.X);
            Assert.AreEqual(State.B, sm.State);
            sm.Fire(Trigger.Y);
            Assert.AreEqual(State.B, sm.State);
        }

        [Test]
        public void AllowTriggerWithTwoParameters()
        {
            var sm = new StateMachine<State, Trigger>(State.B);
            var trigger = sm.SetTriggerParameters<int, string>(Trigger.X);
            const int intParam = 5;
            const string strParam = "Five";
            var callbackInvoked = false;

            sm.Configure(State.B)
                .InternalTransition(trigger, (i, s, transition) =>
                {
                    callbackInvoked = true;
                    Assert.That(i, Is.EqualTo(intParam));
                    Assert.That(s, Is.EqualTo(strParam));
                });

            sm.Fire(trigger, intParam, strParam);
            Assert.That(callbackInvoked, Is.True);
        }

        [Test]
        public void AllowTriggerWithThreeParameters()
        {
            var sm = new StateMachine<State, Trigger>(State.B);
            var trigger = sm.SetTriggerParameters<int, string, bool>(Trigger.X);
            const int intParam = 5;
            const string strParam = "Five";
            var boolParam = true;
            var callbackInvoked = false;

            sm.Configure(State.B)
                .InternalTransition(trigger, (i, s, b, transition) =>
                {
                    callbackInvoked = true;
                    Assert.That(i, Is.EqualTo(intParam));
                    Assert.That(s, Is.EqualTo(strParam));
                    Assert.That(b, Is.EqualTo(boolParam));
                });

            sm.Fire(trigger, intParam, strParam, boolParam);
            Assert.That(callbackInvoked, Is.True);
        }

        [Test]
        public void ConditionalInternalTransition_ShouldBeReflectedInPermittedTriggers()
        {
            var isPermitted = true;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .InternalTransitionIf(Trigger.X, () => isPermitted, t => { });

            Assert.That(sm.PermittedTriggers.ToArray().Length, Is.EqualTo(1));
            isPermitted = false;
            Assert.That(sm.PermittedTriggers.ToArray().Length, Is.EqualTo(0));
        }
    }
}
