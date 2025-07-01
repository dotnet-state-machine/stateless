namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class TransitioningTriggerBehaviourAsync : TriggerBehaviourAsync
        {
            internal TState Destination { get; }

            // transitionGuard can be null if there is no guard function on the transition
            public TransitioningTriggerBehaviourAsync(TTrigger trigger, TState destination, TransitionGuardAsync transitionGuard)
                : base(trigger, transitionGuard)
            {
                Destination = destination;
            }
        }
    }
}
