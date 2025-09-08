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
    }
}
