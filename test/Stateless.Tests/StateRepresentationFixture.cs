using Stateless.Reflection;

namespace Stateless.Tests;

public class StateRepresentationFixture {
    private static void CreateSuperSubstatePair(out StateMachine<State, Trigger>.StateRepresentation super,
                                                out StateMachine<State, Trigger>.StateRepresentation sub) {
        super = CreateRepresentation(State.A);
        sub   = CreateRepresentation(State.B);
        super.AddSubstate(sub);
        sub.Superstate = super;
    }

    private static StateMachine<State, Trigger>.StateRepresentation CreateRepresentation(State state) => new(state);

    // Issue #422 - Add all guard descriptions to result if multiple guards fail for same trigger
    [Fact]
    public void AddAllGuardDescriptionsWhenMultipleGuardsFailForSameTrigger() {
        ICollection<string> expectedGuardDescriptions =
            new List<string> { "PermitReentryIf guard failed", "PermitIf guard failed" };
        ICollection<string> guardDescriptions = null;

        var fsm = new StateMachine<State, Trigger>(State.A);
        fsm.OnUnhandledTrigger((_, _, descriptions) => guardDescriptions = descriptions);

        fsm.Configure(State.A)
           .PermitReentryIf(Trigger.X, () => false, "PermitReentryIf guard failed")
           .PermitIf(Trigger.X, State.C, () => false, "PermitIf guard failed");

        fsm.Fire(Trigger.X);

        Assert.Equal(fsm.State, State.A);
        Assert.True(guardDescriptions is { });
        Assert.Equal(2, guardDescriptions.Count);
        foreach (var description in guardDescriptions) Assert.True(expectedGuardDescriptions.Contains(description));
    }

    [Fact]
    public void DoesNotIncludeSuperstate() {
        var stateRepresentation = CreateRepresentation(State.B);
        stateRepresentation.Superstate = CreateRepresentation(State.C);
        Assert.False(stateRepresentation.Includes(State.C));
    }

    [Fact]
    public void DoesNotIncludeUnrelatedState() {
        var stateRepresentation = CreateRepresentation(State.B);
        Assert.False(stateRepresentation.Includes(State.C));
    }

    [Fact]
    public async Task EntryActionsExecuteInOrder() {
        var actual = new List<int>();

        var rep = CreateRepresentation(State.B);
        rep.AddEntryAction((_, _) => actual.Add(0), InvocationInfo.Create(null, "entryActionDescription"));
        rep.AddEntryAction((_, _) => actual.Add(1), InvocationInfo.Create(null, "entryActionDescription"));

        await rep.EnterAsync(new StateMachine<State, Trigger>.Transition(State.A, State.B, Trigger.X));

        Assert.Equal(2, actual.Count);
        Assert.Equal(0, actual[0]);
        Assert.Equal(1, actual[1]);
    }

    [Fact]
    public async Task ExitActionsExecuteInOrder() {
        var actual = new List<int>();

        var rep = CreateRepresentation(State.B);
        rep.AddExitAction(_ => actual.Add(0), InvocationInfo.Create(null, "entryActionDescription"));
        rep.AddExitAction(_ => actual.Add(1), InvocationInfo.Create(null, "entryActionDescription"));

        await rep.ExitAsync(new StateMachine<State, Trigger>.Transition(State.B, State.C, Trigger.X));

        Assert.Equal(2, actual.Count);
        Assert.Equal(0, actual[0]);
        Assert.Equal(1, actual[1]);
    }

    [Fact]
    public void IncludesSubstate() {
        var stateRepresentation = CreateRepresentation(State.B);
        stateRepresentation.AddSubstate(CreateRepresentation(State.C));
        Assert.True(stateRepresentation.Includes(State.C));
    }

    [Fact]
    public void IncludesUnderlyingState() {
        var stateRepresentation = CreateRepresentation(State.B);
        Assert.True(stateRepresentation.Includes(State.B));
    }

