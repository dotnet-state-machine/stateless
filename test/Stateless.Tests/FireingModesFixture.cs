using System.Collections.Generic;
using Xunit;


namespace Stateless.Tests
{
    /// <summary>
    /// This test class verifies that the firing modes are working as expected
    /// </summary>
    public class FireingModesFixture
    {
        /// <summary>
        /// Check that the immediate fireing modes executes entry/exit out of order.
        /// </summary>
        [Fact]
        public void ImmediateEntryAProcessedBeforeEnterB()
        {
            var record = new List<string>();
            var sm = new StateMachine<State, Trigger>(State.A, FiringMode.Immediate);

            sm.Configure(State.A)
                .OnEntry(() => record.Add("EnterA"))
                .Permit(Trigger.X, State.B)
                .OnExit(() => record.Add("ExitA"));

            sm.Configure(State.B)
                .OnEntry(() =>
                {
                    // Fire this before finishing processing the entry action
                    sm.Fire(Trigger.Y);
                    record.Add("EnterB");
                })
                .Permit(Trigger.Y, State.A)
                .OnExit(() => record.Add("ExitB"));

            sm.Fire(Trigger.X);

            // Expected sequence of events: Exit A -> Exit B -> Enter A -> Enter B
            Assert.Equal("ExitA", record[0]);
            Assert.Equal("ExitB", record[1]);
            Assert.Equal("EnterA", record[2]);
            Assert.Equal("EnterB", record[3]);
        }

        /// <summary>
        /// Checks that queued fireing mode executes triggers in order
        /// </summary>
        [Fact]
        public void ImmediateEntryAProcessedBeforeEterB()
        {
            var record = new List<string>();
            var sm = new StateMachine<State, Trigger>(State.A, FiringMode.Queued);

            sm.Configure(State.A)
                .OnEntry(() => record.Add("EnterA"))
                .Permit(Trigger.X, State.B)
                .OnExit(() => record.Add("ExitA"));

            sm.Configure(State.B)
                .OnEntry(() =>
                {
                    // Fire this before finishing processing the entry action
                    sm.Fire(Trigger.Y);
                    record.Add("EnterB");
                })
                .Permit(Trigger.Y, State.A)
                .OnExit(() => record.Add("ExitB"));

            sm.Fire(Trigger.X);

            // Expected sequence of events: Exit A -> Enter B -> Exit B -> Enter A
            Assert.Equal("ExitA", record[0]);
            Assert.Equal("EnterB", record[1]);
            Assert.Equal("ExitB", record[2]);
            Assert.Equal("EnterA", record[3]);
        }

        /// <summary>
        /// Check that the immediate fireing modes executes entry/exit out of order.
        /// </summary>
        [Fact]
        public void ImmediateFireingOnEntryEndsUpInCorrectState()
        {
            var record = new List<string>();
            var sm = new StateMachine<State, Trigger>(State.A, FiringMode.Immediate);

            sm.Configure(State.A)
                .OnEntry(() => record.Add("EnterA"))
                .Permit(Trigger.X, State.B)
                .OnExit(() => record.Add("ExitA"));

            sm.Configure(State.B)
                .OnEntry(() =>
                {
                    record.Add("EnterB");
                    // Fire this before finishing processing the entry action
                    sm.Fire(Trigger.X);
                })
                .Permit(Trigger.X, State.C)
                .OnExit(() => record.Add("ExitB"));

            sm.Configure(State.C)
                .OnEntry(() => record.Add("EnterC"))
                .Permit(Trigger.X, State.A)
                .OnExit(() => record.Add("ExitC"));

            sm.Fire(Trigger.X);

            // Expected sequence of events: Exit A -> Exit B -> Enter A -> Enter B
            Assert.Equal("ExitA", record[0]);
            Assert.Equal("EnterB", record[1]);
            Assert.Equal("ExitB", record[2]);
            Assert.Equal("EnterC", record[3]);

            Assert.Equal(State.C, sm.State);
        }
    }
}
