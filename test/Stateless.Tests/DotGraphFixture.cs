#define WRITE_DOTS_TO_FOLDER

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

        enum Style
        {
            SLE,
            UML
        }

        static readonly string suffix = System.Environment.NewLine + "}";

        string prefix(Style style)
        {
            string s;

            if (style == Style.SLE)
                s = "digraph {\ncompound=true;\nrankdir=\"LR\"\n";
            else
            { 
                s = "digraph {\n"
                    + "compound=true;\n"
                    + "node [shape=Mrecord]\n"
                    + "rankdir=\"LR\"\n";
            }
            return s.Replace("\n", System.Environment.NewLine);
        }

        string box(Style style, string label, List<String> entries = null, List<String> exits = null)
        {
            string b;

            if (style == Style.SLE)
            {
                b = $"\t{label} [   label=<\n\t"
                    + $"<TABLE BORDER=\"1\" CELLBORDER=\"1\" CELLSPACING=\"0\" >\n\t"
                    + $"<tr><td>\n";

                if (entries != null)
                {
                    b += $"\t\t<TABLE BORDER=\"0\" CELLBORDER=\"1\" CELLSPACING=\"0\" BGCOLOR=\"yellow\">\n";
                    foreach (string entry in entries)
                        b += $"\t\t<TR><TD><sup>" + entry + $"</sup></TD></TR>\n";
                    b += "\t\t</TABLE>\n\n";
                }

                b += $"\t</td></tr>\n\t"
                    + $"<TR><TD PORT=\"{label}\">{label}</TD></TR>"
                    + $"<tr><td>\n";

                if (exits != null)
                {
                    b += $"\t\t<TABLE BORDER=\"0\" CELLBORDER=\"1\" CELLSPACING=\"0\" BGCOLOR=\"yellow\">\n";
                    foreach (string exit in exits)
                        b += $"\t\t<TR><TD><sup>" + exit + $"</sup></TD></TR>\n";
                    b += "\t\t</TABLE>\n\n";
                }

                b += $"\t</td></tr>\n\t</TABLE>>,shape=\"plaintext\",color=\"blue\" ];\n";
            }
            else
            {
                List<string> es = new List<string>();
                if (entries != null)
                {
                    foreach (string entry in entries)
                        es.Add("entry / " + entry);
                }
                if (exits != null)
                {
                    foreach (string exit in exits)
                        es.Add("exit / " + exit);
                }

                if (es.Count == 0)
                    b = label + " [label = \"" + label + "\"]\n";
                else
                {
                    b = label + " [label = \"" + label + "|" + String.Join("\\n", es) + "\"]\n";
                }
            }

            return b.Replace("\n", System.Environment.NewLine);
        }

        string decision(Style style, string nodeName, string label)
        {
            string b;

            b = "\n" + nodeName + " [shape = \"diamond\"; label = \"" + label + "\"]\n";

            return b.Replace("\n", System.Environment.NewLine);
        }

        string line(string from, string to, string label)
        {
            string s = "\n" + from + " -> " + to
                + " [   style=\"solid\"";

            if (label != null)
                s += ",label=\"" + label + "\"";

            s += " ];";

            return s.Replace("\n", System.Environment.NewLine);
        }

        string subgraph(Style style, string graphName, string label, string contents)
        {
            if (style != Style.UML)
                throw new Exception("WRITE MORE CODE");

            string s = "\n"
                + "subgraph " + graphName + "\n"
                + "\t{\n"
                + "\tlabel = \"" + label + "\"\n";

            s = s.Replace("\n", System.Environment.NewLine)
                + contents          // \n already replaced with NewLine
                + "}" + System.Environment.NewLine;

            return s;
        }

        [Fact]
        public void SimpleTransition()
        {
            var expected = prefix(Style.SLE) + box(Style.SLE, "A") + box(Style.SLE, "B") + line("A", "B", "X") + " " + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            string dotGraph = DotGraphFormatter.Format(sm.GetInfo(), new SleGraphStyle());

#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "SimpleTransition.dot", dotGraph);
#endif

            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void SimpleTransitionUML()
        {
            var expected = prefix(Style.UML) + box(Style.UML, "A") + box(Style.UML, "B") + line("A", "B", "X") + " " + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            string dotGraph = DotGraphFormatter.Format(sm.GetInfo(), new UmlGraphStyle());

#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "SimpleTransitionUML.dot", dotGraph);
#endif

            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void TwoSimpleTransitions()
        {
            var expected = prefix(Style.SLE) + box(Style.SLE, "A") + box(Style.SLE, "B") + box(Style.SLE, "C")
                + line("A", "B", "X") + line("A", "C", "Y")
                + " " + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B)
                .Permit(Trigger.Y, State.C);

            Assert.Equal(expected, DotGraphFormatter.Format(sm.GetInfo(), new SleGraphStyle()));
        }

        [Fact]
        public void WhenDiscriminatedByAnonymousGuard()
        {
            Func<bool> anonymousGuard = () => true;

            var expected = prefix(Style.SLE) + box(Style.SLE, "A") + box(Style.SLE, "B")
                + line("A", "B", "X " + InvocationInfo.DefaultFunctionDescription)
                + " " + suffix;
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, anonymousGuard);
            sm.Configure(State.B);

            Assert.Equal(expected, DotGraphFormatter.Format(sm.GetInfo(), new SleGraphStyle()));
        }

        [Fact]
        public void WhenDiscriminatedByAnonymousGuardWithDescription()
        {
            Func<bool> anonymousGuard = () => true;

            var expected = prefix(Style.SLE) + box(Style.SLE, "A") + box(Style.SLE, "B") + line("A", "B", "X description") + " " + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, anonymousGuard, "description");

            string dotGraph = DotGraphFormatter.Format(sm.GetInfo(), new SleGraphStyle());

