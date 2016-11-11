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
            var expected =
                $"digraph {{{System.Environment.NewLine}compound=true;{System.Environment.NewLine}rankdir=\"LR\"{System.Environment.NewLine}\tA [   label=<{System.Environment.NewLine}\t<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >{System.Environment.NewLine}\t<tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t<TR><TD PORT=\"A\">A</TD></TR><tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t</TABLE>>,shape=\"plaintext\",color=\"blue\" ];{System.Environment.NewLine}{System.Environment.NewLine}A -> B [   style=\"solid\",label=\"X\" ];  {System.Environment.NewLine}}}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void TwoSimpleTransitions()
        {
            var expected = $"digraph {{{System.Environment.NewLine}compound=true;{System.Environment.NewLine}rankdir=\"LR\"{System.Environment.NewLine}\tA [   label=<{System.Environment.NewLine}\t<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >{System.Environment.NewLine}\t<tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t<TR><TD PORT=\"A\">A</TD></TR><tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t</TABLE>>,shape=\"plaintext\",color=\"blue\" ];{System.Environment.NewLine}{System.Environment.NewLine}A -> B [   style=\"solid\",label=\"X\" ]; {System.Environment.NewLine}A -> C [   style=\"solid\",label=\"Y\" ];  {System.Environment.NewLine}}}";

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

            var expected = $"digraph {{{System.Environment.NewLine}compound=true;{System.Environment.NewLine}rankdir=\"LR\"{System.Environment.NewLine}\tA [   label=<{System.Environment.NewLine}\t<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >{System.Environment.NewLine}\t<tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t<TR><TD PORT=\"A\">A</TD></TR><tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t</TABLE>>,shape=\"plaintext\",color=\"blue\" ];{System.Environment.NewLine}\tB [   label=<{System.Environment.NewLine}\t<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >{System.Environment.NewLine}\t<tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t<TR><TD PORT=\"B\">B</TD></TR><tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t</TABLE>>,shape=\"plaintext\",color=\"blue\" ];{System.Environment.NewLine}{System.Environment.NewLine}A -> B [   style=\"solid\",label=\"X " + anonymousGuard.TryGetMethodName() + $"\" ]; {System.Environment.NewLine}}}";
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, anonymousGuard);
            sm.Configure(State.B);

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void WhenDiscriminatedByAnonymousGuardWithDescription()
        {
            Func<bool> anonymousGuard = () => true;

            var expected = $"digraph {{{System.Environment.NewLine}compound=true;{System.Environment.NewLine}rankdir=\"LR\"{System.Environment.NewLine}\tA [   label=<{System.Environment.NewLine}\t<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >{System.Environment.NewLine}\t<tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t<TR><TD PORT=\"A\">A</TD></TR><tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t</TABLE>>,shape=\"plaintext\",color=\"blue\" ];{System.Environment.NewLine}{System.Environment.NewLine}A -> B [   style=\"solid\",label=\"X description\" ]; {System.Environment.NewLine}}}";
        var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, anonymousGuard, "description");

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void WhenDiscriminatedByNamedDelegate()
        {
            var expected = $"digraph {{{System.Environment.NewLine}compound=true;{System.Environment.NewLine}rankdir=\"LR\"{System.Environment.NewLine}\tA [   label=<{System.Environment.NewLine}\t<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >{System.Environment.NewLine}\t<tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t<TR><TD PORT=\"A\">A</TD></TR><tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t</TABLE>>,shape=\"plaintext\",color=\"blue\" ];{System.Environment.NewLine}{System.Environment.NewLine}A -> B [   style=\"solid\",label=\"X IsTrue\" ]; {System.Environment.NewLine}}}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue);

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void WhenDiscriminatedByNamedDelegateWithDescription()
        {
            var expected = $"digraph {{{System.Environment.NewLine}compound=true;{System.Environment.NewLine}rankdir=\"LR\"{System.Environment.NewLine}\tA [   label=<{System.Environment.NewLine}\t<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >{System.Environment.NewLine}\t<tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t<TR><TD PORT=\"A\">A</TD></TR><tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t</TABLE>>,shape=\"plaintext\",color=\"blue\" ];{System.Environment.NewLine}\tB [   label=<{System.Environment.NewLine}\t<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >{System.Environment.NewLine}\t<tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t<TR><TD PORT=\"B\">B</TD></TR><tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t</TABLE>>,shape=\"plaintext\",color=\"blue\" ];{System.Environment.NewLine}{System.Environment.NewLine}A -> B [   style=\"solid\",label=\"X description\" ]; {System.Environment.NewLine}}}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue, "description");
            sm.Configure(State.B);
            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void DestinationStateIsDynamic()
        {
            var expected = $"digraph {{{System.Environment.NewLine}compound=true;{System.Environment.NewLine}rankdir=\"LR\"{System.Environment.NewLine}\tA [   label=<{System.Environment.NewLine}\t<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >{System.Environment.NewLine}\t<tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t<TR><TD PORT=\"A\">A</TD></TR><tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t</TABLE>>,shape=\"plaintext\",color=\"blue\" ];{System.Environment.NewLine}{System.Environment.NewLine}\tUnk0_A [   label=<{System.Environment.NewLine}\t<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >{System.Environment.NewLine}\t<tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t<TR><TD PORT=\"Unk0_A\">Unk0_A</TD></TR><tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t</TABLE>>,shape=\"plaintext\",color=\"blue\" ];{System.Environment.NewLine}{System.Environment.NewLine}A -> Unk0_A [   style=\"solid\",label=\"X\" ];  {System.Environment.NewLine}}}";

            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .PermitDynamic(Trigger.X, () => State.B);

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void DestinationStateIsCalculatedBasedOnTriggerParameters()
        {
            var expected = $"digraph {{{System.Environment.NewLine}compound=true;{System.Environment.NewLine}rankdir=\"LR\"{System.Environment.NewLine}\tA [   label=<{System.Environment.NewLine}\t<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >{System.Environment.NewLine}\t<tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t<TR><TD PORT=\"A\">A</TD></TR><tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t</TABLE>>,shape=\"plaintext\",color=\"blue\" ];{System.Environment.NewLine}{System.Environment.NewLine}\tUnk0_A [   label=<{System.Environment.NewLine}\t<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >{System.Environment.NewLine}\t<tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t<TR><TD PORT=\"Unk0_A\">Unk0_A</TD></TR><tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t</TABLE>>,shape=\"plaintext\",color=\"blue\" ];{System.Environment.NewLine}{System.Environment.NewLine}A -> Unk0_A [   style=\"solid\",label=\"X\" ];  {System.Environment.NewLine}}}".Replace("{System.Environment.NewLine}", System.Environment.NewLine);

            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitDynamic(trigger, i => i == 1 ? State.B : State.C);

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void OnEntryWithAnonymousActionAndDescription()
        {
            var expected = $"digraph {{{System.Environment.NewLine}compound=true;{System.Environment.NewLine}rankdir=\"LR\"{System.Environment.NewLine}\tA [   label=<{System.Environment.NewLine}\t<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >{System.Environment.NewLine}\t<tr><td>{System.Environment.NewLine}\t\t<TABLE BORDER=\"0\" CELLBORDER=\"1\" CELLSPACING=\"0\" BGCOLOR=\"yellow\">{System.Environment.NewLine}\t\t<TR><TD><sup>enteredA</sup></TD></TR>{System.Environment.NewLine}\t\t</TABLE>{System.Environment.NewLine}{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t<TR><TD PORT=\"A\">A</TD></TR><tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t</TABLE>>,shape=\"plaintext\",color=\"blue\" ];{System.Environment.NewLine}{System.Environment.NewLine}}}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => { }, "enteredA");

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void OnEntryWithNamedDelegateActionAndDescription()
        {
            var expected = $"digraph {{{System.Environment.NewLine}compound=true;{System.Environment.NewLine}rankdir=\"LR\"{System.Environment.NewLine}\tA [   label=<{System.Environment.NewLine}\t<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >{System.Environment.NewLine}\t<tr><td>{System.Environment.NewLine}\t\t<TABLE BORDER=\"0\" CELLBORDER=\"1\" CELLSPACING=\"0\" BGCOLOR=\"yellow\">{System.Environment.NewLine}\t\t<TR><TD><sup>enteredA</sup></TD></TR>{System.Environment.NewLine}\t\t</TABLE>{System.Environment.NewLine}{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t<TR><TD PORT=\"A\">A</TD></TR><tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t</TABLE>>,shape=\"plaintext\",color=\"blue\" ];{System.Environment.NewLine}{System.Environment.NewLine}}}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(OnEntry, "enteredA");

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void OnExitWithAnonymousActionAndDescription()
        {
            var expected = $"digraph {{{System.Environment.NewLine}compound=true;{System.Environment.NewLine}rankdir=\"LR\"{System.Environment.NewLine}\tA [   label=<{System.Environment.NewLine}\t<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >{System.Environment.NewLine}\t<tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t<TR><TD PORT=\"A\">A</TD></TR><tr><td>{System.Environment.NewLine}\t\t<TABLE BORDER=\"0\" CELLBORDER=\"1\" CELLSPACING=\"0\" BGCOLOR=\"yellow\">{System.Environment.NewLine}\t\t<TR><TD><sup>exitA</sup></TD></TR>{System.Environment.NewLine}\t\t</TABLE>{System.Environment.NewLine}{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t</TABLE>>,shape=\"plaintext\",color=\"blue\" ];{System.Environment.NewLine}{System.Environment.NewLine}}}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(() => { }, "exitA");

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void OnExitWithNamedDelegateActionAndDescription()
        {
            var expected = $"digraph {{{System.Environment.NewLine}compound=true;{System.Environment.NewLine}rankdir=\"LR\"{System.Environment.NewLine}\tA [   label=<{System.Environment.NewLine}\t<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >{System.Environment.NewLine}\t<tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t<TR><TD PORT=\"A\">A</TD></TR><tr><td>{System.Environment.NewLine}\t\t<TABLE BORDER=\"0\" CELLBORDER=\"1\" CELLSPACING=\"0\" BGCOLOR=\"yellow\">{System.Environment.NewLine}\t\t<TR><TD><sup>exitA</sup></TD></TR>{System.Environment.NewLine}\t\t</TABLE>{System.Environment.NewLine}{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t</TABLE>>,shape=\"plaintext\",color=\"blue\" ];{System.Environment.NewLine}{System.Environment.NewLine}}}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(OnExit, "exitA");

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void TransitionWithIgnore()
        {
            // Ignored triggers do not appear in the graph
            var expected = $"digraph {{{System.Environment.NewLine}compound=true;{System.Environment.NewLine}rankdir=\"LR\"{System.Environment.NewLine}\tA [   label=<{System.Environment.NewLine}\t<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >{System.Environment.NewLine}\t<tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t<TR><TD PORT=\"A\">A</TD></TR><tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t</TABLE>>,shape=\"plaintext\",color=\"blue\" ];{System.Environment.NewLine}{System.Environment.NewLine}A -> B [   style=\"solid\",label=\"X\" ];  {System.Environment.NewLine}}}";

        var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Ignore(Trigger.Y)
                .Permit(Trigger.X, State.B);

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void test()
        {
            // Ignored triggers do not appear in the graph
            var expected = $"digraph {{{System.Environment.NewLine}compound=true;{System.Environment.NewLine}rankdir=\"LR\"{System.Environment.NewLine}\tA [   label=<{System.Environment.NewLine}\t<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >{System.Environment.NewLine}\t<tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t<TR><TD PORT=\"A\">A</TD></TR><tr><td>{System.Environment.NewLine}\t</td></tr>{System.Environment.NewLine}\t</TABLE>>,shape=\"plaintext\",color=\"blue\" ];{System.Environment.NewLine}{System.Environment.NewLine}A -> B [   style=\"solid\",label=\"X\" ];  {System.Environment.NewLine}}}";
            Func<bool> anonymousGuard = () => true;
            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
            .OnEntry(() => { },"OnEntry")
            .Permit(Trigger.X, State.B)
            .PermitIf(Trigger.Y, State.C, anonymousGuard, "IsTriggerY")
            .PermitIf(Trigger.Z, State.B, anonymousGuard, "IsTriggerZ");
            Assert.AreEqual(expected, sm.ToDotGraph());
        }
    }
}
