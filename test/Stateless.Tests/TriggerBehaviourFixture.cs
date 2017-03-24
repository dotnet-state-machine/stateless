using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Stateless.Tests
{
    public class TriggerBehaviourFixture
    {
        [Fact]
        public void ExposesCorrectUnderlyingTrigger()
        {
            var transtioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
                Trigger.X, State.C, () => true);

            Assert.Equal(Trigger.X, transtioning.Trigger);
        }

        [Fact]
        public void WhenGuardConditionFalse_GuardConditionsMetIsFalse()
        {
            var transtioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
                Trigger.X, State.C, () => false);

            Assert.False(transtioning.GuardConditionsMet);
        }

        [Fact]
        public void WhenGuardConditionTrue_GuardConditionsMetIsTrue()
        {
            var transtioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
                Trigger.X, State.C, () => true);

            Assert.True(transtioning.GuardConditionsMet);
        }

        [Fact]
        public void WhenOneOfMultipleGuardConditionsFalse_GuardConditionsMetIsFalse()
        {
            var falseGuard = new[] {
                new Tuple<Func<bool>, string>(() => true, "1"),
                new Tuple<Func<bool>, string>(() => true, "2")
            };

            var transtioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
                Trigger.X, State.C, new StateMachine<State, Trigger>.TransitionGuard(falseGuard));

            Assert.True(transtioning.GuardConditionsMet);
        }

        [Fact]
        public void WhenAllMultipleGuardConditionsFalse_IsGuardConditionsMetIsFalse()
        {
            var falseGuard = new[] {
                new Tuple<Func<bool>, string>(() => false, "1"),
                new Tuple<Func<bool>, string>(() => false, "2")
            };

            var transtioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
                Trigger.X, State.C, new StateMachine<State, Trigger>.TransitionGuard(falseGuard));

            Assert.False(transtioning.GuardConditionsMet);
        }

        [Fact]
        public void WhenAllGuardConditionsTrue_GuardConditionsMetIsTrue()
        {
            var trueGuard = new[] {
                new Tuple<Func<bool>, string>(() => true, "1"),
                new Tuple<Func<bool>, string>(() => true, "2")
            };

            var transtioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
                Trigger.X, State.C, new StateMachine<State, Trigger>.TransitionGuard(trueGuard));

            Assert.True(transtioning.GuardConditionsMet);
        }
    }
}
