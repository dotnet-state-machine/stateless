using Xunit;

namespace Stateless.Tests
{
    public class MermaidGraphFixture
    {
        [Fact]
        public void Format_InitialTransition_ShouldReturns()
        {
            var expected = "stateDiagram-v2\r\n[*] --> A";

            var sm = new StateMachine<State, Trigger>(State.A);

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            Assert.Equal(expected, result);

        }

        [Fact]
        public void Format_SimpleTransition()
        {
            var expected = "stateDiagram-v2\r\n\tA --> B : X\r\n[*] --> A";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            Assert.Equal(expected, result);

        }

        [Fact]
        public void TwoSimpleTransitions()
        {
            var expected = """
        stateDiagram-v2
            A --> B : X 
            A --> C : Y
        """;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                   .Permit(Trigger.X, State.B)
                   .Permit(Trigger.Y, State.C);

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            Assert.Equal(expected, result);

        }
    }
}
