namespace Stateless.Tests;

public class DynamicTriggerBehaviour {
    [Fact]
    public async Task DestinationStateIsCalculatedBasedOnTriggerParameters() {
        var sm = new StateMachine<State, Trigger>(State.A);
        var trigger = sm.SetTriggerParameters<int>(Trigger.X);
        sm.Configure(State.A)
          .PermitDynamic(trigger, i => i == 1 ? State.B : State.C);

        await sm.FireAsync(trigger, 1);

        Assert.Equal(State.B, sm.State);
    }

    [Fact]
    public async Task DestinationStateIsDynamic() {
        var sm = new StateMachine<State, Trigger>(State.A);
        sm.Configure(State.A)
          .PermitDynamic(Trigger.X, () => State.B);

        await sm.FireAsync(Trigger.X);

        Assert.Equal(State.B, sm.State);
    }

    [Fact]
    public async Task Sdfsf() {
        var sm = new StateMachine<State, Trigger>(State.A);
        var trigger = sm.SetTriggerParameters<int>(Trigger.X);
        sm.Configure(State.A)
          .PermitDynamicIf(trigger, i => i == 1 ? State.C : State.B, i => i == 1);

        // Should not throw
        var unused = sm.GetPermittedTriggers().ToList();

        await sm.FireAsync(trigger, 1);
    }
}