    [Fact]
    public void IsIncludedInSuperstate() {
        var stateRepresentation = CreateRepresentation(State.B);
        stateRepresentation.Superstate = CreateRepresentation(State.C);
        Assert.True(stateRepresentation.IsIncludedIn(State.C));
    }

    [Fact]
    public void IsIncludedInUnderlyingState() {
        var stateRepresentation = CreateRepresentation(State.B);
        Assert.True(stateRepresentation.IsIncludedIn(State.B));
    }

    [Fact]
    public void IsNotIncludedInSubstate() {
        var stateRepresentation = CreateRepresentation(State.B);
        stateRepresentation.AddSubstate(CreateRepresentation(State.C));
        Assert.False(stateRepresentation.IsIncludedIn(State.C));
    }

    [Fact]
    public void IsNotIncludedInUnrelatedState() {
        var stateRepresentation = CreateRepresentation(State.B);
        Assert.False(stateRepresentation.IsIncludedIn(State.C));
    }

    // Issue #398 - Set guard description if substate transition fails
    [Fact]
    public void SetGuardDescriptionWhenSubstateGuardFails() {
        const string expectedGuardDescription = "Guard failed";
        ICollection<string> guardDescriptions = null;

        var fsm = new StateMachine<State, Trigger>(State.B);
        fsm.OnUnhandledTrigger((_, _, descriptions) => guardDescriptions = descriptions);

        fsm.Configure(State.B).SubstateOf(State.A).PermitIf(Trigger.X, State.C, () => false, expectedGuardDescription);

        fsm.Fire(Trigger.X);

        Assert.Equal(fsm.State, State.B);
        Assert.True(guardDescriptions is { });
        Assert.Equal(guardDescriptions.Count, 1);
        Assert.Equal(guardDescriptions.First(), expectedGuardDescription);
    }

    [Fact]
    public async Task UponEntering_EnteringActionsExecuted() {
        var stateRepresentation = CreateRepresentation(State.B);
        StateMachine<State, Trigger>.Transition
            transition = new(State.A, State.B, Trigger.X),
            actualTransition = null;
        stateRepresentation.AddEntryAction((t, _) => actualTransition = t,
                                           InvocationInfo.Create(null, "entryActionDescription"));
        await stateRepresentation.EnterAsync(transition);
        Assert.Equal(transition, actualTransition);
    }

    [Fact]
    public async Task UponEntering_LeavingActionsNotExecuted() {
        var stateRepresentation = CreateRepresentation(State.A);
        StateMachine<State, Trigger>.Transition
            transition = new(State.A, State.B, Trigger.X),
            actualTransition = null;
        stateRepresentation.AddExitAction(t => actualTransition = t,
                                          InvocationInfo.Create(null, "exitActionDescription"));
        await stateRepresentation.EnterAsync(transition);
        Assert.Null(actualTransition);
    }

    [Fact]
    public async Task UponLeaving_EnteringActionsNotExecuted() {
        var stateRepresentation = CreateRepresentation(State.B);
        StateMachine<State, Trigger>.Transition
            transition = new(State.A, State.B, Trigger.X),
            actualTransition = null;
        stateRepresentation.AddEntryAction((t, _) => actualTransition = t,
                                           InvocationInfo.Create(null, "entryActionDescription"));
        await stateRepresentation.ExitAsync(transition);
        Assert.Null(actualTransition);
    }

    [Fact]
    public async Task UponLeaving_LeavingActionsExecuted() {
        var stateRepresentation = CreateRepresentation(State.A);
        StateMachine<State, Trigger>.Transition
            transition = new(State.A, State.B, Trigger.X),
            actualTransition = null;
        stateRepresentation.AddExitAction(t => actualTransition = t,
                                          InvocationInfo.Create(null, "entryActionDescription"));
        await stateRepresentation.ExitAsync(transition);
        Assert.Equal(transition, actualTransition);
    }

