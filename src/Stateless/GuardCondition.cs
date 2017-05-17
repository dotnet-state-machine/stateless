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
                Guard = Enforce.ArgumentNotNull(guard, nameof(guard));
                _methodDescription = Enforce.ArgumentNotNull(description, nameof(description));
            }
            internal Func<bool> Guard { get; }

            // Return the description of the guard method: the caller-defined description if one
            // was provided, else the name of the method itself
            internal string Description => _methodDescription.Description;

            // Return a more complete description of the guard method
            internal Reflection.InvocationInfo MethodDescription => _methodDescription;
        }
    }
}