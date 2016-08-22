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
        public void StayInSameStateOneState()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .InternalTransition(Trigger.X, (t) => { });

            Assert.AreEqual(State.A, sm.State);
            sm.Fire(Trigger.X);
            Assert.AreEqual(State.A, sm.State);
        }

        [Test]
        public void StayInSameStateTwoStates()
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
        public void StayInSameSubStateTransitionInSupersate()
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
        public void StayInSameSubStateTransitionInSubstate()
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
    }
}
