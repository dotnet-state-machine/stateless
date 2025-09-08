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
        [InlineData(false, false, true, "D")]
        [InlineData(false, true, false, "E")]
        [InlineData(false, true, true, "D")]
        [InlineData(true, false, false, "F")]
        [InlineData(true, false, true, "D")]
        [InlineData(true, true, false, "E")]
        [InlineData(true, true, true, "D")]
        public async Task GivenMultiLayerSubstates_AndGuardConditionIsClosed_OpenTransitionOnClosestAncestorIsUsedAsync(
            bool parentGuardConditionValue,
            bool childGuardConditionValue,
            bool grandchildGuardConditionValue,
            string expectedState
        )
        {
            var sm = new StateMachine<string, Trigger>("C");

            sm
                .Configure("A")
                .PermitIf(Trigger.X, "F", () => parentGuardConditionValue);

            sm
                .Configure("B")
                .SubstateOf("A")
                .PermitIf(Trigger.X, "E", () => childGuardConditionValue);

            sm
                .Configure("C")
                .SubstateOf("B")
                .PermitIf(Trigger.X, "D", () => grandchildGuardConditionValue);

            await sm.FireAsync(Trigger.X);
            Assert.Equal(expectedState, sm.State);
        }
    }
}
