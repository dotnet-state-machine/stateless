using System;

namespace Stateless
{
    internal abstract class MethodDescription
    {
        readonly string _description;                     // _description can be null if user didn't specify a description

        protected MethodDescription(string description)      // description can be null if user didn't specify a description
        {
            _description = description;
        }

        /// <summary>
        /// The name of the invoked method.  If the method is a lambda or delegate, the name will be a compiler-generated
        /// name that is often not human-friendly (e.g. "(.ctor)b__2_0" except with angle brackets instead of parentheses)
        /// </summary>
        internal abstract string MethodName { get; }

        /// <summary>
        /// A description of the invoked method.  If the user has specified the description, returns that, otherwise
        /// returns the name of the invoked method.
        /// </summary>
        internal string Description { get { return _description ?? MethodName; } }

        /// <summary>
        /// Returns true if the value returned from ActionDescription is calculated from the action's method,
        /// false if it is a value specified by the user.
        /// </summary>
        internal bool DescriptionIsCalculated { get { return (_description == null);  } }

        /// <summary>
        /// Returns true if the method is invoked asynchronously.
        /// </summary>
        internal abstract bool IsAsync { get; }
    }
}
