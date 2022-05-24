namespace Stateless.Tests;

public class TriggerWithParametersFixture {
    /// <summary>
    ///     Issue #450 - GetPermittedTriggers throws exception when triggers have different parameter types.
    /// </summary>
    [Theory]
    [MemberData(nameof(DifferentTypeTriggersData))]
    public void DifferentTypeTriggersThrowNoException(params object[] arg) {
        var fsm = new StateMachine<State, Trigger>(State.A);
        var twp1 = fsm.SetTriggerParameters<bool>(Trigger.X);
        var twp2 = fsm.SetTriggerParameters<int, string>(Trigger.Y);
        var twp3 = fsm.SetTriggerParameters<string, int, bool>(Trigger.Z);
        fsm.Configure(State.A)
           .PermitIf(twp1, State.B, _ => true)
           .PermitIf(twp2, State.C, (_, _) => true)
           .PermitIf(twp3, State.D, (_, _, _) => true);
        Assert.Equal(1, fsm.GetPermittedTriggers(arg).Count());
    }

    public static IEnumerable<object[]> DifferentTypeTriggersData() {
        yield return new object[] { "arg", 123, true };
        yield return new object[] { 123, "arg" };
        yield return new object[] { true };
    }

    [Fact]
    public void DescribesUnderlyingTrigger() {
        var twp = new TriggerWithParameters<Trigger, string>(Trigger.X);
        Assert.Equal(Trigger.X, twp.Trigger);
    }

    [Fact]
    public void GetPermittedTriggersShouldAcceptStronglyTypedTriggersWithConditionalGuardsConfigurations() {
        var stateMachine = new StateMachine<State, Trigger>(State.A);
        var firstTrigger = new TriggerWithParameters<Trigger, FirstFakeTrigger>(Trigger.X);
        var secondTrigger = new TriggerWithParameters<Trigger, SecondFakeTrigger>(Trigger.Y);

        stateMachine.Configure(State.A)
                    .PermitIf(firstTrigger, State.B, trigger => trigger.IsAllowed)
                    .PermitIf(secondTrigger, State.C, trigger => trigger.IsOk);

        var fakeTriggers = new List<object> {
            new FirstFakeTrigger { IsAllowed = true }, new SecondFakeTrigger { IsOk = false }
        };

        var availableTriggers = new List<Trigger>();
        foreach (var fakeTrigger in fakeTriggers) {
            var temp = stateMachine.GetPermittedTriggers(fakeTrigger);
            availableTriggers.AddRange(temp);
        }

        Assert.Contains(Trigger.X, availableTriggers);
        Assert.DoesNotContain(Trigger.Y, availableTriggers);
    }

    [Fact]
    public void IncompatibleParameterListIsNotValid() {
        var twp = new TriggerWithParameters<Trigger>(Trigger.X, typeof(int), typeof(string));
        Assert.Throws<ArgumentException>(() => twp.ValidateParameters(new object[] { 123 }));
    }

    [Fact]
    public void IncompatibleParametersAreNotValid() {
        var twp = new TriggerWithParameters<Trigger, string>(Trigger.X);
        Assert.Throws<ArgumentException>(() => twp.ValidateParameters(new object[] { 123 }));
    }

    [Fact]
    public void ParameterListOfCorrectTypeAreAccepted() {
        var twp = new TriggerWithParameters<Trigger>(Trigger.X, typeof(int), typeof(string));
        twp.ValidateParameters(new object[] { 123, "arg" });
    }

    [Fact]
    public void ParametersArePolymorphic() {
        var twp = new TriggerWithParameters<Trigger, object>(Trigger.X);
        twp.ValidateParameters(new[] { "arg" });
    }

    [Fact]
    public void ParametersOfCorrectTypeAreAccepted() {
        var twp = new TriggerWithParameters<Trigger, string>(Trigger.X);
        twp.ValidateParameters(new[] { "arg" });
    }

    /// <summary>
    ///     issue #380 - default params on PermitIfDynamic lead to ambiguity at compile time... explicits work properly.
    /// </summary>
    [Fact]
    public void StateParameterIsNotAmbiguous() {
        var fsm = new StateMachine<State, Trigger>(State.A);
        var pressTrigger = fsm.SetTriggerParameters<State>(Trigger.X);

        fsm.Configure(State.A)
           .PermitDynamicIf(pressTrigger, state => state);
    }

    [Fact]
    public void TooFewParametersDetected() {
        var twp = new TriggerWithParameters<Trigger, string, string>(Trigger.X);
        Assert.Throws<ArgumentException>(() => twp.ValidateParameters(new[] { "a" }));
    }

    [Fact]
    public void TooManyParametersDetected() {
        var twp = new TriggerWithParameters<Trigger, string, string>(Trigger.X);
        Assert.Throws<ArgumentException>(() => twp.ValidateParameters(new[] { "a", "b", "c" }));
    }
}