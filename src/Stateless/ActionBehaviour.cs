using System;

namespace Stateless
{
    internal abstract class ActionBehaviour
    {
        readonly string _actionDescription;                     // _actionDescription can be null if user didn't specify a description

        protected ActionBehaviour(string actionDescription)     // actionDescription can be null if user didn't specify a description
        {
            _actionDescription = actionDescription;
        }

        /// <summary>
        /// The name of the action's method.  If the method is a lambda or delegate, the name will be a compiler-generated
        /// name that is often not human-friendly (e.g. "(.ctor)b__2_0" except with angle brackets instead of parentheses)
        /// </summary>
        internal abstract string ActionMethodName { get; }

        /// <summary>
        /// A description of the action.  If the user has specified the description, returns that, otherwise
        /// returns the name of the action's method.
        /// </summary>
        internal string ActionDescription { get { return _actionDescription ?? ActionMethodName; } }

        /// <summary>
        /// Returns true if the value returned from ActionDescription is calculated from the action's method,
        /// false if it is a value specified by the user.
        /// </summary>
        internal bool ActionDescriptionIsCalculated { get { return (_actionDescription == null);  } }

        /// <summary>
        /// Returns true if the action's method is asynchronous.
        /// </summary>
        internal abstract bool IsAsync { get; }
    }
}
