using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Stateless.Decoration;

namespace Stateless.Tests
{
    [TestFixture]
    public class InfiniteTransitionsDecoratorFixture : StateMachineFixture
    {
        private const int MaximumNumberOfTransistions = short.MaxValue;
        private int _numberOfTransistions;
        private StateMachine<State, Trigger>.TriggerWithParameters<int> _triggerWithParameter;

        protected override IStateMachine<State, Trigger> CreateStateMachine(State initialState)
        {
            var stateMachine = base.CreateStateMachine(initialState);
            stateMachine = new InfiniteTransitionsDecorator<State, Trigger>(stateMachine);
            return stateMachine;
        }

        protected override IStateMachine<TState, TTrigger> CreateStateMachine<TState, TTrigger>(TState initialState)
        {
            var stateMachine = base.CreateStateMachine<TState, TTrigger>(initialState);
            stateMachine = new InfiniteTransitionsDecorator<TState, TTrigger>(stateMachine);
            return stateMachine;
        }

        protected override IStateMachine<State, Trigger> CreateStateMachine(Func<State> stateAccessor, Action<State> stateMutator)
        {
            var stateMachine = base.CreateStateMachine(stateAccessor, stateMutator);
            stateMachine = new InfiniteTransitionsDecorator<State, Trigger>(stateMachine);
            return stateMachine;
        }

        [Test]
        public void AnInfiniteNumberOfTransistionsIsSupported()
        {
            //Given
            _numberOfTransistions = 0;
            var stateMachine = CreateStateMachine(State.A);

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
            var stateMachine = CreateStateMachine(State.A);
            _triggerWithParameter = stateMachine.SetTriggerParameters<int>(Trigger.X);

            stateMachine.Configure(State.A)
                .OnEntry(() =>
                {
                    _numberOfTransistions++;
                    if (_numberOfTransistions == short.MaxValue) return;
                    stateMachine.Fire(_triggerWithParameter, 1);
                })
                .Permit(Trigger.X, State.B);

            stateMachine.Configure(State.B)
                .OnEntryFrom(_triggerWithParameter, (value) =>
                {
                    _numberOfTransistions += value;
                    if (_numberOfTransistions == short.MaxValue) return;
                    stateMachine.Fire(Trigger.Z);
                })
                .Permit(Trigger.Z, State.A);

            //When
            stateMachine.Fire(_triggerWithParameter, 1);

            //Then
            Assert.AreEqual(MaximumNumberOfTransistions, _numberOfTransistions, "The number of transistions expected does not match.");
        }
    }
}
