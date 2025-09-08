using Xunit;

namespace Stateless.Tests
{
    public class TransitionTests
    {
        [Fact]
        public void IsReentry_ShouldBeTrue_WhenSourceAndDestinationAreEqual()
        {
            StateMachine<int, int>.Transition t = new StateMachine<int, int>.Transition(1, 1, 0);
            Assert.True(t.IsReentry);
        }

        [Fact]
        public void IsReentry_ShouldBeFalse_WhenSourceAndDestinationDiffer()
        {
            StateMachine<int, int>.Transition t = new StateMachine<int, int>.Transition(1, 2, 0);
            Assert.False(t.IsReentry);
        }

        [Fact]
        public void InternalTransitionIf_ShouldExecuteOnlyFirstMatchingAction()
        {
            // Verifies that only one internal action is executed
            var machine = new StateMachine<int, int>(1);

            machine.Configure(1)
                .InternalTransitionIf(
                    1,
                    t => { return true; },
                    () =>
                    {
                        Assert.True(true);
                    })
                .InternalTransitionIf(
                    1,
                    u => { return false; },
                    () =>
                    {
                        Assert.True(false);
                    });

            machine.Fire(1);
        }

        [Fact]
        public void GivenTriggerHandledOnSuperStateAndSubState_WhenTriggerFiredSync_ThenShouldUseSubstateTransition()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm
                .Configure(State.A)
                .Permit(Trigger.X, State.B);

            // Overrides the superstate transition
            sm
                .Configure(State.B)
                .SubstateOf(State.A)
                .Permit(Trigger.X, State.C);

            sm.Fire(Trigger.X);
            Assert.Equal(State.B, sm.State);

            sm.Fire(Trigger.X);
            Assert.Equal(State.C, sm.State); // WORKS!
        }

        [Fact]
        public void GivenTriggerHandledOnSuperStateAndSubState_WhenSubstateTransitionGuardBlocked_ThenShouldUseSuperstateTransition()
        {
            var guardConditionValue = false;
            var sm = new StateMachine<State, Trigger>(State.B);

            sm
                .Configure(State.A)
                .PermitIf(Trigger.X, State.D);

            sm
                .Configure(State.B)
                .SubstateOf(State.A)
                .PermitIf(Trigger.X, State.C, () => guardConditionValue);

            sm.Fire(Trigger.X);
            Assert.Equal(State.D, sm.State);
        }

        [Fact]
        public void GivenTriggerHandledOnSuperStateAndSubState_WhenSubstateTransitionGuardOpen_ThenShouldUseSubstateTransition()
        {
            var guardConditionValue = true;
            var sm = new StateMachine<State, Trigger>(State.B);

            sm
                .Configure(State.A)
                .PermitIf(Trigger.X, State.D);

            sm
                .Configure(State.B)
                .SubstateOf(State.A)
                .PermitIf(Trigger.X, State.C, () => guardConditionValue);

            sm.Fire(Trigger.X);
            Assert.Equal(State.C, sm.State);
        }

        [Theory]
        [InlineData(false, false, true, "GrandchildStateTarget")]
        [InlineData(false, true, false, "ChildStateTarget")]
        [InlineData(false, true, true, "GrandchildStateTarget")]
        [InlineData(true, false, false, "ParentStateTarget")]
        [InlineData(true, false, true, "GrandchildStateTarget")]
        [InlineData(true, true, false, "ChildStateTarget")]
        [InlineData(true, true, true, "GrandchildStateTarget")]
        public void GivenMultiLayerSubstates_AndGuardConditionIsClosed_OpenTransitionOnClosestAncestorIsUsed(
            bool parentGuardConditionValue,
            bool childGuardConditionValue,
            bool grandchildGuardConditionValue,
            string expectedState
        )
        {
            var sm = new StateMachine<string, Trigger>("GrandchildState");

            sm
                .Configure("ParentState")
                .PermitIf(Trigger.X, "ParentStateTarget", () => parentGuardConditionValue);

            sm
                .Configure("ChildState")
                .SubstateOf("ParentState")
                .PermitIf(Trigger.X, "ChildStateTarget", () => childGuardConditionValue);

            sm
                .Configure("GrandchildState")
                .SubstateOf("ChildState")
                .PermitIf(Trigger.X, "GrandchildStateTarget", () => grandchildGuardConditionValue);

            sm.Fire(Trigger.X);
            Assert.Equal(expectedState, sm.State);
        }
    }
}
