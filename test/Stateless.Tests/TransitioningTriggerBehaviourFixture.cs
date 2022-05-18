using System;
using Xunit;

namespace Stateless.Tests; 

public class TransitioningTriggerBehaviourFixture
{
    [Fact]
    public void TransitionsToDestinationState()
    {
        var transtioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(Trigger.X, State.C, null);
        Assert.True(transtioning.ResultsInTransitionFrom(State.B, Array.Empty<object>(), out State destination));
        Assert.Equal(State.C, destination);
    }
}