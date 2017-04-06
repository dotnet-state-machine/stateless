using System;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class GuardCondition
        {
            private readonly string _description;
            internal GuardCondition(Func<bool> guard, string description)
            {
                Guard = Enforce.ArgumentNotNull(guard, nameof(guard));
                _description = description;
            }
            internal Func<bool> Guard { get; }
            internal string Description => _description ?? Guard.TryGetMethodName();
        }
    }
}