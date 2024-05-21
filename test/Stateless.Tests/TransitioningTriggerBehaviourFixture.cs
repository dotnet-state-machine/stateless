using Xunit;

namespace Stateless.Tests
{
    public class TransitioningTriggerBehaviourFixture
    {
        [Fact]
        public void TransitionsToDestinationState()
        {
            var transitioning = new StateMachine<State, Trigger>.TransitioningTriggerBehaviour(Trigger.X, State.C, null);
            Assert.Equal(State.C, transitioning.Destination);
        }
    }
}
