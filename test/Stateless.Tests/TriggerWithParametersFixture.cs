using System;
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
