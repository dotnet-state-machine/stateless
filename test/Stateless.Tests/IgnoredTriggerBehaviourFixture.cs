using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Stateless.Tests
{
    public class IgnoredTriggerBehaviourFixture
    {
        [Fact]
        public void StateRemainsUnchanged()
        {
            var ignored = new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(Trigger.X, null);
            State destination = State.A;
            Assert.False(ignored.ResultsInTransitionFrom(State.B, new object[0], out destination));
        }

        [Fact]
        public void ExposesCorrectUnderlyingTrigger()
        {
            var ignored = new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(
                Trigger.X, null);

            Assert.Equal(Trigger.X, ignored.Trigger);
        }

        protected bool False()
        {
            return false;
        }

        [Fact]
        public void WhenGuardConditionFalse_IsGuardConditionMetIsFalse()
        {
            var ignored = new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(
                Trigger.X, new StateMachine<State, Trigger>.TransitionGuard(False));

            Assert.False(ignored.GuardConditionsMet);
        }

        protected bool True()
        {
            return true;
        }

        [Fact]
        public void WhenGuardConditionTrue_IsGuardConditionMetIsTrue()
        {
            var ignored = new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(
                Trigger.X, new StateMachine<State, Trigger>.TransitionGuard(True));

            Assert.True(ignored.GuardConditionsMet);
        }
    }
}
