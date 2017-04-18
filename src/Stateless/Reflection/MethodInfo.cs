using System;
using System.Collections.Generic;
using System.Linq;

namespace Stateless.Reflection
{
    /// <summary>
    /// Describes an internal method through the reflection API.
    /// </summary>
    public class MethodInfo
    {
        /// <summary>
        /// Text returned for compiler-generated functions where the caller has not specified a description
        /// </summary>
        static public string DefaultFunctionName { get; set; }  = "Function";

        internal static MethodInfo Create(MethodDescription methodDescription)
        {
            // Get the default method description (which is the description provided from the user (if any)
            // else the name of the method)
            string descrip = methodDescription.Description;

            // If the method description was calculated based on the method name, and the method was a delegate or lambda,
            // don't use the compiler-generated method name.
            if ( (methodDescription.DescriptionIsCalculated) && (descrip.IndexOfAny(new char[] { '<', '>' }) >= 0) )
                descrip = DefaultFunctionName;

            return new MethodInfo(descrip)
            {
                IsAsync = methodDescription.IsAsync,
                MethodName = methodDescription.MethodName
            };
        }

        private MethodInfo(string description)
        {
            Description = description;
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
        /// The name of the method, as reported by C#
        /// </summary>
        public string MethodName { get; private set; }

        /// <summary>
        /// Passes through to the value's ToString.
        /// </summary>
        public override string ToString()
        {
            return Description?.ToString() ?? "<null>";
        }
    }
}
