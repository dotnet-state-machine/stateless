namespace Stateless.Reflection
{
    /// <summary>
    /// Describes a transition that can be initiated from a trigger, but whose result is non-deterministic.
    /// </summary>
    public class DynamicTransitionInfo
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

        /// <summary>
        /// The trigger whose firing resulted in this transition.
        /// </summary>
        public TriggerInfo Trigger { get; private set; }

        /// <summary>
        /// Friendly text for dynamic transitions.
        /// </summary>
        public string Destination { get; private set; }

        /// <summary>
        /// Description of provided guard clause, if any.
        /// </summary>
        public string GuardDescription { get; private set; }
    }
}
