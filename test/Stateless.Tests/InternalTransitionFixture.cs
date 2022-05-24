namespace Stateless.Tests;

public class InternalTransitionFixture {
    [Fact]
    public async Task AllowTriggerWithThreeParameters() {
        var sm = new StateMachine<State, Trigger>(State.B);
        var trigger = sm.SetTriggerParameters<int, string, bool>(Trigger.X);
        const int intParam = 5;
        const string strParam = "Five";
        var boolParam = true;
        var callbackInvoked = false;

        sm.Configure(State.B)
          .InternalTransition(trigger, (i, s, b, _) => {
               callbackInvoked = true;
               Assert.Equal(intParam, i);
               Assert.Equal(strParam, s);
               Assert.Equal(boolParam, b);
           });

        await sm.FireAsync(trigger, intParam, strParam, boolParam);
        Assert.True(callbackInvoked);
    }

    [Fact]
    public async Task AllowTriggerWithTwoParameters() {
        var sm = new StateMachine<State, Trigger>(State.B);
        var trigger = sm.SetTriggerParameters<int, string>(Trigger.X);
        const int intParam = 5;
        const string strParam = "Five";
        var callbackInvoked = false;

        sm.Configure(State.B)
          .InternalTransition(trigger, (i, s, _) => {
               callbackInvoked = true;
               Assert.Equal(intParam, i);
               Assert.Equal(strParam, s);
           });

        await sm.FireAsync(trigger, intParam, strParam);
        Assert.True(callbackInvoked);
    }

    [Fact]
    public async Task AsyncHandlesNonAsyndActionAsync() {
        var handled = false;

        var sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .InternalTransition(Trigger.Y, () => handled = true);

        await sm.FireAsync(Trigger.Y);

        Assert.True(handled);
    }

    [Fact]
    public void ConditionalInternalTransition_ShouldBeReflectedInPermittedTriggers() {
        var isPermitted = true;
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.A)
          .InternalTransitionIf(Trigger.X, () => isPermitted, _ => { });

