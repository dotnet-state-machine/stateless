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
                .PermitDynamic(Trigger.X, () => State.B);

            sm.Fire(Trigger.X);

            Assert.Equal(State.B, sm.State);
        }

        [Fact]
        public void DestinationStateIsCalculatedBasedOnTriggerParameters()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitDynamic(trigger, i => i == 1 ? State.B : State.C);

            sm.Fire(trigger, 1);

            Assert.Equal(State.B, sm.State);
        }

        [Fact]
        public void Sdfsf()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitDynamicIf(trigger, (i) => i == 1 ? State.C : State.B, (i) => i == 1 ? true : false);

            // Should not throw
            sm.GetPermittedTriggers().ToList();

            sm.Fire(trigger, 1);
        }
    }
}
