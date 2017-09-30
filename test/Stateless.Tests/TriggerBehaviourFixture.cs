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
            var transitioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
                Trigger.X, State.C, null);

            Assert.Equal(Trigger.X, transitioning.Trigger);
        }

        protected bool False()
        {
            return false;
        }

        [Fact]
        public void WhenGuardConditionFalse_GuardConditionsMetIsFalse()
        {
            var transitioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
                Trigger.X, State.C, new StateMachine<State, Trigger>.TransitionGuard(False));

            Assert.False(transitioning.GuardConditionsMet);
        }

        protected bool True()
        {
            return true;
        }

        [Fact]
        public void WhenGuardConditionTrue_GuardConditionsMetIsTrue()
        {
            var transitioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
                Trigger.X, State.C, new StateMachine<State, Trigger>.TransitionGuard(True));

            Assert.True(transitioning.GuardConditionsMet);
        }

        [Fact]
        public void WhenOneOfMultipleGuardConditionsFalse_GuardConditionsMetIsFalse()
        {
            var falseGuard = new[] {
                new Tuple<Func<bool>, string>(() => true, "1"),
                new Tuple<Func<bool>, string>(() => true, "2")
            };

            var transitioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
                Trigger.X, State.C, new StateMachine<State, Trigger>.TransitionGuard(falseGuard));

            Assert.True(transitioning.GuardConditionsMet);
        }

        [Fact]
        public void WhenAllMultipleGuardConditionsFalse_IsGuardConditionsMetIsFalse()
        {
            var falseGuard = new[] {
                new Tuple<Func<bool>, string>(() => false, "1"),
                new Tuple<Func<bool>, string>(() => false, "2")
            };

            var transitioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
                Trigger.X, State.C, new StateMachine<State, Trigger>.TransitionGuard(falseGuard));

            Assert.False(transitioning.GuardConditionsMet);
        }

        [Fact]
        public void WhenAllGuardConditionsTrue_GuardConditionsMetIsTrue()
        {
            var trueGuard = new[] {
                new Tuple<Func<bool>, string>(() => true, "1"),
                new Tuple<Func<bool>, string>(() => true, "2")
            };

            var transitioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
                Trigger.X, State.C, new StateMachine<State, Trigger>.TransitionGuard(trueGuard));

            Assert.True(transitioning.GuardConditionsMet);
        }
    }
}
