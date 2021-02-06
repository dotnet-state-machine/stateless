using System.Collections.Generic;

using Xunit;

namespace Stateless.Tests
{
    public class ActiveStatesFixture
    {
        [Fact]
        public void WhenActivate()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var expectedOrdering = new List<string> { "ActivatedC", "ActivatedA" };
            var actualOrdering = new List<string>();

            sm.Configure(State.A)
              .SubstateOf(State.C)
              .OnActivate(() => actualOrdering.Add("ActivatedA"));

            sm.Configure(State.C)
              .OnActivate(() => actualOrdering.Add("ActivatedC"));

            // should not be called for activation
            sm.OnTransitioned(t => actualOrdering.Add("OnTransitioned"));
            sm.OnTransitionCompleted(t => actualOrdering.Add("OnTransitionCompleted"));

            sm.Activate();

            Assert.Equal(expectedOrdering.Count, actualOrdering.Count);
            for (int i = 0; i < expectedOrdering.Count; i++)
                Assert.Equal(expectedOrdering[i], actualOrdering[i]);
        }

        [Fact]
        public void WhenActivateIsIdempotent()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var actualOrdering = new List<string>();

            sm.Configure(State.A)
              .SubstateOf(State.C)
              .OnActivate(() => actualOrdering.Add("ActivatedA"));

            sm.Configure(State.C)
              .OnActivate(() => actualOrdering.Add("ActivatedC"));

            sm.Activate();

            Assert.Equal(2, actualOrdering.Count);
        }

        [Fact]
        public void WhenDeactivate()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var expectedOrdering = new List<string> { "DeactivatedA", "DeactivatedC" };
            var actualOrdering = new List<string>();

            sm.Configure(State.A)
              .SubstateOf(State.C)
              .OnDeactivate(() => actualOrdering.Add("DeactivatedA"));

            sm.Configure(State.C)
              .OnDeactivate(() => actualOrdering.Add("DeactivatedC"));

            // should not be called for activation
            sm.OnTransitioned(t => actualOrdering.Add("OnTransitioned"));
            sm.OnTransitionCompleted(t => actualOrdering.Add("OnTransitionCompleted"));

            sm.Activate();
            sm.Deactivate();

            Assert.Equal(expectedOrdering.Count, actualOrdering.Count);
            for (int i = 0; i < expectedOrdering.Count; i++)
                Assert.Equal(expectedOrdering[i], actualOrdering[i]);
        }

        [Fact]
        public void WhenDeactivateIsIdempotent()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var actualOrdering = new List<string>();

            sm.Configure(State.A)
              .SubstateOf(State.C)
              .OnDeactivate(() => actualOrdering.Add("DeactivatedA"));

            sm.Configure(State.C)
              .OnDeactivate(() => actualOrdering.Add("DeactivatedC"));

            sm.Activate();
            sm.Deactivate();

            actualOrdering.Clear();
            sm.Activate();

            Assert.Equal(0, actualOrdering.Count);
        }

        [Fact]
        public void WhenTransitioning()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var expectedOrdering = new List<string>
            {
                "ActivatedA",
                "ExitedA",
                "OnTransitioned",
                "EnteredB",
                "OnTransitionCompleted",

                "ExitedB",
                "OnTransitioned",
                "EnteredA",
                "OnTransitionCompleted",

            };

            var actualOrdering = new List<string>();

            sm.Configure(State.A)
              .OnActivate(() => actualOrdering.Add("ActivatedA"))
              .OnDeactivate(() => actualOrdering.Add("DeactivatedA"))
              .OnEntry(() => actualOrdering.Add("EnteredA"))
              .OnExit(() => actualOrdering.Add("ExitedA"))
              .Permit(Trigger.X, State.B);

            sm.Configure(State.B)
              .OnActivate(() => actualOrdering.Add("ActivatedB"))
              .OnDeactivate(() => actualOrdering.Add("DeactivatedB"))
              .OnEntry(() => actualOrdering.Add("EnteredB"))
              .OnExit(() => actualOrdering.Add("ExitedB"))
              .Permit(Trigger.Y, State.A);

            sm.OnTransitioned(t => actualOrdering.Add("OnTransitioned"));
            sm.OnTransitionCompleted(t => actualOrdering.Add("OnTransitionCompleted"));

            sm.Activate();
            sm.Fire(Trigger.X);
            sm.Fire(Trigger.Y);

            Assert.Equal(expectedOrdering.Count, actualOrdering.Count);
            for (int i = 0; i < expectedOrdering.Count; i++)
                Assert.Equal(expectedOrdering[i], actualOrdering[i]);
        }

        [Fact]
        public void WhenTransitioningWithinSameSuperstate()
        {
            var sm = new StateMachine<State, Trigger>(State.A);

            var expectedOrdering = new List<string>
            {
                "ActivatedC",
                "ActivatedA",
            };

            var actualOrdering = new List<string>();

            sm.Configure(State.A)
              .SubstateOf(State.C)
              .OnActivate(() => actualOrdering.Add("ActivatedA"))
              .OnDeactivate(() => actualOrdering.Add("DeactivatedA"))
              .Permit(Trigger.X, State.B);

            sm.Configure(State.B)
              .SubstateOf(State.C)
              .OnActivate(() => actualOrdering.Add("ActivatedB"))
              .OnDeactivate(() => actualOrdering.Add("DeactivatedB"))
              .Permit(Trigger.Y, State.A);

            sm.Configure(State.C)
              .OnActivate(() => actualOrdering.Add("ActivatedC"))
              .OnDeactivate(() => actualOrdering.Add("DeactivatedC"));

            sm.Activate();
            sm.Fire(Trigger.X);
            sm.Fire(Trigger.Y);

            Assert.Equal(expectedOrdering.Count, actualOrdering.Count);
            for (int i = 0; i < expectedOrdering.Count; i++)
                Assert.Equal(expectedOrdering[i], actualOrdering[i]);
        }
    }
}
