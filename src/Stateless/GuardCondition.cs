using System;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class GuardCondition
        {
            MethodDescription _methodDescription;

            internal GuardCondition(Func<bool> guard, MethodDescription description)
            {
                Guard = Enforce.ArgumentNotNull(guard, nameof(guard));
                _methodDescription = Enforce.ArgumentNotNull(description, nameof(description));
            }
            internal Func<bool> Guard { get; }

            internal string Description => _methodDescription.Description;
        }
    }
}