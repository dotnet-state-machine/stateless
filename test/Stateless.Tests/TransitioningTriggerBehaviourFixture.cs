using Xunit;

namespace Stateless.Tests
{
    public class TransitioningTriggerBehaviourFixture
    {
        [Fact]
        public void TransitionsToDestinationState()
        {
            var transitioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(Trigger.X, State.C, null);
            Assert.True(transitioning.ResultsInTransitionFrom(State.B, new object[0], out State destination));
            Assert.Equal(State.C, destination);
        }
    }
}
