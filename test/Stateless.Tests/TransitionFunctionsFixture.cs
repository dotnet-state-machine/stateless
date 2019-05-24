using System.Collections.Generic;
using Xunit;


namespace Stateless.Tests
{
    /// <summary>
    /// This test class verifies that transition functions gets executed when permitted
    /// </summary>
    public class TransitionFunctionsFixture
    {
        /// <summary>
        /// Check that the transition function gets called after OnExit and before OnEntry
        /// </summary>
        [Fact]
        public void OrderOfExecutingTransitionFunction()
        {
            var record = new List<string>();
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => record.Add("EnterA"))
                .Permit(Trigger.X, State.B, () => record.Add("TransitionFromAToBThroughX"))
                .OnExit(() => record.Add("ExitA"));

            sm.Configure(State.B)
                .OnEntry(() =>
                {
                    record.Add("EnterB");
                })
                .PermitDynamic(Trigger.Z, () => { return State.A; }, null, null, () => record.Add("DynamicTransitionFromBToAThroughZ"))
                .OnExit(() => record.Add("ExitB"));


            sm.Fire(Trigger.X);
            sm.Fire(Trigger.Z);
            // Expected sequence of events: Exit A -> Exit B -> Enter A -> Enter B
            Assert.Equal("ExitA", record[0]);
            Assert.Equal("TransitionFromAToBThroughX", record[1]);
            Assert.Equal("EnterB", record[2]);
            Assert.Equal("ExitB", record[3]);
            Assert.Equal("DynamicTransitionFromBToAThroughZ", record[4]);
            Assert.Equal("EnterA", record[5]);
        }
    }
}
