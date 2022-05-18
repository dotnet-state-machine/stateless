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
            Uml
        }

        static readonly string Suffix =
            $"{Environment.NewLine} init [label=\"\", shape=point];{Environment.NewLine} init -> \"A\"[style = \"solid\"]{Environment.NewLine}}}";

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
            if (entries is { })
            {
                foreach (string entry in entries)
                    es.Add($"entry / {entry}");
            }
            if (exits is { })
            {
                foreach (string exit in exits)
                    es.Add($"exit / {exit}");
            }

            if (es.Count == 0)
                b = $"\"{label}\" [label=\"{label}\"];\n";
            else
            {
                b = $"\"{label}\" [label=\"{label}|{string.Join("\\n", es)}\"];\n";
            }

            return b.Replace("\n", Environment.NewLine);
        }

        static string Decision(string nodeName, string label)
        {
            var b = $"\"{nodeName}\" [shape = \"diamond\", label = \"{label}\"];\n";

            return b.Replace("\n", Environment.NewLine);
        }

        static string Line(string from, string to, string label)
        {
            string s = $"\n\"{from}\" -> \"{to}\" [style=\"solid\"";

            if (label is { })
                s += $", label=\"{label}\"";

            s += "];";

            return s.Replace("\n", Environment.NewLine);
        }

        static string Subgraph(Style style, string graphName, string label, string contents)
        {
            if (style != Style.Uml)
                throw new Exception("WRITE MORE CODE");

            string s = $"\nsubgraph \"cluster{graphName}\"\n\t{{\n\tlabel = \"{label}\"\n";

            s = $"{s.Replace("\n", Environment.NewLine)}{contents}}}{Environment.NewLine}";

            return s;
        }

        [Fact]
        public void SimpleTransition()
        {
            var expected = Prefix() + Box("A") + Box("B") + Line("A", "B", "X") + Suffix;

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
        public void SimpleTransitionUml()
        {
            var expected = Prefix() + Box("A") + Box("B") + Line("A", "B", "X") + Suffix;

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
                + Suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B)
                .Permit(Trigger.Y, State.C);

            Assert.Equal(expected, UmlDotGraph.Format(sm.GetInfo()));
        }

        [Fact]
        public void WhenDiscriminatedByAnonymousGuard()
        {
            static bool AnonymousGuard() => true;

            var expected = Prefix() + Box("A") + Box("B")
                + Line("A", "B", $"X [{InvocationInfo.DefaultFunctionDescription}]")
                + Suffix;
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, AnonymousGuard);
            sm.Configure(State.B);

            Assert.Equal(expected, UmlDotGraph.Format(sm.GetInfo()));
        }

        [Fact]
        public void WhenDiscriminatedByAnonymousGuardWithDescription()
        {
            static bool AnonymousGuard() => true;

            var expected = Prefix()
                + Box("A") + Box("B")
                + Line("A", "B", "X [description]")
                +  Suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, AnonymousGuard, "description");

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
                + Suffix;

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
                + Suffix;

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
                + Line("A", "Decision1", "X") + Suffix;

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
                + Line("A", "Decision1", "X") + Suffix;

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
            var expected = Prefix() + Box("A", new List<string> { "enteredA" }) + Suffix;

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
            var expected = Prefix() + Box("A", new List<string> { "enteredA" }) + Suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(OnEntry, "enteredA");

            Assert.Equal(expected, UmlDotGraph.Format(sm.GetInfo()));
        }

        [Fact]
        public void OnExitWithAnonymousActionAndDescription()
        {
            var expected = Prefix() + Box("A", null, new List<string> { "exitA" }) + Suffix;

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

            var expected = Prefix() + Box("A", null, new List<string> { "exitA" }) + Suffix;
            Assert.Equal(expected, UmlDotGraph.Format(sm.GetInfo()));
            expected = Prefix() + Box("A", null, new List<string> { "exitA" }) + Suffix;
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
                + Suffix;

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
                                    + Suffix;

            static bool AnonymousGuard() => true;
            var sm = new StateMachine<State, Trigger>(State.A);
            var parmTrig = sm.SetTriggerParameters<string>(Trigger.Y);

            sm.Configure(State.A)
                .OnEntry(() => { }, "OnEntry")
                .Permit(Trigger.X, State.B)
                .PermitIf(Trigger.Y, State.C, AnonymousGuard, "IsTriggerY")
                .PermitIf(Trigger.Z, State.B, AnonymousGuard, "IsTriggerZ");

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
            string stateA = "State A";
            string stateB = "State B";
            string stateC = "State C";
            string stateD = "State D";
            string triggerX = "Trigger X";
            string triggerY = "Trigger Y";
            
            var expected =
                $"{Prefix()}{Subgraph(Style.Uml, stateD, $"{stateD}\\n----------\\nentry / Enter D", Box(stateB) + Box(stateC))}{Box(stateA, new List<string> { "Enter A" }, new List<string> { "Exit A" })}{Line(stateA, stateB, triggerX)}{Line(stateA, stateC, triggerY)}{Environment.NewLine} init [label=\"\", shape=point];{Environment.NewLine} init -> \"{stateA}\"[style = \"solid\"]{Environment.NewLine}}}";

            var sm = new StateMachine<string, string>("State A");

            sm.Configure(stateA)
                .Permit(triggerX, stateB)
                .Permit(triggerY, stateC)
                .OnEntry(TestEntryAction, "Enter A")
                .OnExit(TestEntryAction, "Exit A");

            sm.Configure(stateB)
                .SubstateOf(stateD);
            sm.Configure(stateC)
                .SubstateOf(stateD);
            sm.Configure(stateD)
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
                + Subgraph(Style.Uml, "D", "D\\n----------\\nentry / EnterD",
                           Box("B")
                         + Box("C"))
                + Box("A", new List<string> { "EnterA" }, new List<string> { "ExitA" })
                + Line("A", "B", "X") + Line("A", "C", "Y")
                + Suffix;

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
                + Suffix;

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
                + Suffix;

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
