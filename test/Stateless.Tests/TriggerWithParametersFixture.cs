using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Stateless.Tests
{
    public class TriggerWithParametersFixture
    {
        [Fact]
        public void DescribesUnderlyingTrigger()
        {
            var twp = new StateMachine<State, Trigger>.TriggerWithParameters<string>(Trigger.X);
            Assert.Equal(Trigger.X, twp.Trigger);
        }

        [Fact]
        public void ParametersOfCorrectTypeAreAccepted()
        {
            var twp = new StateMachine<State, Trigger>.TriggerWithParameters<string>(Trigger.X);
            twp.ValidateParameters(new[] { "arg" });
        }
        [Fact]
        public void ParametersWithInValidGuardConditionAreRejected()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var twp = sm.SetTriggerParameters<string>(Trigger.X);
            sm.Configure(State.A).PermitIf(twp, State.B, o => o.ToString() == "3");
            Assert.Equal(sm.State, State.A);

            Assert.Throws<InvalidOperationException>(() => sm.Fire(twp, "2"));
        }
        [Fact]
        public void ParametersWithValidGuardConditionAreAccepted()
        {
            var sm = new StateMachine<State, Trigger>(State.A);
            var twp = sm.SetTriggerParameters<string>(Trigger.X);
            sm.Configure(State.A).PermitIf(twp, State.B, o =>
            {
                return o.ToString() == "2";
            });
             sm.Fire(twp, "2");
            Assert.Equal(sm.State,State.B);
        }
        [Fact]
        public void ParametersArePolymorphic()
        {
            var twp = new StateMachine<State, Trigger>.TriggerWithParameters<object>(Trigger.X);
            twp.ValidateParameters(new[] { "arg" });
        }

        [Fact]
        public void IncompatibleParametersAreNotValid()
        {
            var twp = new StateMachine<State, Trigger>.TriggerWithParameters<string>(Trigger.X);
            Assert.Throws<ArgumentException>(() => twp.ValidateParameters(new object[] { 123 }));
        }

        [Fact]
        public void TooFewParametersDetected()
        {
            var twp = new StateMachine<State, Trigger>.TriggerWithParameters<string, string>(Trigger.X);
            Assert.Throws<ArgumentException>(() => twp.ValidateParameters(new[] { "a" }));
        }

        [Fact]
        public void TooManyParametersDetected()
        {
            var twp = new StateMachine<State, Trigger>.TriggerWithParameters<string, string>(Trigger.X);
            Assert.Throws<ArgumentException>(() => twp.ValidateParameters(new[] { "a", "b", "c" }));
        }
    }
}
