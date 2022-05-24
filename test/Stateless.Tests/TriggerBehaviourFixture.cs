namespace Stateless.Tests;

public class TriggerBehaviourFixture {
    private static bool False(params object[] args) => false;

    private static bool True(params object[] args) => true;

    [Fact]
    public void ExposesCorrectUnderlyingTrigger() {
        var transitioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
         Trigger.X, State.C, null);

        Assert.Equal(Trigger.X, transitioning.Trigger);
    }

    [Fact]
    public void WhenAllGuardConditionsTrue_GuardConditionsMetIsTrue() {
        var trueGuard = new[] {
            new Tuple<Func<object[], bool>, string>(_ => true, "1"),
            new Tuple<Func<object[], bool>, string>(_ => true, "2")
        };

        var transitioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
         Trigger.X, State.C, new TransitionGuard(trueGuard));

        Assert.True(transitioning.GuardConditionsMet());
    }

    [Fact]
    public void WhenAllMultipleGuardConditionsFalse_IsGuardConditionsMetIsFalse() {
        var falseGuard = new[] {
            new Tuple<Func<object[], bool>, string>(_ => false, "1"),
            new Tuple<Func<object[], bool>, string>(_ => false, "2")
        };

        var transitioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
         Trigger.X, State.C, new TransitionGuard(falseGuard));

        Assert.False(transitioning.GuardConditionsMet());
    }

    [Fact]
    public void WhenGuardConditionFalse_GuardConditionsMetIsFalse() {
        var transitioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
         Trigger.X, State.C, new TransitionGuard(False));

        Assert.False(transitioning.GuardConditionsMet());
    }

    [Fact]
    public void WhenGuardConditionTrue_GuardConditionsMetIsTrue() {
        var transitioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
         Trigger.X, State.C, new TransitionGuard(True));

        Assert.True(transitioning.GuardConditionsMet());
    }

    [Fact]
    public void WhenOneOfMultipleGuardConditionsFalse_GuardConditionsMetIsFalse() {
        var falseGuard = new[] {
            new Tuple<Func<object[], bool>, string>(_ => true, "1"),
            new Tuple<Func<object[], bool>, string>(_ => true, "2")
        };

        var transitioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(
         Trigger.X, State.C, new TransitionGuard(falseGuard));

        Assert.True(transitioning.GuardConditionsMet());
    }
}