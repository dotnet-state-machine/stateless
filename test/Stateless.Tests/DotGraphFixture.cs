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
            var expected = @"digraph {
compound=true;
rankdir=""LR""
	A [   label=<
	<TABLE BORDER=""1"" CELLBORDER=""1"" CELLSPACING=""0"" >
	<tr><td>
	</td></tr>
	<TR><TD PORT=""A"">A</TD></TR><tr><td>
	</td></tr>
	</TABLE>>,shape=""plaintext"",color=""blue"" ];

A -> B [   style=""solid"",label=""X"" ];  
}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Permit(Trigger.X, State.B);

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void TwoSimpleTransitions()
        {
            var expected = @"digraph {
compound=true;
rankdir=""LR""
	A [   label=<
	<TABLE BORDER=""1"" CELLBORDER=""1"" CELLSPACING=""0"" >
	<tr><td>
	</td></tr>
	<TR><TD PORT=""A"">A</TD></TR><tr><td>
	</td></tr>
	</TABLE>>,shape=""plaintext"",color=""blue"" ];

A -> B [   style=""solid"",label=""X"" ]; 
A -> C [   style=""solid"",label=""Y"" ];  
}";

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

            var expected = @"digraph {
compound=true;
rankdir=""LR""
	A [   label=<
	<TABLE BORDER=""1"" CELLBORDER=""1"" CELLSPACING=""0"" >
	<tr><td>
	</td></tr>
	<TR><TD PORT=""A"">A</TD></TR><tr><td>
	</td></tr>
	</TABLE>>,shape=""plaintext"",color=""blue"" ];
	B [   label=<
	<TABLE BORDER=""1"" CELLBORDER=""1"" CELLSPACING=""0"" >
	<tr><td>
	</td></tr>
	<TR><TD PORT=""B"">B</TD></TR><tr><td>
	</td></tr>
	</TABLE>>,shape=""plaintext"",color=""blue"" ];

A -> B [   style=""solid"",label=""X " + anonymousGuard.TryGetMethodName() + @""" ]; 
}";
                
              

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

            var expected = @"digraph {
compound=true;
rankdir=""LR""
	A [   label=<
	<TABLE BORDER=""1"" CELLBORDER=""1"" CELLSPACING=""0"" >
	<tr><td>
	</td></tr>
	<TR><TD PORT=""A"">A</TD></TR><tr><td>
	</td></tr>
	</TABLE>>,shape=""plaintext"",color=""blue"" ];

