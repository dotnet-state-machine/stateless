using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// issue #380 - default params on PermitIfDynamic lead to ambiguity at compile time... explicits work properly.
        /// </summary>
        [Fact]
        public void StateParameterIsNotAmbiguous()
        {
            var fsm = new StateMachine<State, Trigger>(State.A);
            StateMachine<State, Trigger>.TriggerWithParameters<State> pressTrigger = fsm.SetTriggerParameters<State>(Trigger.X);

            fsm.Configure(State.A)
                .PermitDynamicIf(pressTrigger, state => state);
        }

        [Fact]
        public void IncompatibleParameterListIsNotValid()
        {
            var twp = new StateMachine<State, Trigger>.TriggerWithParameters(Trigger.X, new Type[] { typeof(int), typeof(string) });
            Assert.Throws<ArgumentException>(() => twp.ValidateParameters(new object[] { 123 }));
        }

        [Fact]
        public void ParameterListOfCorrectTypeAreAccepted()
        {
            var twp = new StateMachine<State, Trigger>.TriggerWithParameters(Trigger.X, new Type[] { typeof(int), typeof(string) });
            twp.ValidateParameters(new object[] { 123, "arg" });
        }

        [Fact]
        public void Arguments_Returs_Single_Argument()
        {
            var twp = new StateMachine<State, Trigger>.TriggerWithParameters<int>(Trigger.X);
            Assert.Single(twp.ArgumentTypes);
            Assert.Equal(typeof(int), twp.ArgumentTypes.First());
        }

        [Fact]
        public void Arguments_Returs_Two_Arguments()
        {
            var twp = new StateMachine<State, Trigger>.TriggerWithParameters<int, string>(Trigger.X);
            var args = twp.ArgumentTypes.ToList();
            Assert.Equal(2, args.Count);
            Assert.Equal(typeof(int), args[0]);
            Assert.Equal(typeof(string), args[1]);
        }

        [Fact]
        public void Arguments_Returs_Three_Arguments()
        {
            var twp = new StateMachine<State, Trigger>.TriggerWithParameters<int, string, List<bool>>(Trigger.X);
            var args = twp.ArgumentTypes.ToList();
            Assert.Equal(3, args.Count);
            Assert.Equal(typeof(int), args[0]);
            Assert.Equal(typeof(string), args[1]);
            Assert.Equal(typeof(List<bool>), args[2]);
        }
    }
}
