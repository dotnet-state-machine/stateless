using System;
using Xunit;

namespace Stateless.Tests
{
    public class IgnoredTriggerBehaviourFixture
    {
        [Fact]
        public void StateRemainsUnchanged()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A).Ignore(Trigger.X);
            
            sm.Fire(Trigger.X);
            
            Assert.Equal(State.A, sm.State);
        }

        [Fact]
        public void ExposesCorrectUnderlyingTrigger()
        {
            var ignored = new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(
                Trigger.X, null);

            Assert.Equal(Trigger.X, ignored.Trigger);
        }

        private bool False(params object[] args)
        {
            return false;
        }

        [Fact]
        public void WhenGuardConditionFalse_IsGuardConditionMetIsFalse()
        {
            var ignored = new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(
                Trigger.X, new StateMachine<State, Trigger>.TransitionGuard(False));

            Assert.False(ignored.GuardConditionsMet());
        }

        private bool True(params object[] args)
        {
            return true;
        }

        [Fact]
        public void WhenGuardConditionTrue_IsGuardConditionMetIsTrue()
        {
            var ignored = new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(
                Trigger.X, new StateMachine<State, Trigger>.TransitionGuard(True));

            Assert.True(ignored.GuardConditionsMet());
        }
        
        [Fact]
        public void IgnoredTriggerMustBeIgnoredSync()
        {
            bool internalActionExecuted = false;
            var stateMachine = new StateMachine<State, Trigger>(State.B);
            stateMachine.Configure(State.A)
                .Permit(Trigger.X, State.C);

            stateMachine.Configure(State.B)
                .SubstateOf(State.A)
                .Ignore(Trigger.X);

            try
            {
                // >>> The following statement should not execute the internal action
                stateMachine.Fire(Trigger.X);
            }
            catch (NullReferenceException)
            {
                internalActionExecuted = true;
            }

            Assert.False(internalActionExecuted);
        }

        [Fact]
        public void IgnoreIfTrueTriggerMustBeIgnored()
        {
            var stateMachine = new StateMachine<State, Trigger>(State.B);
            stateMachine.Configure(State.A)
                .Permit(Trigger.X, State.C);

            stateMachine.Configure(State.B)
                .SubstateOf(State.A)
                .IgnoreIf(Trigger.X, () => true);

                stateMachine.Fire(Trigger.X);

            Assert.Equal(State.B, stateMachine.State);
        }
        [Fact]
        public void IgnoreIfFalseTriggerMustNotBeIgnored()
        {
            var stateMachine = new StateMachine<State, Trigger>(State.B);
            stateMachine.Configure(State.A)
                .Permit(Trigger.X, State.C);

            stateMachine.Configure(State.B)
                .SubstateOf(State.A)
                .IgnoreIf(Trigger.X, () => false);

            stateMachine.Fire(Trigger.X);

            Assert.Equal(State.C, stateMachine.State);
        }
    }
}
