using System;
using System.Diagnostics;
using Stateless.Graph;
using Xunit;

namespace Stateless.Tests
{
    public class InitialTransitionFixture
    {
        [Fact]
        public void EntersSubState()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A).Permit(Trigger.X, State.B);

            sm.Configure(State.B)
                .InitialTransition(State.C);

            sm.Configure(State.C)
                .SubstateOf(State.B);

            sm.Fire(Trigger.X);
            Assert.Equal(State.C, sm.State);
        }
        [Fact]
        public void EntersSubStateOfSubstate()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            sm.Configure(State.B)
                .InitialTransition(State.C);

            sm.Configure(State.C)
                .InitialTransition(State.D)
                .SubstateOf(State.B);

            sm.Configure(State.D)
                .SubstateOf(State.C);

            sm.Fire(Trigger.X);
            Assert.Equal(State.D, sm.State);
        }


        [Fact]
        public void SubStateOfSubstateOnEntryCountAndOrder() 
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var onEntryCount = "";

            sm.Configure(State.A)
                .OnEntry(() => onEntryCount += "A")
                .Permit(Trigger.X, State.B);

            sm.Configure(State.B)
                .OnEntry(() => onEntryCount += "B")
                .InitialTransition(State.C);

            sm.Configure(State.C)
                .OnEntry(() => onEntryCount += "C")
                .InitialTransition(State.D)
                .SubstateOf(State.B);

            sm.Configure(State.D)
                .OnEntry(() => onEntryCount += "D")
                .SubstateOf(State.C);

            sm.Fire(Trigger.X);
            Assert.Equal("BCD", onEntryCount);
        }

        [Fact]
        public void DoesNotEnterSubStateofSubstate()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A).Permit(Trigger.X, State.B);

            sm.Configure(State.B);

            sm.Configure(State.C)
                .InitialTransition(State.D)
                .SubstateOf(State.B);

            sm.Configure(State.D)
                .SubstateOf(State.C);

            sm.Fire(Trigger.X);
            Assert.Equal(State.B, sm.State);
        }
        [Fact]
        public async void EntersSubStateAsync()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A).Permit(Trigger.X, State.B);

            sm.Configure(State.B)
                .InitialTransition(State.C);

            sm.Configure(State.C)
                .SubstateOf(State.B);

            await sm.FireAsync(Trigger.X);
            Assert.Equal(State.C, sm.State);
        }
        [Fact]
        public async void EntersSubStateofSubstateAsync()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A).Permit(Trigger.X, State.B);

            sm.Configure(State.B)
                .InitialTransition(State.C);

            sm.Configure(State.C)
                .InitialTransition(State.D)
                .SubstateOf(State.B);

            sm.Configure(State.D)
                .SubstateOf(State.C);

            await sm.FireAsync(Trigger.X);
            Assert.Equal(State.D, sm.State);
        }
        [Fact]
        public async void DoesNotEnterSubStateofSubstateAsync()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A).Permit(Trigger.X, State.B);

            sm.Configure(State.B);

            sm.Configure(State.C)
                .InitialTransition(State.D)
                .SubstateOf(State.B);

            sm.Configure(State.D)
                .SubstateOf(State.C);

            await sm.FireAsync(Trigger.X);
            Assert.Equal(State.B, sm.State);
        }
        [Fact]
        public void DoNotAllowTransitionToSelf()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            Assert.Throws(typeof(ArgumentException), () =>
                // This configuration would create an infinite loop
                sm.Configure(State.A)
                    .InitialTransition(State.A) );
        }

        [Fact]
        public void DoNotAllowTransitionToAnotherSuperstate()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A).Permit(Trigger.X, State.B);

            sm.Configure(State.B)
                .InitialTransition(State.A); // Invalid configuration, State a is a superstate

            Assert.Throws(typeof(InvalidOperationException), () =>
                sm.Fire(Trigger.X) );
        }
        [Fact]
        public async void DoNotAllowTransitionToAnotherSuperstateAsync()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A).Permit(Trigger.X, State.B);

            sm.Configure(State.B)
                .InitialTransition(State.A);

            await Assert.ThrowsAsync (typeof(InvalidOperationException), async () =>
                await sm.FireAsync(Trigger.X) );
        }

        [Fact]
        public void DoNotAllowMoreThanOneInitialTransition()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A).Permit(Trigger.X, State.B);

            sm.Configure(State.B)
                .InitialTransition(State.C);

            Assert.Throws(typeof(InvalidOperationException), () =>
                sm.Configure(State.B)
                .InitialTransition(State.A) );

        }
    }
}
