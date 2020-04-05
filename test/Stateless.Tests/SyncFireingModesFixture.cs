#if TASKS
using System.Collections.Generic;
using Xunit;


namespace Stateless.Tests
{
    /// <summary>
    /// This test class verifies that the firing modes are working as expected
    /// </summary>
    public class SyncFireingModesFixture
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
                .Permit(Trigger.X, State.B);

            sm.Configure(State.B)
                .OnEntry(() =>
                {
                    System.Console.WriteLine("OnEntryS2()");
                    sm.Fire(Trigger.X);
                })
                .Permit(Trigger.X, State.C);

            sm.Configure(State.C)
                .OnEntry(() =>
                {
                    System.Console.WriteLine("OnEntryS3()");
                })
                .Permit(Trigger.X, State.A);


            sm.Fire(Trigger.X);

            Assert.Equal(State.C, sm.State);
        }
    }
}
#endif