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
        /// Checks that queued fireing mode executes triggers in order
        /// </summary>
        [Fact]
        public void ImmediateEntryAProcessedBeforeEterB()
        {
            var record = new List<string>();
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => record.Add("EnterA"))
                .Transition(Trigger.X).To(State.B)
                .OnExit(() => record.Add("ExitA"));

            sm.Configure(State.B)
                .OnEntry(() =>
                {
                    // Fire this before finishing processing the entry action
                    sm.Fire(Trigger.Y);
                    record.Add("EnterB");
                })
                .Transition(Trigger.Y).To(State.A)
                .OnExit(() => record.Add("ExitB"));

            sm.Fire(Trigger.X);

            // Expected sequence of events: Exit A -> Enter B -> Exit B -> Enter A
            Assert.Equal("ExitA", record[0]);
            Assert.Equal("EnterB", record[1]);
            Assert.Equal("ExitB", record[2]);
            Assert.Equal("EnterA", record[3]);
        }

    }
}
