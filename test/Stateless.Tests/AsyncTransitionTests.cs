using System.Threading.Tasks;
using Xunit;

namespace Stateless.Tests
{
    public class AsyncTransitionTests
    {
        [Fact]
        public async Task GivenTriggerHandledOnSuperStateAndSubState_WhenTriggerFiredAsync_ThenShouldUseSubstateTransitionAsync()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm
                .Configure(State.A)
                .Permit(Trigger.X, State.B);

            sm
                .Configure(State.B)
                .SubstateOf(State.A)
                .Permit(Trigger.X, State.C);

            await sm.FireAsync(Trigger.X);
            Assert.Equal(State.B, sm.State);

            await sm.FireAsync(Trigger.X);
            Assert.Equal(State.C, sm.State);
        }

        [Fact]
        public async Task GivenTriggerHandledOnSuperStateAndSubState_WhenSubstateTransitionGuardBlocked_ThenShouldUseSuperstateTransitionAsync()
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

            await sm.FireAsync(Trigger.X);
            Assert.Equal(State.D, sm.State);
        }

        [Fact]
        public async Task GivenTriggerHandledOnSuperStateAndSubState_WhenSubstateTransitionGuardIsOpen_ThenShouldUseSubstateTransitionAsync()
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

            await sm.FireAsync(Trigger.X);
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
        public async Task GivenMultiLayerSubstates_AndGuardConditionIsClosed_OpenTransitionOnClosestAncestorIsUsedAsync(
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

            await sm.FireAsync(Trigger.X);
            Assert.Equal(expectedState, sm.State);
        }
    }
}
