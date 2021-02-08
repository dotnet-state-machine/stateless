﻿// #define WRITE_DOTS_TO_FOLDER

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
            + $" init -> A[style = \"solid\"]" + Environment.NewLine
            + "}";

        string Prefix(Style style)
        {
            string s;

            s = "digraph {\n"
                + "compound=true;\n"
                + "node [shape=Mrecord]\n"
                + "rankdir=\"LR\"\n";

            return s.Replace("\n", Environment.NewLine);
        }

        string Box(Style style, string label, List<String> entries = null, List<String> exits = null)
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
                b = label + " [label=\"" + label + "\"];\n";
            else
            {
                b = label + " [label=\"" + label + "|" + String.Join("\\n", es) + "\"];\n";
            }

            return b.Replace("\n", Environment.NewLine);
        }

        string Decision(Style style, string nodeName, string label)
        {
            string b;

            b = nodeName + " [shape = \"diamond\", label = \"" + label + "\"];\n";

            return b.Replace("\n", Environment.NewLine);
        }

        string Line(string from, string to, string label)
        {
            string s = "\n" + from + " -> " + to
                + " [style=\"solid\"";

            if (label != null)
                s += ", label=\"" + label + "\"";

            s += "];";

            return s.Replace("\n", Environment.NewLine);
        }

        string Subgraph(Style style, string graphName, string label, string contents)
        {
            if (style != Style.UML)
                throw new Exception("WRITE MORE CODE");

            string s = "\n"
                + "subgraph cluster" + graphName + "\n"
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
            var expected = Prefix(Style.UML) + Box(Style.UML, "A") + Box(Style.UML, "B") + Line("A", "B", "X") + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Transition(Trigger.X).To(State.B);

            string dotGraph = UmlDotGraph.Format(sm.GetInfo());

#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "SimpleTransition.dot", dotGraph);
#endif

            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void SimpleTransitionUML()
        {
            var expected = Prefix(Style.UML) + Box(Style.UML, "A") + Box(Style.UML, "B") + Line("A", "B", "X") + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Transition(Trigger.X).To(State.B);

            string dotGraph = UmlDotGraph.Format(sm.GetInfo());

#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "SimpleTransitionUML.dot", dotGraph);
#endif

            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void TwoSimpleTransitions()
        {
            var expected = Prefix(Style.UML) + Box(Style.UML, "A") + Box(Style.UML, "B") + Box(Style.UML, "C")
                + Line("A", "B", "X")
                + Line("A", "C", "Y")
                + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Transition(Trigger.X).To(State.B)
                .Transition(Trigger.Y).To(State.C);

            Assert.Equal(expected, UmlDotGraph.Format(sm.GetInfo()));
        }

        [Fact]
        public void WhenDiscriminatedByAnonymousGuard()
        {
            bool anonymousGuard() => true;

            var expected = Prefix(Style.UML) + Box(Style.UML, "A") + Box(Style.UML, "B")
                + Line("A", "B", "X [" + InvocationInfo.DefaultFunctionDescription + "]")
                + suffix;
            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Transition(Trigger.X).To(State.B).If(anonymousGuard);

            sm.Configure(State.B);

            Assert.Equal(expected, UmlDotGraph.Format(sm.GetInfo()));
        }

        [Fact]
        public void WhenDiscriminatedByAnonymousGuardWithDescription()
        {
            bool anonymousGuard() => true;

            var expected = Prefix(Style.UML)
                + Box(Style.UML, "A") + Box(Style.UML, "B")
                + Line("A", "B", "X [description]")
                +  suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Transition(Trigger.X).To(State.B).If(anonymousGuard, "description");

            string dotGraph = UmlDotGraph.Format(sm.GetInfo());

#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "WhenDiscriminatedByAnonymousGuardWithDescription.dot", dotGraph);
#endif

            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void WhenDiscriminatedByNamedDelegate()
        {
            var expected = Prefix(Style.UML)
                + Box(Style.UML, "A") + Box(Style.UML, "B")
                + Line("A", "B", "X [IsTrue]")
                + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Transition(Trigger.X).To(State.B).If(IsTrue);

            Assert.Equal(expected, UmlDotGraph.Format(sm.GetInfo()));
        }

        [Fact]
        public void WhenDiscriminatedByNamedDelegateWithDescription()
        {
            var expected = Prefix(Style.UML)
                + Box(Style.UML, "A") + Box(Style.UML, "B")
                + Line("A", "B", "X [description]")
                + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Transition(Trigger.X).To(State.B).If(IsTrue, "description");
            sm.Configure(State.B);
            Assert.Equal(expected, UmlDotGraph.Format(sm.GetInfo()));
        }

        [Fact]
        public void DestinationStateIsDynamic()
        {
            var expected = Prefix(Style.UML)
                + Box(Style.UML, "A")
                + Decision(Style.UML, "Decision1", "Function")
                + Line("A", "Decision1", "X") + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                    .Transition(Trigger.X).Dynamic(() => State.B);

            string dotGraph = UmlDotGraph.Format(sm.GetInfo());

#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "DestinationStateIsDynamic.dot", dotGraph);
#endif

            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void DestinationStateIsCalculatedBasedOnTriggerParameters()
        {
            var expected = Prefix(Style.UML)
                + Box(Style.UML, "A")
                + Decision(Style.UML, "Decision1", "Function")
                + Line("A", "Decision1", "X") + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .Transition(trigger).Dynamic<int>(i => i == 1 ? State.B : State.C);
                

            string dotGraph = UmlDotGraph.Format(sm.GetInfo());

#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(DestinationFolder + "DestinationStateIsCalculatedBasedOnTriggerParameters.dot", dotGraph);
#endif
            Assert.Equal(expected, dotGraph);
        }

        [Fact]
        public void OnEntryWithAnonymousActionAndDescription()
        {
            var expected = Prefix(Style.UML) + Box(Style.UML, "A", new List<string> { "enteredA" }) + suffix;

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
            var expected = Prefix(Style.UML) + Box(Style.UML, "A", new List<string> { "enteredA" }) + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(OnEntry, "enteredA");

            Assert.Equal(expected, UmlDotGraph.Format(sm.GetInfo()));
        }

        [Fact]
        public void OnExitWithAnonymousActionAndDescription()
        {
            var expected = Prefix(Style.UML) + Box(Style.UML, "A", null, new List<string> { "exitA" }) + suffix;

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

            var expected = Prefix(Style.UML) + Box(Style.UML, "A", null, new List<string> { "exitA" }) + suffix;
            Assert.Equal(expected, UmlDotGraph.Format(sm.GetInfo()));
            expected = Prefix(Style.UML) + Box(Style.UML, "A", null, new List<string> { "exitA" }) + suffix;
            Assert.Equal(expected, UmlDotGraph.Format(sm.GetInfo()));
        }

        [Fact]
        public void TransitionWithIgnore()
        {
            // Ignored triggers do not appear in the graph
            var expected = Prefix(Style.UML)
                + Box(Style.UML, "A") + Box(Style.UML, "B")
                + Line("A", "B", "X")
                + Line("A", "A", "Y")
                + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Ignore(Trigger.Y)
                .Transition(Trigger.X).To(State.B);

            Assert.Equal(expected, UmlDotGraph.Format(sm.GetInfo()));
        }

        [Fact]
        public void OnEntryWithTriggerParameter()
        {
            var expected = Prefix(Style.UML) + Box(Style.UML, "A", new List<string> { "OnEntry" })
                + Box(Style.UML, "B") + Box(Style.UML, "C")
                + Line("A", "B", "X / BX")
                + Line("A", "C", "Y / TestEntryActionString [IsTriggerY]")
                + Line("A", "B", "Z [IsTriggerZ]")
                + suffix;

            bool anonymousGuard() => true;
            var sm = new StateMachine<State, Trigger>(State.A);
            var parmTrig = sm.SetTriggerParameters<string>(Trigger.Y);

            sm.Configure(State.A)
                .OnEntry(() => { }, "OnEntry")
                .Transition(Trigger.X).To(State.B)
                .Transition(Trigger.Y).To(State.C).If(anonymousGuard, "IsTriggerY")
                .Transition(Trigger.Z).To(State.B).If(anonymousGuard, "IsTriggerZ");

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
        public void UmlWithSubstate()
        {
            var expected = Prefix(Style.UML)
                + Subgraph(Style.UML, "D", "D\\n----------\\nentry / EnterD",
                    Box(Style.UML, "B")
                    + Box(Style.UML, "C"))
                + Box(Style.UML, "A", new List<string> { "EnterA" }, new List<string> { "ExitA" })
                + Line("A", "B", "X") + Line("A", "C", "Y")
                + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(TestEntryAction, "EnterA")
                .OnExit(TestEntryAction, "ExitA")
                .Transition(Trigger.X).To(State.B)
                .Transition(Trigger.Y).To(State.C);

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
            var expected = Prefix(Style.UML)
                + Box(Style.UML, "A")
                + Box(Style.UML, "B")
                + Box(Style.UML, "C")
                + Decision(Style.UML, "Decision1", "DestinationSelector")
                + Line("A", "Decision1", "X")
                + Line("Decision1", "B", "X [ChoseB]")
                + Line("Decision1", "C", "X [ChoseC]")
                + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Transition(Trigger.X).Dynamic(DestinationSelector, null, new DynamicStateInfos { { State.B, "ChoseB" }, { State.C, "ChoseC" } });

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
            var expected = Prefix(Style.UML)
                + Box(Style.UML, "A", new List<string> { "DoEntry" })
                + Box(Style.UML, "B", new List<string> { "DoThisEntry" })
                + Line("A", "B", "X")
                + Line("A", "A", "Y") 
                + Line("B", "B", "Z / DoThisEntry")
                + suffix;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(TestEntryAction, "DoEntry")
                .Ignore(Trigger.Y)
                .Transition(Trigger.X).To(State.B);

            sm.Configure(State.B)
                .OnEntry(TestEntryAction, "DoThisEntry")
                .Transition(Trigger.Z).Self();

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
