namespace Stateless.Reflection
{
    /// <summary>
    /// Describes a transition that can be initiated from a trigger, but whose result is non-deterministic.
    /// </summary>
    public class DynamicTransitionInfo : TransitionInfo
    {
        internal static DynamicTransitionInfo Create<TState, TTrigger>(TTrigger trigger, StateMachine<TState, TTrigger>.DynamicTriggerBehaviour b, string destination)
        {
            var transition = new DynamicTransitionInfo
            {
                Trigger = new TriggerInfo(trigger, typeof(TTrigger)),
                Destination = destination
            };

            return transition;
        }

        private DynamicTransitionInfo() { }

        /// <summary>
        /// Friendly text for dynamic transitions.
        /// </summary>
        public string Destination { get; private set; }
    }
}
