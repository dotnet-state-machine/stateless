using System.Collections.Generic;

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
        /// Method descriptions of the guard conditions
        /// </summary>
        public IEnumerable<MethodInfo> GuardConditionsMethodDescriptions;
    }
}
