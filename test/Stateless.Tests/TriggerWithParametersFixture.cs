﻿using System;
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

        /// <summary>
        /// Issue #450 - GetPermittedTriggers throws exception when triggers have different parameter types.
        /// </summary>
        [Theory]
        [MemberData(nameof(DifferentTypeTriggersData))]
        public void DifferentTypeTriggersThrowNoException(params object[] arg)
        {
            var fsm = new StateMachine<State, Trigger>(State.A);
            StateMachine<State, Trigger>.TriggerWithParameters<bool> twp1 = fsm.SetTriggerParameters<bool>(Trigger.X);
            StateMachine<State, Trigger>.TriggerWithParameters<int, string> twp2 = fsm.SetTriggerParameters<int, string>(Trigger.Y);
            StateMachine<State, Trigger>.TriggerWithParameters<string, int, bool> twp3 = fsm.SetTriggerParameters<string, int, bool>(Trigger.Z);
            fsm.Configure(State.A)
                .PermitIf(twp1, State.B, p1 => true)
                .PermitIf(twp2, State.C, (p1, p2) => true)
                .PermitIf(twp3, State.D, (p1, p2, p3) => true);
            Assert.Equal(1, fsm.GetPermittedTriggers(arg).Count());
        }

        public static IEnumerable<object[]> DifferentTypeTriggersData()
        {
            yield return new object[] { "arg", 123, true };
            yield return new object[] { 123, "arg" };
            yield return new object[] { true };
        }
    }
}