    [Fact]
    public async Task WhenEnteringSubstate_SuperEntryActionsExecuted() {
        CreateSuperSubstatePair(out var super, out var sub);

        var executed = false;
        super.AddEntryAction((_, _) => executed = true, InvocationInfo.Create(null, "entryActionDescription"));
        var transition = new StateMachine<State, Trigger>.Transition(State.C, sub.UnderlyingState, Trigger.X);
        await sub.EnterAsync(transition);
        Assert.True(executed);
    }

    [Fact]
    public async Task WhenEnteringSubstate_SuperstateEntryActionsExecuteBeforeSubstate() {
        CreateSuperSubstatePair(out var super, out var sub);

        int order = 0, subOrder = 0, superOrder = 0;
        super.AddEntryAction((_, _) => superOrder = order++, InvocationInfo.Create(null, "entryActionDescription"));
        sub.AddEntryAction((_,   _) => subOrder   = order++, InvocationInfo.Create(null, "entryActionDescription"));
        var transition = new StateMachine<State, Trigger>.Transition(State.C, sub.UnderlyingState, Trigger.X);
        await sub.EnterAsync(transition);
        Assert.True(superOrder < subOrder);
    }

    [Fact]
    public async Task WhenExitingSubstate_SubstateEntryActionsExecuteBeforeSuperstate() {
        CreateSuperSubstatePair(out var super, out var sub);

        int order = 0, subOrder = 0, superOrder = 0;
        super.AddExitAction(_ => superOrder = order++, InvocationInfo.Create(null, "entryActionDescription"));
        sub.AddExitAction(_ => subOrder     = order++, InvocationInfo.Create(null, "entryActionDescription"));
        var transition = new StateMachine<State, Trigger>.Transition(sub.UnderlyingState, State.C, Trigger.X);
        await sub.ExitAsync(transition);
        Assert.True(subOrder < superOrder);
    }

    [Fact]
    public async Task WhenLeavingSubstate_SuperExitActionsExecuted() {
        CreateSuperSubstatePair(out var super, out var sub);

        var executed = false;
        super.AddExitAction(_ => executed = true, InvocationInfo.Create(null, "exitActionDescription"));
        var transition = new StateMachine<State, Trigger>.Transition(sub.UnderlyingState, State.C, Trigger.X);
        await sub.ExitAsync(transition);
        Assert.True(executed);
    }

    [Fact]
    public void WhenTransitionDoesNotExist_TriggerCanBeFired() {
        var rep = CreateRepresentation(State.B);
        rep.AddTriggerBehaviour(new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(Trigger.X, null));
        Assert.True(rep.CanHandle(Trigger.X));
    }

    [Fact]
    public void WhenTransitionExistAndSuperstateUnmetGuardConditions_FireNotPossible() {
        CreateSuperSubstatePair(out var super, out var sub);

        var falseConditions = new[] {
            new Tuple<Func<object[], bool>, string>(_ => true, "1"),
            new Tuple<Func<object[], bool>, string>(_ => false, "2")
        };
        var transitionGuard = new TransitionGuard(falseConditions);
        var transition =
            new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(Trigger.X, State.C, transitionGuard);
        super.AddTriggerBehaviour(transition);

        var result = sub.TryFindHandler(Trigger.X, Array.Empty<object>(), out _);

        Assert.False(result);
        Assert.False(sub.CanHandle(Trigger.X));
        Assert.False(super.CanHandle(Trigger.X));
    }

    [Fact]
    public void WhenTransitionExists_TriggerCannotBeFired() {
        var rep = CreateRepresentation(State.B);
        Assert.False(rep.CanHandle(Trigger.X));
    }

    [Fact]
    public void WhenTransitionExistsInSuperstate_TriggerCanBeFired() {
        var rep = CreateRepresentation(State.B);
        rep.AddTriggerBehaviour(new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(Trigger.X, null));
        var sub = CreateRepresentation(State.C);
        sub.Superstate = rep;
        rep.AddSubstate(sub);
        Assert.True(sub.CanHandle(Trigger.X));
    }

