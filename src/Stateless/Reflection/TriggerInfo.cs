using System;

namespace Stateless.Reflection
{
    /// <summary>
    /// Represents a trigger in a statemachine.
    /// </summary>
    public class TriggerInfo
    {
        internal TriggerInfo(object value, Type triggerType)
        {
            Value = value;
            TriggerType = triggerType;
        }

        /// <summary>
        /// The instance or value this trigger represents.
        /// </summary>
        public object Value { get; }
        
        /// <summary>
        /// The type of the underlying trigger.
        /// </summary>
        /// <returns></returns>
        public Type TriggerType { get; private set; }

        /// <summary>
        /// Passes through to the value's ToString.
        /// </summary>
        public override string ToString()
        {
            return Value?.ToString() ?? "<null>";
        }
    }
}
