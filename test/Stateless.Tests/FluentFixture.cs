
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

            var trans = sm.Configure(State.A).Transition(Trigger.X);
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
        public void Fire_Transition_To_EndsUpInAnotherState()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Transition(Trigger.X)
                .To(State.B);

            sm.Fire(Trigger.X);

            Assert.Equal(State.B, sm.State);
        }

        [Fact]
        public void Fire_Transition_To_Self()
        {
            bool _entered = false;
            bool _exited = false;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => _entered = true)
                .OnExit(() => _exited = true)
                .Transition(Trigger.X).Self();

            sm.Fire(Trigger.X);

            Assert.Equal(State.A, sm.State);
            Assert.True(_entered);
            Assert.True(_exited);
        }

        [Fact]
        public void Fire_Transition_Internal()
        {
            bool _entered = false;
            bool _exited = false;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => _entered = true)
                .OnExit(() => _exited = true)
                .Transition(Trigger.X).Internal();

            sm.Fire(Trigger.X);

            Assert.Equal(State.A, sm.State);
            Assert.False(_entered);
            Assert.False(_exited);
        }
    }
}
