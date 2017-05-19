// #define WRITE_DOTS_TO_FOLDER

using System;
using System.Collections.Generic;
using Xunit;
using Stateless.Reflection;
using Stateless.DotGraph;

namespace Stateless.Tests
{
    public class DotGraphFixture
    {
#if WRITE_DOTS_TO_FOLDER
        static readonly string DestinationFolder = "c:\\temp\\";
#endif

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

        static readonly string prefix = $"digraph {{{System.Environment.NewLine}compound=true;{System.Environment.NewLine}rankdir=\"LR\"{System.Environment.NewLine}";
        static readonly string suffix = System.Environment.NewLine + "}";

        string box(string label, List<String> entries = null, List<String> exits = null)
        {
            string b = $"\t{label} [   label=<{System.Environment.NewLine}\t"
                + $"<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >{System.Environment.NewLine}\t"
                + $"<tr><td>{System.Environment.NewLine}";

            if (entries != null)
            {
                b += $"\t\t<TABLE BORDER=\"0\" CELLBORDER=\"1\" CELLSPACING=\"0\" BGCOLOR=\"yellow\">{System.Environment.NewLine}";
                foreach (string entry in entries)
                    b += $"\t\t<TR><TD><sup>" + entry + $"</sup></TD></TR>{System.Environment.NewLine}";
                b += "\t\t</TABLE>" + System.Environment.NewLine + System.Environment.NewLine;
            }

            b += $"\t</td></tr>{System.Environment.NewLine}\t"
                + $"<TR><TD PORT=\"{label}\">{label}</TD></TR>"
                + $"<tr><td>{System.Environment.NewLine}";

            if (exits != null)
            {
                b += $"\t\t<TABLE BORDER=\"0\" CELLBORDER=\"1\" CELLSPACING=\"0\" BGCOLOR=\"yellow\">{System.Environment.NewLine}";
                foreach (string exit in exits)
                    b += $"\t\t<TR><TD><sup>" + exit + $"</sup></TD></TR>{System.Environment.NewLine}";
                b += "\t\t</TABLE>" + System.Environment.NewLine + System.Environment.NewLine;
            }

            b += $"\t</td></tr>{System.Environment.NewLine}\t</TABLE>>,shape=\"plaintext\",color=\"blue\" ];{System.Environment.NewLine}";

            return b;
        }

        string line(string from, string to, string label)
        {
            return $"{System.Environment.NewLine}" + from + " -> " + to + " [   style=\"solid\",label=\"" + label + "\" ];";
        }

        [Fact]
        public void SimpleTransition()
        {
            var expected = prefix + box("A") + box("B") + line("A", "B", "X") + " " + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            Assert.Equal(expected, new DotGraphFormatter().ToDotGraph(sm.GetInfo()));
        }

        [Fact]
        public void TwoSimpleTransitions()
        {
            var expected = prefix + box("A") + box("B") + box("C")
                + line("A", "B", "X") + line("A", "C", "Y")
                + " " + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B)
                .Permit(Trigger.Y, State.C);

            Assert.Equal(expected, new DotGraphFormatter().ToDotGraph(sm.GetInfo()));
        }

        [Fact]
        public void WhenDiscriminatedByAnonymousGuard()
        {
            Func<bool> anonymousGuard = () => true;

            var expected = prefix + box("A") + box("B")
                + line("A", "B", "X " + InvocationInfo.DefaultFunctionDescription)
                + " " + suffix;
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, anonymousGuard);
            sm.Configure(State.B);

