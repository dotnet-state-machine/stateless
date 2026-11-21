using System;
using System.Text;
using Xunit;

namespace Stateless.Tests
{
    public class MermaidGraphFixture
    {
        [Fact]
        public void Format_InitialTransition_ShouldReturns()
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("[*] --> A")
                .ToString().TrimEnd();

            var sm = new StateMachine<State, Trigger>(State.A);

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            WriteToFile(nameof(Format_InitialTransition_ShouldReturns), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void SimpleTransition()
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("	A --> B : X")
                .AppendLine("[*] --> A")
                .ToString().TrimEnd();

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            WriteToFile(nameof(SimpleTransition), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void SimpleTransition_LeftToRight()
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("	direction LR")
                .AppendLine("	A --> B : X")
                .AppendLine("[*] --> A")
                .ToString().TrimEnd();

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            var result = Graph.MermaidGraph.Format(sm.GetInfo(), Graph.MermaidGraphDirection.LeftToRight);

            WriteToFile(nameof(SimpleTransition_LeftToRight), result);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(Graph.MermaidGraphDirection.RightToLeft, "RL")]
        [InlineData(Graph.MermaidGraphDirection.TopToBottom, "TB")]
        [InlineData(Graph.MermaidGraphDirection.BottomToTop, "BT")]
        public void SimpleTransition_WithDirection_IncludesDirectionCode(Graph.MermaidGraphDirection direction, string directionCode)
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine($"	direction {directionCode}")
                .AppendLine("	A --> B : X")
                .AppendLine("[*] --> A")
                .ToString().TrimEnd();

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            var result = Graph.MermaidGraph.Format(sm.GetInfo(), direction);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void Format_WithUnsupportedDirection_ThrowsArgumentOutOfRangeException()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                Graph.MermaidGraph.Format(sm.GetInfo(), (Graph.MermaidGraphDirection)999));

            Assert.Equal("direction", exception.ParamName);
        }

