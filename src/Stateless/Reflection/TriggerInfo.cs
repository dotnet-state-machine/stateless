namespace Stateless.Reflection
{
    /// <summary>
    /// Represents a trigger in a statemachine.
    /// </summary>
    public class TriggerInfo
    {
        internal TriggerInfo(object underlyingTrigger)
        {
            UnderlyingTrigger = underlyingTrigger;
        }

        /// <summary>
        /// The instance or value this trigger represents.
        /// </summary>
        public object UnderlyingTrigger { get; }
        
        /// <summary>
        /// Describes the trigger.
        /// </summary>
        public override string ToString()
        {
            return UnderlyingTrigger?.ToString() ?? "<null>";
        }
    }
}
