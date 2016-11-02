using System;
using NUnit.Framework;

namespace Stateless.Tests
{
    [TestFixture]
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

        [Test]
        public void SimpleTransition()
        {
            var expected = "digraph {" + System.Environment.NewLine
                         + " A -> B [label=\"X\"];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void TwoSimpleTransitions()
        {
            var expected = "digraph {" + System.Environment.NewLine
                         + " A -> B [label=\"X\"];" + System.Environment.NewLine
                         + " A -> C [label=\"Y\"];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B)
                .Permit(Trigger.Y, State.C);

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void WhenDiscriminatedByAnonymousGuard()
        {
            Func<bool> anonymousGuard = () => true;

            var expected = "digraph {" + System.Environment.NewLine
                         + " A -> B [label=\"X ["+ anonymousGuard.TryGetMethodName() +"]\"];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, anonymousGuard);

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void WhenDiscriminatedByAnonymousGuardWithDescription()
        {
            Func<bool> anonymousGuard = () => true;

            var expected = "digraph {" + System.Environment.NewLine
                         + " A -> B [label=\"X [description]\"];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, anonymousGuard, "description");

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void WhenDiscriminatedByNamedDelegate()
        {
            var expected = "digraph {" + System.Environment.NewLine
                         + " A -> B [label=\"X [IsTrue]\"];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue);

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void WhenDiscriminatedByNamedDelegateWithDescription()
        {
            var expected = "digraph {" + System.Environment.NewLine
                         + " A -> B [label=\"X [description]\"];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue, "description");

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void DestinationStateIsDynamic()
        {
            var expected = "digraph {" + System.Environment.NewLine
                         + " { node [label=\"?\"] unknownDestination_0 };" + System.Environment.NewLine
                         + " A -> unknownDestination_0 [label=\"X\"];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .PermitDynamic(Trigger.X, () => State.B);

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void DestinationStateIsCalculatedBasedOnTriggerParameters()
        {
            var expected = "digraph {" + System.Environment.NewLine
                         + " { node [label=\"?\"] unknownDestination_0 };" + System.Environment.NewLine
                         + " A -> unknownDestination_0 [label=\"X\"];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitDynamic(trigger, i => i == 1 ? State.B : State.C);

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void OnEntryWithAnonymousActionAndDescription()
        {
            var expected = "digraph {" + System.Environment.NewLine
                         + "node [shape=box];" + System.Environment.NewLine
                         + " A -> \"enteredA\" [label=\"On Entry\" style=dotted];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => { }, "enteredA");

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void OnEntryWithNamedDelegateActionAndDescription()
        {
            var expected = "digraph {" + System.Environment.NewLine
                         + "node [shape=box];" + System.Environment.NewLine
                         + " A -> \"enteredA\" [label=\"On Entry\" style=dotted];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(OnEntry, "enteredA");

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void OnExitWithAnonymousActionAndDescription()
        {
            var expected = "digraph {" + System.Environment.NewLine
                         + "node [shape=box];" + System.Environment.NewLine
                         + " A -> \"exitA\" [label=\"On Exit\" style=dotted];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(() => { }, "exitA");

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void OnExitWithNamedDelegateActionAndDescription()
        {
            var expected = "digraph {" + System.Environment.NewLine
                         + "node [shape=box];" + System.Environment.NewLine
                         + " A -> \"exitA\" [label=\"On Exit\" style=dotted];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(OnExit, "exitA");

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void TransitionWithIgnore()
        {
            // Ignored triggers do not appear in the graph
            var expected = "digraph {" + System.Environment.NewLine
                         + " A -> B [label=\"X\"];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Ignore(Trigger.Y)
                .Permit(Trigger.X, State.B);

            Assert.AreEqual(expected, sm.ToDotGraph());
        }
    }
}
