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
        private int _numberOfTransistions = 0;
        private const int MaximumNumberOfTransistions = short.MaxValue;

        [Test]
        public void AnInfiniteNumberOfTransistionsIsSupported()
        {
            //Given
            var stateMachine = new StateMachine<State, Trigger>(State.A);
            var infiniteStateMachine = new InfiniteTransitionsDecorator<State, Trigger>(stateMachine);
            ConfigureAuditTrailStateMachine(infiniteStateMachine);

            //When
            infiniteStateMachine.Fire(Trigger.X);

            //Then
            Assert.AreEqual(MaximumNumberOfTransistions, _numberOfTransistions, "The number of transistions expected does not match.");
        }

        private void ConfigureAuditTrailStateMachine(IStateMachine<State, Trigger> stateMachine)
        {
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
        }


    }
}
