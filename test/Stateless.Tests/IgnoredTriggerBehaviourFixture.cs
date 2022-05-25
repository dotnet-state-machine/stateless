namespace Stateless.Tests;

public class IgnoredTriggerBehaviourFixture {
    private static bool False(params object[] args) => false;

    private static bool True(params object[] args) => true;

    [Fact]
    public void ExposesCorrectUnderlyingTrigger() {
        var ignored = new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(
                                                                               Trigger.X, null);

        Assert.Equal(Trigger.X, ignored.Trigger);
    }

    [Fact]
    public void IgnoredTriggerMustBeIgnoredSync() {
        var internalActionExecuted = false;
        var stateMachine = new StateMachine<State, Trigger>(State.B);
        stateMachine.Configure(State.A)
                    .Permit(Trigger.X, State.C);

        stateMachine.Configure(State.B)
                    .SubstateOf(State.A)
                    .Ignore(Trigger.X);

        try {
            // >>> The following statement should not execute the internal action
            stateMachine.Fire(Trigger.X);
        } catch (NullReferenceException) {
            internalActionExecuted = true;
        }

        Assert.False(internalActionExecuted);
    }

    [Fact]
    public void IgnoreIfFalseTriggerMustNotBeIgnored() {
        var stateMachine = new StateMachine<State, Trigger>(State.B);
        stateMachine.Configure(State.A)
                    .Permit(Trigger.X, State.C);

        stateMachine.Configure(State.B)
                    .SubstateOf(State.A)
                    .IgnoreIf(Trigger.X, () => false);

        stateMachine.Fire(Trigger.X);

        Assert.Equal(State.C, stateMachine.State);
    }

    [Fact]
    public void IgnoreIfTrueTriggerMustBeIgnored() {
        var stateMachine = new StateMachine<State, Trigger>(State.B);
        stateMachine.Configure(State.A)
                    .Permit(Trigger.X, State.C);

        stateMachine.Configure(State.B)
                    .SubstateOf(State.A)
                    .IgnoreIf(Trigger.X, () => true);

        stateMachine.Fire(Trigger.X);

        Assert.Equal(State.B, stateMachine.State);
    }

    [Fact]
    public void StateRemainsUnchanged() {
        var ignored = new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(Trigger.X, null);
        Assert.False(ignored.ResultsInTransitionFrom(State.B, Array.Empty<object>(), out _));
    }

    [Fact]
    public void WhenGuardConditionFalse_IsGuardConditionMetIsFalse() {
        var ignored = new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(
                                                                               Trigger.X, new TransitionGuard(False));

        Assert.False(ignored.GuardConditionsMet());
    }

    [Fact]
    public void WhenGuardConditionTrue_IsGuardConditionMetIsTrue() {
        var ignored = new StateMachine<State, Trigger>.IgnoredTriggerBehaviour(
                                                                               Trigger.X, new TransitionGuard(True));

        Assert.True(ignored.GuardConditionsMet());
    }
}