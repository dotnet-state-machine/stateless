
using Xunit;
using Stateless;

namespace Stateless.Tests
{
    public class FluentFixture
    {
        [Fact]
        public void Transition_Returns_TransitionConfiguration()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var trans =  sm.Configure(State.A).Transition(Trigger.X);
            Assert.NotNull(trans);
            Assert.IsType<StateMachine<State, Trigger>.TransitionConfiguration>(trans);
        }

        [Fact]
        public void Transition_To_Returns_DestinationConfiguration()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var trans = sm.Configure(State.A).Transition(Trigger.X).To(State.B);
            Assert.NotNull(trans);
            Assert.IsType<StateMachine<State, Trigger>.DestinationConfiguration>(trans);
        }

        [Fact]
        public void Fire_Transition_To_EndsUpInExpectedState()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A).Transition(Trigger.X).To(State.B);

            sm.Fire(Trigger.X);

            Assert.Equal(State.B, sm.State);
        }

    }
}
