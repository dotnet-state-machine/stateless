using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless.Reflection
{
    /// <summary>
    /// Describes an internal Action through the reflection API.
    /// </summary>
    public class ActionInfo
    {
        static readonly string _defaultFunctionName = "Function";

        internal static ActionInfo CreateActionInfo(ActionBehaviour action)
        {
            string descrip;

            descrip = action.ActionDescription;

            // If the action description was calculated based on the method name, and the method was a delegate or lambda,
            // don't use the compiler-generated method name.
            if ( (action.ActionDescriptionIsCalculated) && (descrip.IndexOfAny(new char[] { '<', '>' }) >= 0) )
                descrip = _defaultFunctionName;

            ActionInfo actionInfo = new ActionInfo(descrip, action.IsAsync);

            return actionInfo;
        }

        private ActionInfo(string description, bool isAsync)
        {
            Description = description;
            IsAsync = isAsync;
        }

        /// <summary>
        /// The action's description field
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Whether the action is synchronous or asynchronous
        /// </summary>
        public bool IsAsync { get; private set; }

        /// <summary>
        /// Passes through to the value's ToString.
        /// </summary>
        public override string ToString()
        {
            return Description?.ToString() ?? "<null>";
        }
    }
}
