#if TASKS

using System;
using System.Threading.Tasks;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal partial class GuardCondition
        {
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
