namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class TransitioningTriggerBehaviour : TriggerBehaviour
        {
            internal TState Destination { get; }

            // transitionGuard can be null if there is no guard function on the transition
            public TransitioningTriggerBehaviour(TTrigger trigger, TState destination, TransitionGuard transitionGuard)
                : base(trigger, transitionGuard)
            {
                Destination = destination;
            }
        }
    }
}
