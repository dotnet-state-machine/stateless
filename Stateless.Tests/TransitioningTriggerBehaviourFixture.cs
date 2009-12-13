using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Stateless.Tests
{
    [TestFixture]
    public class TransitioningTriggerBehaviourFixture
    {
        [Test]
        public void TransitionsToDestinationState()
        {
            var transtioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(Trigger.X, State.C, () => true);
            State destination;
            Assert.IsTrue(transtioning.ResultsInTransitionFrom(State.B, new object[0], out destination));
            Assert.AreEqual(State.C, destination);
        }
    }
}