A -> B [   style=""solid"",label=""X description"" ]; 
}";
        var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, anonymousGuard, "description");

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void WhenDiscriminatedByNamedDelegate()
        {
            var expected = @"digraph {
compound=true;
rankdir=""LR""
	A [   label=<
	<TABLE BORDER=""1"" CELLBORDER=""1"" CELLSPACING=""0"" >
	<tr><td>
	</td></tr>
	<TR><TD PORT=""A"">A</TD></TR><tr><td>
	</td></tr>
	</TABLE>>,shape=""plaintext"",color=""blue"" ];

A -> B [   style=""solid"",label=""X IsTrue"" ]; 
}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue);

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void WhenDiscriminatedByNamedDelegateWithDescription()
        {
            var expected = @"digraph {
compound=true;
rankdir=""LR""
	A [   label=<
	<TABLE BORDER=""1"" CELLBORDER=""1"" CELLSPACING=""0"" >
	<tr><td>
	</td></tr>
	<TR><TD PORT=""A"">A</TD></TR><tr><td>
	</td></tr>
	</TABLE>>,shape=""plaintext"",color=""blue"" ];
	B [   label=<
	<TABLE BORDER=""1"" CELLBORDER=""1"" CELLSPACING=""0"" >
	<tr><td>
	</td></tr>
	<TR><TD PORT=""B"">B</TD></TR><tr><td>
	</td></tr>
	</TABLE>>,shape=""plaintext"",color=""blue"" ];

A -> B [   style=""solid"",label=""X description"" ]; 
}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .PermitIf(Trigger.X, State.B, IsTrue, "description");
            sm.Configure(State.B);
            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void DestinationStateIsDynamic()
        {
            var expected = @"digraph {
compound=true;
rankdir=""LR""
	A [   label=<
	<TABLE BORDER=""1"" CELLBORDER=""1"" CELLSPACING=""0"" >
	<tr><td>
	</td></tr>
	<TR><TD PORT=""A"">A</TD></TR><tr><td>
	</td></tr>
	</TABLE>>,shape=""plaintext"",color=""blue"" ];

	Unk0_A [   label=<
	<TABLE BORDER=""1"" CELLBORDER=""1"" CELLSPACING=""0"" >
	<tr><td>
	</td></tr>
	<TR><TD PORT=""Unk0_A"">Unk0_A</TD></TR><tr><td>
	</td></tr>
	</TABLE>>,shape=""plaintext"",color=""blue"" ];

A -> Unk0_A [   style=""solid"",label=""X"" ];  
}";

            var sm = new StateMachine<State, Trigger>(State.A);
            sm.Configure(State.A)
                .PermitDynamic(Trigger.X, () => State.B);

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void DestinationStateIsCalculatedBasedOnTriggerParameters()
        {
            var expected = @"digraph {
compound=true;
rankdir=""LR""
	A [   label=<
	<TABLE BORDER=""1"" CELLBORDER=""1"" CELLSPACING=""0"" >
	<tr><td>
	</td></tr>
	<TR><TD PORT=""A"">A</TD></TR><tr><td>
	</td></tr>
	</TABLE>>,shape=""plaintext"",color=""blue"" ];

	Unk0_A [   label=<
	<TABLE BORDER=""1"" CELLBORDER=""1"" CELLSPACING=""0"" >
	<tr><td>
	</td></tr>
	<TR><TD PORT=""Unk0_A"">Unk0_A</TD></TR><tr><td>
	</td></tr>
	</TABLE>>,shape=""plaintext"",color=""blue"" ];

A -> Unk0_A [   style=""solid"",label=""X"" ];  
}".Replace("\r\n", System.Environment.NewLine);

            var sm = new StateMachine<State, Trigger>(State.A);
            var trigger = sm.SetTriggerParameters<int>(Trigger.X);
            sm.Configure(State.A)
                .PermitDynamic(trigger, i => i == 1 ? State.B : State.C);

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void OnEntryWithAnonymousActionAndDescription()
        {
            var expected = @"digraph {
compound=true;
rankdir=""LR""
	A [   label=<
	<TABLE BORDER=""1"" CELLBORDER=""1"" CELLSPACING=""0"" >
	<tr><td>
		<TABLE BORDER=""0"" CELLBORDER=""1"" CELLSPACING=""0"" BGCOLOR=""yellow"">
		<TR><TD><sup>enteredA</sup></TD></TR>
		</TABLE>

	</td></tr>
	<TR><TD PORT=""A"">A</TD></TR><tr><td>
	</td></tr>
	</TABLE>>,shape=""plaintext"",color=""blue"" ];

}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(() => { }, "enteredA");

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void OnEntryWithNamedDelegateActionAndDescription()
        {
            var expected = @"digraph {
compound=true;
rankdir=""LR""
	A [   label=<
	<TABLE BORDER=""1"" CELLBORDER=""1"" CELLSPACING=""0"" >
	<tr><td>
		<TABLE BORDER=""0"" CELLBORDER=""1"" CELLSPACING=""0"" BGCOLOR=""yellow"">
		<TR><TD><sup>enteredA</sup></TD></TR>
		</TABLE>

	</td></tr>
	<TR><TD PORT=""A"">A</TD></TR><tr><td>
	</td></tr>
	</TABLE>>,shape=""plaintext"",color=""blue"" ];

}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnEntry(OnEntry, "enteredA");

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void OnExitWithAnonymousActionAndDescription()
        {
            var expected = @"digraph {
compound=true;
rankdir=""LR""
	A [   label=<
	<TABLE BORDER=""1"" CELLBORDER=""1"" CELLSPACING=""0"" >
	<tr><td>
	</td></tr>
	<TR><TD PORT=""A"">A</TD></TR><tr><td>
		<TABLE BORDER=""0"" CELLBORDER=""1"" CELLSPACING=""0"" BGCOLOR=""yellow"">
		<TR><TD><sup>exitA</sup></TD></TR>
		</TABLE>

	</td></tr>
	</TABLE>>,shape=""plaintext"",color=""blue"" ];

}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(() => { }, "exitA");

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void OnExitWithNamedDelegateActionAndDescription()
        {
            var expected = @"digraph {
compound=true;
rankdir=""LR""
	A [   label=<
	<TABLE BORDER=""1"" CELLBORDER=""1"" CELLSPACING=""0"" >
	<tr><td>
	</td></tr>
	<TR><TD PORT=""A"">A</TD></TR><tr><td>
		<TABLE BORDER=""0"" CELLBORDER=""1"" CELLSPACING=""0"" BGCOLOR=""yellow"">
		<TR><TD><sup>exitA</sup></TD></TR>
		</TABLE>

	</td></tr>
	</TABLE>>,shape=""plaintext"",color=""blue"" ];

}";

            var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .OnExit(OnExit, "exitA");

            Assert.AreEqual(expected, sm.ToDotGraph());
        }

        [Test]
        public void TransitionWithIgnore()
        {
            // Ignored triggers do not appear in the graph
            var expected = @"digraph {
compound=true;
rankdir=""LR""
	A [   label=<
	<TABLE BORDER=""1"" CELLBORDER=""1"" CELLSPACING=""0"" >
	<tr><td>
	</td></tr>
	<TR><TD PORT=""A"">A</TD></TR><tr><td>
	</td></tr>
	</TABLE>>,shape=""plaintext"",color=""blue"" ];

A -> B [   style=""solid"",label=""X"" ];  
}";

        var sm = new StateMachine<State, Trigger>(State.A);

            sm.Configure(State.A)
                .Ignore(Trigger.Y)
                .Permit(Trigger.X, State.B);

            Assert.AreEqual(expected, sm.ToDotGraph());
        }
    }
}
