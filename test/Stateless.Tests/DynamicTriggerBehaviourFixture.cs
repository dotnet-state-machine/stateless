using System.Linq;
using Xunit;

namespace Stateless.Tests
{
    public class DynamicTriggerBehaviour
    {
        [Fact]
        public void DestinationStateIsDynamic()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .Transition(Trigger.X).Dynamic(() => State.B);

            sm.Fire(Trigger.X);

            Assert.Equal(State.B, sm.State);
        }

        [Fact]
        public void DestinationStateIsCalculatedBasedOnTriggerParameters()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .Transition(trigger).Dynamic<int>(i => i == 1 ? State.B : State.C);

            sm.Fire(trigger, 1);

            Assert.Equal(State.B, sm.State);
        }

        [Fact]
        public void DynamicOneArg()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .Transition(trigger).Dynamic<int>((i) => i == 1 ? State.C : State.B);

            // Should not throw
            Assert.NotNull(sm.GetPermittedTriggers().ToList());

            sm.Fire(trigger, 1);

            Assert.Equal(State.C, sm.State);
        }

        [Fact]
        public void DynamicTwoArg()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int, int>(Trigger.X);
            sm.Configure(State.A)
                .Transition(trigger).Dynamic<int, int>((i, j) => i == 1 ? State.C : State.B);

            // Should not throw
            Assert.NotNull( sm.GetPermittedTriggers().ToList());

            sm.Fire(trigger, 1, 2);

            Assert.Equal(State.C, sm.State);
        }

        [Fact]
        public void DynamicThreeArg()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int, int, int>(Trigger.X);
            sm.Configure(State.A)
                .Transition(trigger).Dynamic<int, int, int>((i, j, k) => i == 1 ? State.C : State.B);

            // Should not throw
            Assert.NotNull(sm.GetPermittedTriggers().ToList());

            sm.Fire(trigger, 1,2,3);

            Assert.Equal(State.C, sm.State);
        }
    }
}
