using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Stateless.Decoration;

namespace Stateless.Tests
{
    [TestFixture]
    public class InfiniteTransitionsDecoratorFixture
    {
        private const int MaximumNumberOfTransistions = short.MaxValue;
        private int _numberOfTransistions;
        private StateMachine<State, Trigger>.TriggerWithParameters<int> triggerWithParameter;
            
        [Test]
        public void AnInfiniteNumberOfTransistionsIsSupported()
        {
            //Given
            _numberOfTransistions = 0;
            IStateMachine<State, Trigger> stateMachine = new StateMachine<State, Trigger>(State.A);
            stateMachine = new InfiniteTransitionsDecorator<State, Trigger>(stateMachine);

            stateMachine.Configure(State.A)
                .OnEntry(() =>
                {
                    _numberOfTransistions++;
                    if (_numberOfTransistions == short.MaxValue) return;
                    stateMachine.Fire(Trigger.X);
                })
                .Permit(Trigger.X, State.B);

            stateMachine.Configure(State.B)
                .OnEntry(() =>
                {
                    _numberOfTransistions++;
                    if (_numberOfTransistions == short.MaxValue) return;
                    stateMachine.Fire(Trigger.Z);
                })
                .Permit(Trigger.Z, State.A);

            //When
            stateMachine.Fire(Trigger.X);

            //Then
            Assert.AreEqual(MaximumNumberOfTransistions, _numberOfTransistions, "The number of transistions expected does not match.");
        }

        /// <summary>
        /// Test whether firing triggers with different method signatures allows infinite transistions as well.
        /// </summary>
        [Test]
        public void FiringTriggerMethodsCanBeUsedRandomly()
        {
            //Given
            _numberOfTransistions = 0;
            IStateMachine<State, Trigger> stateMachine = new StateMachine<State, Trigger>(State.A);
            stateMachine = new InfiniteTransitionsDecorator<State, Trigger>(stateMachine);
            triggerWithParameter = stateMachine.SetTriggerParameters<int>(Trigger.X);

            stateMachine.Configure(State.A)
                .OnEntry(() =>
                {
                    _numberOfTransistions++;
                    if (_numberOfTransistions == short.MaxValue) return;
                    stateMachine.Fire(triggerWithParameter, 1);
                })
                .Permit(Trigger.X, State.B);

            stateMachine.Configure(State.B)
                .OnEntryFrom(triggerWithParameter, (value) =>
                {
                    _numberOfTransistions += value;
                    if (_numberOfTransistions == short.MaxValue) return;
                    stateMachine.Fire(Trigger.Z);
                })
                .Permit(Trigger.Z, State.A);

            //When
            stateMachine.Fire(triggerWithParameter, 1);

            //Then
            Assert.AreEqual(MaximumNumberOfTransistions, _numberOfTransistions, "The number of transistions expected does not match.");
        }
    }
}
