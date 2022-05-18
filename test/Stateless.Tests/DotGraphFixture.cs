// #define WRITE_DOTS_TO_FOLDER

using System;
using System.Collections.Generic;
using Xunit;
using Stateless.Reflection;
using Stateless.Graph;

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
            UML
        }

        static readonly string suffix = Environment.NewLine
            + $" init [label=\"\", shape=point];" + Environment.NewLine
            + $" init -> \"A\"[style = \"solid\"]" + Environment.NewLine
            + "}";

        static string Prefix()
        {
            var s = "digraph {\n"
                  + "compound=true;\n"
                  + "node [shape=Mrecord]\n"
                  + "rankdir=\"LR\"\n";

            return s.Replace("\n", Environment.NewLine);
        }

        static string Box(string label, List<string> entries = null, List<string> exits = null)
        {
            string b;

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
                b = $"\"{label}\" [label=\"{label}\"];\n";
            else
            {
                b = $"\"{label}\"" + " [label=\"" + label + "|" + string.Join("\\n", es) + "\"];\n";
            }

            return b.Replace("\n", Environment.NewLine);
        }

        static string Decision(string nodeName, string label)
        {
            var b = $"\"{nodeName}\"" + " [shape = \"diamond\", label = \"" + label + "\"];\n";

            return b.Replace("\n", Environment.NewLine);
        }

        static string Line(string from, string to, string label)
        {
            string s = "\n\"" + from + "\" -> \"" + to
                + "\" [style=\"solid\"";

            if (label != null)
                s += ", label=\"" + label + "\"";

            s += "];";

            return s.Replace("\n", Environment.NewLine);
        }

        static string Subgraph(Style style, string graphName, string label, string contents)
        {
            if (style != Style.UML)
                throw new Exception("WRITE MORE CODE");

            string s = "\n"
                + "subgraph \"cluster" + graphName + "\"\n"
                + "\t{\n"
                + "\tlabel = \"" + label + "\"\n";

            s = s.Replace("\n", Environment.NewLine)
                + contents          // \n already replaced with NewLine
                + "}" + Environment.NewLine;

            return s;
        }

        [Fact]
        public void SimpleTransition()
        {
            var expected = Prefix() + Box("A") + Box("B") + Line("A", "B", "X") + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            string dotGraph = UmlDotGraph.Format(sm.GetInfo());

#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "SimpleTransition.dot", dotGraph);
#endif

            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void SimpleTransitionUML()
        {
            var expected = Prefix() + Box("A") + Box("B") + Line("A", "B", "X") + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            string dotGraph = UmlDotGraph.Format(sm.GetInfo());

#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "SimpleTransitionUML.dot", dotGraph);
#endif

            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void TwoSimpleTransitions()
        {
            var expected = Prefix() + Box("A") + Box("B") + Box("C")
                + Line("A", "B", "X")
                + Line("A", "C", "Y")
                + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B)
                .Permit(Trigger.Y, State.C);

            Assert.Equal(expected, UmlDotGraph.Format(sm.GetInfo()));
        }

        [Fact]
        public void WhenDiscriminatedByAnonymousGuard()
        {
            static bool anonymousGuard() => true;

            var expected = Prefix() + Box("A") + Box("B")
                + Line("A", "B", "X [" + InvocationInfo.DefaultFunctionDescription + "]")
                + suffix;
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, anonymousGuard);
            sm.Configure(State.B);

            Assert.Equal(expected, UmlDotGraph.Format(sm.GetInfo()));
        }

        [Fact]
        public void WhenDiscriminatedByAnonymousGuardWithDescription()
        {
            static bool anonymousGuard() => true;

            var expected = Prefix()
                + Box("A") + Box("B")
                + Line("A", "B", "X [description]")
                +  suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, anonymousGuard, "description");

            string dotGraph = UmlDotGraph.Format(sm.GetInfo());

