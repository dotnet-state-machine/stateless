#if TASKS

using System;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal partial class GuardCondition
        {
            /// <summary>
            /// Constructor that takes in a guard with no argument.
            /// This is needed because we wrap the no-arg guard with a lamba and therefore method description won't match what was origianlly passed in.
            /// We need to preserve the method description before wrapping so Reflection methods will work.
            /// </summary>
            /// <param name="guard">No Argument Guard Condition</param>
            /// <param name="description"></param>
            internal GuardCondition(Func<Task<bool>> guard, Reflection.InvocationInfo description)
                : this(args => guard(), description)
            {
            }

            internal GuardCondition(Func<object[], Task<bool>> guard, Reflection.InvocationInfo description)
            {
                GuardAsync = guard ?? throw new ArgumentNullException(nameof(guard));
                _methodDescription = description ?? throw new ArgumentNullException(nameof(description));
            }

            internal Func<object[], Task<bool>> GuardAsync { get; }
        }
    }
}

#endif
