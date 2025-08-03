namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class ReentryTriggerBehaviourAsync : TriggerBehaviourAsync
        {
            readonly TState _destination;

            internal TState Destination { get { return _destination; } }

            // transitionGuard can be null if there is no guard function on the transition
            public ReentryTriggerBehaviourAsync(TTrigger trigger, TState destination, TransitionGuardAsync transitionGuard)
                : base(trigger, transitionGuard)
            {
                _destination = destination;
            }
        }
    }
}