    [Fact]
    public void WhenTransitionExistSuperstateMetGuardConditions_CanBeFired() {
        CreateSuperSubstatePair(out var super, out var sub);

        var trueConditions = new[] {
            new Tuple<Func<object[], bool>, string>(_ => true, "1"),
            new Tuple<Func<object[], bool>, string>(_ => true, "2")
        };
        var transitionGuard = new TransitionGuard(trueConditions);
        var transition =
            new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(Trigger.X, State.C, transitionGuard);

        super.AddTriggerBehaviour(transition);
        sub.TryFindHandler(Trigger.X, Array.Empty<object>(), out var result);

        Assert.True(sub.CanHandle(Trigger.X));
        Assert.True(super.CanHandle(Trigger.X));
        Assert.NotNull(result);
        Assert.True(result.Handler.GuardConditionsMet());
        Assert.False(result.UnmetGuardConditions.Any());
    }

    [Fact]
    public void WhenTransitionGuardConditionsMet_TriggerCanBeFired() {
        var rep = CreateRepresentation(State.B);

        var trueConditions = new[] {
            new Tuple<Func<object[], bool>, string>(_ => true, "1"),
            new Tuple<Func<object[], bool>, string>(_ => true, "2")
        };

        var transitionGuard = new TransitionGuard(trueConditions);
        var transition =
            new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(Trigger.X, State.C, transitionGuard);
        rep.AddTriggerBehaviour(transition);

        Assert.True(rep.CanHandle(Trigger.X));
    }

    [Fact]
    public async Task WhenTransitioningFromSubToSuperstate_SubstateEntryActionsExecuted() {
        CreateSuperSubstatePair(out var super, out var sub);

        var executed = false;
        sub.AddEntryAction((_, _) => executed = true, InvocationInfo.Create(null, "entryActionDescription"));
        var transition =
            new StateMachine<State, Trigger>.Transition(super.UnderlyingState, sub.UnderlyingState, Trigger.X);
        await sub.EnterAsync(transition);
        Assert.True(executed);
    }

    [Fact]
    public async Task WhenTransitioningFromSubToSuperstate_SubstateExitActionsExecuted() {
        CreateSuperSubstatePair(out var super, out var sub);

        var executed = false;
        sub.AddExitAction(_ => executed = true, InvocationInfo.Create(null, "exitActionDescription"));
        var transition =
            new StateMachine<State, Trigger>.Transition(sub.UnderlyingState, super.UnderlyingState, Trigger.X);
        await sub.ExitAsync(transition);
        Assert.True(executed);
    }

    [Fact]
    public async Task WhenTransitioningFromSuperToSubstate_SuperExitActionsNotExecuted() {
        CreateSuperSubstatePair(out var super, out var sub);

        var executed = false;
        super.AddExitAction(_ => executed = true, InvocationInfo.Create(null, "exitActionDescription"));
        var transition =
            new StateMachine<State, Trigger>.Transition(super.UnderlyingState, sub.UnderlyingState, Trigger.X);
        await super.ExitAsync(transition);
        Assert.False(executed);
    }

    [Fact]
    public async Task WhenTransitioningToSuperFromSubstate_SuperEntryActionsNotExecuted() {
        CreateSuperSubstatePair(out var super, out var sub);

        var executed = false;
        super.AddEntryAction((_, _) => executed = true, InvocationInfo.Create(null, "entryActionDescription"));
        var transition =
            new StateMachine<State, Trigger>.Transition(super.UnderlyingState, sub.UnderlyingState, Trigger.X);
        await super.EnterAsync(transition);
        Assert.False(executed);
    }

    [Fact]
    public void WhenTransitionUnmetGuardConditions_TriggerCannotBeFired() {
        var rep = CreateRepresentation(State.B);

        var falseConditions = new[] {
            new Tuple<Func<object[], bool>, string>(_ => true, "1"),
            new Tuple<Func<object[], bool>, string>(_ => false, "2")
        };

        var transitionGuard = new TransitionGuard(falseConditions);
        var transition =
            new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(Trigger.X, State.C, transitionGuard);
        rep.AddTriggerBehaviour(transition);

        Assert.False(rep.CanHandle(Trigger.X));
    }
}