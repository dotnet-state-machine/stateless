namespace Stateless.Reflection
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class TransitionInfo
    {
        /// <summary>
        /// The trigger whose firing resulted in this transition.
        /// </summary>
        public TriggerInfo Trigger { get; protected set; }

        /// <summary>
        /// Description of provided guard clause, if any.
        /// </summary>
        public string GuardDescription { get; protected set; }
    }
}
