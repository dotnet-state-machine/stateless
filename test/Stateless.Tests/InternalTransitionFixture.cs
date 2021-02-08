﻿using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Stateless.Tests
{
    public class InternalTransitionFixture
    {
        /// <summary>
        /// The expected behaviour of the internal transition is that the state does not change.
        /// This will fail if the state changes after the trigger has fired.
        /// </summary>
        [Fact]
        public void StayInSameStateOneState_Transition()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .Transition(Trigger.X).Internal().Do((_) => { });

            Assert.Equal(State.A, sm.State);
            sm.Fire(Trigger.X);
            Assert.Equal(State.A, sm.State);
        }

        [Fact]
        public void StayInSameStateTwoStates_Transition()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Transition(Trigger.X).Internal().Do((_) => { })
                .Transition(Trigger.Y).To(State.B);

            sm.Configure(State.B)
                    .Transition(Trigger.X).Internal().Do((_) => { })
                    .Transition(Trigger.Y).To(State.A);

            // This should not cause any state changes
            Assert.Equal(State.A, sm.State);
            sm.Fire(Trigger.X);
            Assert.Equal(State.A, sm.State);

            // Change state to B
            sm.Fire(Trigger.Y);

            // This should also not cause any state changes
            Assert.Equal(State.B, sm.State);
            sm.Fire(Trigger.X);
            Assert.Equal(State.B, sm.State);
        }
        [Fact]
        public void StayInSameSubStateTransitionInSuperstate_Transition()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.A)
                    .Transition(Trigger.X).Internal().Do((_) => { });                    

            sm.Configure(State.B)
                    .SubstateOf(State.A);

            // This should not cause any state changes
            Assert.Equal(State.B, sm.State);
            sm.Fire(Trigger.X);
            Assert.Equal(State.B, sm.State);
        }
        [Fact]
        public void StayInSameSubStateTransitionInSubstate_Transition()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.A);

            sm.Configure(State.B)
                    .SubstateOf(State.A)
                    .Transition(Trigger.X).Internal().Do((_) => { });
                    

            // This should not cause any state changes
            Assert.Equal(State.B, sm.State);
            sm.Fire(Trigger.X);
            Assert.Equal(State.B, sm.State);
        }

        [Fact]
        public void StayInSameStateOneState_Action()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .Transition(Trigger.X).Internal();

            Assert.Equal(State.A, sm.State);
            sm.Fire(Trigger.X);
            Assert.Equal(State.A, sm.State);
        }

        [Fact]
        public void StayInSameStateTwoStates_Action()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Transition(Trigger.X).Internal()
                .Transition(Trigger.Y).To(State.B);

            sm.Configure(State.B)
                    .Transition(Trigger.X).Internal()
                    .Transition(Trigger.Y).To(State.A);

            // This should not cause any state changes
            Assert.Equal(State.A, sm.State);
            sm.Fire(Trigger.X);
            Assert.Equal(State.A, sm.State);

            // Change state to B
            sm.Fire(Trigger.Y);

            // This should also not cause any state changes
            Assert.Equal(State.B, sm.State);
            sm.Fire(Trigger.X);
            Assert.Equal(State.B, sm.State);
        }
        [Fact]
        public void StayInSameSubStateTransitionInSuperstate_Action()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.A)
                    .Transition(Trigger.X).Internal()
                    .Transition(Trigger.Y).Internal();

            sm.Configure(State.B)
                    .SubstateOf(State.A);

            // This should not cause any state changes
            Assert.Equal(State.B, sm.State);
            sm.Fire(Trigger.X);
            Assert.Equal(State.B, sm.State);
            sm.Fire(Trigger.Y);
            Assert.Equal(State.B, sm.State);
        }
        [Fact]
        public void StayInSameSubStateTransitionInSubstate_Action()
        {
            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.A);

            sm.Configure(State.B)
                    .SubstateOf(State.A)
                    .Transition(Trigger.X).Internal()
                    .Transition(Trigger.Y).Internal();

            // This should not cause any state changes
            Assert.Equal(State.B, sm.State);
            sm.Fire(Trigger.X);
            Assert.Equal(State.B, sm.State);
            sm.Fire(Trigger.Y);
            Assert.Equal(State.B, sm.State);
        }

        [Fact]
        public void AllowTriggerWithTwoParameters()
        {
            var sm = new StateMachine<State, Trigger>(State.B);
            var trigger = sm.SetTriggerParameters<int, string>(Trigger.X);
            const int intParam = 5;
            const string strParam = "Five";
            var callbackInvoked = false;

            sm.Configure(State.B)
                .Transition(Trigger.X).Internal().Do<int, string>((i, s, transition) =>
                {
                    callbackInvoked = true;
                    Assert.Equal(intParam, i);
                    Assert.Equal(strParam, s);
                });

            sm.Fire(trigger, intParam, strParam);
            Assert.True(callbackInvoked);
        }


        [Fact]
        public void AllowTriggerWithThreeParameters()
        {
            var sm = new StateMachine<State, Trigger>(State.B);
            var trigger = sm.SetTriggerParameters<int, string, bool>(Trigger.X);
            const int intParam = 5;
            const string strParam = "Five";
            var boolParam = true;
            var callbackInvoked = false;

            sm.Configure(State.B)
                .Transition(Trigger.X).Internal().Do<int, string, bool>((i, s, b, transition) =>
                {
                    callbackInvoked = true;
                    Assert.Equal(intParam, i);
                    Assert.Equal(strParam, s);
                    Assert.Equal(boolParam, b);
                });

            sm.Fire(trigger, intParam, strParam, boolParam);
            Assert.True(callbackInvoked);
        }

        [Fact]
        public void ConditionalInternalTransition_ShouldBeReflectedInPermittedTriggers()
        {
            var isPermitted = true;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .Transition(Trigger.X).Internal().If(u => isPermitted).Do(t => { });

            Assert.Equal(1, sm.GetPermittedTriggers().ToArray().Length);
            isPermitted = false;
            Assert.Equal(0, sm.GetPermittedTriggers().ToArray().Length);
        }

        [Fact]
        public void InternalTriggerHandledOnlyOnceInSuper()
        {
            State handledIn = State.C;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Transition(Trigger.X).Internal().Do(() => handledIn = State.A);

            sm.Configure(State.B)
                .SubstateOf(State.A)
                .Transition(Trigger.X).Internal().Do(() => handledIn = State.B);

            // The state machine is in state A. It should only be handled in in State A, so handledIn should be equal to State.A
            sm.Fire(Trigger.X);

            Assert.Equal(State.A, handledIn);
        }
        [Fact]
        public void InternalTriggerHandledOnlyOnceInSub()
        {
            State handledIn = State.C;

            var sm = new StateMachine<State, Trigger>(State.B);

            sm.Configure(State.A)
                .Transition(Trigger.X).Internal().Do(() => handledIn = State.A);

            sm.Configure(State.B)
                .SubstateOf(State.A)
                .Transition(Trigger.X).Internal().Do(() => handledIn = State.B);

            // The state machine is in state B. It should only be handled in State B, so handledIn should be equal to State.B
            sm.Fire(Trigger.X);

            Assert.Equal(State.B, handledIn);
        }
        [Fact]
        public void OnlyOneHandlerExecuted()
        {
            var handled = 0;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Transition(Trigger.X).Internal().Do(() => handled++)
                .Transition(Trigger.Y).Internal().Do(() => handled++);

            sm.Fire(Trigger.X);

            Assert.Equal(1, handled);

            sm.Fire(Trigger.Y);

            Assert.Equal(2, handled);
        }

        [Fact]
        public async Task AsyncHandlesNonAsyndActionAsync()
        {
            var handled = false;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Transition(Trigger.Y).Internal().Do(() => handled = true);

            await sm.FireAsync(Trigger.Y);

            Assert.True(handled);
        }
    }
}
