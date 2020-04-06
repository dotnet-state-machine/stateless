
using Xunit;
using Stateless;
using System;

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
            bool _entered = false;
            bool _exited = false;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(() => _exited = true)
                .Transition(Trigger.X)
                .To(State.B);

            sm.Configure(State.B)
                .OnEntry(() => _entered = true);

            sm.Fire(Trigger.X);

            Assert.Equal(State.B, sm.State);
            Assert.True(_entered);
            Assert.True(_exited);
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

        [Fact]
        public void Fire_Transition_To_If_True_EndsUpInAnotherState()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Transition(Trigger.X)
                .To(State.B)
                .If((t) => SomethingTrue(t));

            sm.Configure(State.B);

            sm.Fire(Trigger.X);

            Assert.Equal(State.B, sm.State);
        }

        [Fact]
        public void Fire_Transition_To_If_False_StayInState()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Transition(Trigger.X)
                .To(State.B)
                .If((t) => SomethingFalse(t), "guard description");

            sm.Configure(State.B);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(Trigger.X));

            Assert.Equal(State.A, sm.State);
        }

        [Fact]
        public void Fire_Transition_Self_If_True_Self()
        {
            bool _entered = false;
            bool _exited = false;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => _entered = true)
                .OnExit(() => _exited = true)
                .Transition(Trigger.X).Self().If((t) => SomethingTrue(t));

            sm.Fire(Trigger.X);

            Assert.Equal(State.A, sm.State);
            Assert.True(_entered);
            Assert.True(_exited);
        }

        [Fact]
        public void Fire_Transition_Self_If_False()
        {
            bool _entered = false;
            bool _exited = false;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => _entered = true)
                .OnExit(() => _exited = true)
                .Transition(Trigger.X).Self().If((t) => SomethingFalse(t));

            Assert.Throws<InvalidOperationException>(() => sm.Fire(Trigger.X));
            Assert.False(_entered);
            Assert.False(_exited);
        }

        [Fact]
        public void Fire_Transition_Internal_If_True()
        {
            bool _entered = false;
            bool _exited = false;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => _entered = true)
                .OnExit(() => _exited = true)
                .Transition(Trigger.X).Internal().If((t) => SomethingTrue(t));

            sm.Fire(Trigger.X);

            Assert.Equal(State.A, sm.State);
            Assert.False(_entered);
            Assert.False(_exited);
        }

        [Fact]
        public void Fire_Transition_Internal_If_False()
        {
            bool _entered = false;
            bool _exited = false;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => _entered = true)
                .OnExit(() => _exited = true)
                .Transition(Trigger.X).Internal().If((t) => SomethingFalse(t));

            Assert.Throws<InvalidOperationException>(() => sm.Fire(Trigger.X));

            Assert.Equal(State.A, sm.State);
            Assert.False(_entered);
            Assert.False(_exited);
        }

        private bool SomethingTrue(object _)
        {
            return true;
        }

        private bool SomethingFalse(object _)
        {
            return false;
        }
    }
}
