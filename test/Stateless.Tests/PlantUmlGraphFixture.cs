using System;
using Xunit;

namespace Stateless.Tests
{
    public class PlantUmlGraphFixture
    {
        [Fact]
        public void Format_InitialTransition_ShouldReturn()
        {
            var expected = Uml(
                "[*] --> A");

            var sm = new StateMachine<State, Trigger>(State.A);

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            WriteToFile(nameof(Format_InitialTransition_ShouldReturn), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void SimpleTransition()
        {
            var expected = Uml(
                "state A",
                "state B",
                "A --> B : X",
                "[*] --> A");

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            WriteToFile(nameof(SimpleTransition), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void SimpleTransition_LeftToRight()
        {
            var expected = Uml(
                "left to right direction",
                "state A",
                "state B",
                "A --> B : X",
                "[*] --> A");

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo(), Graph.PlantUmlGraphDirection.LeftToRight);

            WriteToFile(nameof(SimpleTransition_LeftToRight), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void SimpleTransition_TopToBottom()
        {
            var expected = Uml(
                "top to bottom direction",
                "state A",
                "state B",
                "A --> B : X",
                "[*] --> A");

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo(), Graph.PlantUmlGraphDirection.TopToBottom);

            WriteToFile(nameof(SimpleTransition_TopToBottom), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void TwoSimpleTransitions()
        {
            var expected = Uml(
                "state A",
                "state B",
                "state C",
                "A --> B : X",
                "A --> C : Y",
                "[*] --> A");

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B)
                .Permit(Trigger.Y, State.C);

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            WriteToFile(nameof(TwoSimpleTransitions), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void WhenDiscriminatedByAnonymousGuard()
        {
            var expected = Uml(
                "state A",
                "state B",
                "A --> B : X [Function]",
                "[*] --> A");

            bool anonymousGuard() => true;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, anonymousGuard);
            sm.Configure(State.B);

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            WriteToFile(nameof(WhenDiscriminatedByAnonymousGuard), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void WhenDiscriminatedByAnonymousGuardWithDescription()
        {
            var expected = Uml(
                "state A",
                "state B",
                "A --> B : X [description]",
                "[*] --> A");

            bool anonymousGuard() => true;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, anonymousGuard, "description");
            sm.Configure(State.B);

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            WriteToFile(nameof(WhenDiscriminatedByAnonymousGuardWithDescription), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void WhenDiscriminatedByNamedDelegate()
        {
            var expected = Uml(
                "state A",
                "state B",
                "A --> B : X [IsTrue]",
                "[*] --> A");

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue);

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            WriteToFile(nameof(WhenDiscriminatedByNamedDelegate), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void WhenDiscriminatedByNamedDelegateWithDescription()
        {
            var expected = Uml(
                "state A",
                "state B",
                "A --> B : X [description]",
                "[*] --> A");

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue, "description");

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            WriteToFile(nameof(WhenDiscriminatedByNamedDelegateWithDescription), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void DestinationStateIsDynamic()
        {
            var expected = Uml(
                "state A",
                "state Decision1 <<choice>>",
                "A --> Decision1 : X",
                "[*] --> A");

            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .PermitDynamic(Trigger.X, () => State.B);

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            WriteToFile(nameof(DestinationStateIsDynamic), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void DestinationStateIsDynamicWithPossibleDestinations()
        {
            var expected = Uml(
                "state A",
                "state B",
                "state C",
                "state Decision1 <<choice>>",
                "A --> Decision1 : X",
                "Decision1 --> B : X [ToB]",
                "Decision1 --> C : X [ToC]",
                "[*] --> A");

            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .PermitDynamic(Trigger.X, () => State.B, null, new Reflection.DynamicStateInfos
                {
                    { State.B, "ToB" },
                    { State.C, "ToC" }
                });
            sm.Configure(State.B);
            sm.Configure(State.C);

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            WriteToFile(nameof(DestinationStateIsDynamicWithPossibleDestinations), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void DestinationStateIsCalculatedBasedOnTriggerParameters()
        {
            var expected = Uml(
                "state A",
                "state Decision1 <<choice>>",
                "A --> Decision1 : X",
                "[*] --> A");

            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitDynamic(trigger, i => i == 1 ? State.B : State.C);

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            WriteToFile(nameof(DestinationStateIsCalculatedBasedOnTriggerParameters), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void TransitionWithIgnore()
        {
            var expected = Uml(
                "state A",
                "state B",
                "A --> B : X",
                "A --> A : Y",
                "[*] --> A");

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Ignore(Trigger.Y)
                .Permit(Trigger.X, State.B);

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            WriteToFile(nameof(TransitionWithIgnore), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void OnEntryWithTriggerParameter()
        {
            var expected = Uml(
                "state A",
                "A : entry / OnEntry",
                "state B",
                "state C",
                "A --> B : X / BX",
                "A --> C : Y / TestEntryActionString [IsTriggerY]",
                "A --> B : Z [IsTriggerZ]",
                "[*] --> A");

            bool anonymousGuard() => true;
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

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            WriteToFile(nameof(OnEntryWithTriggerParameter), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void WithEntryAndExitActions()
        {
            var expected = Uml(
                "state A",
                "A : entry / Enter A",
                "A : exit / Exit A",
                "state B",
                "A --> B : X",
                "[*] --> A");

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(TestEntryAction, "Enter A")
                .OnExit(TestEntryAction, "Exit A")
                .Permit(Trigger.X, State.B);

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            WriteToFile(nameof(WithEntryAndExitActions), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ReentryShowsEntryAction()
        {
            var expected = Uml(
                "state A",
                "A : entry / Enter A",
                "A --> A : X / Enter A",
                "[*] --> A");

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(TestEntryAction, "Enter A")
                .PermitReentry(Trigger.X);

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            WriteToFile(nameof(ReentryShowsEntryAction), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void WithSubstate()
        {
            var expected = Uml(
                "state D {",
                "\tstate B",
                "\tstate C",
                "}",
                "state A",
                "A --> B : X",
                "A --> C : Y",
                "[*] --> A");

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B)
                .Permit(Trigger.Y, State.C);

            sm.Configure(State.B)
                .SubstateOf(State.D);
            sm.Configure(State.C)
                .SubstateOf(State.D);
            sm.Configure(State.D);

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            WriteToFile(nameof(WithSubstate), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void SpacedWithSubstate()
        {
            string StateA = "State A";
            string StateB = "State B";
            string StateC = "State C";
            string StateD = "State D";
            string TriggerX = "Trigger X";
            string TriggerY = "Trigger Y";

            var expected = Uml(
                "state \"State D\" as StateD {",
                "\tstate \"State B\" as StateB",
                "\tstate \"State C\" as StateC",
                "}",
                "StateD : entry / Enter D",
                "state \"State A\" as StateA",
                "StateA : entry / Enter A",
                "StateA : exit / Exit A",
                "StateA --> StateB : Trigger X",
                "StateA --> StateC : Trigger Y",
                "[*] --> StateA");

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

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            WriteToFile(nameof(SpacedWithSubstate), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void WithNestedSubstates()
        {
            var expected = Uml(
                "state D {",
                "\tstate C {",
                "\t\tstate B",
                "\t\tB : entry / Enter B",
                "\t}",
                "}",
                "state A",
                "A --> B : X",
                "B --> D : Y",
                "[*] --> A");

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            sm.Configure(State.B)
                .SubstateOf(State.C)
                .OnEntry(TestEntryAction, "Enter B")
                .Permit(Trigger.Y, State.D);
            sm.Configure(State.C)
                .SubstateOf(State.D);
            sm.Configure(State.D);

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            WriteToFile(nameof(WithNestedSubstates), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void StateNamesWithSpacesAreAliased()
        {
            var expected = Uml(
                "state \"A A\" as AA",
                "state \"A  A\" as AA_1",
                "state \"A   A\" as AA_2",
                "state B",
                "AA --> B : X",
                "AA_1 --> B : X",
                "AA_2 --> B : X",
                "[*] --> AA");

            var sm = new StateMachine<string, Trigger>("A A");

            sm.Configure("A A").Permit(Trigger.X, "B");
            sm.Configure("A  A").Permit(Trigger.X, "B");
            sm.Configure("A   A").Permit(Trigger.X, "B");

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            WriteToFile(nameof(StateNamesWithSpacesAreAliased), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void AliasDoesNotCollideWithExistingStateName()
        {
            var expected = Uml(
                "state AB",
                "state \"A-B\" as AB_1",
                "AB --> AB_1 : X",
                "[*] --> AB");

            var sm = new StateMachine<string, Trigger>("AB");

            sm.Configure("AB").Permit(Trigger.X, "A-B");

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            WriteToFile(nameof(AliasDoesNotCollideWithExistingStateName), result);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("state", "state_1")]
        [InlineData("note", "note_1")]
        [InlineData("1st", "_1st")]
        [InlineData("\"quoted\"", "quoted")]
        [InlineData("---", "StateAlias")]
        public void StateNamesThatAreNotValidIdentifiersAreAliased(string stateName, string alias)
        {
            var displayName = stateName.Replace("\"", "<U+0022>");
            var expected = Uml(
                $"state \"{displayName}\" as {alias}",
                $"[*] --> {alias}");

            var sm = new StateMachine<string, Trigger>(stateName);
            sm.Configure(stateName);

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            Assert.Equal(expected, result);
        }

        [Fact]
        public void TransitionLabelWithColonIsNotEscaped()
        {
            var expected = Uml(
                "state A",
                "state B",
                "A --> B : X [time: 10:00]",
                "[*] --> A");

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue, "time: 10:00");

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            WriteToFile(nameof(TransitionLabelWithColonIsNotEscaped), result);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("first\nsecond")]
        [InlineData("first\r\nsecond")]
        [InlineData("first\rsecond")]
        public void LineBreaksInLabelsAreWrittenAsPlantUmlNewLines(string guardDescription)
        {
            var expected = Uml(
                "state A",
                "state B",
                "A --> B : X [first\\nsecond]",
                "[*] --> A");

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue, guardDescription);

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            Assert.Equal(expected, result);
        }

        [Fact]
        public void BackslashesInLabelsAreEscaped()
        {
            var expected = Uml(
                "state A",
                "state B",
                "A --> B : X [C:<U+005C>temp]",
                "[*] --> A");

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue, "C:\\temp");

            var result = Graph.PlantUmlGraph.Format(sm.GetInfo());

            Assert.Equal(expected, result);
        }

        [Fact]
        public void TriggerWithoutLabelOmitsSeparator()
        {
            var style = new Graph.PlantUmlGraphStyle(
                new Graph.StateGraph(new StateMachine<State, Trigger>(State.A).GetInfo()), null);

            var result = style.FormatOneTransition("A", null, null, "B", Array.Empty<string>());

            Assert.Equal("A --> B", result);
        }

        [Fact]
        public void UnsupportedDirectionThrows()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            Assert.Throws<ArgumentOutOfRangeException>(
                () => Graph.PlantUmlGraph.Format(sm.GetInfo(), (Graph.PlantUmlGraphDirection)42));
        }

        private static string Uml(params string[] lines)
        {
            return "@startuml" + Environment.NewLine
                + string.Join(Environment.NewLine, lines) + Environment.NewLine
                + "@enduml";
        }

        private bool IsTrue()
        {
            return true;
        }

        private void TestEntryAction() { }

        private void TestEntryActionString(string val) { }

        private void WriteToFile(string fileName, string content)
        {
#if WRITE_DOTS_TO_FOLDER
            System.IO.File.WriteAllText(System.IO.Path.Combine("c:\\temp", $"{fileName}.puml"), content);
#endif
        }
    }
}
