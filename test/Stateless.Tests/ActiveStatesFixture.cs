namespace Stateless.Tests;

public class ActiveStatesFixture {
    [Fact]
    public async Task WhenActivate() {
        var sm = new StateMachine<State, Trigger>(State.A);

        var expectedOrdering = new List<string> { "ActivatedC", "ActivatedA" };
        var actualOrdering = new List<string>();

        sm.Configure(State.A)
          .SubstateOf(State.C)
          .OnActivate(() => actualOrdering.Add("ActivatedA"));

        sm.Configure(State.C)
          .OnActivate(() => actualOrdering.Add("ActivatedC"));

        // should not be called for activation
        sm.OnTransitioned(_ => actualOrdering.Add("OnTransitioned"));
        sm.OnTransitionCompleted(_ => actualOrdering.Add("OnTransitionCompleted"));

        await sm.ActivateAsync();

        Assert.Equal(expectedOrdering.Count, actualOrdering.Count);
        for (var i = 0; i < expectedOrdering.Count; i++)
            Assert.Equal(expectedOrdering[i], actualOrdering[i]);
    }

    [Fact]
    public async Task WhenActivateIsIdempotent() {
        var sm = new StateMachine<State, Trigger>(State.A);

        var actualOrdering = new List<string>();

        sm.Configure(State.A)
          .SubstateOf(State.C)
          .OnActivate(() => actualOrdering.Add("ActivatedA"));

        sm.Configure(State.C)
          .OnActivate(() => actualOrdering.Add("ActivatedC"));

        await sm.ActivateAsync();

        Assert.Equal(2, actualOrdering.Count);
    }

    [Fact]
    public async Task WhenDeactivate() {
        var sm = new StateMachine<State, Trigger>(State.A);

        var expectedOrdering = new List<string> { "DeactivatedA", "DeactivatedC" };
        var actualOrdering = new List<string>();

        sm.Configure(State.A)
          .SubstateOf(State.C)
          .OnDeactivate(() => actualOrdering.Add("DeactivatedA"));

        sm.Configure(State.C)
          .OnDeactivate(() => actualOrdering.Add("DeactivatedC"));

        // should not be called for activation
        sm.OnTransitioned(_ => actualOrdering.Add("OnTransitioned"));
        sm.OnTransitionCompleted(_ => actualOrdering.Add("OnTransitionCompleted"));

        await sm.ActivateAsync();
        await sm.DeactivateAsync();

        Assert.Equal(expectedOrdering.Count, actualOrdering.Count);
        for (var i = 0; i < expectedOrdering.Count; i++)
            Assert.Equal(expectedOrdering[i], actualOrdering[i]);
    }

    [Fact]
    public async Task WhenDeactivateIsIdempotent() {
        var sm = new StateMachine<State, Trigger>(State.A);

        var actualOrdering = new List<string>();

        sm.Configure(State.A)
          .SubstateOf(State.C)
          .OnDeactivate(() => actualOrdering.Add("DeactivatedA"));

        sm.Configure(State.C)
          .OnDeactivate(() => actualOrdering.Add("DeactivatedC"));

        await sm.ActivateAsync();
        await sm.DeactivateAsync();

        actualOrdering.Clear();
        await sm.ActivateAsync();

        Assert.Equal(0, actualOrdering.Count);
    }

    [Fact]
    public async Task WhenTransitioning() {
        var sm = new StateMachine<State, Trigger>(State.A);

        var expectedOrdering = new List<string> {
            "ActivatedA",
            "ExitedA",
            "OnTransitioned",
            "EnteredB",
            "OnTransitionCompleted",
            "ExitedB",
            "OnTransitioned",
            "EnteredA",
            "OnTransitionCompleted"
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

        sm.OnTransitioned(_ => actualOrdering.Add("OnTransitioned"));
        sm.OnTransitionCompleted(_ => actualOrdering.Add("OnTransitionCompleted"));

        await sm.ActivateAsync();
        await sm.FireAsync(Trigger.X);
        await sm.FireAsync(Trigger.Y);

        Assert.Equal(expectedOrdering.Count, actualOrdering.Count);
        for (var i = 0; i < expectedOrdering.Count; i++)
            Assert.Equal(expectedOrdering[i], actualOrdering[i]);
    }

    [Fact]
    public async Task WhenTransitioningWithinSameSuperstate() {
        var sm = new StateMachine<State, Trigger>(State.A);

        var expectedOrdering = new List<string> { "ActivatedC", "ActivatedA" };

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

        await sm.ActivateAsync();
        await sm.FireAsync(Trigger.X);
        await sm.FireAsync(Trigger.Y);

        Assert.Equal(expectedOrdering.Count, actualOrdering.Count);
        for (var i = 0; i < expectedOrdering.Count; i++)
            Assert.Equal(expectedOrdering[i], actualOrdering[i]);
    }
}