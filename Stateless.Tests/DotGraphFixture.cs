using System;
using NUnit.Framework;

namespace Stateless.Tests
{
    [TestFixture]
    public class DotGraphFixture
    {
        bool IsTrue() {
            return true;
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
                         + " A -> B [label=\"X ["+ anonymousGuard.Method.Name +"]\"];" + System.Environment.NewLine
                         + "}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, anonymousGuard);

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
    }
}
