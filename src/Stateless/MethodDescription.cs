using System;

namespace Stateless
{
    internal class MethodDescription
    {
        readonly string _description;                     // _description can be null if user didn't specify a description

        public enum Timing
        {
            Synchronous,
            Asynchronous
        }
        Timing _timing;

        public static MethodDescription Create(Delegate method, string description, Timing timing = Timing.Synchronous)
        {
            return new MethodDescription(method?.TryGetMethodName(), description, timing);
        }

        private MethodDescription(string methodName, string description, Timing timing)      // description can be null if user didn't specify a description
        {
            MethodName = methodName;
            _description = description;
            _timing = timing;
        }

        /// <summary>
        /// The name of the invoked method.  If the method is a lambda or delegate, the name will be a compiler-generated
        /// name that is often not human-friendly (e.g. "(.ctor)b__2_0" except with angle brackets instead of parentheses)
        /// </summary>
        internal string MethodName { get; private set; }

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
        internal bool IsAsync => (_timing == Timing.Asynchronous);
    }
}