        [Fact]
        public void TwoSimpleTransitions()
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("	A --> B : X") 
                .AppendLine("	A --> C : Y")
                .AppendLine("[*] --> A")
                .ToString().TrimEnd();

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                   .Permit(Trigger.X, State.B)
                   .Permit(Trigger.Y, State.C);

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            WriteToFile(nameof(TwoSimpleTransitions), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void WhenDiscriminatedByAnonymousGuard()
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("	A --> B : X [Function]")
                .AppendLine("[*] --> A")
                .ToString().TrimEnd();

            bool anonymousGuard() => true;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, anonymousGuard);
            sm.Configure(State.B);

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            WriteToFile(nameof(WhenDiscriminatedByAnonymousGuard), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void WhenDiscriminatedByAnonymousGuardWithDescription()
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("	A --> B : X [description]")
                .AppendLine("[*] --> A")
                .ToString().TrimEnd();

            bool guardFunction() => true;

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, guardFunction, "description");
            sm.Configure(State.B);

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            WriteToFile(nameof(WhenDiscriminatedByAnonymousGuard), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void WhenDiscriminatedByNamedDelegate()
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("	A --> B : X [IsTrue]")
                .AppendLine("[*] --> A")
                .ToString().TrimEnd();

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue);

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            WriteToFile(nameof(WhenDiscriminatedByNamedDelegate), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void WhenDiscriminatedByNamedDelegateWithDescription()
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("	A --> B : X [description]")
                .AppendLine("[*] --> A")
                .ToString().TrimEnd();

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue, "description");

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            WriteToFile(nameof(WhenDiscriminatedByNamedDelegateWithDescription), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void DestinationStateIsDynamic()
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("	state Decision1 <<choice>>")
                .AppendLine("	A --> Decision1 : X")
                .AppendLine("[*] --> A")
                .ToString().TrimEnd();

            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .PermitDynamic(Trigger.X, () => State.B);

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            WriteToFile(nameof(DestinationStateIsDynamic), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void DestinationStateIsCalculatedBasedOnTriggerParameters()
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("	state Decision1 <<choice>>")
                .AppendLine("	A --> Decision1 : X")
                .AppendLine("[*] --> A")
                .ToString().TrimEnd();

            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitDynamic(trigger, i => i == 1 ? State.B : State.C);

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            WriteToFile(nameof(DestinationStateIsCalculatedBasedOnTriggerParameters), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void TransitionWithIgnore()
        {
            // This test duplicates the behaviour expressed in the TransitionWithIgnore test in DotGraphFixture, 
            // but it seems counter-intuitive to show the ignored trigger as a transition back to the same state.
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("	A --> B : X")
                .AppendLine("	A --> A : Y")
                .AppendLine("[*] --> A")
                .ToString().TrimEnd();

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Ignore(Trigger.Y)
                .Permit(Trigger.X, State.B);

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            WriteToFile(nameof(TransitionWithIgnore), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void OnEntryWithTriggerParameter()
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("	A --> B : X / BX")
                .AppendLine("	A --> C : Y / TestEntryActionString [IsTriggerY]")
                .AppendLine("	A --> B : Z [IsTriggerZ]")
                .AppendLine("[*] --> A")
                .ToString().TrimEnd();

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

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            WriteToFile(nameof(TransitionWithIgnore), result);

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

            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("	StateD : State D")
                .AppendLine("	StateB : State B")
                .AppendLine("	StateC : State C")
                .AppendLine("	StateA : State A")
                .AppendLine("	state StateD {")
                .AppendLine("		StateB")
                .AppendLine("		StateC")
                .AppendLine("	}")
                .AppendLine("	StateA --> StateB : Trigger X")
                .AppendLine("	StateA --> StateC : Trigger Y")
                .AppendLine("[*] --> StateA")
                .ToString().TrimEnd();

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

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            WriteToFile(nameof(SpacedWithSubstate), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void WithSubstate()
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("	state D {")
                .AppendLine("		B")
                .AppendLine("		C")
                .AppendLine("	}")
                .AppendLine("	A --> B : X")
                .AppendLine("	A --> C : Y")
                .AppendLine("[*] --> A")
                .ToString().TrimEnd();

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B)
                .Permit(Trigger.Y, State.C);

            sm.Configure(State.B)
                .SubstateOf(State.D);
            sm.Configure(State.C)
                .SubstateOf(State.D);
            sm.Configure(State.D);

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            WriteToFile(nameof(WithSubstate), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void StateNamesWithSpacesAreAliased()
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("	AA : A A")
                .AppendLine("	AA_1 : A  A")
                .AppendLine("	AA_2 : A   A")
                .AppendLine("	AA --> B : X")
                .AppendLine("	AA_1 --> B : X")
                .AppendLine("	AA_2 --> B : X")
                .AppendLine("[*] --> AA")
                .ToString().TrimEnd();

            var sm = new StateMachine<string, Trigger>("A A");

            sm.Configure("A A").Permit(Trigger.X, "B");
            sm.Configure("A  A").Permit(Trigger.X, "B");
            sm.Configure("A   A").Permit(Trigger.X, "B");

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            WriteToFile(nameof(StateNamesWithSpacesAreAliased), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void StateNamesWithColonsAreEscaped()
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("	Doneok : Done#58; ok")
                .AppendLine("	A --> Doneok : X")
                .AppendLine("[*] --> A")
                .ToString().TrimEnd();

            var sm = new StateMachine<string, Trigger>("A");

            sm.Configure("A").Permit(Trigger.X, "Done: ok");

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            WriteToFile(nameof(StateNamesWithColonsAreEscaped), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void StateNameStartingWithHashIsAliased()
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("\tA : #35;A")
                .AppendLine("\tA --> B : go")
                .AppendLine("[*] --> A")
                .ToString().TrimEnd();

            var sm = new StateMachine<string, string>("#A");

            sm.Configure("#A").Permit("go", "B");
            sm.Configure("B");

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("%", "#37;")]
        [InlineData("%%", "#37;#37;")]
        public void StateNameMadeOnlyOfSyntaxCharactersUsesNonEmptyAlias(string stateName, string escapedStateName)
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine($"\tStateAlias_1 : {escapedStateName}")
                .AppendLine("\tStateAlias_1 --> StateAlias : go")
                .AppendLine("[*] --> StateAlias_1")
                .ToString().TrimEnd();

            var sm = new StateMachine<string, string>(stateName);

            sm.Configure(stateName).Permit("go", "StateAlias");
            sm.Configure("StateAlias");

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            Assert.Equal(expected, result);
        }

        [Fact]
        public void StateNameMadeOnlyOfSyntaxCharactersUsesFallbackAlias()
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("\tStateAlias : #37;")
                .AppendLine("\tStateAlias --> B : go")
                .AppendLine("[*] --> StateAlias")
                .ToString().TrimEnd();

            var sm = new StateMachine<string, string>("%");

            sm.Configure("%").Permit("go", "B");
            sm.Configure("B");

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            Assert.Equal(expected, result);
        }

        [Fact]
        public void StateNamesMadeOnlyOfSyntaxCharactersGetDistinctFallbackAliases()
        {
            var sm = new StateMachine<string, string>("%");

            sm.Configure("%").Permit("go", "%%");
            sm.Configure("%%");

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            Assert.Contains("\tStateAlias : #37;" + Environment.NewLine, result);
            Assert.Contains("\tStateAlias_1 : #37;#37;", result);
            Assert.Contains("\tStateAlias --> StateAlias_1 : go", result);
        }

        [Fact]
        public void StateNameDifferingOnlyByHashDoesNotCollideWithExistingState()
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("\tC_1 : C#35;")
                .AppendLine("\tC_1 --> C : go")
                .AppendLine("[*] --> C_1")
                .ToString().TrimEnd();

            var sm = new StateMachine<string, string>("C#");

            sm.Configure("C#").Permit("go", "C");
            sm.Configure("C");

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("a;b", "ab", "a#59;b")]
        [InlineData("a{b}", "ab", "a{b}")]
        [InlineData("a[1]", "a1", "a[1]")]
        [InlineData("a\"b\"", "ab", "a\"b\"")]
        [InlineData("a<b>", "ab", "a#lt;b#gt;")]
        [InlineData("a&b", "ab", "a#amp;b")]
        public void StateNamesWithOtherSyntaxCharactersAreAliased(string stateName, string alias, string escapedStateName)
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine($"\t{alias} : {escapedStateName}")
                .AppendLine($"\t{alias} --> B : go")
                .AppendLine($"[*] --> {alias}")
                .ToString().TrimEnd();

            var sm = new StateMachine<string, string>(stateName);

            sm.Configure(stateName).Permit("go", "B");
            sm.Configure("B");

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("sta#te", "state_1", "sta#35;te")]
        [InlineData("class", "class_1", "class")]
        public void StateNamesProducingReservedAliasesAreRenamed(string stateName, string alias, string escapedStateName)
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine($"\t{alias} : {escapedStateName}")
                .AppendLine($"\t{alias} --> B : go")
                .AppendLine($"[*] --> {alias}")
                .ToString().TrimEnd();

            var sm = new StateMachine<string, string>(stateName);

            sm.Configure(stateName).Permit("go", "B");
            sm.Configure("B");

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            Assert.Equal(expected, result);
        }

        [Fact]
        public void MermaidGraph_EscapesLabelsAndGuardDescriptions()
        {
            var unsafeState = "B\n%% injected state";
            var unsafeTrigger = "X\n%% injected trigger";
            var unsafeGuardDescription = "guard\n%% injected guard";
            var sm = new StateMachine<string, string>("A");

            sm.Configure("A")
                .PermitIf(unsafeTrigger, unsafeState, () => true, unsafeGuardDescription);
            sm.Configure(unsafeState);

            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("\tBinjectedstate : B<br/>#37;#37; injected state")
                .AppendLine("\tA --> Binjectedstate : X<br/>#37;#37; injected trigger [guard<br/>#37;#37; injected guard]")
                .AppendLine("[*] --> A")
                .ToString().TrimEnd();

            var graph = Graph.MermaidGraph.Format(sm.GetInfo());

            // No line may be a Mermaid comment, which is what a smuggled newline followed by %% would create.
            Assert.DoesNotContain(graph.Split('\n'), line => line.TrimStart().StartsWith("%%"));
            Assert.DoesNotContain("\n%% injected state", graph);
            Assert.DoesNotContain("\n%% injected trigger", graph);
            Assert.DoesNotContain("\n%% injected guard", graph);
            Assert.Equal(expected, graph);
        }

        [Fact]
        public void MermaidGraph_EscapesDynamicTransitionsAndClusters()
        {
            var sm = new StateMachine<string, string>("A");

            sm.Configure("A")
                .PermitDynamic("X\n%% injected trigger", () => "B\n%% injected state",
                    null, new Stateless.Reflection.DynamicStateInfos { { "B\n%% injected state", "crit\n%% injected criterion" } });
            sm.Configure("B\n%% injected state")
                .SubstateOf("Parent\n%% injected parent");
            sm.Configure("Parent\n%% injected parent");

            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("\tParentinjectedparent : Parent<br/>#37;#37; injected parent")
                .AppendLine("\tBinjectedstate : B<br/>#37;#37; injected state")
                .AppendLine("\tstate Parentinjectedparent {")
                .AppendLine("\t\tBinjectedstate")
                .AppendLine("\t}")
                .AppendLine("\tstate Decision1 <<choice>>")
                .AppendLine("\tA --> Decision1 : X<br/>#37;#37; injected trigger")
                .AppendLine("\tDecision1 --> Binjectedstate : X<br/>#37;#37; injected trigger [crit<br/>#37;#37; injected criterion]")
                .AppendLine("[*] --> A")
                .ToString().TrimEnd();

            var graph = Graph.MermaidGraph.Format(sm.GetInfo());

            Assert.DoesNotContain(graph.Split('\n'), line => line.TrimStart().StartsWith("%%"));
            Assert.Equal(expected, graph);
        }

        [Fact]
        public void MermaidGraph_LineBreaksBecomeHtmlBreaks()
        {
            var graph = Graph.MermaidGraph.Format(GetMachineWithLineBreaks());

            Assert.Contains("\tA --> B : X<br/>Y<br/>Z<br/>W [g<br/>h]", graph);
        }

        [Fact]
        public void TransitionLabelsWithColonsAreEscaped()
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("	A --> B : Pay#58; now [Limit#58; 100]")
                .AppendLine("[*] --> A")
                .ToString().TrimEnd();

            var sm = new StateMachine<State, string>(State.A);

            sm.Configure(State.A).PermitIf("Pay: now", State.B, () => true, "Limit: 100");

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            WriteToFile(nameof(TransitionLabelsWithColonsAreEscaped), result);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void TransitionLabelsWithHtmlCharactersAreEscaped()
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("	A --> B : Add#lt;List#lt;int#gt;#gt; [x #lt; 5 #amp;#amp; y #gt; 2]")
                .AppendLine("[*] --> A")
                .ToString().TrimEnd();

            var sm = new StateMachine<State, string>(State.A);

            sm.Configure(State.A).PermitIf("Add<List<int>>", State.B, () => true, "x < 5 && y > 2");

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            Assert.Equal(expected, result);
        }

        [Fact]
        public void LiteralEntityCodesInLabelsAreNotDecoded()
        {
            var expected = new StringBuilder()
                .AppendLine("stateDiagram-v2")
                .AppendLine("	A --> B : #35;58#59; #35;lt#59; #35;1 [#35;#35;]")
                .AppendLine("[*] --> A")
                .ToString().TrimEnd();

            var sm = new StateMachine<State, string>(State.A);

            sm.Configure(State.A).PermitIf("#58; #lt; #1", State.B, () => true, "##");

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            Assert.Equal(expected, result);
        }

        [Fact]
        public void HtmlCharactersAndLineBreaksInLabels_KeepBreakTokens()
        {
            var sm = new StateMachine<State, string>(State.A);

            sm.Configure(State.A).PermitIf("a<b\nc>d", State.B, () => true, "<br/>");

            var result = Graph.MermaidGraph.Format(sm.GetInfo());

            // Only the real line break becomes a <br/> token; a literal "<br/>" in the input is escaped.
            Assert.Contains("\tA --> B : a#lt;b<br/>c#gt;d [#lt;br/#gt;]", result);
        }

        private static Stateless.Reflection.StateMachineInfo GetMachineWithLineBreaks()
        {
            var sm = new StateMachine<string, string>("A");

            sm.Configure("A")
                .PermitIf("X\nY\r\nZ\rW", "B", () => true, "g\nh");

            return sm.GetInfo();
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
            System.IO.File.WriteAllText(System.IO.Path.Combine("c:\\temp", $"{fileName}.txt"), content);
#endif
        }
    }
}