            Assert.Equal(expected, new DotGraphFormatter().ToDotGraph(sm.GetInfo()));
        }

        [Fact]
        public void WhenDiscriminatedByAnonymousGuardWithDescription()
        {
            Func<bool> anonymousGuard = () => true;

            var expected = prefix + box("A") + box("B") + line("A", "B", "X description") + " " + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, anonymousGuard, "description");

            Assert.Equal(expected, new DotGraphFormatter().ToDotGraph(sm.GetInfo()));
        }

        [Fact]
        public void WhenDiscriminatedByNamedDelegate()
        {
            var expected = prefix + box("A") + box("B") + line("A", "B", "X IsTrue") + " " + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue);

            Assert.Equal(expected, new DotGraphFormatter().ToDotGraph(sm.GetInfo()));
        }

        [Fact]
        public void WhenDiscriminatedByNamedDelegateWithDescription()
        {
            var expected = prefix + box("A") + box("B") + line("A", "B", "X description") + " " + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue, "description");
            sm.Configure(State.B);
            Assert.Equal(expected, new DotGraphFormatter().ToDotGraph(sm.GetInfo()));
        }

        [Fact]
        public void DestinationStateIsDynamic()
        {
            var expected = prefix + box("A") + line("A", "Dynamic", "X") + " " + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .PermitDynamic(Trigger.X, () => State.B);

            string dotGraph = new DotGraphFormatter().ToDotGraph(sm.GetInfo());

            int len = Math.Min(dotGraph.Length, expected.Length);
            for (int i = 0; i < len; i += 10)
            {
                int c = Math.Min(10, len - i);
                if (expected.Substring(i, c) != dotGraph.Substring(i, c))
                {
                    //for (int j = 0; j < c; j++)
                    //    Assert.Equal(expected[i + j], dotGraph[i + j]);
                    Assert.Equal(expected.Substring(i, c), dotGraph.Substring(i, c));
                    break;
                }
            }
            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void DestinationStateIsCalculatedBasedOnTriggerParameters()
        {
            var expected = prefix + box("A") + line("A", "Dynamic", "X") + " " + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitDynamic(trigger, i => i == 1 ? State.B : State.C);

            string dotGraph = new DotGraphFormatter().ToDotGraph(sm.GetInfo());

#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "DestinationStateIsCalculatedBasedOnTriggerParameters.dot", dotGraph);
#endif
            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void OnEntryWithAnonymousActionAndDescription()
        {
            var expected = prefix + box("A", new List<string>() { "enteredA" }) + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => { }, "enteredA");

            Assert.Equal(expected, new DotGraphFormatter().ToDotGraph(sm.GetInfo()));
        }

        [Fact]
        public void OnEntryWithNamedDelegateActionAndDescription()
        {
            var expected = prefix + box("A", new List<string>() { "enteredA" }) + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(OnEntry, "enteredA");

            Assert.Equal(expected, new DotGraphFormatter().ToDotGraph(sm.GetInfo()));
        }

        [Fact]
        public void OnExitWithAnonymousActionAndDescription()
        {
            var expected = prefix + box("A", null, new List<string>() { "exitA" }) + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(() => { }, "exitA");

            Assert.Equal(expected, new DotGraphFormatter().ToDotGraph(sm.GetInfo()));
        }

        [Fact]
        public void OnExitWithNamedDelegateActionAndDescription()
        {
            var expected = prefix + box("A", null, new List<string>() { "exitA" }) + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(OnExit, "exitA");

            Assert.Equal(expected, new DotGraphFormatter().ToDotGraph(sm.GetInfo()));
        }

        [Fact]
        public void TransitionWithIgnore()
        {
            // Ignored triggers do not appear in the graph
            var expected = prefix + box("A") + box("B") + line("A", "B", "X") + " " + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Ignore(Trigger.Y)
                .Permit(Trigger.X, State.B);

            Assert.Equal(expected, new DotGraphFormatter().ToDotGraph(sm.GetInfo()));
        }

        [Fact]
        public void OnEntryWithTriggerParameter()
        {
            var expected = prefix + box("A", new List<string>() { "OnEntry", "TestEntryAction", "TestEntryActionString" })
                + box("B") + box("C")
                + line("A", "B", "X") + line("A", "C", "Y IsTriggerY") + line("A", "B", "Z IsTriggerZ")
                + " " + suffix;

            Func<bool> anonymousGuard = () => true;
            var sm = new StateMachine<State, Trigger>(State.A);
            var parmTrig = sm.SetTriggerParameters<string>(Trigger.Y);

            sm.Configure(State.A)
                .OnEntry(() => { }, "OnEntry")
                .OnEntryFrom(Trigger.X, TestEntryAction)
                .OnEntryFrom(parmTrig, TestEntryActionString)
                .Permit(Trigger.X, State.B)
                .PermitIf(Trigger.Y, State.C, anonymousGuard, "IsTriggerY")
                .PermitIf(Trigger.Z, State.B, anonymousGuard, "IsTriggerZ");

            string dotGraph = new DotGraphFormatter().ToDotGraph(sm.GetInfo());
#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "OnEntryWithTriggerParameter.dot", dotGraph);
#endif

            Assert.Equal(expected, dotGraph);
        }

        private void TestEntryAction() { }
        private void TestEntryActionString(string val) { }
    }
}
