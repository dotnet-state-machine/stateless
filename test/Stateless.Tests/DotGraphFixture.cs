using System;
using Xunit;
using Stateless.Reflection;
using Stateless.DotGraph;

namespace Stateless.Tests
{
    public class DotGraphFixture
    {
        bool IsTrue() 
        {
            return true;
        }

        void OnEntry()
        {

        }

        void OnExit()
        {

        }

        [Fact]
        public void SimpleTransition_DotGraph()
        {
            var expected = "digraph {" + System.Environment.NewLine
                         + " A -> B [label=\"X\"];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            Assert.Equal(expected, DotGraphFormatter.Format(sm.GetInfo()));
        }

        [Fact]
        public void TwoSimpleTransitions_DotGraph()
        {
            var expected = "digraph {" + System.Environment.NewLine
                         + " A -> B [label=\"X\"];" + System.Environment.NewLine
                         + " A -> C [label=\"Y\"];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B)
                .Permit(Trigger.Y, State.C);

            Assert.Equal(expected, DotGraphFormatter.Format(sm.GetInfo()));
        }

        [Fact]
        public void WhenDiscriminatedByAnonymousGuard_DotGraph()
        {
            Func<bool> anonymousGuard = () => true;

            var expected = "digraph {" + System.Environment.NewLine
                         + " A -> B [label=\"X [" + InvocationInfo.DefaultFunctionDescription +"]\"];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B).If(anonymousGuard);

            Assert.Equal(expected, DotGraphFormatter.Format(sm.GetInfo()));
        }

        [Fact]
        public void WhenDiscriminatedByAnonymousGuardWithDescription_DotGraph()
        {
            Func<bool> anonymousGuard = () => true;

            var expected = "digraph {" + System.Environment.NewLine
                         + " A -> B [label=\"X [description]\"];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B).If(anonymousGuard, "description");

            Assert.Equal(expected, DotGraphFormatter.Format(sm.GetInfo()));
        }

        [Fact]
        public void WhenDiscriminatedByNamedDelegate_DotGraph()
        {
            var expected = "digraph {" + System.Environment.NewLine
                         + " A -> B [label=\"X [IsTrue]\"];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue);

            Assert.Equal(expected, DotGraphFormatter.Format(sm.GetInfo()));
        }

        [Fact]
        public void WhenDiscriminatedByNamedDelegateWithDescription_DotGraph()
        {
            var expected = "digraph {" + System.Environment.NewLine
                         + " A -> B [label=\"X [description]\"];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue, "description");

            Assert.Equal(expected, DotGraphFormatter.Format(sm.GetInfo()));
        }

        [Fact]
        public void DestinationStateIsDynamic_DotGraph()
        {
            // TODO: Since the spec doesn't guarantee that the destination text will have a
            // specific format, we shouldn't be writing a test that assumes a specific format.

            var expected = "digraph {" + System.Environment.NewLine
                         + " { node [label=\"?\"] unknownDestination_0 };" + System.Environment.NewLine
                         + " A -> unknownDestination_0 [label=\"X\"];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .PermitDynamic(Trigger.X, () => State.B);

            Assert.Equal(expected, DotGraphFormatter.Format(sm.GetInfo()));
        }

        [Fact]
        public void DestinationStateIsCalculatedBasedOnTriggerParameters_DotGraph()
        {
            var expected = "digraph {" + System.Environment.NewLine
                         + " { node [label=\"?\"] unknownDestination_0 };" + System.Environment.NewLine
                         + " A -> unknownDestination_0 [label=\"X\"];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitDynamic(trigger, i => i == 1 ? State.B : State.C);
            Assert.Equal(expected, DotGraphFormatter.Format(sm.GetInfo()));
        }

        [Fact]
        public void OnEntryWithAnonymousActionAndDescription_DotGraph()
        {
            var expected = "digraph {" + System.Environment.NewLine
                         + "node [shape=box];" + System.Environment.NewLine
                         + " A -> \"enteredA\" [label=\"On Entry\" style=dotted];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => { }, "enteredA");

            Assert.Equal(expected, DotGraphFormatter.Format(sm.GetInfo()));
        }

        [Fact]
        public void OnEntryWithNamedDelegateActionAndDescription_DotGraph()
        {
            var expected = "digraph {" + System.Environment.NewLine
                         + "node [shape=box];" + System.Environment.NewLine
                         + " A -> \"enteredA\" [label=\"On Entry\" style=dotted];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(OnEntry, "enteredA");

            Assert.Equal(expected, DotGraphFormatter.Format(sm.GetInfo()));
        }

        [Fact]
        public void OnExitWithAnonymousActionAndDescription_DotGraph()
        {
            var expected = "digraph {" + System.Environment.NewLine
                         + "node [shape=box];" + System.Environment.NewLine
                         + " A -> \"exitA\" [label=\"On Exit\" style=dotted];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(() => { }, "exitA");

            Assert.Equal(expected, DotGraphFormatter.Format(sm.GetInfo()));
        }

        [Fact]
        public void OnExitWithNamedDelegateActionAndDescription_DotGraph()
        {
            var expected = "digraph {" + System.Environment.NewLine
                         + "node [shape=box];" + System.Environment.NewLine
                         + " A -> \"exitA\" [label=\"On Exit\" style=dotted];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(OnExit, "exitA");

            Assert.Equal(expected, DotGraphFormatter.Format(sm.GetInfo()));
        }

        [Fact]
        public void TransitionWithIgnore_DotGraph()
        {
            // Ignored triggers do not appear in the graph
            var expected = "digraph {" + System.Environment.NewLine
                         + " A -> B [label=\"X\"];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Ignore(Trigger.Y)
                .Permit(Trigger.X, State.B);

            Assert.Equal(expected, DotGraphFormatter.Format(sm.GetInfo()));
        }
    }
}
