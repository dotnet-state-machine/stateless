using System;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class GuardCondition
        {
            Reflection.InvocationInfo _methodDescription;

            internal GuardCondition(Func<bool> guard, Reflection.InvocationInfo description)
            {
                Guard = guard ?? throw new ArgumentNullException(nameof(guard));
                _methodDescription = description ?? throw new ArgumentNullException(nameof(description));
            }
            internal GuardCondition(Func<object,bool> paramitrizedGuard, Reflection.InvocationInfo description)
            {
                ParamitrizedGuard = paramitrizedGuard ?? throw new ArgumentNullException(nameof(paramitrizedGuard));
                _methodDescription = description ?? throw new ArgumentNullException(nameof(description));
            }
            internal Func<bool> Guard { get; }
            internal Func<object,bool> ParamitrizedGuard
            {
                get;
            }
            // Return the description of the guard method: the caller-defined description if one
            // was provided, else the name of the method itself
            internal string Description => _methodDescription.Description;

            // Return a more complete description of the guard method
            internal Reflection.InvocationInfo MethodDescription => _methodDescription;
        }
    }
}