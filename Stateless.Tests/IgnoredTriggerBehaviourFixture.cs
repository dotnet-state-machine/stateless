using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Stateless.Tests
{
    [TestFixture]
    public class IgnoredTriggerBehaviourFixture
    {
        [Test]
        public void StateRemainsUnchanged()
        {
            var ignored = new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(Trigger.X, () => true);
            State destination = State.A;
            Assert.IsFalse(ignored.ResultsInTransitionFrom(State.B, out destination));
        }
    }
}
