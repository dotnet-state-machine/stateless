using System;

namespace Stateless.Reflection
{
    /// <summary>
    /// Represents a trigger in a statemachine.
    /// </summary>
    public class TriggerInfo
    {
        internal TriggerInfo(object value)
        {
            Value = value;
        }

        /// <summary>
        /// The instance or value this trigger represents.
        /// </summary>
        public object Value { get; private set; }
        
        /// <summary>
        /// Passes through to the value's ToString.
        /// </summary>
        public override string ToString()
        {
            return Value?.ToString() ?? "<null>";
        }
    }
}
