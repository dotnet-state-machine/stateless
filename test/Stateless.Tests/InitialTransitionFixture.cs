using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public void EntersSubStateofSubstate()
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

            sm.Fire(Trigger.X);
            Assert.Equal(State.D, sm.State);
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
                    .InitialTransition(State.A));
        }

        [Fact]
        public void DoNotAllowTransitionToAnotherSuperstate()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A).Permit(Trigger.X, State.B);

            sm.Configure(State.B)
                .InitialTransition(State.A); // Invalid configuration, State a is a superstate

            Assert.Throws(typeof(InvalidOperationException), () =>
                sm.Fire(Trigger.X));
        }

        [Fact]
        public async void DoNotAllowTransitionToAnotherSuperstateAsync()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A).Permit(Trigger.X, State.B);

            sm.Configure(State.B)
                .InitialTransition(State.A);

            await Assert.ThrowsAsync(typeof(InvalidOperationException), async () =>
               await sm.FireAsync(Trigger.X));
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
                .InitialTransition(State.A));
        }

        [Fact]
        public void Transition_with_reentry_Test()
        {
            //   -------------------------
            //   | A                     |---\
            //   |        ---------      |    X (PermitReentry)
            //   |   o--->| B     |      |<--/         
            //   |        ---------      |          
            //   |                    *  |
            //   -------------------------          
            //                 
            // X: Exit A => Enter A => Enter B

            var sm = new StateMachine<State, Trigger>(State.A); //never triggers any action!

            int order = 0;

            int onEntryStateAfired = 0;
            int onEntryStateBfired = 0;
            int onExitStateAfired = 0;
            int onExitStateBfired = 0;

            sm.Configure(State.A)
                .InitialTransition(State.B)
                .OnEntry(t => onEntryStateAfired = ++order)
                .OnExit(t => onExitStateAfired = ++order)
                .PermitReentry(Trigger.X);

            sm.Configure(State.B)
                .SubstateOf(State.A)
                .OnEntry(t => onEntryStateBfired = ++order)
                .OnExit(t => onExitStateBfired = ++order);

            sm.Fire(Trigger.X);

            Assert.Equal(State.B, sm.State);
            Assert.Equal(0, onExitStateBfired);
            Assert.Equal(1, onExitStateAfired);
            Assert.Equal(2, onEntryStateAfired);
            Assert.Equal(3, onEntryStateBfired);
        }

        [Fact]
        public void VerifyNotEnterSuperstateWhenDoingInitialTransition()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            sm.Configure(State.B)
                .InitialTransition(State.C)
                .OnEntry(() => sm.Fire(Trigger.Y))
                .Permit(Trigger.Y, State.D);

            sm.Configure(State.C)
                .SubstateOf(State.B)
                .Permit(Trigger.Y, State.D);

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
        public void TransitionEvents_OrderingWithInitialTransition()
        {
            var expectedOrdering = new List<string> { "OnExitA", "OnTransitionedAB", "OnEntryB", "OnTransitionedBC", "OnEntryC", "OnTransitionCompletedAC" };
            var actualOrdering = new List<string>();

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B)
                .OnExit(() => actualOrdering.Add("OnExitA"));

            sm.Configure(State.B)
                .InitialTransition(State.C)
                .OnEntry(() => actualOrdering.Add("OnEntryB"));

            sm.Configure(State.C)
                .SubstateOf(State.B)
                .OnEntry(() => actualOrdering.Add("OnEntryC"));

            sm.OnTransitioned(t => actualOrdering.Add($"OnTransitioned{t.Source}{t.Destination}"));
            sm.OnTransitionCompleted(t => actualOrdering.Add($"OnTransitionCompleted{t.Source}{t.Destination}"));

            sm.Fire(Trigger.X);
            Assert.Equal(State.C, sm.State);

            Assert.Equal(expectedOrdering.Count, actualOrdering.Count);
            for (int i = 0; i < expectedOrdering.Count; i++)
            {
                Assert.Equal(expectedOrdering[i], actualOrdering[i]);
            }
        }

        [Fact]
        public async void AsyncTransitionEvents_OrderingWithInitialTransition()
        {
            var expectedOrdering = new List<string> { "OnExitA", "OnTransitionedAB", "OnEntryB", "OnTransitionedBC", "OnEntryC", "OnTransitionCompletedAC" };
            var actualOrdering = new List<string>();

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B)
                .OnExit(() => actualOrdering.Add("OnExitA"));

            sm.Configure(State.B)
                .InitialTransition(State.C)
                .OnEntry(() => actualOrdering.Add("OnEntryB"));

            sm.Configure(State.C)
                .SubstateOf(State.B)
                .OnEntry(() => actualOrdering.Add("OnEntryC"));

            sm.OnTransitionedAsync(t => Task.Run(() => actualOrdering.Add($"OnTransitioned{t.Source}{t.Destination}")));
            sm.OnTransitionCompletedAsync(t => Task.Run(() => actualOrdering.Add($"OnTransitionCompleted{t.Source}{t.Destination}")));

            // await so that the async call completes before asserting anything
            await sm.FireAsync(Trigger.X);
            Assert.Equal(State.C, sm.State);

            Assert.Equal(expectedOrdering.Count, actualOrdering.Count);
            for (int i = 0; i < expectedOrdering.Count; i++)
            {
                Assert.Equal(expectedOrdering[i], actualOrdering[i]);
            }
        }
    }
}
