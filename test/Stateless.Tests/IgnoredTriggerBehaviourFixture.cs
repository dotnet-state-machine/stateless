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
            var ignored = new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(Trigger.X, () => true);
            State destination = State.A;
            Assert.False(ignored.ResultsInTransitionFrom(State.B, new object[0], out destination));
        }

        [Fact]
        public void ExposesCorrectUnderlyingTrigger()
        {
            var ignored = new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(
                Trigger.X, () => true);

            Assert.Equal(Trigger.X, ignored.Trigger);
        }

        [Fact]
        public void WhenGuardConditionFalse_IsGuardConditionMetIsFalse()
        {
            var ignored = new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(
                Trigger.X, () => false);

            Assert.False(ignored.GuardConditionsMet);
        }

        [Fact]
        public void WhenGuardConditionTrue_IsGuardConditionMetIsTrue()
        {
            var ignored = new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(
                Trigger.X, () => true);

            Assert.True(ignored.GuardConditionsMet);
        }
    }
}
