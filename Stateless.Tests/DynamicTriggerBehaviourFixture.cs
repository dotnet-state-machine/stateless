using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Stateless.Tests
{
    [TestFixture]
    public class DynamicTriggerBehaviour
    {
        [Test]
        public void DestinationStateIsDynamic()
        {
            var sm = StateMachine<State, Trigger>.Create(State.A);
            sm.Configure(State.A)
                .PermitDynamic(Trigger.X, () => State.B);

            var csm = sm.FinishConfiguration();

            csm.Fire(Trigger.X);

            Assert.AreEqual(State.B, csm.State);
        }

        [Test]
        public void DestinationStateIsCalculatedBasedOnTriggerParameters()
        {
            var sm = StateMachine<State, Trigger>.Create(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitDynamic(trigger, i => i == 1 ? State.B : State.C);

            var csm = sm.FinishConfiguration();

            csm.Fire(trigger, 1);

            Assert.AreEqual(State.B, csm.State);
        }
    }
}
