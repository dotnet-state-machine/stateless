using System.Threading.Tasks;
using Xunit;

namespace Stateless.Tests
{
    public class TransitionFixture
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
    }
}
