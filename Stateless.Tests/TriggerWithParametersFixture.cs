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
            twp.ValidateParameters(new [] { "arg" });
        }

        [Test]
        public void ParametersArePolymorphic()
        {
            var twp = new StateMachine<State, Trigger>.TriggerWithParameters<object>(Trigger.X);
            twp.ValidateParameters(new[] { "arg" });
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void IncompatibleParametersAreNotValid()
        {
            var twp = new StateMachine<State, Trigger>.TriggerWithParameters<string>(Trigger.X);
            twp.ValidateParameters(new object[] { 123 });
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void TooFewParametersDetected()
        {
            var twp = new StateMachine<State, Trigger>.TriggerWithParameters<string, string>(Trigger.X);
            twp.ValidateParameters(new[] { "a" });
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void TooManyParametersDetected()
        {
            var twp = new StateMachine<State, Trigger>.TriggerWithParameters<string, string>(Trigger.X);
            twp.ValidateParameters(new[] { "a", "b", "c" });
        }
    }
}
