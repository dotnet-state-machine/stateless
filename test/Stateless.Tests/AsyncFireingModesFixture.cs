using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;


namespace Stateless.Tests
{
    /// <summary>
    /// This test class verifies that the firing modes are working as expected
    /// </summary>
    public class AsyncFireingModesFixture
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
                    sm.FireAsync(Trigger.Y).GetAwaiter().GetResult();
                    record.Add("EnterB");
                })
                .Transition(Trigger.Y).To(State.A)
                .OnExit(() => record.Add("ExitB"));

            sm.FireAsync(Trigger.X).GetAwaiter().GetResult();

            // Expected sequence of events: Exit A -> Enter B -> Exit B -> Enter A
            Assert.Equal("ExitA", record[0]);
            Assert.Equal("EnterB", record[1]);
            Assert.Equal("ExitB", record[2]);
            Assert.Equal("EnterA", record[3]);
        }

        [Fact]
        public async void EntersSubStateofSubstateAsyncOnEntryCountAndOrder()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var onEntryCount = "";

            sm.Configure(State.A)
                .OnEntry(async () =>
                {
                    onEntryCount += "A";
                    await Task.Delay(10);
                })
                .Transition(Trigger.X).To(State.B);

            sm.Configure(State.B)
                .OnEntry(async () =>
                {
                    onEntryCount += "B";
                    await Task.Delay(10);
                })
                .InitialTransition(State.C);

            sm.Configure(State.C)
                .OnEntry(async () =>
                {
                    onEntryCount += "C";
                    await Task.Delay(10);
                })
                .InitialTransition(State.D)
                .SubstateOf(State.B);

            sm.Configure(State.D)
                .OnEntry(async () =>
                {
                    onEntryCount += "D";
                    await Task.Delay(10);
                })
                .SubstateOf(State.C);

            await sm.FireAsync(Trigger.X);

            Assert.Equal("BCD", onEntryCount);
        }
    }
}