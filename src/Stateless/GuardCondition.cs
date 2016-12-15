using System;

namespace Stateless
{
    public partial class StateMachine<TState, TTrigger>
    {
        internal class GuardCondition
        {
            Func<bool> _guard;
            string _description;
            internal GuardCondition(Func<bool> guard, string description)
            {
                _guard = Enforce.ArgumentNotNull(guard, nameof(guard));
                _description = description;
            }
            internal Func<bool> Guard { get { return _guard; } }
            internal string Description { get { return _description ?? _guard.TryGetMethodName(); } }
        }
    }
}