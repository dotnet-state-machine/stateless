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
            Assert.IsFalse(ignored.ResultsInTransitionFrom(State.B, new object[0], out destination));
        }

        [Test]
        public void ExposesCorrectUnderlyingTrigger()
        {
            var ignored = new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(
                Trigger.X, () => true);

            Assert.AreEqual(Trigger.X, ignored.Trigger);
        }

        [Test]
        public void WhenGuardConditionFalse_IsGuardConditionMetIsFalse()
        {
            var ignored = new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(
                Trigger.X, () => false);

            Assert.IsFalse(ignored.IsGuardConditionMet);
        }

        [Test]
        public void WhenGuardConditionTrue_IsGuardConditionMetIsTrue()
        {
            var ignored = new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(
                Trigger.X, () => true);

            Assert.IsTrue(ignored.IsGuardConditionMet);
        }
    }
}
