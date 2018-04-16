using System;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class GuardCondition
        {
            Reflection.InvocationInfo _methodDescription;

            /// <summary>
            /// Constructor that takes in a guard with no argument.
            /// This is needed because we wrap the no-arg guard with a lamba and therefore method description won't match what was origianlly passed in.
            /// We need to preserve the method description before wrapping so Reflection methods will work.
            /// </summary>
            /// <param name="guard">No Argument Guard Condition</param>
            /// <param name="description"></param>
            internal GuardCondition(Func<bool> guard, Reflection.InvocationInfo description)
                : this(args => guard(), description)
            {
            }

            internal GuardCondition(Func<object[], bool> guard, Reflection.InvocationInfo description)
            {
                Guard = guard ?? throw new ArgumentNullException(nameof(guard));
                _methodDescription = description ?? throw new ArgumentNullException(nameof(description));
            }

            internal Func<object[], bool> Guard { get; }

            // Return the description of the guard method: the caller-defined description if one
            // was provided, else the name of the method itself
            internal string Description => _methodDescription.Description;

            // Return a more complete description of the guard method
            internal Reflection.InvocationInfo MethodDescription => _methodDescription;
        }
    }
}