        Assert.Equal(1, sm.GetPermittedTriggers().ToArray().Length);
        isPermitted = false;
        Assert.Equal(0, sm.GetPermittedTriggers().ToArray().Length);
    }

    [Fact]
    public async Task ConditionalTriggerWithoutParameters() {
        var sm = new StateMachine<State, Trigger>(State.B);
        var predicateInvoked = false;
        var callbackInvoked = false;

        sm.Configure(State.B)
          .InternalTransitionIf(Trigger.X, () => {
                                    predicateInvoked = true;
                                    return true;
                                },
                                () => { callbackInvoked = true; });

        await sm.FireAsync(Trigger.X);
        Assert.True(predicateInvoked);
        Assert.True(callbackInvoked);
    }

    [Fact]
    public async Task ConditionalTriggerWithParameter() {
        var sm = new StateMachine<State, Trigger>(State.B);
        var trigger = sm.SetTriggerParameters<int>(Trigger.X);
        const int intParam = 5;
        var predicateInvoked = false;
        var callbackInvoked = false;

        sm.Configure(State.B)
          .InternalTransitionIf(trigger, i => {
                                    predicateInvoked = true;
                                    Assert.Equal(intParam, i);
                                    return true;
                                },
                                i => {
                                    callbackInvoked = true;
                                    Assert.Equal(intParam, i);
                                });

        await sm.FireAsync(trigger, intParam);
        Assert.True(predicateInvoked);
        Assert.True(callbackInvoked);
    }

    [Fact]
    public async Task InternalTriggerHandledOnlyOnceInSub() {
        var handledIn = State.C;

        var sm = new StateMachine<State, Trigger>(State.B);

        sm.Configure(State.A)
          .InternalTransition(Trigger.X, () => handledIn = State.A);

        sm.Configure(State.B)
          .SubstateOf(State.A)
          .InternalTransition(Trigger.X, () => handledIn = State.B);

        // The state machine is in state B. It should only be handled in in State B, so handledIn should be equal to State.B
        await sm.FireAsync(Trigger.X);

        Assert.Equal(State.B, handledIn);
    }

    [Fact]
    public async Task InternalTriggerHandledOnlyOnceInSuper() {
        var handledIn = State.C;

        var sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .InternalTransition(Trigger.X, () => handledIn = State.A);

        sm.Configure(State.B)
          .SubstateOf(State.A)
          .InternalTransition(Trigger.X, () => handledIn = State.B);

        // The state machine is in state A. It should only be handled in in State A, so handledIn should be equal to State.A
        await sm.FireAsync(Trigger.X);

        Assert.Equal(State.A, handledIn);
    }

    [Fact]
    public async Task OnlyOneHandlerExecuted() {
        var handled = 0;

        var sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .InternalTransition(Trigger.X, () => handled++)
          .InternalTransition(Trigger.Y, () => handled++);

        await sm.FireAsync(Trigger.X);

        Assert.Equal(1, handled);

        await sm.FireAsync(Trigger.Y);

        Assert.Equal(2, handled);
    }

    [Fact]
    public async Task StayInSameStateOneState_Action() {
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.A)
          .InternalTransition(Trigger.X, () => { });

        Assert.Equal(State.A, sm.State);
        await sm.FireAsync(Trigger.X);
        Assert.Equal(State.A, sm.State);
    }

    /// <summary>
    ///     The expected behaviour of the internal transistion is that the state does not change.
    ///     This will fail if the state changes after the trigger has fired.
    /// </summary>
    [Fact]
    public async Task StayInSameStateOneState_Transition() {
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.A)
          .InternalTransition(Trigger.X, _ => { });

        Assert.Equal(State.A, sm.State);
        await sm.FireAsync(Trigger.X);
        Assert.Equal(State.A, sm.State);
    }

    [Fact]
    public async Task StayInSameStateTwoStates_Action() {
        var sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .InternalTransition(Trigger.X, () => { })
          .Permit(Trigger.Y, State.B);

        sm.Configure(State.B)
          .InternalTransition(Trigger.X, () => { })
          .Permit(Trigger.Y, State.A);

        // This should not cause any state changes
        Assert.Equal(State.A, sm.State);
        await sm.FireAsync(Trigger.X);
        Assert.Equal(State.A, sm.State);

        // Change state to B
        await sm.FireAsync(Trigger.Y);

        // This should also not cause any state changes
        Assert.Equal(State.B, sm.State);
        await sm.FireAsync(Trigger.X);
        Assert.Equal(State.B, sm.State);
    }

    [Fact]
    public async Task StayInSameStateTwoStates_Transition() {
        var sm = new StateMachine<State, Trigger>(State.A);

        sm.Configure(State.A)
          .InternalTransition(Trigger.X, _ => { })
          .Permit(Trigger.Y, State.B);

        sm.Configure(State.B)
          .InternalTransition(Trigger.X, _ => { })
          .Permit(Trigger.Y, State.A);

        // This should not cause any state changes
        Assert.Equal(State.A, sm.State);
        await sm.FireAsync(Trigger.X);
        Assert.Equal(State.A, sm.State);

        // Change state to B
        await sm.FireAsync(Trigger.Y);

        // This should also not cause any state changes
        Assert.Equal(State.B, sm.State);
        await sm.FireAsync(Trigger.X);
        Assert.Equal(State.B, sm.State);
    }

    [Fact]
    public async Task StayInSameSubStateTransitionInSubstate_Action() {
        var sm = new StateMachine<State, Trigger>(State.B);

        sm.Configure(State.A);

        sm.Configure(State.B)
          .SubstateOf(State.A)
          .InternalTransition(Trigger.X, () => { })
          .InternalTransition(Trigger.Y, () => { });

        // This should not cause any state changes
        Assert.Equal(State.B, sm.State);
        await sm.FireAsync(Trigger.X);
        Assert.Equal(State.B, sm.State);
        await sm.FireAsync(Trigger.Y);
        Assert.Equal(State.B, sm.State);
    }

    [Fact]
    public async Task StayInSameSubStateTransitionInSubstate_Transition() {
        var sm = new StateMachine<State, Trigger>(State.B);

        sm.Configure(State.A);

        sm.Configure(State.B)
          .SubstateOf(State.A)
          .InternalTransition(Trigger.X, _ => { });

        // This should not cause any state changes
        Assert.Equal(State.B, sm.State);
        await sm.FireAsync(Trigger.X);
        Assert.Equal(State.B, sm.State);
    }

    [Fact]
    public async Task StayInSameSubStateTransitionInSuperstate_Action() {
        var sm = new StateMachine<State, Trigger>(State.B);

        sm.Configure(State.A)
          .InternalTransition(Trigger.X, () => { })
          .InternalTransition(Trigger.Y, () => { });

        sm.Configure(State.B)
          .SubstateOf(State.A);

        // This should not cause any state changes
        Assert.Equal(State.B, sm.State);
        await sm.FireAsync(Trigger.X);
        Assert.Equal(State.B, sm.State);
        await sm.FireAsync(Trigger.Y);
        Assert.Equal(State.B, sm.State);
    }

    [Fact]
    public async Task StayInSameSubStateTransitionInSuperstate_Transition() {
        var sm = new StateMachine<State, Trigger>(State.B);

        sm.Configure(State.A)
          .InternalTransition(Trigger.X, _ => { });

        sm.Configure(State.B)
          .SubstateOf(State.A);

        // This should not cause any state changes
        Assert.Equal(State.B, sm.State);
        await sm.FireAsync(Trigger.X);
        Assert.Equal(State.B, sm.State);
    }
}