#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "WhenDiscriminatedByAnonymousGuardWithDescription.dot", dotGraph);
#endif

            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void WhenDiscriminatedByNamedDelegate()
        {
            var expected = prefix(Style.SLE) + box(Style.SLE, "A") + box(Style.SLE, "B") + line("A", "B", "X IsTrue") + " " + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue);

            Assert.Equal(expected, DotGraphFormatter.Format(sm.GetInfo(), new SleGraphStyle()));
        }

        [Fact]
        public void WhenDiscriminatedByNamedDelegateWithDescription()
        {
            var expected = prefix(Style.SLE) + box(Style.SLE, "A") + box(Style.SLE, "B") + line("A", "B", "X description") + " " + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue, "description");
            sm.Configure(State.B);
            Assert.Equal(expected, DotGraphFormatter.Format(sm.GetInfo(), new SleGraphStyle()));
        }

        [Fact]
        public void DestinationStateIsDynamic()
        {
            var expected = prefix(Style.SLE)
                + box(Style.SLE, "A")
                + decision(Style.SLE, "Decision1", "Function")
                + line("A", "Decision1", "X") + " " + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .PermitDynamic(Trigger.X, () => State.B);

            string dotGraph = DotGraphFormatter.Format(sm.GetInfo(), new SleGraphStyle());

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
            var expected = prefix(Style.SLE)
                + box(Style.SLE, "A")
                + decision(Style.SLE, "Decision1", "Function")
                + line("A", "Decision1", "X") + " " + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitDynamic(trigger, i => i == 1 ? State.B : State.C);

            string dotGraph = DotGraphFormatter.Format(sm.GetInfo(), new SleGraphStyle());

#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "DestinationStateIsCalculatedBasedOnTriggerParameters.dot", dotGraph);
#endif
            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void OnEntryWithAnonymousActionAndDescription()
        {
            var expected = prefix(Style.SLE) + box(Style.SLE, "A", new List<string>() { "enteredA" }) + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => { }, "enteredA");

            string dotGraph = DotGraphFormatter.Format(sm.GetInfo(), new SleGraphStyle());

#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "OnEntryWithAnonymousActionAndDescription.dot", dotGraph);
#endif

            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void OnEntryWithNamedDelegateActionAndDescription()
        {
            var expected = prefix(Style.SLE) + box(Style.SLE, "A", new List<string>() { "enteredA" }) + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(OnEntry, "enteredA");

            Assert.Equal(expected, DotGraphFormatter.Format(sm.GetInfo(), new SleGraphStyle()));
        }

        [Fact]
        public void OnExitWithAnonymousActionAndDescription()
        {
            var expected = prefix(Style.SLE) + box(Style.SLE, "A", null, new List<string>() { "exitA" }) + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(() => { }, "exitA");

            Assert.Equal(expected, DotGraphFormatter.Format(sm.GetInfo(), new SleGraphStyle()));
        }

        [Fact]
        public void OnExitWithNamedDelegateActionAndDescription()
        {
            var expected = prefix(Style.SLE) + box(Style.SLE, "A", null, new List<string>() { "exitA" }) + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(OnExit, "exitA");

            Assert.Equal(expected, DotGraphFormatter.Format(sm.GetInfo(), new SleGraphStyle()));
        }

        [Fact]
        public void TransitionWithIgnore()
        {
            // Ignored triggers do not appear in the graph
            var expected = prefix(Style.SLE) + box(Style.SLE, "A") + box(Style.SLE, "B") + line("A", "B", "X") + " " + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Ignore(Trigger.Y)
                .Permit(Trigger.X, State.B);

            Assert.Equal(expected, DotGraphFormatter.Format(sm.GetInfo(), new SleGraphStyle()));
        }

        [Fact]
        public void OnEntryWithTriggerParameter()
        {
            var expected = prefix(Style.SLE) + box(Style.SLE, "A", new List<string>() { "OnEntry", "TestEntryAction", "TestEntryActionString" })
                + box(Style.SLE, "B") + box(Style.SLE, "C")
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

            string dotGraph = DotGraphFormatter.Format(sm.GetInfo(), new SleGraphStyle());
#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "OnEntryWithTriggerParameter.dot", dotGraph);
#endif

            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void UmlWithSubstate()
        {
            var expected = prefix(Style.UML)
                + subgraph(Style.UML, "cluster0", "D",
                    box(Style.UML, "D")
                    + box(Style.UML, "B")
                    + box(Style.UML, "C"))
                + box(Style.UML, "A", new List<string>() { "EnterA" }, new List<string>() { "ExitA" })
                + line("A", "B", "X") + line("A", "C", "Y")
                + " " + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B)
                .Permit(Trigger.Y, State.C)
                .OnEntry(TestEntryAction, "EnterA")
                .OnExit(TestEntryAction, "ExitA");

            sm.Configure(State.B)
                .SubstateOf(State.D);
            sm.Configure(State.C)
                .SubstateOf(State.D);

            string dotGraph = DotGraphFormatter.Format(sm.GetInfo(), new UmlGraphStyle());
#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "UmlWithSubstate.dot", dotGraph);
#endif

            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void UmlWithDynamic()
        {
            var expected = prefix(Style.UML)
                + box(Style.UML, "A")
                + decision(Style.UML, "Decision1", "DestinationSelector")
                + line("A", "Decision1", "X")
                + line("Decision1", "B", null)
                + line("Decision1", "C", null)
                + " " + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitDynamic(Trigger.X, DestinationSelector, null, new State[] { State.B, State.C });

            string dotGraph = DotGraphFormatter.Format(sm.GetInfo(), new UmlGraphStyle());
#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "UmlWithDynamic.dot", dotGraph);
#endif

            Assert.Equal(expected, dotGraph);
        }

        private void TestEntryAction() { }
        private void TestEntryActionString(string val) { }
        private State DestinationSelector() { return State.A; }
    }
}
