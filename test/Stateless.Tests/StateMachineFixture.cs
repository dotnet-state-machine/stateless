namespace Stateless.Tests;

public class StateMachineFixture {
    private const string
        StateA   = "A",
        StateB   = "B",
        StateC   = "C",
        TriggerX = "X",
        TriggerY = "Y";

    private int _numCalls;

    private void CountCalls() {
        _numCalls++;
    }

    private static async Task RunSimpleTest<TState, TTransition>(IEnumerable<TState>      states,
                                                                 IEnumerable<TTransition> transitions) {
        var a = states.First();
        var b = states.Skip(1).First();
        var x = transitions.First();

        var sm = new StateMachine<TState, TTransition>(a);

        sm.Configure(a)
          .Permit(x, b);

        await sm.FireAsync(x);

        Assert.Equal(b, sm.State);
    }

    [Fact]
    public void AcceptedTriggersRespectGuards() {
        var sm = new StateMachine<State, Trigger>(State.B);

        sm.Configure(State.B)
          .PermitIf(Trigger.X, State.A, () => false);

        Assert.Equal(0, sm.GetPermittedTriggers().Count());
    }

    [Fact]
    public void AcceptedTriggersRespectMultipleGuards() {
        var sm = new StateMachine<State, Trigger>(State.B);

        sm.Configure(State.B)
          .PermitIf(Trigger.X, State.A,
                    new Tuple<Func<bool>, string>(() => true, "1"),
                    new Tuple<Func<bool>, string>(() => false, "2"));

        Assert.Equal(0, sm.GetPermittedTriggers().Count());
    }

    [Fact]
    public void CanFire_GetEmptyUnmetGuardDescriptionsIfGuardPasses() {
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.A).PermitIf(Trigger.X, State.B, () => true, "Guard passed");
        var result = sm.CanFire(Trigger.X, out var unmetGuards);

