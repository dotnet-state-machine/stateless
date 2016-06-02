using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using Stateless.Decoration;

namespace Stateless.Tests
{
    [TestFixture]
    public class AuditTrailDecoratorFixture
    {
        [Test]
        public void AuditTrailIsBuildCorreclty()
        {
            //Given
            var stateMachine = new StateMachine<State, Trigger>(State.A);
            AuditTrailDecorator<State, Trigger> auditTrailStateMachine = new AuditTrailDecorator<State, Trigger>(stateMachine);
            ConfigureAuditTrailStateMachine(auditTrailStateMachine);
            var expectedTransistions = new List<StateMachine<State, Trigger>.Transition>
            {
                {new StateMachine<State, Trigger>.Transition(State.A, State.B, Trigger.X)},
                {new StateMachine<State, Trigger>.Transition(State.B, State.C, Trigger.Y)},
                {new StateMachine<State, Trigger>.Transition(State.C, State.A, Trigger.Z)},
            };

            var expectedDateTimes = new List<DateTime>();

            //When
            foreach (var trigger in expectedTransistions.Select(t => t.Trigger))
            {
                expectedDateTimes.Add(DateTime.Now);
                stateMachine.Fire(trigger);
                Thread.Sleep(1000);
            }

            //Then
            var actualAuditTrail = auditTrailStateMachine.AuditTrail.ToList();
            Assert.AreEqual(expectedTransistions.Count, actualAuditTrail.Count, "The actual audit trail does not contain the same number of transitions as the expected audit trail.");
            for (var i = 0; i < expectedTransistions.Count; i++)
            {
                var areEqual = AreEqual(expectedTransistions[i], actualAuditTrail[i].Transition);
                Assert.IsTrue(areEqual,
                    "Expected transistion from state '" + expectedTransistions[i].Source + "' " +
                    "to state '" + expectedTransistions[i].Destination + "' " +
                    "via trigger '" + expectedTransistions[i].Trigger + "'.");

                var differenceInTimeInMilliSeconds = Math.Abs((expectedDateTimes[i] - actualAuditTrail[i].DateTime).TotalMilliseconds);
                Assert.IsTrue(differenceInTimeInMilliSeconds < 250, "It seems the audit trail decorator does not accurately define the moment a trigger occurred."); //Somewhere near the moment the trigger occurred is fine.
            }
        }

        [Test]
        public void OldestTransitionIsRemovedFromAuditTrail()
        {
            //Given
            const int maximumNumberOfTransistionsInAuditTrail = 5;
            var stateMachine = new StateMachine<State, Trigger>(State.A);
            var auditTrailStateMachine = new AuditTrailDecorator<State, Trigger>(stateMachine, maximumNumberOfTransistionsInAuditTrail);
            ConfigureAuditTrailStateMachine(auditTrailStateMachine);
            var expectedTransistions = new List<StateMachine<State, Trigger>.Transition>
            {
                {new StateMachine<State, Trigger>.Transition(State.B, State.C, Trigger.Y)},
                {new StateMachine<State, Trigger>.Transition(State.C, State.A, Trigger.Z)},
                {new StateMachine<State, Trigger>.Transition(State.A, State.B, Trigger.X)},
                {new StateMachine<State, Trigger>.Transition(State.B, State.C, Trigger.Y)},
                {new StateMachine<State, Trigger>.Transition(State.C, State.A, Trigger.Z)},
            };

            //When
            for (var i = 0; i < 10; i++)
            {
                auditTrailStateMachine.Fire(Trigger.X);
                auditTrailStateMachine.Fire(Trigger.Y);
                auditTrailStateMachine.Fire(Trigger.Z);
            }

            //Then
            var actualAuditTrail = auditTrailStateMachine.AuditTrail.ToList();
            Assert.AreEqual(expectedTransistions.Count, actualAuditTrail.Count, "The actual audit trail does not contain the same number of transitions as the expected audit trail.");
            for (var i = 0; i < expectedTransistions.Count; i++)
            {
                var areEqual = AreEqual(expectedTransistions[i], actualAuditTrail[i].Transition);
                Assert.IsTrue(areEqual,
                    "Expected transistion from state '" + expectedTransistions[i].Source + "' " +
                    "to state '" + expectedTransistions[i].Destination + "' " +
                    "via trigger '" + expectedTransistions[i].Trigger + "'.");
            }
        }

        private void ConfigureAuditTrailStateMachine(IStateMachine<State, Trigger> stateMachine)
        {
            stateMachine.Configure(State.A)
                .Permit(Trigger.X, State.B)
                .Permit(Trigger.Y, State.C);

            stateMachine.Configure(State.B)
                .Permit(Trigger.Z, State.A)
                .Permit(Trigger.Y, State.C);

            stateMachine.Configure(State.C)
                .Permit(Trigger.Z, State.A)
                .Permit(Trigger.X, State.B);
        }

        private bool AreEqual(StateMachine<State, Trigger>.Transition expected, StateMachine<State, Trigger>.Transition actual)
        {
            return expected.Source.Equals(actual.Source)
                   && expected.Destination.Equals(actual.Destination)
                   && expected.Trigger.Equals(actual.Trigger);
        }
    }
}