#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "WhenDiscriminatedByAnonymousGuardWithDescription.dot", dotGraph);
#endif

            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void WhenDiscriminatedByNamedDelegate()
        {
            var expected = Prefix()
                + Box("A") + Box("B")
                + Line("A", "B", "X [IsTrue]")
                + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue);

            Assert.Equal(expected, UmlDotGraph.Format(sm.GetInfo()));
        }

        [Fact]
        public void WhenDiscriminatedByNamedDelegateWithDescription()
        {
            var expected = Prefix()
                + Box("A") + Box("B")
                + Line("A", "B", "X [description]")
                + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue, "description");
            sm.Configure(State.B);
            Assert.Equal(expected, UmlDotGraph.Format(sm.GetInfo()));
        }

        [Fact]
        public void DestinationStateIsDynamic()
        {
            var expected = Prefix()
                + Box("A")
                + Decision("Decision1", "Function")
                + Line("A", "Decision1", "X") + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .PermitDynamic(Trigger.X, () => State.B);

            string dotGraph = UmlDotGraph.Format(sm.GetInfo());

#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "DestinationStateIsDynamic.dot", dotGraph);
#endif

            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void DestinationStateIsCalculatedBasedOnTriggerParameters()
        {
            var expected = Prefix()
                + Box("A")
                + Decision("Decision1", "Function")
                + Line("A", "Decision1", "X") + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitDynamic(trigger, i => i == 1 ? State.B : State.C);

            string dotGraph = UmlDotGraph.Format(sm.GetInfo());

#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "DestinationStateIsCalculatedBasedOnTriggerParameters.dot", dotGraph);
#endif
            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void OnEntryWithAnonymousActionAndDescription()
        {
            var expected = Prefix() + Box("A", new List<string> { "enteredA" }) + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => { }, "enteredA");

            string dotGraph = UmlDotGraph.Format(sm.GetInfo());

#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "OnEntryWithAnonymousActionAndDescription.dot", dotGraph);
#endif

            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void OnEntryWithNamedDelegateActionAndDescription()
        {
            var expected = Prefix() + Box("A", new List<string> { "enteredA" }) + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(OnEntry, "enteredA");

            Assert.Equal(expected, UmlDotGraph.Format(sm.GetInfo()));
        }

        [Fact]
        public void OnExitWithAnonymousActionAndDescription()
        {
            var expected = Prefix() + Box("A", null, new List<string> { "exitA" }) + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(() => { }, "exitA");

            Assert.Equal(expected, UmlDotGraph.Format(sm.GetInfo()));
        }

        [Fact]
        public void OnExitWithNamedDelegateActionAndDescription()
        {

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(OnExit, "exitA");

            var expected = Prefix() + Box("A", null, new List<string> { "exitA" }) + suffix;
            Assert.Equal(expected, UmlDotGraph.Format(sm.GetInfo()));
            expected = Prefix() + Box("A", null, new List<string> { "exitA" }) + suffix;
            Assert.Equal(expected, UmlDotGraph.Format(sm.GetInfo()));
        }

        [Fact]
        public void TransitionWithIgnore()
        {
            // Ignored triggers do not appear in the graph
            var expected = Prefix()
                + Box("A") + Box("B")
                + Line("A", "B", "X")
                + Line("A", "A", "Y")
                + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Ignore(Trigger.Y)
                .Permit(Trigger.X, State.B);

            Assert.Equal(expected, UmlDotGraph.Format(sm.GetInfo()));
        }

        [Fact]
        public void OnEntryWithTriggerParameter()
        {
            var expected = Prefix()            + Box("A", new List<string> { "OnEntry" })
                                    + Box("B") + Box("C")
                                    + Line("A", "B", "X / BX")
                                    + Line("A", "C", "Y / TestEntryActionString [IsTriggerY]")
                                    + Line("A", "B", "Z [IsTriggerZ]")
                                    + suffix;

            static bool anonymousGuard() => true;
            var sm = new StateMachine<State, Trigger>(State.A);
            var parmTrig = sm.SetTriggerParameters<string>(Trigger.Y);

            sm.Configure(State.A)
                .OnEntry(() => { }, "OnEntry")
                .Permit(Trigger.X, State.B)
                .PermitIf(Trigger.Y, State.C, anonymousGuard, "IsTriggerY")
                .PermitIf(Trigger.Z, State.B, anonymousGuard, "IsTriggerZ");

            sm.Configure(State.B)
                .OnEntryFrom(Trigger.X, TestEntryAction, "BX");

            sm.Configure(State.C)
                .OnEntryFrom(parmTrig, TestEntryActionString);

            string dotGraph = UmlDotGraph.Format(sm.GetInfo());
#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "OnEntryWithTriggerParameter.dot", dotGraph);
#endif

            Assert.Equal(expected, dotGraph);
        }
        
        [Fact]
        public void SpacedUmlWithSubstate()
        {
            string StateA = "State A";
            string StateB = "State B";
            string StateC = "State C";
            string StateD = "State D";
            string TriggerX = "Trigger X";
            string TriggerY = "Trigger Y";
            
            var expected = Prefix()
                           + Subgraph(Style.UML, StateD, $"{StateD}\\n----------\\nentry / Enter D",
                                      Box(StateB)
                                    + Box(StateC))
                           + Box(StateA, new List<string> { "Enter A" }, new List<string> { "Exit A" })
                           + Line(StateA, StateB, TriggerX) + Line(StateA, StateC, TriggerY)
                           +  Environment.NewLine
                           + $" init [label=\"\", shape=point];" + Environment.NewLine
                           + $" init -> \"{StateA}\"[style = \"solid\"]" + Environment.NewLine
                           + "}";

            var sm = new StateMachine<string, string>("State A");

            sm.Configure(StateA)
                .Permit(TriggerX, StateB)
                .Permit(TriggerY, StateC)
                .OnEntry(TestEntryAction, "Enter A")
                .OnExit(TestEntryAction, "Exit A");

            sm.Configure(StateB)
                .SubstateOf(StateD);
            sm.Configure(StateC)
                .SubstateOf(StateD);
            sm.Configure(StateD)
                .OnEntry(TestEntryAction, "Enter D");

            string dotGraph = UmlDotGraph.Format(sm.GetInfo());
#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "UmlWithSubstate.dot", dotGraph);
#endif

            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void UmlWithSubstate()
        {
            var expected = Prefix()
                + Subgraph(Style.UML, "D", "D\\n----------\\nentry / EnterD",
                           Box("B")
                         + Box("C"))
                + Box("A", new List<string> { "EnterA" }, new List<string> { "ExitA" })
                + Line("A", "B", "X") + Line("A", "C", "Y")
                + suffix;

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
            sm.Configure(State.D)
                .OnEntry(TestEntryAction, "EnterD");

            string dotGraph = UmlDotGraph.Format(sm.GetInfo());
#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "UmlWithSubstate.dot", dotGraph);
#endif

            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void UmlWithDynamic()
        {
            var expected = Prefix()
                + Box("A")
                + Box("B")
                + Box("C")
                + Decision("Decision1", "DestinationSelector")
                + Line("A", "Decision1", "X")
                + Line("Decision1", "B", "X [ChoseB]")
                + Line("Decision1", "C", "X [ChoseC]")
                + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitDynamic(Trigger.X, DestinationSelector, null, new DynamicStateInfos { { State.B, "ChoseB"}, { State.C, "ChoseC" } });

            sm.Configure(State.B);
            sm.Configure(State.C);

            string dotGraph = UmlDotGraph.Format(sm.GetInfo());
#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "UmlWithDynamic.dot", dotGraph);
#endif

            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void TransitionWithIgnoreAndEntry()
        {
            var expected = Prefix()
                + Box("A", new List<string> { "DoEntry" })
                + Box("B", new List<string> { "DoThisEntry" })
                + Line("A", "B", "X")
                + Line("A", "A", "Y") 
                + Line("B", "B", "Z / DoThisEntry")
                + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(TestEntryAction, "DoEntry")
                .Ignore(Trigger.Y)
                .Permit(Trigger.X, State.B);

            sm.Configure(State.B)
                .OnEntry(TestEntryAction, "DoThisEntry")
                .PermitReentry(Trigger.Z);

            string dotGraph = UmlDotGraph.Format(sm.GetInfo());

#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "TransitionWithIgnoreAndEntry.dot", dotGraph);
#endif

            Assert.Equal(expected, dotGraph);
        }

        private void TestEntryAction() { }
        private void TestEntryActionString(string val) { }
        private State DestinationSelector() { return State.A; }
    }
}