        Assert.True(result);
        Assert.True(unmetGuards?.Count == 0);
    }

    [Fact]
    public void CanFire_GetEmptyUnmetGuardDescriptionsIfValidTrigger() {
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.A).Permit(Trigger.X, State.B);
        var result = sm.CanFire(Trigger.X, out var unmetGuards);

        Assert.True(result);
        Assert.True(unmetGuards?.Count == 0);
    }

    [Fact]
    public void CanFire_GetNullUnmetGuardDescriptionsIfInvalidTrigger() {
        var sm = new StateMachine<State, Trigger>(State.A);
        var result = sm.CanFire(Trigger.X, out var unmetGuards);

        Assert.False(result);
        Assert.Null(unmetGuards);
    }

    [Fact]
    public void CanFire_GetUnmetGuardDescriptionsIfGuardFails() {
        const string guardDescription = "Guard failed";
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.A)
          .PermitIf(Trigger.X, State.B, () => false, guardDescription);

        var result = sm.CanFire(Trigger.X, out var unmetGuards);

        Assert.False(result);
        Assert.True(unmetGuards?.Count == 1);
        Assert.Contains(guardDescription, unmetGuards);
    }

    [Fact]
    public async Task CanUseReferenceTypeMarkers() {
        await RunSimpleTest(
                            new[] { StateA, StateB, StateC },
                            new[] { TriggerX, TriggerY });
    }

    [Fact]
    public async Task CanUseValueTypeMarkers() {
        await RunSimpleTest(
                            Enum.GetValues(typeof(State)).Cast<State>(),
                            Enum.GetValues(typeof(Trigger)).Cast<Trigger>());
    }

    [Fact]
    public void DelayedNestedCyclicConfigurationDetected() {
        // Set up two states and substates, then join them
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.B).SubstateOf(State.A);

        sm.Configure(State.C);
        sm.Configure(State.A).SubstateOf(State.C);

        Assert.Throws(typeof(ArgumentException), () => { sm.Configure(State.C).SubstateOf(State.B); });
    }

    [Fact]
    public void DirectCyclicConfigurationDetected() {
        var sm = new StateMachine<State, Trigger>(State.A);

        Assert.Throws(typeof(ArgumentException), () => { sm.Configure(State.A).SubstateOf(State.A); });
    }

    [Fact]
    public async Task ExceptionThrownForInvalidTransition() {
        var sm = new StateMachine<State, Trigger>(State.A);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => sm.FireAsync(Trigger.X));
        Assert.Equal(exception.Message,
                     "No valid leaving transitions are permitted from state 'A' for trigger 'X'. Consider ignoring the trigger.");
    }

    [Fact]
    public async Task ExceptionThrownForInvalidTransitionMentionsGuardDescriptionIfPresent() {
        // If guard description is empty then method name of guard is used
        // so I have skipped empty description test.
        const string guardDescription = "test";

        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.A).PermitIf(Trigger.X, State.B, () => false, guardDescription);
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => sm.FireAsync(Trigger.X));
        Assert.Equal(typeof(InvalidOperationException), exception.GetType());
    }

    [Fact]
    public async Task ExceptionThrownForInvalidTransitionMentionsMultiGuardGuardDescriptionIfPresent() {
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.A).PermitIf(Trigger.X, State.B,
                                       new Tuple<Func<bool>, string>(() => false, "test1"),
                                       new Tuple<Func<bool>, string>(() => false, "test2"));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => sm.FireAsync(Trigger.X));
        Assert.Equal(typeof(InvalidOperationException), exception.GetType());
    }

    [Fact]
    public async Task ExceptionWhenBothParameterizedGuardClausesFalse() {
        var sm = new StateMachine<State, Trigger>(State.A);
        var x = sm.SetTriggerParameters<int>(Trigger.X);
        // Create Two guards that both must be true
        var positiveGuard = Tuple.Create(new Func<int, bool>(o => o == 2), "Positive Guard");
        var negativeGuard = Tuple.Create(new Func<int, bool>(o => o != 3), "Negative Guard");
        sm.Configure(State.A).PermitIf(x, State.B, positiveGuard, negativeGuard);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sm.FireAsync(x, 3));
    }

    [Fact]
    public async Task ExceptionWhenGuardFalseOnTriggerWithMultipleParameters() {
        var sm = new StateMachine<State, Trigger>(State.A);
        var x = sm.SetTriggerParameters<string, int>(Trigger.X);
        sm.Configure(State.A).PermitIf(x, State.B, (s, i) => s == "3" && i == 3);
        Assert.Equal(sm.State, State.A);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sm.FireAsync(x, "2", 2));
        await Assert.ThrowsAsync<InvalidOperationException>(() => sm.FireAsync(x, "3", 2));
        await Assert.ThrowsAsync<InvalidOperationException>(() => sm.FireAsync(x, "2", 3));
    }

    [Fact]
    public async Task ExceptionWhenIgnoreIfParameterizedGuardFalse() {
        var sm = new StateMachine<State, Trigger>(State.A);
        var x = sm.SetTriggerParameters<int>(Trigger.X);
        sm.Configure(State.A).IgnoreIf(x, i => i == 3);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sm.FireAsync(x, 2));
    }

    [Fact]
    public async Task ExceptionWhenParameterizedGuardFalse() {
        var sm = new StateMachine<State, Trigger>(State.A);
        var x = sm.SetTriggerParameters<int>(Trigger.X);
        sm.Configure(State.A).PermitIf(x, State.B, i => i == 3);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sm.FireAsync(x, 2));
    }

    [Fact]
    public async Task ExceptionWhenPermitDynamicIfHasMultipleNonExclusiveGuards() {
        var sm = new StateMachine<State, Trigger>(State.A);
        var x = sm.SetTriggerParameters<int>(Trigger.X);
        sm.Configure(State.A).PermitDynamicIf(x, i => i == 4 ? State.B : State.C, i => i % 2 == 0)
          .PermitDynamicIf(x, i => i                    == 2 ? State.C : State.D, i => i     == 2);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sm.FireAsync(x, 2));
    }

    [Fact]
    public async Task ExceptionWhenPermitIfHasMultipleNonExclusiveGuards() {
        var sm = new StateMachine<State, Trigger>(State.A);
        var x = sm.SetTriggerParameters<int>(Trigger.X);
        sm.Configure(State.A).PermitIf(x, State.B, i => i % 2 == 0) // Is Even
          .PermitIf(x, State.C, i => i                        == 2);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sm.FireAsync(x, 2));
    }

    /// <summary>
    ///     Verifies guard clauses are only called one time during a transition evaluation.
    /// </summary>
    [Fact]
    public async Task GuardClauseCalledOnlyOnce() {
        var sm = new StateMachine<State, Trigger>(State.A);
        var i = 0;

        sm.Configure(State.A).PermitIf(Trigger.X, State.B, () => {
            ++i;
            return true;
        });

        await sm.FireAsync(Trigger.X);

        Assert.Equal(1, i);
    }

    [Fact]
    public async Task IfSelfTransitionPermited_ActionsFire() {
        var sm = new StateMachine<State, Trigger>(State.B);

        var fired = false;

        sm.Configure(State.B)
          .OnEntry(_ => fired = true)
          .PermitReentry(Trigger.X);

        await sm.FireAsync(Trigger.X);

        Assert.True(fired);
    }

    [Fact]
    public async Task IfSelfTransitionPermited_ActionsFire_InSubstate() {
        var sm = new StateMachine<State, Trigger>(State.A);

        var onEntryStateBFired = false;
        var onExitStateBFired = false;
        var onExitStateAFired = false;

        sm.Configure(State.B)
          .OnEntry(_ => onEntryStateBFired = true)
          .PermitReentry(Trigger.X)
          .OnExit(_ => onExitStateBFired = true);

        sm.Configure(State.A)
          .SubstateOf(State.B)
          .OnExit(_ => onExitStateAFired = true);

        await sm.FireAsync(Trigger.X);

        Assert.Equal(State.B, sm.State);
        Assert.True(onEntryStateBFired);
        Assert.True(onExitStateBFired);
        Assert.True(onExitStateAFired);
    }

    [Fact]
    public async Task IgnoreVsPermitReentry() {
        var sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .OnEntry(CountCalls)
          .PermitReentry(Trigger.X)
          .Ignore(Trigger.Y);

        _numCalls = 0;

        await sm.FireAsync(Trigger.X);
        await sm.FireAsync(Trigger.Y);

        Assert.Equal(1, _numCalls);
    }

    [Fact]
    public async Task IgnoreVsPermitReentryFrom() {
        var sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .OnEntryFrom(Trigger.X, CountCalls)
          .OnEntryFrom(Trigger.Y, CountCalls)
          .PermitReentry(Trigger.X)
          .Ignore(Trigger.Y);

        _numCalls = 0;

        await sm.FireAsync(Trigger.X);
        await sm.FireAsync(Trigger.Y);

        Assert.Equal(1, _numCalls);
    }

    [Fact]
    public void ImplicitReentryIsDisallowed() {
        var sm = new StateMachine<State, Trigger>(State.B);

        Assert.Throws<ArgumentException>(() => sm.Configure(State.B)
                                                 .Permit(Trigger.X, State.B));
    }

    [Fact]
    public void InitialStateIsCurrent() {
        var initial = State.B;
        var sm = new StateMachine<State, Trigger>(initial);
        Assert.Equal(initial, sm.State);
    }

    [Fact]
    public void NestedCyclicConfigurationDetected() {
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.B).SubstateOf(State.A);

        Assert.Throws(typeof(ArgumentException), () => { sm.Configure(State.A).SubstateOf(State.B); });
    }

    [Fact]
    public void NestedTwoLevelsCyclicConfigurationDetected() {
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.B).SubstateOf(State.A);
        sm.Configure(State.C).SubstateOf(State.B);

        Assert.Throws(typeof(ArgumentException), () => { sm.Configure(State.A).SubstateOf(State.C); });
    }

    [Fact]
    public async Task NoExceptionWhenPermitIfHasMultipleExclusiveGuardsBothFalse() {
        var sm = new StateMachine<State, Trigger>(State.A);
        var onUnhandledTriggerWasCalled = false;
        sm.OnUnhandledTrigger((_, _) => { onUnhandledTriggerWasCalled = true; }); // NEVER CALLED
        var i = 0;
        sm.Configure(State.A)
          .PermitIf(Trigger.X, State.B, () => i == 2)
          .PermitIf(Trigger.X, State.C, () => i == 1);

        await sm.FireAsync(Trigger.X); // THROWS EXCEPTION

        Assert.True(onUnhandledTriggerWasCalled, "OnUnhandledTrigger was called");
        Assert.Equal(sm.State, State.A);
    }

    [Fact]
    public async Task NoTransitionWhenIgnoreIfParameterizedGuardTrue() {
        var sm = new StateMachine<State, Trigger>(State.A);
        var x = sm.SetTriggerParameters<int>(Trigger.X);
        sm.Configure(State.A).IgnoreIf(x, i => i == 3);
        await sm.FireAsync(x, 3);

        Assert.Equal(sm.State, State.A);
    }

    [Fact]
    public async Task OnExitFiresOnlyOnceReentrySubstate() {
        var sm = new StateMachine<State, Trigger>(State.A);

        var exitB = 0;
        var exitA = 0;
        var entryB = 0;
        var entryA = 0;

        sm.Configure(State.A)
          .SubstateOf(State.B)
          .OnEntry(() => entryA++)
          .PermitReentry(Trigger.X)
          .OnExit(() => exitA++);

        sm.Configure(State.B)
          .OnEntry(() => entryB++)
          .OnExit(() => exitB++);

        await sm.FireAsync(Trigger.X);

        Assert.Equal(0, exitB);
        Assert.Equal(0, entryB);
        Assert.Equal(1, exitA);
        Assert.Equal(1, entryA);
    }

    [Fact]
    public async Task ParametersSuppliedToFireArePassedToEntryAction() {
        var sm = new StateMachine<State, Trigger>(State.B);

        var x = sm.SetTriggerParameters<string, int>(Trigger.X);

        sm.Configure(State.B)
          .Permit(Trigger.X, State.C);

        string entryArgS = null;
        var entryArgI = 0;

        sm.Configure(State.C)
          .OnEntryFrom(x, (s, i) => {
               entryArgS = s;
               entryArgI = i;
           });

        var suppliedArgS = "something";
        var suppliedArgI = 42;

        await sm.FireAsync(x, suppliedArgS, suppliedArgI);

        Assert.Equal(suppliedArgS, entryArgS);
        Assert.Equal(suppliedArgI, entryArgI);
    }

    [Fact]
    public void PermittedTriggersAreDistinctValues() {
        var sm = new StateMachine<State, Trigger>(State.B);

        sm.Configure(State.B)
          .SubstateOf(State.C)
          .Permit(Trigger.X, State.A);

        sm.Configure(State.C)
          .Permit(Trigger.X, State.B);

        var permitted = sm.GetPermittedTriggers();
        Assert.Equal(1, permitted.Count());
        Assert.Equal(Trigger.X, permitted.First());
    }

    [Fact]
    public void PermittedTriggersExcludeAllUndefinedTriggers() {
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.A)
          .Permit(Trigger.X, State.B);
        Assert.All(new[] { Trigger.Y, Trigger.Z }, trigger => Assert.DoesNotContain(trigger, sm.PermittedTriggers));
    }

    [Fact]
    public void PermittedTriggersIncludeAllDefinedTriggers() {
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.A)
          .Permit(Trigger.X, State.B)
          .InternalTransition(Trigger.Y, _ => { })
          .Ignore(Trigger.Z);
        Assert.All(new[] { Trigger.X, Trigger.Y, Trigger.Z },
                   trigger => Assert.Contains(trigger, sm.PermittedTriggers));
    }

    [Fact]
    public void PermittedTriggersIncludeAllInheritedTriggers() {
        State superState = State.A,
              subState = State.B,
              otherState = State.C;
        Trigger superStateTrigger = Trigger.X,
                subStateTrigger = Trigger.Y;

        StateMachine<State, Trigger> Hsm(State initialState)
            => new StateMachine<State, Trigger>(initialState)
              .Configure(superState)
              .Permit(superStateTrigger, otherState)
              .Machine
              .Configure(subState)
              .SubstateOf(superState)
              .Permit(subStateTrigger, otherState)
              .Machine;

        var hsmInSuperstate = Hsm(superState);
        var hsmInSubstate = Hsm(subState);

        Assert.All(hsmInSuperstate.PermittedTriggers,
                   trigger => Assert.Contains(trigger, hsmInSubstate.PermittedTriggers));
        Assert.Contains(superStateTrigger, hsmInSubstate.PermittedTriggers);
        Assert.Contains(superStateTrigger, hsmInSuperstate.PermittedTriggers);
        Assert.Contains(subStateTrigger, hsmInSubstate.PermittedTriggers);
        Assert.DoesNotContain(subStateTrigger, hsmInSuperstate.PermittedTriggers);
    }

    [Fact]
    public void PermittedTriggersIncludeSuperstatePermittedTriggers() {
        var sm = new StateMachine<State, Trigger>(State.B);

        sm.Configure(State.A)
          .Permit(Trigger.Z, State.B);

        sm.Configure(State.B)
          .SubstateOf(State.C)
          .Permit(Trigger.X, State.A);

        sm.Configure(State.C)
          .Permit(Trigger.Y, State.A);

        var permitted = sm.GetPermittedTriggers();

        Assert.True(permitted.Contains(Trigger.X));
        Assert.True(permitted.Contains(Trigger.Y));
        Assert.False(permitted.Contains(Trigger.Z));
    }

    [Fact]
    public async Task StateCanBeStoredExternally() {
        var state = State.B;
        var sm = new StateMachine<State, Trigger>(() => state, s => state = s);
        sm.Configure(State.B).Permit(Trigger.X, State.C);
        Assert.Equal(State.B, sm.State);
        Assert.Equal(State.B, state);
        await sm.FireAsync(Trigger.X);
        Assert.Equal(State.C, sm.State);
        Assert.Equal(State.C, state);
    }

    [Fact]
    public async Task StateMutatorShouldBeCalledOnlyOnce() {
        var state = State.B;
        var count = 0;
        var sm = new StateMachine<State, Trigger>(() => state, s => {
            state = s;
            count++;
        });
        sm.Configure(State.B).Permit(Trigger.X, State.C);
        await sm.FireAsync(Trigger.X);
        Assert.Equal(1, count);
    }

    [Fact]
    public void SubstateIsIncludedInCurrentState() {
        var sm = new StateMachine<State, Trigger>(State.B);
        sm.Configure(State.B).SubstateOf(State.C);

        Assert.Equal(State.B, sm.State);
        Assert.True(sm.IsInState(State.C));
    }

    /// <summary>
    ///     The expected ordering is OnExit, OnTransitioned, OnEntry, OnTransitionCompleted
    /// </summary>
    [Fact]
    public async Task TheOnTransitionedEventFiresBeforeTheOnEntryEventAndOnTransitionCompletedFiresAfterwards() {
        var sm = new StateMachine<State, Trigger>(State.B);
        var expectedOrdering = new List<string> { "OnExit", "OnTransitioned", "OnEntry", "OnTransitionCompleted" };
        var actualOrdering = new List<string>();

        sm.Configure(State.B)
          .Permit(Trigger.X, State.A)
          .OnExit(() => actualOrdering.Add("OnExit"));

        sm.Configure(State.A)
          .OnEntry(() => actualOrdering.Add("OnEntry"));

        sm.OnTransitioned(_ => actualOrdering.Add("OnTransitioned"));
        sm.OnTransitionCompleted(_ => actualOrdering.Add("OnTransitionCompleted"));

        await sm.FireAsync(Trigger.X);

        Assert.Equal(expectedOrdering.Count, actualOrdering.Count);
        for (var i = 0; i < expectedOrdering.Count; i++) Assert.Equal(expectedOrdering[i], actualOrdering[i]);
    }

    [Fact]
    public async Task TransitionToSuperstateDoesNotExitSuperstate() {
        var sm = new StateMachine<State, Trigger>(State.B);

        var superExit = false;
        var superEntry = false;
        var subExit = false;

        sm.Configure(State.A)
          .OnEntry(() => superEntry = true)
          .OnExit(() => superExit   = true);

        sm.Configure(State.B)
          .SubstateOf(State.A)
          .Permit(Trigger.Y, State.A)
          .OnExit(() => subExit = true);

        await sm.FireAsync(Trigger.Y);

        Assert.True(subExit);
        Assert.False(superEntry);
        Assert.False(superExit);
    }

    [Fact]
    public async Task TransitionWhenBothParameterizedGuardClausesTrue() {
        var sm = new StateMachine<State, Trigger>(State.A);
        var x = sm.SetTriggerParameters<int>(Trigger.X);
        var positiveGuard = Tuple.Create(new Func<int, bool>(o => o == 2), "Positive Guard");
        var negativeGuard = Tuple.Create(new Func<int, bool>(o => o != 3), "Negative Guard");
        sm.Configure(State.A).PermitIf(x, State.B, positiveGuard, negativeGuard);
        await sm.FireAsync(x, 2);

        Assert.Equal(sm.State, State.B);
    }

    [Fact]
    public async Task TransitionWhenGuardReturnsTrueOnTriggerWithMultipleParameters() {
        var sm = new StateMachine<State, Trigger>(State.A);
        var x = sm.SetTriggerParameters<string, int>(Trigger.X);
        sm.Configure(State.A).PermitIf(x, State.B, (s, i) => s == "3" && i == 3);
        await sm.FireAsync(x, "3", 3);
        Assert.Equal(sm.State, State.B);
    }

    [Fact]
    public async Task TransitionWhenParameterizedGuardTrue() {
        var sm = new StateMachine<State, Trigger>(State.A);
        var x = sm.SetTriggerParameters<int>(Trigger.X);
        sm.Configure(State.A).PermitIf(x, State.B, i => i == 2);
        await sm.FireAsync(x, 2);

        Assert.Equal(sm.State, State.B);
    }

    [Fact]
    public async Task TransitionWhenPermitDynamicIfHasMultipleExclusiveGuards() {
        var sm = new StateMachine<State, Trigger>(State.A);
        var x = sm.SetTriggerParameters<int>(Trigger.X);
        sm.Configure(State.A)
          .PermitDynamicIf(x, i => i == 3 ? State.B : State.C, i => i == 3 || i == 5)
          .PermitDynamicIf(x, i => i == 2 ? State.C : State.D, i => i == 2 || i == 4);
        await sm.FireAsync(x, 3);
        Assert.Equal(sm.State, State.B);
    }

    [Fact]
    public async Task TransitionWhenPermitIfHasMultipleExclusiveGuards() {
        var sm = new StateMachine<State, Trigger>(State.A);
        var x = sm.SetTriggerParameters<int>(Trigger.X);
        sm.Configure(State.A)
          .PermitIf(x, State.B, i => i == 3)
          .PermitIf(x, State.C, i => i == 2);
        await sm.FireAsync(x, 3);
        Assert.Equal(sm.State, State.B);
    }

    [Fact]
    public async Task TransitionWhenPermitIfHasMultipleExclusiveGuardsWithSuperStateFalse() {
        var sm = new StateMachine<State, Trigger>(State.B);
        var x = sm.SetTriggerParameters<int>(Trigger.X);
        sm.Configure(State.A).PermitIf(x, State.D, i => i == 3);
        {
            sm.Configure(State.B).SubstateOf(State.A).PermitIf(x, State.C, i => i == 2);
        }
        await sm.FireAsync(x, 2);
        Assert.Equal(sm.State, State.C);
    }

    [Fact]
    public async Task TransitionWhenPermitIfHasMultipleExclusiveGuardsWithSuperStateTrue() {
        var sm = new StateMachine<State, Trigger>(State.B);
        var x = sm.SetTriggerParameters<int>(Trigger.X);
        sm.Configure(State.A).PermitIf(x, State.D, i => i == 3);
        {
            sm.Configure(State.B).SubstateOf(State.A).PermitIf(x, State.C, i => i == 2);
        }
        await sm.FireAsync(x, 3);
        Assert.Equal(sm.State, State.D);
    }

    [Fact]
    public async Task TransitionWhenPermitReentryIfParameterizedGuardFalse() {
        var sm = new StateMachine<State, Trigger>(State.A);
        var x = sm.SetTriggerParameters<int>(Trigger.X);
        sm.Configure(State.A)
          .PermitReentryIf(x, i => i == 3);

        await Assert.ThrowsAsync<InvalidOperationException>(() => sm.FireAsync(x, 2));
    }

    [Fact]
    public async Task TransitionWhenPermitReentryIfParameterizedGuardTrue() {
        var sm = new StateMachine<State, Trigger>(State.A);
        var x = sm.SetTriggerParameters<int>(Trigger.X);
        sm.Configure(State.A)
          .PermitReentryIf(x, i => i == 3);
        await sm.FireAsync(x, 3);
        Assert.Equal(sm.State, State.A);
    }

    [Fact]
    public void TriggerParametersAreImmutableOnceSet() {
        var sm = new StateMachine<State, Trigger>(State.B);
        sm.SetTriggerParameters<string, int>(Trigger.X);
        Assert.Throws<InvalidOperationException>(() => sm.SetTriggerParameters<string>(Trigger.X));
    }

    [Fact]
    public async Task WhenAnUnhandledTriggerIsFired_TheProvidedHandlerIsCalledWithStateAndTrigger() {
        var sm = new StateMachine<State, Trigger>(State.B);

        State? state = null;
        Trigger? trigger = null;
        sm.OnUnhandledTrigger((s, t, _) => {
            state   = s;
            trigger = t;
        });

        await sm.FireAsync(Trigger.Z);

        Assert.Equal(State.B, state);
        Assert.Equal(Trigger.Z, trigger);
    }

    [Fact]
    public async Task WhenATransitionOccurs_TheOnTransitionCompletedEventFires() {
        var sm = new StateMachine<State, Trigger>(State.B);

        sm.Configure(State.B)
          .Permit(Trigger.X, State.A);

        StateMachine<State, Trigger>.Transition transition = null;
        sm.OnTransitionCompleted(t => transition = t);

        await sm.FireAsync(Trigger.X);

        Assert.NotNull(transition);
        Assert.Equal(Trigger.X, transition.Trigger);
        Assert.Equal(State.B, transition.Source);
        Assert.Equal(State.A, transition.Destination);
        Assert.Equal(Array.Empty<object>(), transition.Parameters);
    }

    [Fact]
    public async Task WhenATransitionOccurs_TheOnTransitionedEventFires() {
        var sm = new StateMachine<State, Trigger>(State.B);

        sm.Configure(State.B)
          .Permit(Trigger.X, State.A);

        StateMachine<State, Trigger>.Transition transition = null;
        sm.OnTransitioned(t => transition = t);

        await sm.FireAsync(Trigger.X);

        Assert.NotNull(transition);
        Assert.Equal(Trigger.X, transition.Trigger);
        Assert.Equal(State.B, transition.Source);
        Assert.Equal(State.A, transition.Destination);
        Assert.Equal(Array.Empty<object>(), transition.Parameters);
    }

    [Fact]
    public async Task WhenATransitionOccurs_WithAParameterizedTrigger_TheOnTransitionCompletedEventFires() {
        var sm = new StateMachine<State, Trigger>(State.B);
        var triggerX = sm.SetTriggerParameters<string>(Trigger.X);

        sm.Configure(State.B)
          .Permit(Trigger.X, State.A);

        StateMachine<State, Trigger>.Transition transition = null;
        sm.OnTransitionCompleted(t => transition = t);

        var parameter = "the parameter";
        await sm.FireAsync(triggerX, parameter);

        Assert.NotNull(transition);
        Assert.Equal(Trigger.X, transition.Trigger);
        Assert.Equal(State.B, transition.Source);
        Assert.Equal(State.A, transition.Destination);
        Assert.Equal(1, transition.Parameters.Length);
        Assert.Equal(parameter, transition.Parameters[0]);
    }

    [Fact]
    public async Task WhenATransitionOccurs_WithAParameterizedTrigger_TheOnTransitionedEventFires() {
        var sm = new StateMachine<State, Trigger>(State.B);
        var triggerX = sm.SetTriggerParameters<string>(Trigger.X);

        sm.Configure(State.B)
          .Permit(Trigger.X, State.A);

        StateMachine<State, Trigger>.Transition transition = null;
        sm.OnTransitioned(t => transition = t);

        var parameter = "the parameter";
        await sm.FireAsync(triggerX, parameter);

        Assert.NotNull(transition);
        Assert.Equal(Trigger.X, transition.Trigger);
        Assert.Equal(State.B, transition.Source);
        Assert.Equal(State.A, transition.Destination);
        Assert.Equal(1, transition.Parameters.Length);
        Assert.Equal(parameter, transition.Parameters[0]);
    }

    [Fact]
    public async Task
        WhenATransitionOccurs_WithAParameterizedTrigger_WithMultipleParameters_TheOnTransitionCompletedEventFires() {
        var sm = new StateMachine<State, Trigger>(State.B);
        var triggerX = sm.SetTriggerParameters<string, int, bool>(Trigger.X);

        sm.Configure(State.B)
          .Permit(Trigger.X, State.A);

        StateMachine<State, Trigger>.Transition transition = null;
        sm.OnTransitionCompleted(t => transition = t);
        
        const string firstParameter = "the parameter";
        const int secondParameter = 99;
        const bool thirdParameter = true;
        await sm.FireAsync(triggerX, firstParameter, secondParameter, thirdParameter);

        Assert.NotNull(transition);
        Assert.Equal(Trigger.X, transition.Trigger);
        Assert.Equal(State.B, transition.Source);
        Assert.Equal(State.A, transition.Destination);
        Assert.Equal(3, transition.Parameters.Length);
        Assert.Equal(firstParameter, transition.Parameters[0]);
        Assert.Equal(secondParameter, transition.Parameters[1]);
        Assert.Equal(thirdParameter, transition.Parameters[2]);
    }

    [Fact]
    public async Task
        WhenATransitionOccurs_WithAParameterizedTrigger_WithMultipleParameters_TheOnTransitionedEventFires() {
        var sm = new StateMachine<State, Trigger>(State.B);
        var triggerX = sm.SetTriggerParameters<string, int, bool>(Trigger.X);

        sm.Configure(State.B)
          .Permit(Trigger.X, State.A);

        StateMachine<State, Trigger>.Transition transition = null;
        sm.OnTransitioned(t => transition = t);

        const string firstParameter = "the parameter";
        const int secondParameter = 99;
        const bool thirdParameter = true;
        await sm.FireAsync(triggerX, firstParameter, secondParameter, thirdParameter);

        Assert.NotNull(transition);
        Assert.Equal(Trigger.X, transition.Trigger);
        Assert.Equal(State.B, transition.Source);
        Assert.Equal(State.A, transition.Destination);
        Assert.Equal(3, transition.Parameters.Length);
        Assert.Equal(firstParameter, transition.Parameters[0]);
        Assert.Equal(secondParameter, transition.Parameters[1]);
        Assert.Equal(thirdParameter, transition.Parameters[2]);
    }

    [Fact]
    public void
        WhenConfigureConditionallyPermittedTransitionOnTriggerWithParameters_ThenStateMachineCanEnumeratePermittedTriggers() {
        const Trigger trigger = Trigger.X;
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.A).PermitIf(sm.SetTriggerParameters<string>(trigger), State.B, _ => true);
        Assert.Single(sm.PermittedTriggers, trigger);
    }

    [Fact]
    public void WhenConfigureConditionallyPermittedTransitionOnTriggerWithParameters_ThenStateMachineCanFireTrigger() {
        const Trigger trigger = Trigger.X;
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.A).PermitIf(sm.SetTriggerParameters<string>(trigger), State.B, _ => true);
        Assert.True(sm.CanFire(trigger));
    }

    [Fact]
    public void
        WhenConfigureInternalTransitionOnTriggerWithoutParameters_ThenStateMachineCanEnumeratePermittedTriggers() {
        var trigger = Trigger.X;
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.A).InternalTransition(trigger, _ => { });
        Assert.Single(sm.PermittedTriggers, trigger);
    }

    [Fact]
    public void WhenConfigureInternalTransitionOnTriggerWithoutParameters_ThenStateMachineCanFireTrigger() {
        var trigger = Trigger.X;
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.A).InternalTransition(trigger, _ => { });
        Assert.True(sm.CanFire(trigger));
    }

    [Fact]
    public void WhenConfigureInternalTransitionOnTriggerWithParameters_ThenStateMachineCanEnumeratePermittedTriggers() {
        var trigger = Trigger.X;
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.A).InternalTransition(sm.SetTriggerParameters<string>(trigger), (_, _) => { });
        Assert.Single(sm.PermittedTriggers, trigger);
    }

    [Fact]
    public void WhenConfigureInternalTransitionOnTriggerWithParameters_ThenStateMachineCanFireTrigger() {
        var trigger = Trigger.X;
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.A).InternalTransition(sm.SetTriggerParameters<string>(trigger), (_, _) => { });
        Assert.True(sm.CanFire(trigger));
    }

    [Fact]
    public void
        WhenConfigurePermittedTransitionOnTriggerWithoutParameters_ThenStateMachineCanEnumeratePermittedTriggers() {
        var trigger = Trigger.X;
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.A).Permit(trigger, State.B);
        Assert.Single(sm.PermittedTriggers, trigger);
    }

    [Fact]
    public void WhenConfigurePermittedTransitionOnTriggerWithoutParameters_ThenStateMachineCanFireTrigger() {
        var trigger = Trigger.X;
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.A).Permit(trigger, State.B);
        Assert.True(sm.CanFire(trigger));
    }

    [Fact]
    public void
        WhenConfigurePermittedTransitionOnTriggerWithParameters_ThenStateMachineCanEnumeratePermittedTriggers() {
        var trigger = Trigger.X;
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.A).Permit(trigger, State.B);
        sm.SetTriggerParameters<string>(trigger);
        Assert.Single(sm.PermittedTriggers, trigger);
    }


    [Fact]
    public void WhenConfigurePermittedTransitionOnTriggerWithParameters_ThenStateMachineCanFireTrigger() {
        var trigger = Trigger.X;
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.A).Permit(trigger, State.B);
        sm.SetTriggerParameters<string>(trigger);
        Assert.True(sm.CanFire(trigger));
    }

    [Fact]
    public async Task WhenDiscriminatedByGuard_ChoosesPermitedTransition() {
        var sm = new StateMachine<State, Trigger>(State.B);

        sm.Configure(State.B)
          .PermitIf(Trigger.X, State.A, () => false)
          .PermitIf(Trigger.X, State.C, () => true);

        await sm.FireAsync(Trigger.X);

        Assert.Equal(State.C, sm.State);
    }

    [Fact]
    public async Task WhenDiscriminatedByMultiConditionGuard_ChoosesPermitedTransition() {
        var sm = new StateMachine<State, Trigger>(State.B);

        sm.Configure(State.B)
          .PermitIf(Trigger.X, State.A,
                    new Tuple<Func<bool>, string>(() => true, "1"),
                    new Tuple<Func<bool>, string>(() => false, "2"))
          .PermitIf(Trigger.X, State.C,
                    new Tuple<Func<bool>, string>(() => true, "1"),
                    new Tuple<Func<bool>, string>(() => true, "2"));

        await sm.FireAsync(Trigger.X);

        Assert.Equal(State.C, sm.State);
    }

    [Fact]
    public async Task WhenInSubstate_TriggerIgnoredInSuperstate_RemainsInSubstate() {
        var sm = new StateMachine<State, Trigger>(State.B);

        sm.Configure(State.B)
          .SubstateOf(State.C);

        sm.Configure(State.C)
          .Ignore(Trigger.X);

        await sm.FireAsync(Trigger.X);

        Assert.Equal(State.B, sm.State);
    }

    [Fact]
    public void WhenParameterizedGuardTrue_ThenStateMachineCanFireTrigger() {
        var sm = new StateMachine<State, Trigger>(State.A);
        var x = sm.SetTriggerParameters<int>(Trigger.X);
        sm.Configure(State.A).PermitIf(x, State.B, i => i == 2);
        Assert.False(sm.CanFire(x, 1));
        Assert.True(sm.CanFire(x, 2));
    }

    [Fact]
    public async Task WhenTriggerIsIgnored_ActionsNotExecuted() {
        var sm = new StateMachine<State, Trigger>(State.B);

        var fired = false;

        sm.Configure(State.B)
          .OnEntry(_ => fired = true)
          .Ignore(Trigger.X);

        await sm.FireAsync(Trigger.X);

        Assert.False(fired);
    }
}