using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Stateless.Tests
{
    [TestFixture]
    public class TriggerBehaviourFixture
    {
        [Test]
        public void ExposesCorrectUnderlyingTrigger()
        {
            var transtioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
                Trigger.X, State.C, () => true);

            Assert.AreEqual(Trigger.X, transtioning.Trigger);
        }

        [Test]
        public void WhenGuardConditionFalse_IsGuardConditionsMetIsFalse()
        {
            var transtioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
                Trigger.X, State.C, () => false);

            Assert.IsFalse(transtioning.GuardConditionsMet);
        }

        [Test]
        public void WhenGuardConditionTrue_IsGuardConditionsMetIsTrue()
        {
            var transtioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
                Trigger.X, State.C, () => true);

            Assert.IsTrue(transtioning.GuardConditionsMet);
        }

        [Test]
        public void WhenOneOfMultipleGuardConditionsFalse_IsGuardConditionsMetIsFalse()
        {
            var falseGuard = new[] {
                new Tuple<Func<bool>, string>(() => true, "1"),
                new Tuple<Func<bool>, string>(() => false, "2")
            };

            var transtioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
                Trigger.X, State.C, new StateMachine<State, Trigger>.TransitionGuard(falseGuard));

            Assert.IsFalse(transtioning.GuardConditionsMet);
        }
        [Test]
        public void WhenAllMultipleGuardConditionsTrue_IsGuardConditionsMetIsTrue()
        {
            var falseGuard = new[] {
                new Tuple<Func<bool>, string>(() => true, "1"),
                new Tuple<Func<bool>, string>(() => true, "2")
            };

            var transtioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
                Trigger.X, State.C, new StateMachine<State, Trigger>.TransitionGuard(falseGuard));

            Assert.IsTrue(transtioning.GuardConditionsMet);
        }

        [Test]
        public void WhenAllMultipleGuardConditionsFalse_IsGuardConditionsMetIsFalse()
        {
            var falseGuard = new[] {
                new Tuple<Func<bool>, string>(() => false, "1"),
                new Tuple<Func<bool>, string>(() => false, "2")
            };

            var transtioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
                Trigger.X, State.C, new StateMachine<State, Trigger>.TransitionGuard(falseGuard));

            Assert.IsFalse(transtioning.GuardConditionsMet);
        }
    }
}
