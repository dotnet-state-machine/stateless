using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Stateless.Tests
{
    [TestFixture]
    public class TriggerWithParametersFixture
    {
        [Test]
        public void DescribesUnderlyingTrigger()
        {
            var twp = new StateMachine<State, Trigger>.TriggerWithParameters<string>(Trigger.X);
            Assert.AreEqual(Trigger.X, twp.Trigger);
        }

        [Test]
        public void ParametersOfCorrectTypeAreAccepted()
        {
            var twp = new StateMachine<State, Trigger>.TriggerWithParameters<string>(Trigger.X);
            twp.ValidateParameters(new[] { "arg" });
        }

        [Test]
        public void ParametersArePolymorphic()
        {
            var twp = new StateMachine<State, Trigger>.TriggerWithParameters<object>(Trigger.X);
            twp.ValidateParameters(new[] { "arg" });
        }

        [Test]
        public void IncompatibleParametersAreNotValid()
        {
            var twp = new StateMachine<State, Trigger>.TriggerWithParameters<string>(Trigger.X);
            Assert.Throws<ArgumentException>(() => twp.ValidateParameters(new object[] { 123 }));
        }

        [Test]
        public void TooFewParametersDetected()
        {
            var twp = new StateMachine<State, Trigger>.TriggerWithParameters<string, string>(Trigger.X);
            Assert.Throws<ArgumentException>(() => twp.ValidateParameters(new[] { "a" }));
        }

        [Test]
        public void TooManyParametersDetected()
        {
            var twp = new StateMachine<State, Trigger>.TriggerWithParameters<string, string>(Trigger.X);
            Assert.Throws<ArgumentException>(() => twp.ValidateParameters(new[] { "a", "b", "c" }));
        }
    }
}
